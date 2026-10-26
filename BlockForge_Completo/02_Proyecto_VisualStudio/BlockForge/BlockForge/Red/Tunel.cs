using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace BlockForge.Red
{
    /// <summary>
    /// Túnel rápido de Cloudflare ("quick tunnel"): da una dirección https://xxxx.trycloudflare.com que
    /// llega a la partida sin abrir puertos en el router ni crear cuentas. Usa el programa oficial
    /// cloudflared, que se instala una sola vez con winget (el instalador de paquetes de Windows).
    /// </summary>
    public sealed class Tunel : IDisposable
    {
        private static readonly Regex PatronUrl = new Regex(@"https://[a-z0-9\-]+\.trycloudflare\.com", RegexOptions.IgnoreCase);

        private Process proceso;

        /// <summary>Dirección pública del túnel (null hasta que Cloudflare la asigna).</summary>
        public string Url { get; private set; }

        public static string CarpetaDatos
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BlockForge"); }
        }

        /// <summary>Comando para instalar cloudflared con el instalador de paquetes de Windows.</summary>
        public const string ComandoInstalar = "winget install --id Cloudflare.cloudflared -e";

        /// <summary>Ruta de cloudflared (instalado con winget, junto al juego o en el PATH), o null si no está.</summary>
        public static string RutaCloudflared()
        {
            string programas86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            string programas = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string[] candidatos =
            {
                Path.Combine(AppContext.BaseDirectory, "cloudflared.exe"),
                Path.Combine(programas86, "cloudflared", "cloudflared.exe"),
                Path.Combine(programas, "cloudflared", "cloudflared.exe"),
                Path.Combine(CarpetaDatos, "cloudflared.exe")
            };
            foreach (string c in candidatos)
            {
                if (File.Exists(c)) return c;
            }
            // El PATH del sistema y del usuario se leen de nuevo: si se acaba de instalar, el del proceso aún no lo tiene.
            string path = (Environment.GetEnvironmentVariable("PATH") ?? string.Empty) + ";" +
                          (Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine) ?? string.Empty) + ";" +
                          (Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? string.Empty);
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

        /// <summary>
        /// Espera a que la dirección nueva responda desde internet: Cloudflare tarda hasta un minuto en
        /// publicarla, y si alguien intenta entrar antes, su equipo puede recordar el error un rato.
        /// </summary>
        public async Task<bool> EsperarDisponible(int esperaMs)
        {
            DateTime limite = DateTime.UtcNow.AddMilliseconds(esperaMs);
            using (System.Net.Http.HttpClient http = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(8) })
            {
                while (DateTime.UtcNow < limite && Activo)
                {
                    try
                    {
                        string pagina = await http.GetStringAsync(Url);
                        if (pagina.Contains("BlockForge")) return true;
                    }
                    catch (Exception)
                    {
                        // Aún no está publicada.
                    }
                    await Task.Delay(3000);
                }
            }
            return false;
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
