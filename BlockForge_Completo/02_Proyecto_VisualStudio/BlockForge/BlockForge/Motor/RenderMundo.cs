using System;
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

        public void Agregar(float x, float y, float z, float u, float v, byte luz, byte alfa)
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
            col[c] = luz; col[c + 1] = luz; col[c + 2] = luz; col[c + 3] = alfa;
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

    /// <summary>
    /// Dibuja el mundo dividido en chunks de 16×16×16. Cada chunk tiene una lista para los bloques
    /// opacos y otra para el agua, y solo se reconstruye cuando cambia un bloque cercano.
    /// La iluminación es la de ClassiCube: una celda está a la sombra si hay un bloque encima.
    /// </summary>
    internal class RenderMundo : IDisposable
    {
        public const int T = 16;
        private const float SombraLuz = 0.62f;

        private readonly Mundo mundo;
        private readonly int cx, cy, cz;
        private readonly uint[] listasOpacas, listasAgua;
        private readonly bool[] sucios;
        private readonly int[] alturas;
        private readonly Malla mallaOpaca = new Malla(), mallaAgua = new Malla();
        private int pendientes;

        public int ChunksDibujados { get; private set; }
        public int TotalChunks { get { return listasOpacas.Length; } }

        /// <summary>Altura del plano exterior (agua o pasto) que rodea el mapa.</summary>
        public float AlturaBorde
        {
            get { return mundo.ConLagos ? mundo.NivelAgua + 0.9f : mundo.NivelAgua + 1f; }
        }

        public RenderMundo(Mundo mundo)
        {
            this.mundo = mundo;
            cx = (mundo.Ancho + T - 1) / T;
            cy = (mundo.Alto + T - 1) / T;
            cz = (mundo.Largo + T - 1) / T;
            listasOpacas = new uint[cx * cy * cz];
            listasAgua = new uint[cx * cy * cz];
            sucios = new bool[cx * cy * cz];
            alturas = new int[mundo.Ancho * mundo.Largo];
            for (int x = 0; x < mundo.Ancho; x++)
            {
                for (int z = 0; z < mundo.Largo; z++) alturas[z * mundo.Ancho + x] = CalcularAltura(x, z);
            }
            for (int i = 0; i < sucios.Length; i++) sucios[i] = true;
            pendientes = sucios.Length;
        }

        private int CalcularAltura(int x, int z)
        {
            for (int y = mundo.Alto - 1; y >= 0; y--)
            {
                if (mundo.ObtenerBloque(x, y, z) != 0) return y;
            }
            return -1;
        }

        public bool EstaIluminado(int x, int y, int z)
        {
            if (x < 0 || z < 0 || x >= mundo.Ancho || z >= mundo.Largo || y >= mundo.Alto) return true;
            return y > alturas[z * mundo.Ancho + x];
        }

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
            return mundo.Celdas[(y * mundo.Largo + z) * mundo.Ancho + x];
        }

        private static bool CaraVisible(ModoDibujo propio, int vecino)
        {
            if (vecino < 0) return false;
            if (vecino == 0) return true;
            ModoDibujo otro = Atlas.Obtener(vecino).Modo;
            if (propio == ModoDibujo.Liquido) return otro != ModoDibujo.Opaco && otro != ModoDibujo.Liquido;
            return otro != ModoDibujo.Opaco;
        }

        private byte Luz(float cara, int nx, int ny, int nz)
        {
            float l = cara * (EstaIluminado(nx, ny, nz) ? 1f : SombraLuz);
            return (byte)(l * 255);
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
                        int id = celdas[(y * mundo.Largo + z) * mundo.Ancho + x];
                        if (id == 0) continue;
                        Apariencia a = Atlas.Obtener(id);
                        if (a.Modo == ModoDibujo.Aire) continue;
                        bool liquido = a.Modo == ModoDibujo.Liquido;
                        Malla m = liquido ? mallaAgua : mallaOpaca;
                        float alto = 1f;
                        if (liquido && Vecino(x, y + 1, z) != id) alto = 0.9f;
                        AgregarCubo(m, a, x, y, z, alto);
                    }
                }
            }

            listasOpacas[indice] = mallaOpaca.Compilar(listasOpacas[indice]);
            listasAgua[indice] = mallaAgua.Compilar(listasAgua[indice]);
        }

        private void AgregarCubo(Malla m, Apariencia a, int x, int y, int z, float alto)
        {
            float u0, v0, u1, v1;
            byte alfa = 255;
            float xa = x, xb = x + 1, ya = y, yb = y + alto, za = z, zb = z + 1;

            if (CaraVisible(a.Modo, Vecino(x, y + 1, z)) || alto < 1f)
            {
                Atlas.Uv(a.Arriba, out u0, out v0, out u1, out v1);
                byte l = Luz(1f, x, y + 1, z);
                m.Agregar(xa, yb, za, u0, v0, l, alfa);
                m.Agregar(xa, yb, zb, u0, v1, l, alfa);
                m.Agregar(xb, yb, zb, u1, v1, l, alfa);
                m.Agregar(xb, yb, za, u1, v0, l, alfa);
            }
            if (CaraVisible(a.Modo, Vecino(x, y - 1, z)))
            {
                Atlas.Uv(a.Abajo, out u0, out v0, out u1, out v1);
                byte l = Luz(0.5f, x, y - 1, z);
                m.Agregar(xa, ya, za, u0, v0, l, alfa);
                m.Agregar(xb, ya, za, u1, v0, l, alfa);
                m.Agregar(xb, ya, zb, u1, v1, l, alfa);
                m.Agregar(xa, ya, zb, u0, v1, l, alfa);
            }

            Atlas.Uv(a.Lado, out u0, out v0, out u1, out v1);
            float vt = v1 - (v1 - v0) * alto;
            if (CaraVisible(a.Modo, Vecino(x, y, z - 1)))
            {
                byte l = Luz(0.8f, x, y, z - 1);
                m.Agregar(xb, ya, za, u0, v1, l, alfa);
                m.Agregar(xa, ya, za, u1, v1, l, alfa);
                m.Agregar(xa, yb, za, u1, vt, l, alfa);
                m.Agregar(xb, yb, za, u0, vt, l, alfa);
            }
            if (CaraVisible(a.Modo, Vecino(x, y, z + 1)))
            {
                byte l = Luz(0.8f, x, y, z + 1);
                m.Agregar(xa, ya, zb, u0, v1, l, alfa);
                m.Agregar(xb, ya, zb, u1, v1, l, alfa);
                m.Agregar(xb, yb, zb, u1, vt, l, alfa);
                m.Agregar(xa, yb, zb, u0, vt, l, alfa);
            }
            if (CaraVisible(a.Modo, Vecino(x - 1, y, z)))
            {
                byte l = Luz(0.6f, x - 1, y, z);
                m.Agregar(xa, ya, za, u0, v1, l, alfa);
                m.Agregar(xa, ya, zb, u1, v1, l, alfa);
                m.Agregar(xa, yb, zb, u1, vt, l, alfa);
                m.Agregar(xa, yb, za, u0, vt, l, alfa);
            }
            if (CaraVisible(a.Modo, Vecino(x + 1, y, z)))
            {
                byte l = Luz(0.6f, x + 1, y, z);
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

            if (!translucido)
            {
                // Paredes interiores en el borde del mapa (se ven al cavar junto al límite o bajo el agua).
                GL.glBindTexture(GL.TEXTURE_2D, Atlas.TexturaPiedraBorde);
                GL.glBegin(GL.QUADS);
                GL.glColor3f(0.6f, 0.6f, 0.6f);
                GL.glTexCoord2f(0, h); GL.glVertex3f(0, 0, l);
                GL.glTexCoord2f(l, h); GL.glVertex3f(0, 0, 0);
                GL.glTexCoord2f(l, 0); GL.glVertex3f(0, h, 0);
                GL.glTexCoord2f(0, 0); GL.glVertex3f(0, h, l);

                GL.glTexCoord2f(0, h); GL.glVertex3f(w, 0, 0);
                GL.glTexCoord2f(l, h); GL.glVertex3f(w, 0, l);
                GL.glTexCoord2f(l, 0); GL.glVertex3f(w, h, l);
                GL.glTexCoord2f(0, 0); GL.glVertex3f(w, h, 0);

                GL.glColor3f(0.8f, 0.8f, 0.8f);
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
                    PlanoExterior(-d, -d, w + d, l + d, mundo.NivelAgua - 3f, 0.62f, 1f);
                }
                else
                {
                    GL.glBindTexture(GL.TEXTURE_2D, Atlas.TexturaPastoBorde);
                    PlanoExterior(-d, -d, w + d, l + d, h, 1f, 1f);
                }
            }
            else if (mundo.ConLagos)
            {
                GL.glBindTexture(GL.TEXTURE_2D, Atlas.TexturaAguaBorde);
                PlanoExterior(-d, -d, w + d, l + d, h, 1f, 0.72f);
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
