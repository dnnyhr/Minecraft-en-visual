using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BlockForge.Models;

namespace BlockForge.Motor
{
    /// <summary>Vértices de un chunk antes de compilarlos en una display list.</summary>
    internal class Malla
    {
        private float[] pos = new float[3 * 8192];
        private float[] uv = new float[2 * 8192];
        private byte[] col = new byte[4 * 8192];
        public int Vertices { get; private set; }

        public void Reiniciar()
        {
            Vertices = 0;
        }

        public void Agregar(float x, float y, float z, float u, float v, Luz luz, byte alfa)
        {
            if (Vertices * 3 + 3 > pos.Length)
            {
                Array.Resize(ref pos, pos.Length * 2);
                Array.Resize(ref uv, uv.Length * 2);
                Array.Resize(ref col, col.Length * 2);
            }
            int p = Vertices * 3, t = Vertices * 2, c = Vertices * 4;
            pos[p] = x; pos[p + 1] = y; pos[p + 2] = z;
            uv[t] = u; uv[t + 1] = v;
            col[c] = luz.R; col[c + 1] = luz.G; col[c + 2] = luz.B; col[c + 3] = alfa;
            Vertices++;
        }

        /// <summary>Compila los vértices en una display list (el controlador la guarda en la GPU).</summary>
        public uint Compilar(uint lista)
        {
            if (Vertices == 0)
            {
                if (lista != 0) GL.glDeleteLists(lista, 1);
                return 0;
            }
            if (lista == 0) lista = GL.glGenLists(1);
            GCHandle hp = GCHandle.Alloc(pos, GCHandleType.Pinned);
            GCHandle ht = GCHandle.Alloc(uv, GCHandleType.Pinned);
            GCHandle hc = GCHandle.Alloc(col, GCHandleType.Pinned);
            try
            {
                GL.glVertexPointer(3, GL.FLOAT, 0, hp.AddrOfPinnedObject());
                GL.glTexCoordPointer(2, GL.FLOAT, 0, ht.AddrOfPinnedObject());
                GL.glColorPointer(4, GL.UNSIGNED_BYTE, 0, hc.AddrOfPinnedObject());
                GL.glNewList(lista, GL.COMPILE);
                GL.glDrawArrays(GL.QUADS, 0, Vertices);
                GL.glEndList();
            }
            finally
            {
                hp.Free();
                ht.Free();
                hc.Free();
            }
            return lista;
        }
    }

    /// <summary>Color de luz de un vértice.</summary>
    internal struct Luz
    {
        public byte R, G, B;
    }

    /// <summary>
    /// Dibuja el mundo dividido en chunks de 16×16×16. Cada chunk tiene una lista para los bloques
    /// opacos y otra para el agua, y solo se reconstruye cuando cambia algo cerca.
    /// Luz: la del sol por columnas (como ClassiCube), multiplicada por la hora del día,
    /// más la luz de bloques (antorchas, lava y lámparas) que se propaga perdiendo un nivel por bloque.
    /// </summary>
    internal class RenderMundo : IDisposable
    {
        public const int T = 16;
        private const float SombraSol = 0.55f;
        private const int AlcanceLuz = 15;

        private readonly Mundo mundo;
        private readonly int cx, cy, cz;
        private readonly uint[] listasOpacas, listasAgua;
        private readonly bool[] sucios;
        private readonly int[] alturas;
        private readonly byte[] luzBloque;
        private readonly Malla mallaOpaca = new Malla(), mallaAgua = new Malla();
        private readonly float[] curvaLuz = new float[16];
        private int pendientes;
        private float dia = 1f, diaMallas = 1f;

        public int ChunksDibujados { get; private set; }
        public int TotalChunks { get { return listasOpacas.Length; } }

        /// <summary>Altura del plano exterior (agua o pasto) que rodea el mapa.</summary>
        public float AlturaBorde
        {
            get { return mundo.ConLagos ? mundo.NivelAgua + 0.9f : mundo.NivelAgua + 1f; }
        }

        /// <summary>
        /// Luz del día de 0,12 (noche) a 1 (mediodía). Al cambiar lo suficiente se vuelven a armar
        /// las mallas poco a poco, igual que hace ClassiCube con su iluminación.
        /// </summary>
        public float Dia
        {
            get { return dia; }
            set
            {
                dia = value;
                if (Math.Abs(dia - diaMallas) >= 0.04f)
                {
                    diaMallas = (float)Math.Round(dia / 0.04f) * 0.04f;
                    for (int i = 0; i < sucios.Length; i++)
                    {
                        if (!sucios[i]) { sucios[i] = true; pendientes++; }
                    }
                }
            }
        }

        public RenderMundo(Mundo mundo, float diaInicial)
        {
            this.mundo = mundo;
            dia = diaMallas = diaInicial;
            cx = (mundo.Ancho + T - 1) / T;
            cy = (mundo.Alto + T - 1) / T;
            cz = (mundo.Largo + T - 1) / T;
            listasOpacas = new uint[cx * cy * cz];
            listasAgua = new uint[cx * cy * cz];
            sucios = new bool[cx * cy * cz];
            alturas = new int[mundo.Ancho * mundo.Largo];
            luzBloque = new byte[mundo.Ancho * mundo.Alto * mundo.Largo];
            for (int i = 1; i < 16; i++) curvaLuz[i] = (float)Math.Pow(0.84, 15 - i);
            for (int x = 0; x < mundo.Ancho; x++)
            {
                for (int z = 0; z < mundo.Largo; z++) alturas[z * mundo.Ancho + x] = CalcularAltura(x, z);
            }
            CalcularLuzBloques();
            for (int i = 0; i < sucios.Length; i++) sucios[i] = true;
            pendientes = sucios.Length;
        }

        private int Indice(int x, int y, int z)
        {
            return (y * mundo.Largo + z) * mundo.Ancho + x;
        }

        private int CalcularAltura(int x, int z)
        {
            for (int y = mundo.Alto - 1; y >= 0; y--)
            {
                int id = mundo.Celdas[Indice(x, y, z)];
                if (id != 0 && !Atlas.Obtener(id).DejaPasarSol) return y;
            }
            return -1;
        }

        public bool EstaIluminado(int x, int y, int z)
        {
            if (x < 0 || z < 0 || x >= mundo.Ancho || z >= mundo.Largo || y >= mundo.Alto) return true;
            if (y < 0) return false;
            return y > alturas[z * mundo.Ancho + x];
        }

        public int LuzBloque(int x, int y, int z)
        {
            if (!mundo.EstaDentro(x, y, z)) return 0;
            return luzBloque[Indice(x, y, z)];
        }

        /// <summary>Brillo de 0 a 1 en una celda (para mobs, partículas y el modelo del jugador).</summary>
        public float BrilloEn(int x, int y, int z)
        {
            float sol = (EstaIluminado(x, y, z) ? 1f : SombraSol) * dia;
            return Math.Max(0.08f, Math.Max(sol, curvaLuz[LuzBloque(x, y, z)]));
        }

        // ------------------------------------------------------------------ luz de bloques
        private static int Atenuacion(int id)
        {
            if (id == 0) return 1;
            Apariencia a = Atlas.Obtener(id);
            switch (a.Modo)
            {
                case ModoDibujo.Planta:
                case ModoDibujo.Vidrio:
                    return 1;
                case ModoDibujo.Liquido:
                case ModoDibujo.Recortado:
                    return 2;
                default:
                    return 99; // opaco: la luz no pasa
            }
        }

        private void CalcularLuzBloques()
        {
            Array.Clear(luzBloque, 0, luzBloque.Length);
            Queue<int> cola = new Queue<int>();
            byte[] celdas = mundo.Celdas;
            for (int i = 0; i < celdas.Length; i++)
            {
                int id = celdas[i];
                if (id == 0) continue;
                int luz = Atlas.Obtener(id).Luz;
                if (luz > 0)
                {
                    luzBloque[i] = (byte)luz;
                    cola.Enqueue(i);
                }
            }
            Propagar(cola, 0, 0, 0, mundo.Ancho - 1, mundo.Alto - 1, mundo.Largo - 1);
        }

        /// <summary>Expande la luz desde la cola sin salir de la caja indicada.</summary>
        private void Propagar(Queue<int> cola, int x0, int y0, int z0, int x1, int y1, int z1)
        {
            int ancho = mundo.Ancho, largo = mundo.Largo;
            byte[] celdas = mundo.Celdas;
            while (cola.Count > 0)
            {
                int i = cola.Dequeue();
                int luz = luzBloque[i];
                if (luz <= 1) continue;
                int x = i % ancho, z = (i / ancho) % largo, y = i / (ancho * largo);
                for (int k = 0; k < 6; k++)
                {
                    int nx = x, ny = y, nz = z;
                    switch (k)
                    {
                        case 0: nx--; break;
                        case 1: nx++; break;
                        case 2: ny--; break;
                        case 3: ny++; break;
                        case 4: nz--; break;
                        default: nz++; break;
                    }
                    if (nx < x0 || ny < y0 || nz < z0 || nx > x1 || ny > y1 || nz > z1) continue;
                    int j = (ny * largo + nz) * ancho + nx;
                    int nueva = luz - Atenuacion(celdas[j]);
                    if (nueva > luzBloque[j])
                    {
                        luzBloque[j] = (byte)nueva;
                        cola.Enqueue(j);
                    }
                }
            }
        }

        /// <summary>Recalcula la luz de bloques alrededor de un cambio y marca los chunks afectados.</summary>
        private void ActualizarLuz(int x, int y, int z)
        {
            int x0 = Math.Max(0, x - AlcanceLuz), y0 = Math.Max(0, y - AlcanceLuz), z0 = Math.Max(0, z - AlcanceLuz);
            int x1 = Math.Min(mundo.Ancho - 1, x + AlcanceLuz), y1 = Math.Min(mundo.Alto - 1, y + AlcanceLuz), z1 = Math.Min(mundo.Largo - 1, z + AlcanceLuz);
            int sx = x1 - x0 + 1, sy = y1 - y0 + 1, sz = z1 - z0 + 1;
            byte[] antes = new byte[sx * sy * sz];
            Queue<int> cola = new Queue<int>();
            byte[] celdas = mundo.Celdas;

            for (int yy = y0; yy <= y1; yy++)
            {
                for (int zz = z0; zz <= z1; zz++)
                {
                    for (int xx = x0; xx <= x1; xx++)
                    {
                        int i = Indice(xx, yy, zz);
                        antes[((yy - y0) * sz + (zz - z0)) * sx + (xx - x0)] = luzBloque[i];
                        int emite = celdas[i] != 0 ? Atlas.Obtener(celdas[i]).Luz : 0;
                        luzBloque[i] = (byte)emite;
                        if (emite > 0) cola.Enqueue(i);
                    }
                }
            }
            // La luz que entra desde fuera de la caja se toma de las celdas vecinas (que no cambian).
            for (int yy = y0; yy <= y1; yy++)
            {
                for (int zz = z0; zz <= z1; zz++)
                {
                    for (int xx = x0; xx <= x1; xx++)
                    {
                        if (xx != x0 && xx != x1 && yy != y0 && yy != y1 && zz != z0 && zz != z1) continue;
                        int mejor = 0;
                        if (xx == x0) mejor = Math.Max(mejor, LuzBloque(xx - 1, yy, zz));
                        if (xx == x1) mejor = Math.Max(mejor, LuzBloque(xx + 1, yy, zz));
                        if (yy == y0) mejor = Math.Max(mejor, LuzBloque(xx, yy - 1, zz));
                        if (yy == y1) mejor = Math.Max(mejor, LuzBloque(xx, yy + 1, zz));
                        if (zz == z0) mejor = Math.Max(mejor, LuzBloque(xx, yy, zz - 1));
                        if (zz == z1) mejor = Math.Max(mejor, LuzBloque(xx, yy, zz + 1));
                        int i = Indice(xx, yy, zz);
                        int entra = mejor - Atenuacion(celdas[i]);
                        if (entra > luzBloque[i])
                        {
                            luzBloque[i] = (byte)entra;
                            cola.Enqueue(i);
                        }
                    }
                }
            }
            Propagar(cola, x0, y0, z0, x1, y1, z1);

            int mx0 = int.MaxValue, my0 = int.MaxValue, mz0 = int.MaxValue, mx1 = -1, my1 = -1, mz1 = -1;
            for (int yy = y0; yy <= y1; yy++)
            {
                for (int zz = z0; zz <= z1; zz++)
                {
                    for (int xx = x0; xx <= x1; xx++)
                    {
                        if (antes[((yy - y0) * sz + (zz - z0)) * sx + (xx - x0)] == luzBloque[Indice(xx, yy, zz)]) continue;
                        mx0 = Math.Min(mx0, xx); my0 = Math.Min(my0, yy); mz0 = Math.Min(mz0, zz);
                        mx1 = Math.Max(mx1, xx); my1 = Math.Max(my1, yy); mz1 = Math.Max(mz1, zz);
                    }
                }
            }
            if (mx1 < 0) return;
            for (int yy = Math.Max(0, my0 - 1); yy <= Math.Min(mundo.Alto - 1, my1 + 1); yy += T) MarcarFila(mx0 - 1, mx1 + 1, yy, mz0 - 1, mz1 + 1);
            MarcarFila(mx0 - 1, mx1 + 1, Math.Min(mundo.Alto - 1, my1 + 1), mz0 - 1, mz1 + 1);
        }

        private void MarcarFila(int xa, int xb, int y, int za, int zb)
        {
            for (int x = xa; x <= xb + T; x += T)
            {
                for (int z = za; z <= zb + T; z += T) MarcarCelda(Math.Min(x, xb), y, Math.Min(z, zb));
            }
        }

        // ------------------------------------------------------------------ cambios de bloques
        /// <summary>Marca para reconstrucción los chunks afectados por el cambio de un bloque.</summary>
        public void BloqueCambiado(int x, int y, int z)
        {
            int indice = z * mundo.Ancho + x;
            int anterior = alturas[indice];
            int nueva = CalcularAltura(x, z);
            alturas[indice] = nueva;

            MarcarCelda(x, y, z);
            MarcarCelda(x - 1, y, z); MarcarCelda(x + 1, y, z);
            MarcarCelda(x, y - 1, z); MarcarCelda(x, y + 1, z);
            MarcarCelda(x, y, z - 1); MarcarCelda(x, y, z + 1);

            if (anterior != nueva)
            {
                // Cambió la sombra de la columna: se actualizan los vecinos en todo ese tramo.
                int y0 = Math.Max(0, Math.Min(anterior, nueva)), y1 = Math.Min(mundo.Alto - 1, Math.Max(anterior, nueva) + 1);
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dz = -1; dz <= 1; dz++)
                    {
                        for (int yy = y0; yy <= y1; yy += T) MarcarCelda(x + dx, yy, z + dz);
                        MarcarCelda(x + dx, y1, z + dz);
                    }
                }
            }

            // La luz de bloques solo cambia si hay luz cerca o si el bloque emite.
            int id = mundo.ObtenerBloque(x, y, z);
            bool hayLuz = (id != 0 && Atlas.Obtener(id).Luz > 0) || luzBloque[Indice(x, y, z)] > 0 ||
                          LuzBloque(x - 1, y, z) > 0 || LuzBloque(x + 1, y, z) > 0 || LuzBloque(x, y - 1, z) > 0 ||
                          LuzBloque(x, y + 1, z) > 0 || LuzBloque(x, y, z - 1) > 0 || LuzBloque(x, y, z + 1) > 0;
            if (hayLuz) ActualizarLuz(x, y, z);
        }

        private void MarcarCelda(int x, int y, int z)
        {
            if (!mundo.EstaDentro(x, y, z)) return;
            int i = ((y / T) * cz + z / T) * cx + x / T;
            if (!sucios[i])
            {
                sucios[i] = true;
                pendientes++;
            }
        }

        /// <summary>Reconstruye hasta "maximo" chunks pendientes. Devuelve cuántos quedan.</summary>
        public int ReconstruirPendientes(int maximo)
        {
            if (pendientes == 0) return 0;
            for (int i = 0; i < sucios.Length && maximo > 0; i++)
            {
                if (!sucios[i]) continue;
                int ix = i % cx, iz = (i / cx) % cz, iy = i / (cx * cz);
                Construir(i, ix * T, iy * T, iz * T);
                sucios[i] = false;
                pendientes--;
                maximo--;
            }
            return pendientes;
        }

        public void Dibujar(Frustum frustum, Vec3 ojo, float distancia, bool agua)
        {
            uint[] listas = agua ? listasAgua : listasOpacas;
            float limite = distancia + T;
            int dibujados = 0;
            for (int i = 0; i < listas.Length; i++)
            {
                if (listas[i] == 0) continue;
                int ix = i % cx, iz = (i / cx) % cz, iy = i / (cx * cz);
                float x0 = ix * T, y0 = iy * T, z0 = iz * T;
                float dx = x0 + T / 2f - ojo.X, dz = z0 + T / 2f - ojo.Z;
                if (dx * dx + dz * dz > limite * limite) continue;
                if (!frustum.CajaVisible(x0, y0, z0, x0 + T, y0 + T, z0 + T)) continue;
                GL.glCallList(listas[i]);
                dibujados++;
            }
            if (!agua) ChunksDibujados = dibujados;
        }

        // ------------------------------------------------------------------ construcción de mallas
        /// <summary>Id del vecino; -1 indica un límite opaco (fuera del mapa por los lados o por debajo).</summary>
        private int Vecino(int x, int y, int z)
        {
            if (y >= mundo.Alto) return 0;
            if (y < 0 || x < 0 || z < 0 || x >= mundo.Ancho || z >= mundo.Largo) return -1;
            return mundo.Celdas[Indice(x, y, z)];
        }

        private static bool CaraVisible(int propioId, ModoDibujo propio, int vecino)
        {
            if (vecino < 0) return false;
            if (vecino == 0) return true;
            ModoDibujo otro = Atlas.Obtener(vecino).Modo;
            switch (propio)
            {
                case ModoDibujo.Liquido:
                    return otro != ModoDibujo.Opaco && otro != ModoDibujo.Liquido && otro != ModoDibujo.Lava;
                case ModoDibujo.Lava:
                    return otro != ModoDibujo.Opaco && otro != ModoDibujo.Lava;
                case ModoDibujo.Vidrio:
                    return otro != ModoDibujo.Opaco && vecino != propioId;
                default:
                    return otro != ModoDibujo.Opaco;
            }
        }

        /// <summary>Luz de una cara según la celda a la que mira (sol por la hora del día y luz de bloques).</summary>
        private Luz LuzCara(float cara, int nx, int ny, int nz, int emitePropio)
        {
            float sol = (EstaIluminado(nx, ny, nz) ? 1f : SombraSol) * diaMallas;
            // De noche el cielo tiñe de azul; la luz de bloques es cálida.
            float t = Math.Max(0f, Math.Min(1f, (diaMallas - 0.12f) / 0.88f));
            float cr = 0.62f + 0.38f * t, cg = 0.68f + 0.32f * t, cb = 1f;
            int lb = Math.Max(LuzBloque(nx, ny, nz), emitePropio);
            float b = curvaLuz[lb];
            float r = Math.Max(Math.Max(sol * cr, b), 0.05f) * cara;
            float g = Math.Max(Math.Max(sol * cg, b * 0.86f), 0.05f) * cara;
            float bl = Math.Max(Math.Max(sol * cb, b * 0.62f), 0.06f) * cara;
            return new Luz { R = (byte)(Math.Min(1f, r) * 255), G = (byte)(Math.Min(1f, g) * 255), B = (byte)(Math.Min(1f, bl) * 255) };
        }

        private void Construir(int indice, int x0, int y0, int z0)
        {
            mallaOpaca.Reiniciar();
            mallaAgua.Reiniciar();
            int x1 = Math.Min(x0 + T, mundo.Ancho), y1 = Math.Min(y0 + T, mundo.Alto), z1 = Math.Min(z0 + T, mundo.Largo);
            byte[] celdas = mundo.Celdas;

            for (int y = y0; y < y1; y++)
            {
                for (int z = z0; z < z1; z++)
                {
                    for (int x = x0; x < x1; x++)
                    {
                        int id = celdas[Indice(x, y, z)];
                        if (id == 0) continue;
                        Apariencia a = Atlas.Obtener(id);
                        switch (a.Modo)
                        {
                            case ModoDibujo.Aire:
                                break;
                            case ModoDibujo.Planta:
                                AgregarPlanta(mallaOpaca, a, x, y, z);
                                break;
                            case ModoDibujo.Liquido:
                            case ModoDibujo.Lava:
                                {
                                    float alto = Vecino(x, y + 1, z) != id ? 0.9f : 1f;
                                    AgregarCubo(a.Modo == ModoDibujo.Liquido ? mallaAgua : mallaOpaca, id, a, x, y, z, alto);
                                    break;
                                }
                            default:
                                AgregarCubo(mallaOpaca, id, a, x, y, z, 1f);
                                break;
                        }
                    }
                }
            }

            listasOpacas[indice] = mallaOpaca.Compilar(listasOpacas[indice]);
            listasAgua[indice] = mallaAgua.Compilar(listasAgua[indice]);
        }

        /// <summary>Dos planos cruzados, con ambas caras, para flores, hierba, brotes y antorchas.</summary>
        private void AgregarPlanta(Malla m, Apariencia a, int x, int y, int z)
        {
            float u0, v0, u1, v1;
            Atlas.Uv(a.Lado, out u0, out v0, out u1, out v1);
            Luz l = LuzCara(0.95f, x, y, z, a.Luz);
            const float e = 0.15f;
            float xa = x + e, xb = x + 1 - e, za = z + e, zb = z + 1 - e, ya = y, yb = y + 1;
            // Diagonal 1 (dos caras)
            m.Agregar(xa, ya, za, u0, v1, l, 255); m.Agregar(xb, ya, zb, u1, v1, l, 255); m.Agregar(xb, yb, zb, u1, v0, l, 255); m.Agregar(xa, yb, za, u0, v0, l, 255);
            m.Agregar(xb, ya, zb, u1, v1, l, 255); m.Agregar(xa, ya, za, u0, v1, l, 255); m.Agregar(xa, yb, za, u0, v0, l, 255); m.Agregar(xb, yb, zb, u1, v0, l, 255);
            // Diagonal 2
            m.Agregar(xa, ya, zb, u0, v1, l, 255); m.Agregar(xb, ya, za, u1, v1, l, 255); m.Agregar(xb, yb, za, u1, v0, l, 255); m.Agregar(xa, yb, zb, u0, v0, l, 255);
            m.Agregar(xb, ya, za, u1, v1, l, 255); m.Agregar(xa, ya, zb, u0, v1, l, 255); m.Agregar(xa, yb, zb, u0, v0, l, 255); m.Agregar(xb, yb, za, u1, v0, l, 255);
        }

        private void AgregarCubo(Malla m, int id, Apariencia a, int x, int y, int z, float alto)
        {
            float u0, v0, u1, v1;
            byte alfa = 255;
            int emite = a.Luz;
            float xa = x, xb = x + 1, ya = y, yb = y + alto, za = z, zb = z + 1;

            if (CaraVisible(id, a.Modo, Vecino(x, y + 1, z)) || alto < 1f)
            {
                Atlas.Uv(a.Arriba, out u0, out v0, out u1, out v1);
                Luz l = LuzCara(1f, x, y + 1, z, emite);
                m.Agregar(xa, yb, za, u0, v0, l, alfa);
                m.Agregar(xa, yb, zb, u0, v1, l, alfa);
                m.Agregar(xb, yb, zb, u1, v1, l, alfa);
                m.Agregar(xb, yb, za, u1, v0, l, alfa);
            }
            if (CaraVisible(id, a.Modo, Vecino(x, y - 1, z)))
            {
                Atlas.Uv(a.Abajo, out u0, out v0, out u1, out v1);
                Luz l = LuzCara(0.5f, x, y - 1, z, emite);
                m.Agregar(xa, ya, za, u0, v0, l, alfa);
                m.Agregar(xb, ya, za, u1, v0, l, alfa);
                m.Agregar(xb, ya, zb, u1, v1, l, alfa);
                m.Agregar(xa, ya, zb, u0, v1, l, alfa);
            }

            Atlas.Uv(a.Lado, out u0, out v0, out u1, out v1);
            float vt = v1 - (v1 - v0) * alto;
            if (CaraVisible(id, a.Modo, Vecino(x, y, z - 1)))
            {
                Luz l = LuzCara(0.8f, x, y, z - 1, emite);
                m.Agregar(xb, ya, za, u0, v1, l, alfa);
                m.Agregar(xa, ya, za, u1, v1, l, alfa);
                m.Agregar(xa, yb, za, u1, vt, l, alfa);
                m.Agregar(xb, yb, za, u0, vt, l, alfa);
            }
            if (CaraVisible(id, a.Modo, Vecino(x, y, z + 1)))
            {
                Luz l = LuzCara(0.8f, x, y, z + 1, emite);
                m.Agregar(xa, ya, zb, u0, v1, l, alfa);
                m.Agregar(xb, ya, zb, u1, v1, l, alfa);
                m.Agregar(xb, yb, zb, u1, vt, l, alfa);
                m.Agregar(xa, yb, zb, u0, vt, l, alfa);
            }
            if (CaraVisible(id, a.Modo, Vecino(x - 1, y, z)))
            {
                Luz l = LuzCara(0.6f, x - 1, y, z, emite);
                m.Agregar(xa, ya, za, u0, v1, l, alfa);
                m.Agregar(xa, ya, zb, u1, v1, l, alfa);
                m.Agregar(xa, yb, zb, u1, vt, l, alfa);
                m.Agregar(xa, yb, za, u0, vt, l, alfa);
            }
            if (CaraVisible(id, a.Modo, Vecino(x + 1, y, z)))
            {
                Luz l = LuzCara(0.6f, x + 1, y, z, emite);
                m.Agregar(xb, ya, zb, u0, v1, l, alfa);
                m.Agregar(xb, ya, za, u1, v1, l, alfa);
                m.Agregar(xb, yb, za, u1, vt, l, alfa);
                m.Agregar(xb, yb, zb, u0, vt, l, alfa);
            }
        }

        // ------------------------------------------------------------------ alrededores del mapa
        /// <summary>Plano exterior (agua o pasto) y paredes de piedra en los bordes, como en ClassiCube.</summary>
        public void DibujarBordes(float distancia, bool translucido)
        {
            float d = distancia + 64;
            float w = mundo.Ancho, l = mundo.Largo, h = AlturaBorde;
            float k = Math.Max(0.1f, dia);

            if (!translucido)
            {
                // Paredes interiores en el borde del mapa (se ven al cavar junto al límite o bajo el agua).
                GL.glBindTexture(GL.TEXTURE_2D, Atlas.TexturaPiedraBorde);
                GL.glBegin(GL.QUADS);
                GL.glColor3f(0.6f * k, 0.6f * k, 0.6f * k);
                GL.glTexCoord2f(0, h); GL.glVertex3f(0, 0, l);
                GL.glTexCoord2f(l, h); GL.glVertex3f(0, 0, 0);
                GL.glTexCoord2f(l, 0); GL.glVertex3f(0, h, 0);
                GL.glTexCoord2f(0, 0); GL.glVertex3f(0, h, l);

                GL.glTexCoord2f(0, h); GL.glVertex3f(w, 0, 0);
                GL.glTexCoord2f(l, h); GL.glVertex3f(w, 0, l);
                GL.glTexCoord2f(l, 0); GL.glVertex3f(w, h, l);
                GL.glTexCoord2f(0, 0); GL.glVertex3f(w, h, 0);

                GL.glColor3f(0.8f * k, 0.8f * k, 0.8f * k);
                GL.glTexCoord2f(0, h); GL.glVertex3f(0, 0, 0);
                GL.glTexCoord2f(w, h); GL.glVertex3f(w, 0, 0);
                GL.glTexCoord2f(w, 0); GL.glVertex3f(w, h, 0);
                GL.glTexCoord2f(0, 0); GL.glVertex3f(0, h, 0);

                GL.glTexCoord2f(0, h); GL.glVertex3f(w, 0, l);
                GL.glTexCoord2f(w, h); GL.glVertex3f(0, 0, l);
                GL.glTexCoord2f(w, 0); GL.glVertex3f(0, h, l);
                GL.glTexCoord2f(0, 0); GL.glVertex3f(w, h, l);
                GL.glEnd();

                if (mundo.ConLagos)
                {
                    GL.glBindTexture(GL.TEXTURE_2D, Atlas.TexturaArenaBorde);
                    PlanoExterior(-d, -d, w + d, l + d, mundo.NivelAgua - 3f, 0.62f * k, 1f);
                }
                else
                {
                    GL.glBindTexture(GL.TEXTURE_2D, Atlas.TexturaPastoBorde);
                    PlanoExterior(-d, -d, w + d, l + d, h, k, 1f);
                }
            }
            else if (mundo.ConLagos)
            {
                GL.glBindTexture(GL.TEXTURE_2D, Atlas.TexturaAguaBorde);
                PlanoExterior(-d, -d, w + d, l + d, h, k, 0.72f);
            }
        }

        /// <summary>Dibuja el plano horizontal alrededor del mapa, sin cubrir el mapa mismo.</summary>
        private void PlanoExterior(float x0, float z0, float x1, float z1, float y, float luz, float alfa)
        {
            float w = mundo.Ancho, l = mundo.Largo;
            GL.glColor4f(luz, luz, luz, alfa);
            GL.glBegin(GL.QUADS);
            Rect(x0, z0, 0, z1, y);
            Rect(w, z0, x1, z1, y);
            Rect(0, z0, w, 0, y);
            Rect(0, l, w, z1, y);
            GL.glEnd();
        }

        private static void Rect(float x0, float z0, float x1, float z1, float y)
        {
            GL.glTexCoord2f(x0, z0); GL.glVertex3f(x0, y, z0);
            GL.glTexCoord2f(x0, z1); GL.glVertex3f(x0, y, z1);
            GL.glTexCoord2f(x1, z1); GL.glVertex3f(x1, y, z1);
            GL.glTexCoord2f(x1, z0); GL.glVertex3f(x1, y, z0);
        }

        public void Dispose()
        {
            for (int i = 0; i < listasOpacas.Length; i++)
            {
                if (listasOpacas[i] != 0) GL.glDeleteLists(listasOpacas[i], 1);
                if (listasAgua[i] != 0) GL.glDeleteLists(listasAgua[i], 1);
                listasOpacas[i] = listasAgua[i] = 0;
            }
        }
    }
}
