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

        private const string RecursoIncluido = "BlockForge.terrain.png";

        /// <summary>
        /// terrain.png que va dentro del ejecutable (solo en las compilaciones hechas en un equipo que tiene
        /// Recursos\terrain.png; ese archivo no se sube al repositorio).
        /// </summary>
        public static bool Incluidas
        {
            get { return typeof(TexturasClassic).Assembly.GetManifestResourceInfo(RecursoIncluido) != null; }
        }

        public static bool Disponibles
        {
            get { return File.Exists(Ruta) || Incluidas; }
        }

        /// <summary>El terrain.png elegido o descargado por el jugador; si no hay, el incluido en el ejecutable.</summary>
        private static Bitmap AbrirImagen(out string origen)
        {
            if (File.Exists(Ruta))
            {
                origen = Ruta;
                return new Bitmap(Ruta);
            }
            origen = "el ejecutable";
            using (Stream recurso = typeof(TexturasClassic).Assembly.GetManifestResourceStream(RecursoIncluido))
            using (Bitmap imagen = new Bitmap(recurso))
            {
                return new Bitmap(imagen); // copia independiente del flujo, que se cierra aquí
            }
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
            if (!Preferidas || !Disponibles)
            {
                Registrar(!Disponibles ? "No hay terrain.png en " + Ruta + " ni en el ejecutable: se usan las texturas propias." : "El jugador eligió las texturas propias.");
                return false;
            }
            string de = "?";
            try
            {
                using (Bitmap origen = AbrirImagen(out de))
                {
                    if (origen.Width < 16 || origen.Width % 16 != 0 || origen.Height < origen.Width)
                    {
                        Registrar("terrain.png tiene un tamaño inválido (" + origen.Width + "×" + origen.Height + ").");
                        return false;
                    }
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
                Registrar("Texturas de Minecraft Classic aplicadas (" + Mapa.Count + " tiles, desde " + de + ").");
                return true;
            }
            catch (Exception ex)
            {
                Registrar("No se pudo leer terrain.png: " + ex); // archivo dañado: se usan las texturas propias
                return false;
            }
        }

        /// <summary>Deja constancia de qué texturas se usaron (texturas.log, para diagnosticar).</summary>
        private static void Registrar(string texto)
        {
            try
            {
                Directory.CreateDirectory(Carpeta);
                string origen = Environment.ProcessPath + (System.Diagnostics.Debugger.IsAttached ? " (depurando en Visual Studio)" : "");
                File.AppendAllText(Path.Combine(Carpeta, "texturas.log"),
                                   DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + texto + "  [" + origen + "]" + Environment.NewLine);
            }
            catch (Exception)
            {
                // El registro es solo informativo.
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
