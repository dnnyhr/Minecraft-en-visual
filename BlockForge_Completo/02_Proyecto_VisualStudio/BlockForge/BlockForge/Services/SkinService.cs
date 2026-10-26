using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BlockForge.Data;
using BlockForge.Models;

namespace BlockForge.Services
{
    /// <summary>
    /// Skins con el formato de Minecraft: se descargan de la cuenta de Minecraft de un jugador
    /// (API pública de Mojang) o se cargan desde un archivo PNG, y se guardan en el usuario.
    /// </summary>
    public class SkinService
    {
        public const int TamanoMaximo = 256 * 1024;
        private static readonly Regex NombreMinecraft = new Regex("^[A-Za-z0-9_]{3,16}$");
        private static readonly HttpClient http = CrearCliente();

        private readonly UsuarioRepository repositorio = new UsuarioRepository();

        private static HttpClient CrearCliente()
        {
            HttpClient c = new HttpClient { Timeout = TimeSpan.FromSeconds(12) };
            c.DefaultRequestHeaders.UserAgent.ParseAdd("BlockForge/3.0");
            return c;
        }

        /// <summary>Guarda la skin del usuario actual (null la quita).</summary>
        public void Guardar(Skin skin)
        {
            if (Sesion.UsuarioActual == null) throw new ExcepcionValidacion("No hay una sesión iniciada.");
            if (skin != null) Validar(skin.Png);
            repositorio.GuardarSkin(Sesion.UsuarioActual.IdUsuario, skin);
            Sesion.UsuarioActual.Skin = skin;
        }

        /// <summary>Carga una skin desde un archivo PNG.</summary>
        public static Skin DesdeArchivo(string ruta, bool delgada)
        {
            FileInfo info = new FileInfo(ruta);
            if (info.Length > TamanoMaximo) throw new ExcepcionValidacion("El archivo es demasiado grande (máximo 256 KB).");
            byte[] png = File.ReadAllBytes(ruta);
            Validar(png);
            return new Skin { Png = png, Delgada = delgada, Origen = "archivo" };
        }

        /// <summary>Descarga la skin de una cuenta de Minecraft (Java Edition) por su nombre.</summary>
        public static async Task<Skin> DescargarDeMinecraft(string nombre)
        {
            nombre = (nombre ?? string.Empty).Trim();
            if (!NombreMinecraft.IsMatch(nombre))
            {
                throw new ExcepcionValidacion("El nombre de Minecraft tiene de 3 a 16 caracteres: letras, números y guion bajo.");
            }

            string id = await BuscarId(nombre);
            string json = await http.GetStringAsync("https://sessionserver.mojang.com/session/minecraft/profile/" + id);
            string url = null;
            bool delgada = false;
            using (JsonDocument perfil = JsonDocument.Parse(json))
            {
                foreach (JsonElement propiedad in perfil.RootElement.GetProperty("properties").EnumerateArray())
                {
                    if (propiedad.GetProperty("name").GetString() != "textures") continue;
                    string texturas = Encoding.UTF8.GetString(Convert.FromBase64String(propiedad.GetProperty("value").GetString()));
                    using (JsonDocument t = JsonDocument.Parse(texturas))
                    {
                        JsonElement skin;
                        if (t.RootElement.GetProperty("textures").TryGetProperty("SKIN", out skin))
                        {
                            url = skin.GetProperty("url").GetString();
                            JsonElement metadata, modelo;
                            delgada = skin.TryGetProperty("metadata", out metadata) && metadata.TryGetProperty("model", out modelo) &&
                                      modelo.GetString() == "slim";
                        }
                    }
                }
            }
            if (url == null)
            {
                throw new ExcepcionValidacion("La cuenta «" + nombre + "» usa la skin predeterminada de Minecraft.");
            }
            Uri uri = new Uri(url);
            if (!uri.Host.EndsWith("minecraft.net", StringComparison.OrdinalIgnoreCase))
            {
                throw new ExcepcionValidacion("La dirección de la skin no es de Minecraft.");
            }
            byte[] png = await http.GetByteArrayAsync(new UriBuilder(uri) { Scheme = "https", Port = -1 }.Uri);
            Validar(png);
            return new Skin { Png = png, Delgada = delgada, Origen = nombre };
        }

        private static async Task<string> BuscarId(string nombre)
        {
            string[] servicios =
            {
                "https://api.minecraftservices.com/minecraft/profile/lookup/name/",
                "https://api.mojang.com/users/profiles/minecraft/"
            };
            Exception ultimo = null;
            foreach (string servicio in servicios)
            {
                try
                {
                    HttpResponseMessage r = await http.GetAsync(servicio + nombre);
                    if (r.StatusCode == HttpStatusCode.NotFound || r.StatusCode == HttpStatusCode.NoContent)
                    {
                        throw new ExcepcionValidacion("No existe una cuenta de Minecraft llamada «" + nombre + "».");
                    }
                    r.EnsureSuccessStatusCode();
                    using (JsonDocument d = JsonDocument.Parse(await r.Content.ReadAsStringAsync()))
                    {
                        return d.RootElement.GetProperty("id").GetString();
                    }
                }
                catch (ExcepcionValidacion)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    ultimo = ex;
                }
            }
            throw new ExcepcionValidacion("No fue posible consultar los servidores de Minecraft. Revise la conexión a internet." +
                                          (ultimo != null ? "\n\nDetalle: " + ultimo.Message : ""));
        }

        /// <summary>Comprueba que sea una imagen PNG con las proporciones de una skin (64×64, 64×32 o múltiplos).</summary>
        public static void Validar(byte[] png)
        {
            if (png == null || png.Length == 0 || png.Length > TamanoMaximo) throw new ExcepcionValidacion("La skin no es válida o es demasiado grande.");
            try
            {
                using (MemoryStream m = new MemoryStream(png))
                using (Bitmap bmp = new Bitmap(m))
                {
                    bool proporcion = bmp.Height == bmp.Width || bmp.Height * 2 == bmp.Width;
                    if (bmp.Width < 64 || bmp.Width % 64 != 0 || bmp.Width > 512 || !proporcion)
                    {
                        throw new ExcepcionValidacion("La imagen debe medir 64×64 o 64×32 píxeles (el formato de las skins de Minecraft).");
                    }
                }
            }
            catch (ArgumentException)
            {
                throw new ExcepcionValidacion("El archivo no es una imagen válida.");
            }
        }

        public static Bitmap Decodificar(byte[] png)
        {
            using (MemoryStream m = new MemoryStream(png))
            using (Bitmap original = new Bitmap(m))
            {
                Bitmap copia = new Bitmap(original.Width, original.Height, PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(copia)) g.DrawImage(original, 0, 0, original.Width, original.Height);
                return copia;
            }
        }

        /// <summary>
        /// Las skins antiguas (64×32) suelen traer la capa del sombrero pintada de un color opaco;
        /// Minecraft la ignora si no tiene ningún píxel transparente, y aquí se hace lo mismo.
        /// </summary>
        public static bool SombreroVisible(Bitmap skin)
        {
            if (skin.Height != skin.Width / 2) return true;
            float k = skin.Width / 64f;
            for (int y = 0; y < (int)(16 * k); y++)
            {
                for (int x = (int)(32 * k); x < (int)(64 * k); x++)
                {
                    if (skin.GetPixel(x, y).A < 128) return true;
                }
            }
            return false;
        }

        /// <summary>Cara del personaje (cabeza y sombrero) para usarla como avatar.</summary>
        public static Bitmap Cara(byte[] png, int tamano)
        {
            Bitmap cara = new Bitmap(tamano, tamano, PixelFormat.Format32bppArgb);
            using (Bitmap skin = Decodificar(png))
            using (Graphics g = Graphics.FromImage(cara))
            {
                float k = skin.Width / 64f;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.DrawImage(skin, new Rectangle(0, 0, tamano, tamano), 8 * k, 8 * k, 8 * k, 8 * k, GraphicsUnit.Pixel);
                if (SombreroVisible(skin)) g.DrawImage(skin, new Rectangle(0, 0, tamano, tamano), 40 * k, 8 * k, 8 * k, 8 * k, GraphicsUnit.Pixel);
            }
            return cara;
        }
    }
}
