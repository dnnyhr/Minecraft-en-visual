using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using BlockForge.Models;

namespace BlockForge.Motor
{
    /// <summary>Cómo se dibuja un tipo de bloque.</summary>
    internal enum ModoDibujo
    {
        Aire,
        Opaco,
        /// <summary>Con píxeles transparentes (hojas): se dibuja con prueba alfa.</summary>
        Recortado,
        /// <summary>Translúcido y no sólido (agua): se dibuja en una segunda pasada.</summary>
        Liquido,
        /// <summary>Líquido opaco y luminoso (lava): forma de líquido pero en la pasada opaca.</summary>
        Lava,
        /// <summary>Dos planos cruzados (flores, hierba, brotes, antorchas).</summary>
        Planta,
        /// <summary>Recortado que no dibuja las caras entre bloques iguales (vidrio).</summary>
        Vidrio
    }

    internal class Apariencia
    {
        public int Arriba, Lado, Abajo;
        public ModoDibujo Modo;
        public bool Solido;
        /// <summary>Luz que emite (0 a 15).</summary>
        public int Luz;
        /// <summary>Se muestra como un dibujo plano (plantas, comida y materiales) en lugar de un cubo.</summary>
        public bool Plano;
        public int Icono;
        /// <summary>Deja pasar la luz del sol (aire, vidrio, plantas, antorchas).</summary>
        public bool DejaPasarSol;
    }

    /// <summary>
    /// Atlas de texturas de 16×16 píxeles por bloque, generado por código (pixel art procedural),
    /// así el proyecto no depende de archivos de texturas externos.
    /// </summary>
    internal static partial class Atlas
    {
        public const int Tile = 16, TilesPorFila = 16, Tamano = Tile * TilesPorFila;

        private const int PastoArriba = 0, PastoLado = 1, Tierra = 2, Piedra = 3, Madera = 4, Agua = 5, Arena = 6,
                          TroncoLado = 7, TroncoArriba = 8, Hojas = 9, Grava = 10, PrimerTileLibre = 80;

        // Tiles nuevos (bloques de construcción, plantas, comida e íconos del HUD).
        public const int Lava = 11, MineralCarbon = 12, Arcilla = 13, MaderaOscura = 14, Ladrillo = 15, LadrillosPiedra = 16,
                         PiedraLisa = 17, Terracota = 18, Vidrio = 19, LanaBlanca = 20, LanaRoja = 21, LanaAmarilla = 22,
                         LanaAzul = 23, LanaNegra = 24, EstanteriaLado = 25, Lampara = 26, Antorcha = 27, Rosa = 28,
                         DienteLeon = 29, Aciano = 30, HierbaAlta = 31, Brote = 32, Carbon = 33, Manzana = 34,
                         ChuletaCruda = 35, ChuletaCocida = 36, FileteCrudo = 37, FileteCocido = 38, PolloCrudo = 39,
                         PolloCocido = 40, CarnePodrida = 41, CorazonLleno = 42, CorazonMedio = 43, CorazonVacio = 44,
                         MusloLleno = 45, MusloMedio = 46, MusloVacio = 47, Burbuja = 48, Sol = 49, Luna = 50,
                         Roca = 51, MineralHierro = 52, MineralDiamante = 53, BloqueHierro = 54, BloqueDiamante = 55,
                         LanaNaranja = 56, LanaVerde = 57, LanaCeleste = 58, LanaMorada = 59, LanaRosa = 60, LanaGris = 61,
                         LingoteHierro = 62, Diamante = 63, EspadaMadera = 64, EspadaPiedra = 65, EspadaHierro = 66,
                         EspadaDiamante = 67, PiedraMusgosa = 68, Obsidiana = 69;

        public static readonly Apariencia[] PorId = new Apariencia[256];
        public static uint Textura { get; private set; }
        public static uint TexturaNubes { get; private set; }
        public static uint TexturaAguaBorde { get; private set; }
        public static uint TexturaPastoBorde { get; private set; }
        public static uint TexturaPiedraBorde { get; private set; }
        public static uint TexturaArenaBorde { get; private set; }

        private static int[] pixeles;

        /// <summary>Genera los píxeles del atlas y la apariencia de cada tipo del catálogo (no usa OpenGL).</summary>
        public static void Preparar(IList<TipoBloque> tipos)
        {
            AsegurarPixeles(true);
            ConfigurarTipos(tipos);
        }

        private static void AsegurarPixeles(bool regenerar)
        {
            if (pixeles != null && !regenerar) return;
            pixeles = new int[Tamano * Tamano];
            DibujarPastoArriba(PastoArriba);
            DibujarPastoLado(PastoLado);
            DibujarTierra(Tierra);
            DibujarPiedra(Piedra);
            DibujarMadera(Madera);
            DibujarAgua(Agua);
            DibujarArena(Arena);
            DibujarTroncoLado(TroncoLado);
            DibujarTroncoArriba(TroncoArriba);
            DibujarHojas(Hojas);
            DibujarGrava(Grava);
            DibujarNuevos();
            ClassicActivas = TexturasClassic.Aplicar(pixeles, Tamano, Tile);
        }

        /// <summary>Se están usando las texturas de Minecraft Classic (terrain.png).</summary>
        public static bool ClassicActivas { get; private set; }

        /// <summary>Vuelve a generar el atlas (por ejemplo, al cambiar de texturas) y lo sube de nuevo. Requiere contexto GL.</summary>
        public static void Recargar(IList<TipoBloque> tipos)
        {
            Liberar();
            Preparar(tipos);
            SubirTexturas();
        }

        private static void ConfigurarTipos(IList<TipoBloque> tipos)
        {
            for (int i = 0; i < PorId.Length; i++) PorId[i] = null;
            PorId[0] = new Apariencia { Modo = ModoDibujo.Aire, DejaPasarSol = true };
            int libre = PrimerTileLibre;
            foreach (TipoBloque tipo in tipos)
            {
                if (tipo.IdTipoBloque <= 0 || tipo.IdTipoBloque > 255) continue;
                Apariencia a = new Apariencia { Modo = ModoDibujo.Opaco, Solido = tipo.EsSolido, Luz = tipo.Luz };
                int unico = TileUnico(tipo.Nombre);
                if (unico >= 0) a.Arriba = a.Lado = a.Abajo = unico;
                switch (tipo.Nombre.ToLowerInvariant())
                {
                    case "pasto": a.Arriba = PastoArriba; a.Lado = PastoLado; a.Abajo = Tierra; break;
                    case "agua": a.Modo = ModoDibujo.Liquido; break;
                    case "lava": a.Modo = ModoDibujo.Lava; break;
                    case "tronco": a.Arriba = a.Abajo = TroncoArriba; a.Lado = TroncoLado; break;
                    case "hojas": a.Modo = ModoDibujo.Recortado; break;
                    case "vidrio": a.Modo = ModoDibujo.Vidrio; break;
                    case "estantería": a.Arriba = a.Abajo = Madera; a.Lado = EstanteriaLado; break;
                    default:
                        if (unico < 0)
                        {
                            // Tipos agregados en la base de datos sin textura propia: se usa su color.
                            int tile = libre < TilesPorFila * TilesPorFila ? libre++ : Piedra;
                            if (tile != Piedra) DibujarLiso(tile, tipo.Color, tipo.IdTipoBloque);
                            a.Arriba = a.Lado = a.Abajo = tile;
                            if (tipo.Categoria == TipoBloque.Liquido) a.Modo = ModoDibujo.Liquido;
                        }
                        break;
                }
                if (tipo.Categoria == TipoBloque.Planta) a.Modo = ModoDibujo.Planta;
                a.Plano = tipo.Categoria == TipoBloque.Planta || tipo.Categoria == TipoBloque.Comida || tipo.Categoria == TipoBloque.Material ||
                          tipo.Categoria == TipoBloque.Herramienta;
                a.Icono = a.Lado;
                a.DejaPasarSol = a.Modo == ModoDibujo.Planta || a.Modo == ModoDibujo.Vidrio;
                PorId[tipo.IdTipoBloque] = a;
            }
        }

        /// <summary>Sube las texturas a la tarjeta de video. Requiere un contexto GL activo y haber llamado a Preparar.</summary>
        public static void SubirTexturas()
        {
            Textura = Subir(pixeles, Tamano, Tamano, false);
            TexturaAguaBorde = Subir(CopiarTile(Agua, 255), Tile, Tile, true);
            TexturaPastoBorde = Subir(CopiarTile(PastoArriba, -1), Tile, Tile, true);
            TexturaPiedraBorde = Subir(CopiarTile(Piedra, -1), Tile, Tile, true);
            TexturaArenaBorde = Subir(CopiarTile(Arena, -1), Tile, Tile, true);
            TexturaNubes = Subir(GenerarNubes(64), 64, 64, true);
        }

        public static Apariencia Obtener(int id)
        {
            return PorId[id] ?? PorId[0];
        }

        // ------------------------------------------------------------------ texturas para la interfaz de Windows Forms
        /// <summary>Tile del atlas como Bitmap de 16×16 (para íconos de los formularios).</summary>
        public static Bitmap BitmapTile(int tile)
        {
            AsegurarPixeles(false);
            Bitmap bmp = new Bitmap(Tile, Tile, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            int ox = (tile % TilesPorFila) * Tile, oy = (tile / TilesPorFila) * Tile;
            for (int y = 0; y < Tile; y++)
            {
                for (int x = 0; x < Tile; x++) bmp.SetPixel(x, y, Color.FromArgb(pixeles[(oy + y) * Tamano + ox + x]));
            }
            return bmp;
        }

        /// <summary>Tiles de arriba y de los lados de un bloque según su nombre (sin necesitar el catálogo).</summary>
        public static void TilesPorNombre(string nombre, out int arriba, out int lado)
        {
            int unico = TileUnico(nombre);
            arriba = lado = unico >= 0 ? unico : Piedra;
            switch ((nombre ?? "").ToLowerInvariant())
            {
                case "pasto": arriba = PastoArriba; lado = PastoLado; break;
                case "tronco": arriba = TroncoArriba; lado = TroncoLado; break;
                case "estantería": arriba = Madera; lado = EstanteriaLado; break;
            }
        }

        /// <summary>Tile de un tipo que usa la misma textura en todas sus caras (o -1 si no tiene una propia).</summary>
        private static int TileUnico(string nombre)
        {
            switch ((nombre ?? "").ToLowerInvariant())
            {
                case "pasto": return PastoArriba;
                case "tierra": return Tierra;
                case "piedra": return Piedra;
                case "madera": return Madera;
                case "agua": return Agua;
                case "arena": return Arena;
                case "tronco": return TroncoLado;
                case "hojas": return Hojas;
                case "grava": return Grava;
                case "lava": return Lava;
                case "mineral de carbón": return MineralCarbon;
                case "arcilla": return Arcilla;
                case "madera oscura": return MaderaOscura;
                case "ladrillo": return Ladrillo;
                case "ladrillos de piedra": return LadrillosPiedra;
                case "piedra lisa": return PiedraLisa;
                case "terracota": return Terracota;
                case "vidrio": return Vidrio;
                case "lana blanca": return LanaBlanca;
                case "lana roja": return LanaRoja;
                case "lana amarilla": return LanaAmarilla;
                case "lana azul": return LanaAzul;
                case "lana negra": return LanaNegra;
                case "estantería": return EstanteriaLado;
                case "lámpara": return Lampara;
                case "antorcha": return Antorcha;
                case "rosa": return Rosa;
                case "diente de león": return DienteLeon;
                case "aciano": return Aciano;
                case "hierba alta": return HierbaAlta;
                case "brote": return Brote;
                case "carbón": return Carbon;
                case "manzana": return Manzana;
                case "chuleta cruda": return ChuletaCruda;
                case "chuleta cocida": return ChuletaCocida;
                case "filete crudo": return FileteCrudo;
                case "filete cocido": return FileteCocido;
                case "pollo crudo": return PolloCrudo;
                case "pollo cocido": return PolloCocido;
                case "carne podrida": return CarnePodrida;
                case "roca": return Roca;
                case "piedra musgosa": return PiedraMusgosa;
                case "obsidiana": return Obsidiana;
                case "mineral de hierro": return MineralHierro;
                case "mineral de diamante": return MineralDiamante;
                case "bloque de hierro": return BloqueHierro;
                case "bloque de diamante": return BloqueDiamante;
                case "lana naranja": return LanaNaranja;
                case "lana verde": return LanaVerde;
                case "lana celeste": return LanaCeleste;
                case "lana morada": return LanaMorada;
                case "lana rosa": return LanaRosa;
                case "lana gris": return LanaGris;
                case "lingote de hierro": return LingoteHierro;
                case "diamante": return Diamante;
                case "espada de madera": return EspadaMadera;
                case "espada de piedra": return EspadaPiedra;
                case "espada de hierro": return EspadaHierro;
                case "espada de diamante": return EspadaDiamante;
                default: return -1;
            }
        }

        public static void Uv(int tile, out float u0, out float v0, out float u1, out float v1)
        {
            const float paso = 1f / TilesPorFila, margen = 1f / (Tamano * 32f);
            u0 = (tile % TilesPorFila) * paso + margen;
            v0 = (tile / TilesPorFila) * paso + margen;
            u1 = u0 + paso - 2 * margen;
            v1 = v0 + paso - 2 * margen;
        }

        public static void Liberar()
        {
            GL.glDeleteTextures(6, new uint[] { Textura, TexturaNubes, TexturaAguaBorde, TexturaPastoBorde, TexturaPiedraBorde, TexturaArenaBorde });
        }

        // ------------------------------------------------------------------ subida a OpenGL
        private static uint Subir(int[] datos, int ancho, int alto, bool repetir)
        {
            uint[] id = new uint[1];
            GL.glGenTextures(1, id);
            GL.glBindTexture(GL.TEXTURE_2D, id[0]);
            GL.glTexParameteri(GL.TEXTURE_2D, GL.TEXTURE_MIN_FILTER, GL.NEAREST);
            GL.glTexParameteri(GL.TEXTURE_2D, GL.TEXTURE_MAG_FILTER, GL.NEAREST);
            GL.glTexParameteri(GL.TEXTURE_2D, GL.TEXTURE_WRAP_S, repetir ? GL.REPEAT : GL.CLAMP);
            GL.glTexParameteri(GL.TEXTURE_2D, GL.TEXTURE_WRAP_T, repetir ? GL.REPEAT : GL.CLAMP);
            GCHandle h = GCHandle.Alloc(datos, GCHandleType.Pinned);
            try
            {
                GL.glTexImage2D(GL.TEXTURE_2D, 0, GL.RGBA, ancho, alto, 0, GL.BGRA_EXT, GL.UNSIGNED_BYTE, h.AddrOfPinnedObject());
            }
            finally
            {
                h.Free();
            }
            return id[0];
        }

        private static int[] CopiarTile(int tile, int alfa)
        {
            int[] r = new int[Tile * Tile];
            int ox = (tile % TilesPorFila) * Tile, oy = (tile / TilesPorFila) * Tile;
            for (int y = 0; y < Tile; y++)
            {
                for (int x = 0; x < Tile; x++)
                {
                    int p = pixeles[(oy + y) * Tamano + ox + x];
                    r[y * Tile + x] = alfa >= 0 ? (p & 0x00FFFFFF) | (alfa << 24) : p;
                }
            }
            return r;
        }

        /// <summary>Color promedio de un tile (para las partículas y el ícono de respaldo).</summary>
        public static Color ColorPromedio(int tile)
        {
            long r = 0, g = 0, b = 0, n = 0;
            int ox = (tile % TilesPorFila) * Tile, oy = (tile / TilesPorFila) * Tile;
            for (int y = 0; y < Tile; y++)
            {
                for (int x = 0; x < Tile; x++)
                {
                    int p = pixeles[(oy + y) * Tamano + ox + x];
                    if (((p >> 24) & 0xFF) < 128) continue;
                    r += (p >> 16) & 0xFF; g += (p >> 8) & 0xFF; b += p & 0xFF; n++;
                }
            }
            if (n == 0) return Color.Gray;
            return Color.FromArgb((int)(r / n), (int)(g / n), (int)(b / n));
        }

        // ------------------------------------------------------------------ pixel art procedural
        private static void Poner(int tile, int x, int y, int r, int g, int b, int a = 255)
        {
            int ox = (tile % TilesPorFila) * Tile, oy = (tile / TilesPorFila) * Tile;
            r = Math.Max(0, Math.Min(255, r));
            g = Math.Max(0, Math.Min(255, g));
            b = Math.Max(0, Math.Min(255, b));
            pixeles[(oy + y) * Tamano + ox + x] = (a << 24) | (r << 16) | (g << 8) | b;
        }

        private static void PonerVariado(int tile, int x, int y, Color c, int variacion, Random azar, int a = 255)
        {
            int d = azar.Next(-variacion, variacion + 1);
            Poner(tile, x, y, c.R + d, c.G + d, c.B + d, a);
        }

        private static void DibujarPastoArriba(int tile)
        {
            Random azar = new Random(11);
            Color baseColor = Color.FromArgb(104, 168, 64);
            for (int y = 0; y < Tile; y++)
            {
                for (int x = 0; x < Tile; x++)
                {
                    int d = azar.Next(-16, 17);
                    if (azar.NextDouble() < 0.12) d -= 22;
                    Poner(tile, x, y, baseColor.R + d, baseColor.G + d, baseColor.B + d / 2);
                }
            }
        }

        private static void DibujarTierraEn(int tile, Random azar)
        {
            Color baseColor = Color.FromArgb(134, 96, 67);
            for (int y = 0; y < Tile; y++)
            {
                for (int x = 0; x < Tile; x++)
                {
                    int d = azar.Next(-12, 13);
                    double p = azar.NextDouble();
                    if (p < 0.10) d -= 26;
                    else if (p < 0.15) d += 18;
                    Poner(tile, x, y, baseColor.R + d, baseColor.G + d, baseColor.B + d);
                }
            }
        }

        private static void DibujarTierra(int tile)
        {
            DibujarTierraEn(tile, new Random(22));
        }

        private static void DibujarPastoLado(int tile)
        {
            Random azar = new Random(33);
            DibujarTierraEn(tile, azar);
            Color pasto = Color.FromArgb(104, 168, 64);
            for (int x = 0; x < Tile; x++)
            {
                int alto = 3 + (azar.NextDouble() < 0.5 ? 1 : 0) + (azar.NextDouble() < 0.2 ? 1 : 0);
                for (int y = 0; y < alto; y++)
                {
                    int d = azar.Next(-14, 15);
                    Poner(tile, x, y, pasto.R + d, pasto.G + d, pasto.B + d / 2);
                }
            }
        }

        private static void DibujarPiedra(int tile)
        {
            Random azar = new Random(44);
            for (int y = 0; y < Tile; y++)
            {
                for (int x = 0; x < Tile; x++)
                {
                    double manchas = Math.Sin(x * 1.3 + y * 0.4) * Math.Cos(y * 1.1 - x * 0.7);
                    int v = 122 + (int)(manchas * 14) + azar.Next(-9, 10);
                    if (azar.NextDouble() < 0.07) v -= 28;
                    Poner(tile, x, y, v, v, v);
                }
            }
        }

        private static void DibujarMadera(int tile)
        {
            Random azar = new Random(55);
            Color baseColor = Color.FromArgb(166, 132, 80);
            for (int fila = 0; fila < 4; fila++)
            {
                int tono = azar.Next(-10, 11);
                int junta = (fila % 2 == 0) ? 3 : 11;
                for (int y = fila * 4; y < fila * 4 + 4; y++)
                {
                    for (int x = 0; x < Tile; x++)
                    {
                        int d = tono + azar.Next(-6, 7);
                        if (y % 4 == 3) d -= 42;
                        else if (x == junta) d -= 30;
                        else if (azar.NextDouble() < 0.08) d -= 14;
                        Poner(tile, x, y, baseColor.R + d, baseColor.G + d, baseColor.B + d);
                    }
                }
            }
        }

        private static void DibujarAgua(int tile)
        {
            Random azar = new Random(66);
            Color baseColor = Color.FromArgb(52, 104, 220);
            for (int y = 0; y < Tile; y++)
            {
                for (int x = 0; x < Tile; x++)
                {
                    double ola = Math.Sin((x + y * 0.5) * 0.8) * 6;
                    PonerVariado(tile, x, y, Color.FromArgb(baseColor.R, baseColor.G + (int)ola, baseColor.B), 8, azar, 175);
                }
            }
        }

        private static void DibujarArena(int tile)
        {
            Random azar = new Random(77);
            Color baseColor = Color.FromArgb(219, 207, 160);
            for (int y = 0; y < Tile; y++)
            {
                for (int x = 0; x < Tile; x++)
                {
                    int d = azar.Next(-9, 10);
                    if (azar.NextDouble() < 0.08) d -= 20;
                    Poner(tile, x, y, baseColor.R + d, baseColor.G + d, baseColor.B + d);
                }
            }
        }

        private static void DibujarTroncoLado(int tile)
        {
            Random azar = new Random(88);
            Color baseColor = Color.FromArgb(104, 82, 50);
            int[] columnas = new int[Tile];
            for (int x = 0; x < Tile; x++) columnas[x] = azar.Next(-14, 15) - (x % 4 == 0 ? 18 : 0);
            for (int y = 0; y < Tile; y++)
            {
                for (int x = 0; x < Tile; x++)
                {
                    int d = columnas[x] + azar.Next(-5, 6);
                    Poner(tile, x, y, baseColor.R + d, baseColor.G + d, baseColor.B + d);
                }
            }
        }

        private static void DibujarTroncoArriba(int tile)
        {
            Random azar = new Random(99);
            for (int y = 0; y < Tile; y++)
            {
                for (int x = 0; x < Tile; x++)
                {
                    double d = Math.Max(Math.Abs(x - 7.5), Math.Abs(y - 7.5));
                    int v = azar.Next(-6, 7);
                    if (d > 6.5) Poner(tile, x, y, 104 + v, 82 + v, 50 + v);
                    else if (((int)d) % 2 == 0) Poner(tile, x, y, 182 + v, 146 + v, 92 + v);
                    else Poner(tile, x, y, 156 + v, 122 + v, 74 + v);
                }
            }
        }

        private static void DibujarHojas(int tile)
        {
            Random azar = new Random(111);
            Color baseColor = Color.FromArgb(56, 136, 40);
            for (int y = 0; y < Tile; y++)
            {
                for (int x = 0; x < Tile; x++)
                {
                    if (azar.NextDouble() < 0.2)
                    {
                        Poner(tile, x, y, 0, 0, 0, 0);
                        continue;
                    }
                    int d = azar.Next(-22, 23);
                    Poner(tile, x, y, baseColor.R + d / 2, baseColor.G + d, baseColor.B + d / 2);
                }
            }
        }

        private static void DibujarGrava(int tile)
        {
            Random azar = new Random(122);
            int[] tonos = { 96, 118, 134, 152 };
            for (int y = 0; y < Tile; y += 2)
            {
                for (int x = 0; x < Tile; x += 2)
                {
                    int t = tonos[azar.Next(tonos.Length)];
                    for (int dy = 0; dy < 2; dy++)
                    {
                        for (int dx = 0; dx < 2; dx++)
                        {
                            int v = t + azar.Next(-8, 9);
                            Poner(tile, x + dx, y + dy, v + 4, v, v - 4);
                        }
                    }
                }
            }
        }

        private static void DibujarLiso(int tile, Color color, int semilla)
        {
            Random azar = new Random(semilla * 7919);
            for (int y = 0; y < Tile; y++)
            {
                for (int x = 0; x < Tile; x++)
                {
                    int d = azar.Next(-12, 13);
                    if (x == 0 || y == 0) d += 14;
                    if (x == Tile - 1 || y == Tile - 1) d -= 18;
                    Poner(tile, x, y, color.R + d, color.G + d, color.B + d);
                }
            }
        }

        /// <summary>Nubes en bloques, como las de los juegos clásicos: cada texel es una celda de nube.</summary>
        private static int[] GenerarNubes(int tamano)
        {
            Random azar = new Random(4242);
            const int celda = 8;
            int n = tamano / celda;
            double[,] reja = new double[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++) reja[i, j] = azar.NextDouble();
            }
            int[] r = new int[tamano * tamano];
            for (int y = 0; y < tamano; y++)
            {
                for (int x = 0; x < tamano; x++)
                {
                    double fx = (double)x / celda, fy = (double)y / celda;
                    int x0 = (int)fx, y0 = (int)fy;
                    double tx = fx - x0, ty = fy - y0;
                    double a = reja[x0 % n, y0 % n], b = reja[(x0 + 1) % n, y0 % n];
                    double c = reja[x0 % n, (y0 + 1) % n], d = reja[(x0 + 1) % n, (y0 + 1) % n];
                    double valor = (a + (b - a) * tx) * (1 - ty) + (c + (d - c) * tx) * ty;
                    valor += (azar.NextDouble() - 0.5) * 0.08;
                    r[y * tamano + x] = valor > 0.6 ? unchecked((int)0xE6FFFFFF) : 0;
                }
            }
            return r;
        }
    }
}
