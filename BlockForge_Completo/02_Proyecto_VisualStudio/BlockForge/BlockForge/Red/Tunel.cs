using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace BlockForge.Red
{
    /// <summary>
    /// Túnel rápido de Cloudflare ("quick tunnel"): da una dirección https://xxxx.trycloudflare.com que
    /// llega a la partida sin abrir puertos en el router ni crear cuentas. Usa el programa oficial
    /// cloudflared, que se descarga una sola vez (con permiso del usuario) en %LocalAppData%\BlockForge.
    /// </summary>
    public sealed class Tunel : IDisposable
    {
        public const string UrlDescarga = "https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-windows-amd64.exe";
        private static readonly Regex PatronUrl = new Regex(@"https://[a-z0-9\-]+\.trycloudflare\.com", RegexOptions.IgnoreCase);

        private Process proceso;

        /// <summary>Dirección pública del túnel (null hasta que Cloudflare la asigna).</summary>
        public string Url { get; private set; }

        public static string CarpetaDatos
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BlockForge"); }
        }

        /// <summary>Ruta de cloudflared: junto al juego, en la carpeta de datos, o null si no está.</summary>
        public static string RutaCloudflared()
        {
            string[] candidatos =
            {
                Path.Combine(AppContext.BaseDirectory, "cloudflared.exe"),
                Path.Combine(CarpetaDatos, "cloudflared.exe")
            };
            foreach (string c in candidatos)
            {
                if (File.Exists(c)) return c;
            }
            // También sirve si el usuario lo instaló (por ejemplo con winget).
            string path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            foreach (string carpeta in path.Split(';'))
            {
                try
                {
                    string c = Path.Combine(carpeta.Trim(), "cloudflared.exe");
                    if (carpeta.Trim().Length > 0 && File.Exists(c)) return c;
                }
                catch (ArgumentException)
                {
                    // Entrada del PATH con caracteres inválidos.
                }
            }
            return null;
        }

        /// <summary>Descarga cloudflared desde la página oficial de Cloudflare en GitHub (unos 60 MB).</summary>
        public static async Task<string> Descargar(IProgress<int> progreso, CancellationToken cancelar)
        {
            Directory.CreateDirectory(CarpetaDatos);
            string destino = Path.Combine(CarpetaDatos, "cloudflared.exe");
            string temporal = destino + ".descarga";
            using (HttpClient http = new HttpClient { Timeout = TimeSpan.FromMinutes(10) })
            {
                http.DefaultRequestHeaders.UserAgent.ParseAdd("BlockForge/4.0");
                using (HttpResponseMessage r = await http.GetAsync(UrlDescarga, HttpCompletionOption.ResponseHeadersRead, cancelar))
                {
                    r.EnsureSuccessStatusCode();
                    long total = r.Content.Headers.ContentLength ?? 0;
                    using (Stream origen = await r.Content.ReadAsStreamAsync())
                    using (FileStream archivo = File.Create(temporal))
                    {
                        byte[] buffer = new byte[81920];
                        long leidos = 0;
                        int n;
                        while ((n = await origen.ReadAsync(buffer, 0, buffer.Length, cancelar)) > 0)
                        {
                            await archivo.WriteAsync(buffer, 0, n, cancelar);
                            leidos += n;
                            if (progreso != null && total > 0) progreso.Report((int)(leidos * 100 / total));
                        }
                    }
                }
            }
            if (File.Exists(destino)) File.Delete(destino);
            File.Move(temporal, destino);
            return destino;
        }

        /// <summary>
        /// Arranca el túnel hacia la partida local y espera a que Cloudflare asigne la dirección.
        /// Lanza una excepción con un mensaje para el usuario si no lo consigue.
        /// </summary>
        public static async Task<Tunel> Abrir(string cloudflared, int puertoLocal, int esperaMs)
        {
            Tunel t = new Tunel();
            TaskCompletionSource<string> url = new TaskCompletionSource<string>();
            // El origen se declara como http: cloudflared reenvía las conexiones WebSocket tal cual y
            // la partida reconoce el saludo HTTP en su puerto normal.
            ProcessStartInfo info = new ProcessStartInfo
            {
                FileName = cloudflared,
                Arguments = "tunnel --no-autoupdate --url http://127.0.0.1:" + puertoLocal,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            t.proceso = new Process { StartInfo = info, EnableRaisingEvents = true };
            DataReceivedEventHandler leer = (s, e) =>
            {
                if (e.Data == null) return;
                Match m = PatronUrl.Match(e.Data);
                if (m.Success && !m.Value.StartsWith("https://api.", StringComparison.OrdinalIgnoreCase)) url.TrySetResult(m.Value);
            };
            t.proceso.ErrorDataReceived += leer;
            t.proceso.OutputDataReceived += leer;
            t.proceso.Exited += (s, e) => url.TrySetException(new IOException("cloudflared se cerró antes de abrir el túnel. Revisa tu conexión a internet."));
            try
            {
                t.proceso.Start();
            }
            catch (Exception ex)
            {
                throw new IOException("No se pudo ejecutar cloudflared: " + ex.Message, ex);
            }
            t.proceso.BeginErrorReadLine();
            t.proceso.BeginOutputReadLine();

            Task fin = await Task.WhenAny(url.Task, Task.Delay(esperaMs));
            if (fin != url.Task)
            {
                t.Dispose();
                throw new IOException("Cloudflare tardó demasiado en dar la dirección del túnel. Inténtalo de nuevo en un momento.");
            }
            try
            {
                t.Url = await url.Task;
            }
            catch
            {
                t.Dispose();
                throw;
            }
            return t;
        }

        public bool Activo
        {
            get
            {
                try { return proceso != null && !proceso.HasExited; } catch (Exception) { return false; }
            }
        }

        public void Dispose()
        {
            if (proceso == null) return;
            try
            {
                if (!proceso.HasExited) proceso.Kill();
            }
            catch (Exception)
            {
                // Ya terminó.
            }
            proceso.Dispose();
            proceso = null;
        }
    }
}
