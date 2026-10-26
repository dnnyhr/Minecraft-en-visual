using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace BlockForge.Red
{
    /// <summary>
    /// Una conexión TCP con otro jugador. Un hilo lee los mensajes y los deja en una cola
    /// que el juego procesa en su propio hilo; otro hilo envía, para que la red nunca congele la pantalla.
    /// </summary>
    internal class Conexion : IDisposable
    {
        private readonly TcpClient cliente;
        private readonly NetworkStream flujo;
        private readonly BlockingCollection<byte[]> salida = new BlockingCollection<byte[]>();
        private ConcurrentQueue<Mensaje> entrada;
        private int cerrada;
        private long ultimaRecepcion = DateTime.UtcNow.Ticks;

        public int Id { get; set; }
        public string Nombre { get; set; }
        public bool Aceptada { get; set; }
        public string Direccion { get; private set; }

        /// <summary>Tamaño máximo de un mensaje recibido (protege al anfitrión de datos enormes).</summary>
        public int LimiteMensaje { get; set; }

        public Conexion(TcpClient cliente)
        {
            this.cliente = cliente;
            cliente.NoDelay = true;
            flujo = cliente.GetStream();
            LimiteMensaje = Protocolo.TamanoMaximo;
            Direccion = cliente.Client.RemoteEndPoint != null ? cliente.Client.RemoteEndPoint.ToString() : "?";
        }

        public bool Abierta
        {
            get { return cerrada == 0; }
        }

        /// <summary>Segundos desde el último mensaje recibido (para detectar conexiones colgadas).</summary>
        public double SegundosSinRecibir
        {
            get { return TimeSpan.FromTicks(DateTime.UtcNow.Ticks - Interlocked.Read(ref ultimaRecepcion)).TotalSeconds; }
        }

        /// <summary>Lee un mensaje de forma bloqueante (se usa solo durante el saludo inicial).</summary>
        public Mensaje LeerDirecto()
        {
            return Leer();
        }

        /// <summary>Arranca los hilos de lectura y escritura. Los mensajes recibidos se agregan a "cola".</summary>
        public void Iniciar(ConcurrentQueue<Mensaje> cola)
        {
            entrada = cola;
            Interlocked.Exchange(ref ultimaRecepcion, DateTime.UtcNow.Ticks);
            new Thread(BucleLectura) { IsBackground = true, Name = "BlockForge red (lectura)" }.Start();
            new Thread(BucleEscritura) { IsBackground = true, Name = "BlockForge red (escritura)" }.Start();
        }

        public void Enviar(TipoMensaje tipo, byte[] datos)
        {
            if (!Abierta) return;
            datos = datos ?? new byte[0];
            byte[] paquete = new byte[5 + datos.Length];
            BitConverter.GetBytes(datos.Length + 1).CopyTo(paquete, 0);
            paquete[4] = (byte)tipo;
            Buffer.BlockCopy(datos, 0, paquete, 5, datos.Length);
            try
            {
                salida.Add(paquete);
            }
            catch (InvalidOperationException)
            {
                // La conexión ya se cerró o está terminando.
            }
        }

        /// <summary>Envía lo que quede en la cola y después cierra (por ejemplo, tras un rechazo).</summary>
        public void TerminarDespuesDeEnviar()
        {
            try { salida.CompleteAdding(); } catch (ObjectDisposedException) { }
        }

        /// <summary>Envío inmediato y bloqueante (solo para el saludo, antes de iniciar los hilos).</summary>
        public void EnviarDirecto(TipoMensaje tipo, byte[] datos)
        {
            datos = datos ?? new byte[0];
            byte[] cabecera = BitConverter.GetBytes(datos.Length + 1);
            flujo.Write(cabecera, 0, 4);
            flujo.WriteByte((byte)tipo);
            flujo.Write(datos, 0, datos.Length);
            flujo.Flush();
        }

        private Mensaje Leer()
        {
            byte[] cabecera = LeerExacto(4);
            int largo = BitConverter.ToInt32(cabecera, 0);
            if (largo < 1 || largo > LimiteMensaje) throw new IOException("Mensaje de red inválido.");
            byte[] cuerpo = LeerExacto(largo);
            Interlocked.Exchange(ref ultimaRecepcion, DateTime.UtcNow.Ticks);
            byte[] datos = new byte[largo - 1];
            Buffer.BlockCopy(cuerpo, 1, datos, 0, datos.Length);
            return new Mensaje { Tipo = (TipoMensaje)cuerpo[0], Datos = datos, Origen = this };
        }

        private byte[] LeerExacto(int n)
        {
            byte[] b = new byte[n];
            int leidos = 0;
            while (leidos < n)
            {
                int r = flujo.Read(b, leidos, n - leidos);
                if (r <= 0) throw new IOException("La conexión se cerró.");
                leidos += r;
            }
            return b;
        }

        private void BucleLectura()
        {
            try
            {
                while (Abierta) entrada.Enqueue(Leer());
            }
            catch (Exception)
            {
                // Fin de la conexión: se informa como mensaje de desconexión.
            }
            Cerrar();
        }

        private void BucleEscritura()
        {
            try
            {
                foreach (byte[] paquete in salida.GetConsumingEnumerable())
                {
                    flujo.Write(paquete, 0, paquete.Length);
                }
                flujo.Flush();
                Thread.Sleep(200); // da tiempo a que el otro extremo lea el último mensaje
            }
            catch (Exception)
            {
                // Error al escribir: la conexión se perdió.
            }
            Cerrar();
        }

        public void Cerrar()
        {
            if (Interlocked.Exchange(ref cerrada, 1) == 1) return;
            try { salida.CompleteAdding(); } catch (Exception) { }
            try { cliente.Close(); } catch (Exception) { }
            if (entrada != null) entrada.Enqueue(new Mensaje { Tipo = TipoMensaje.Desconexion, Origen = this });
        }

        public void Dispose()
        {
            Cerrar();
        }
    }
}
