using System;
using System.IO;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace BlockForge.Red
{
    /// <summary>
    /// Presenta un WebSocket como un flujo de bytes, para que la partida funcione igual por TCP directo
    /// o a través de un túnel HTTPS (Cloudflare o ngrok), que solo dejan pasar tráfico web.
    /// Admite un lector y un escritor a la vez (cada uno en su hilo), como pide WebSocket.
    /// </summary>
    internal sealed class FlujoWebSocket : Stream
    {
        private readonly WebSocket ws;
        private readonly byte[] buffer = new byte[64 * 1024];
        private int inicio, fin;
        private int tiempoLectura = Timeout.Infinite;

        public FlujoWebSocket(WebSocket ws)
        {
            this.ws = ws;
        }

        public override bool CanRead { get { return true; } }
        public override bool CanWrite { get { return true; } }
        public override bool CanSeek { get { return false; } }
        public override bool CanTimeout { get { return true; } }
        public override long Length { get { throw new NotSupportedException(); } }
        public override long Position { get { throw new NotSupportedException(); } set { throw new NotSupportedException(); } }

        public override int ReadTimeout
        {
            get { return tiempoLectura; }
            set { tiempoLectura = value <= 0 ? Timeout.Infinite : value; }
        }

        public override int Read(byte[] destino, int desplazamiento, int cantidad)
        {
            while (inicio >= fin)
            {
                using (CancellationTokenSource cts = tiempoLectura == Timeout.Infinite ? new CancellationTokenSource() : new CancellationTokenSource(tiempoLectura))
                {
                    WebSocketReceiveResult r;
                    try
                    {
                        r = ws.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token).GetAwaiter().GetResult();
                    }
                    catch (OperationCanceledException)
                    {
                        throw new IOException("El otro equipo no respondió a tiempo.");
                    }
                    catch (WebSocketException ex)
                    {
                        throw new IOException(ex.Message, ex);
                    }
                    if (r.MessageType == WebSocketMessageType.Close) return 0;
                    inicio = 0;
                    fin = r.Count;
                }
            }
            int n = Math.Min(cantidad, fin - inicio);
            Buffer.BlockCopy(buffer, inicio, destino, desplazamiento, n);
            inicio += n;
            return n;
        }

        public override void Write(byte[] origen, int desplazamiento, int cantidad)
        {
            try
            {
                ws.SendAsync(new ArraySegment<byte>(origen, desplazamiento, cantidad), WebSocketMessageType.Binary, true, CancellationToken.None).GetAwaiter().GetResult();
            }
            catch (WebSocketException ex)
            {
                throw new IOException(ex.Message, ex);
            }
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin) { throw new NotSupportedException(); }
        public override void SetLength(long value) { throw new NotSupportedException(); }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try { ws.Abort(); } catch (Exception) { }
                ws.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>Flujo que devuelve primero unos bytes ya leídos (los que se usaron para reconocer el protocolo).</summary>
    internal sealed class FlujoConPrefijo : Stream
    {
        private readonly byte[] prefijo;
        private readonly Stream interno;
        private int usados;

        public FlujoConPrefijo(byte[] prefijo, Stream interno)
        {
            this.prefijo = prefijo;
            this.interno = interno;
        }

        public override bool CanRead { get { return true; } }
        public override bool CanWrite { get { return true; } }
        public override bool CanSeek { get { return false; } }
        public override long Length { get { throw new NotSupportedException(); } }
        public override long Position { get { throw new NotSupportedException(); } set { throw new NotSupportedException(); } }

        public override int Read(byte[] destino, int desplazamiento, int cantidad)
        {
            if (usados < prefijo.Length)
            {
                int n = Math.Min(cantidad, prefijo.Length - usados);
                Buffer.BlockCopy(prefijo, usados, destino, desplazamiento, n);
                usados += n;
                return n;
            }
            return interno.Read(destino, desplazamiento, cantidad);
        }

        public override void Write(byte[] origen, int desplazamiento, int cantidad) { interno.Write(origen, desplazamiento, cantidad); }
        public override void Flush() { interno.Flush(); }
        public override long Seek(long offset, SeekOrigin origin) { throw new NotSupportedException(); }
        public override void SetLength(long value) { throw new NotSupportedException(); }
    }

    /// <summary>
    /// Saludo HTTP de un WebSocket entrante, hecho a mano sobre la conexión TCP del anfitrión. Así el mismo
    /// puerto atiende a los jugadores directos y a los que llegan por un túnel, sin permisos de administrador.
    /// </summary>
    internal static class ServidorWebSocket
    {
        private const string Guid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        /// <summary>
        /// Completa el saludo (ya se leyeron los primeros bytes, "GET "). Devuelve el WebSocket o null si era
        /// una visita normal de navegador (se le responde una página que explica qué hacer con la dirección).
        /// </summary>
        public static WebSocket Aceptar(NetworkStream flujo, byte[] leidos)
        {
            StringBuilder cabecera = new StringBuilder(Encoding.ASCII.GetString(leidos));
            while (cabecera.Length < 16 * 1024 && !cabecera.ToString().EndsWith("\r\n\r\n"))
            {
                int b = flujo.ReadByte();
                if (b < 0) throw new IOException("La conexión se cerró durante el saludo.");
                cabecera.Append((char)b);
            }
            string clave = null;
            bool esWebSocket = false;
            foreach (string linea in cabecera.ToString().Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                int dosPuntos = linea.IndexOf(':');
                if (dosPuntos <= 0) continue;
                string nombre = linea.Substring(0, dosPuntos).Trim().ToLowerInvariant(), valor = linea.Substring(dosPuntos + 1).Trim();
                if (nombre == "sec-websocket-key") clave = valor;
                if (nombre == "upgrade" && valor.ToLowerInvariant().Contains("websocket")) esWebSocket = true;
            }

            if (!esWebSocket || clave == null)
            {
                string html = "<html><head><meta charset='utf-8'><title>BlockForge</title></head><body style='font-family:Segoe UI;background:#1c1f26;color:#eee;padding:40px'>" +
                              "<h1>Servidor de BlockForge</h1><p>Esta dirección no se abre en el navegador: cópiala y pégala en " +
                              "<b>BlockForge &gt; Unirse a una partida</b>.</p></body></html>";
                byte[] cuerpo = Encoding.UTF8.GetBytes(html);
                byte[] respuesta = Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\nContent-Type: text/html; charset=utf-8\r\nContent-Length: " + cuerpo.Length + "\r\nConnection: close\r\n\r\n");
                flujo.Write(respuesta, 0, respuesta.Length);
                flujo.Write(cuerpo, 0, cuerpo.Length);
                flujo.Flush();
                return null;
            }

            string aceptar;
            using (SHA1 sha = SHA1.Create())
            {
                aceptar = Convert.ToBase64String(sha.ComputeHash(Encoding.ASCII.GetBytes(clave + Guid)));
            }
            byte[] saludo = Encoding.ASCII.GetBytes("HTTP/1.1 101 Switching Protocols\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Accept: " + aceptar + "\r\n\r\n");
            flujo.Write(saludo, 0, saludo.Length);
            flujo.Flush();
            return WebSocket.CreateFromStream(flujo, true, null, TimeSpan.FromSeconds(20));
        }
    }
}
