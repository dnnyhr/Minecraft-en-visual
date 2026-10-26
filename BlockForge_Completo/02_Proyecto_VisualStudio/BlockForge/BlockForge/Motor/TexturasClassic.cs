using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlockForge.Motor
{
    /// <summary>
    /// Texturas de Minecraft Classic (terrain.png de 16×16 tiles, el mismo formato que usa ClassiCube).
    /// No vienen con el proyecto porque son de Mojang: se descargan del servidor oficial de Mojang en cada equipo
    /// (como hace ClassiCube) o se elige un terrain.png propio. Los bloques que Classic no tiene (diamante,
    /// comida, antorchas...) siguen con el pixel art de BlockForge.
    /// </summary>
    internal static class TexturasClassic
    {
        /// <summary>Cliente oficial de Minecraft Classic 0.30 en los servidores de Mojang (unos 290 KB).</summary>
        public const string UrlCliente = "https://launcher.mojang.com/mc/game/c0.30_01c/client/54622801f5ef1bcc1549a842c5b04cb5d5583005/client.jar";

        // Tile de BlockForge → índice en terrain.png de Classic.
        private static readonly Dictionary<int, int> Mapa = new Dictionary<int, int>
        {
            { 0, 0 }, { 1, 3 }, { 2, 2 }, { 3, 1 }, { 4, 4 }, { 5, 14 }, { 6, 18 }, { 7, 20 }, { 8, 21 }, { 9, 22 }, { 10, 19 },
            { Atlas.Lava, 30 }, { Atlas.MineralCarbon, 34 }, { Atlas.Ladrillo, 7 }, { Atlas.PiedraLisa, 6 }, { Atlas.Vidrio, 49 },
            { Atlas.LanaBlanca, 79 }, { Atlas.LanaRoja, 64 }, { Atlas.LanaAmarilla, 66 }, { Atlas.LanaAzul, 72 }, { Atlas.LanaNegra, 77 },
            { Atlas.EstanteriaLado, 35 }, { Atlas.Rosa, 12 }, { Atlas.DienteLeon, 13 }, { Atlas.Brote, 15 },
            { Atlas.Roca, 16 }, { Atlas.PiedraMusgosa, 36 }, { Atlas.Obsidiana, 37 }, { Atlas.MineralHierro, 33 }, { Atlas.BloqueHierro, 39 },
            { Atlas.LanaNaranja, 65 }, { Atlas.LanaVerde, 68 }, { Atlas.LanaCeleste, 70 }, { Atlas.LanaMorada, 74 },
            { Atlas.LanaRosa, 76 }, { Atlas.LanaGris, 78 }
        };

        private static string Carpeta
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BlockForge"); }
        }

        public static string Ruta
        {
            get { return Path.Combine(Carpeta, "terrain.png"); }
        }

        private static string RutaPreferencia
        {
            get { return Path.Combine(Carpeta, "texturas.txt"); }
        }

        public static bool Disponibles
        {
            get { return File.Exists(Ruta); }
        }

        /// <summary>El jugador quiere las texturas de Classic (si están descargadas). Por defecto, sí.</summary>
        public static bool Preferidas
        {
            get
            {
                try { return !File.Exists(RutaPreferencia) || File.ReadAllText(RutaPreferencia).Trim() != "blockforge"; }
                catch (Exception) { return true; }
            }
            set
            {
                Directory.CreateDirectory(Carpeta);
                File.WriteAllText(RutaPreferencia, value ? "classic" : "blockforge");
            }
        }

        /// <summary>Copia los tiles de terrain.png sobre el atlas. Devuelve true si se aplicaron.</summary>
        public static bool Aplicar(int[] pixeles, int tamanoAtlas, int tile)
        {
            if (!Preferidas || !Disponibles) return false;
            try
            {
                using (Bitmap origen = new Bitmap(Ruta))
                {
                    if (origen.Width < 16 || origen.Width % 16 != 0 || origen.Height < origen.Width) return false;
                    int t = origen.Width / 16;
                    int porFila = tamanoAtlas / tile;
                    foreach (KeyValuePair<int, int> par in Mapa)
                    {
                        int ox = (par.Key % porFila) * tile, oy = (par.Key / porFila) * tile;
                        int sx = (par.Value % 16) * t, sy = (par.Value / 16) * t;
                        for (int y = 0; y < tile; y++)
                        {
                            for (int x = 0; x < tile; x++)
                            {
                                int argb = origen.GetPixel(sx + x * t / tile, sy + y * t / tile).ToArgb();
                                if (par.Key == 5) argb = (argb & 0x00FFFFFF) | (175 << 24); // agua translúcida
                                pixeles[(oy + y) * tamanoAtlas + ox + x] = argb;
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false; // archivo dañado: se usan las texturas propias
            }
        }

        /// <summary>Descarga el cliente oficial de Minecraft Classic de Mojang y guarda solo su terrain.png.</summary>
        public static async Task Descargar()
        {
            byte[] jar;
            using (HttpClient http = new HttpClient { Timeout = TimeSpan.FromMinutes(2) })
            {
                jar = await http.GetByteArrayAsync(UrlCliente);
            }
            using (ZipArchive zip = new ZipArchive(new MemoryStream(jar), ZipArchiveMode.Read))
            {
                ZipArchiveEntry entrada = zip.GetEntry("terrain.png");
                if (entrada == null) throw new InvalidDataException("El archivo descargado no contiene terrain.png.");
                using (Stream s = entrada.Open())
                using (MemoryStream m = new MemoryStream())
                {
                    s.CopyTo(m);
                    Guardar(m.ToArray());
                }
            }
        }

        /// <summary>Usa un terrain.png elegido por el jugador (cualquier paquete con el formato de Classic).</summary>
        public static void Importar(string archivo)
        {
            Guardar(File.ReadAllBytes(archivo));
        }

        private static void Guardar(byte[] png)
        {
            using (MemoryStream m = new MemoryStream(png))
            using (Image imagen = Image.FromStream(m))
            {
                if (imagen.Width < 16 || imagen.Width % 16 != 0 || imagen.Height < imagen.Width)
                {
                    throw new InvalidDataException("La imagen debe ser un terrain.png de 16×16 tiles (por ejemplo 256×256 píxeles).");
                }
            }
            Directory.CreateDirectory(Carpeta);
            File.WriteAllBytes(Ruta, png);
            Preferidas = true;
        }
    }
}
