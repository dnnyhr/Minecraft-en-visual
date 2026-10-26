using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using BlockForge.Helpers;
using BlockForge.Models;
using BlockForge.Motor;
using BlockForge.Services;

namespace BlockForge.Red
{
    /// <summary>Otro jugador conectado a la partida, tal como lo ve este equipo.</summary>
    internal class JugadorRemoto
    {
        public int Id;
        public string Nombre;
        public Vec3 Posicion, Destino;
        public float Giro, Inclinacion, GiroDestino, InclinacionDestino;
        public float Caminata, Velocidad;
        public bool Anfitrion;
        public Color Camiseta;
        /// <summary>Skin del jugador (PNG con el formato de Minecraft) o null.</summary>
        public byte[] Skin;
        public bool SkinDelgada;
        /// <summary>Id del bloque u objeto que lleva en la mano (0 = nada).</summary>
        public int Objeto;
        /// <summary>Segundos desde su último golpe (para animar el brazo).</summary>
        public float DesdeGolpe = 99f;
        /// <summary>Segundos desde que recibió daño (para teñirlo de rojo).</summary>
        public float DesdeDano = 99f;

        /// <summary>Suaviza el movimiento entre las actualizaciones de red (llegan 20 veces por segundo).</summary>
        public void Interpolar(float dt)
        {
            DesdeGolpe += dt;
            DesdeDano += dt;
            float k = Math.Min(1f, dt * 14f);
            Vec3 antes = Posicion;
            Posicion = Posicion + (Destino - Posicion) * k;
            if ((Destino - Posicion).Longitud > 8) Posicion = Destino; // teletransporte
            float dg = (float)Math.IEEERemainder(GiroDestino - Giro, Math.PI * 2);
            Giro += dg * k;
            Inclinacion += (InclinacionDestino - Inclinacion) * k;
            Vec3 d = Posicion - antes;
            float horizontal = (float)Math.Sqrt(d.X * d.X + d.Z * d.Z) / Math.Max(dt, 0.001f);
            Velocidad += (Math.Min(horizontal, 8f) - Velocidad) * Math.Min(1f, dt * 10f);
            Caminata += Velocidad * dt * 2.2f;
        }

        public static Color ColorPara(string nombre)
        {
            int h = 0;
            foreach (char c in nombre ?? "") h = h * 31 + c;
            Color[] paleta =
            {
                Color.FromArgb(52, 152, 219), Color.FromArgb(231, 76, 60), Color.FromArgb(46, 204, 113),
                Color.FromArgb(155, 89, 182), Color.FromArgb(241, 196, 15), Color.FromArgb(230, 126, 34),
                Color.FromArgb(26, 188, 156), Color.FromArgb(236, 112, 160)
            };
            return paleta[(h & 0x7FFFFFFF) % paleta.Length];
        }
    }

    /// <summary>Opciones con que el anfitrión abre su partida.</summary>
    public class OpcionesPartida
    {
        public int Puerto { get; set; }
        /// <summary>Contraseña para entrar (vacía = sin contraseña).</summary>
        public string Contrasena { get; set; }
        public int MaximoJugadores { get; set; }
        /// <summary>Intentar abrir el puerto en el router (UPnP) para jugar por internet.</summary>
        public bool Internet { get; set; }
        /// <summary>Abrir un túnel rápido de Cloudflare (dirección https sin tocar el router).</summary>
        public bool Tunel { get; set; }
        /// <summary>Los jugadores pueden hacerse daño entre sí.</summary>
        public bool Pvp { get; set; }

        public OpcionesPartida()
        {
            Puerto = Protocolo.PuertoPredeterminado;
            Contrasena = string.Empty;
            MaximoJugadores = 8;
            Pvp = true;
        }
    }

    /// <summary>
    /// Partida multijugador por TCP (red local o internet). En modo anfitrión escucha conexiones,
    /// envía el mundo a quien entra y reenvía los cambios; en modo invitado se conecta a un anfitrión.
    /// Todos los eventos se procesan en el hilo del juego mediante Procesar().
    /// </summary>
    public sealed class PartidaRed : IDisposable
    {
        public const int IdAnfitrion = 0;
        private const double SegundosSinRespuesta = 30;
        private const int LimiteMensajeCliente = 400 * 1024;

        private readonly ConcurrentQueue<Mensaje> cola = new ConcurrentQueue<Mensaje>();
        private readonly Dictionary<int, JugadorRemoto> jugadores = new Dictionary<int, JugadorRemoto>();
        private readonly List<Conexion> clientes = new List<Conexion>();
        private TcpListener escucha;
        private Conexion anfitrion;
        private Anunciador anunciador;
        private Mundo mundo;
        private string contrasena = string.Empty;
        private int maximoJugadores = 8;
        private byte[] skinLocal;
        private bool skinLocalDelgada;
        private int siguienteId = 1;
        private volatile bool cerrando;

        public bool EsAnfitrion { get; private set; }
        /// <summary>Solo anfitrión: los jugadores pueden atacarse (se cambia con /pvp).</summary>
        public bool Pvp { get; set; }
        public int Puerto { get; private set; }
        public string NombreLocal { get; private set; }
        public int IdLocal { get; private set; }
        public string NombreMundo { get; private set; }
        public string NombreAnfitrion { get; private set; }
        /// <summary>Motivo que dio el anfitrión al cerrar la conexión (por ejemplo, una expulsión).</summary>
        public string MotivoDesconexion { get; private set; }

        public bool TieneContrasena
        {
            get { return contrasena.Length > 0; }
        }

        /// <summary>Un bloque cambió en otro equipo: (x, y, z, id).</summary>
        public event Action<int, int, int, int> BloqueRecibido;
        /// <summary>Mensaje de chat o aviso del sistema (texto, es del sistema).</summary>
        public event Action<string, bool> ChatRecibido;
        /// <summary>Solo invitado: se perdió la conexión con el anfitrión.</summary>
        public event Action ConexionPerdida;
        /// <summary>Solo invitado: estado de los mobs y hora del día (datos para GestorMobs.AplicarEstado).</summary>
        internal event Action<byte[]> MobsRecibidos;
        /// <summary>Solo anfitrión: un invitado golpeó un mob (id del jugador, posición del jugador, id del mob, daño).</summary>
        internal event Action<int, Vec3, int, float> MobAtacado;
        /// <summary>El jugador local recibe daño (cantidad, origen, causa como "por Ana", si lo hizo otro jugador).</summary>
        internal event Action<float, Vec3, string, bool> DanoRecibido;
        /// <summary>Solo invitado: explotó un creeper (centro, radio).</summary>
        internal event Action<Vec3, float> ExplosionRecibida;
        /// <summary>Solo invitado: objetos ganados (nombre del tipo, cantidad).</summary>
        internal event Action<string, int> RecompensaRecibida;

        private PartidaRed()
        {
        }

        public bool Conectado
        {
            get { return EsAnfitrion ? escucha != null : anfitrion != null && anfitrion.Abierta; }
        }

        public int CantidadJugadores
        {
            get { return jugadores.Count + 1; }
        }

        internal IEnumerable<JugadorRemoto> Jugadores
        {
            get { return jugadores.Values; }
        }

        /// <summary>Nombres de todos los jugadores (el local primero) para la lista de la partida.</summary>
        public List<string> NombresJugadores()
        {
            List<string> r = new List<string> { NombreLocal + (EsAnfitrion ? " (anfitrión)" : "") };
            foreach (JugadorRemoto j in jugadores.Values.OrderBy(j => j.Id)) r.Add(j.Nombre + (j.Anfitrion ? " (anfitrión)" : ""));
            return r;
        }

        private static byte[] SkinSegura(Skin skin)
        {
            return skin != null && skin.Png != null && skin.Png.Length <= SkinService.TamanoMaximo ? skin.Png : null;
        }

        // ------------------------------------------------------------------ anfitrión
        /// <summary>Abre el mundo a la red. Lanza SocketException si el puerto está ocupado.</summary>
        public static PartidaRed Hospedar(Mundo mundo, string nombreJugador, OpcionesPartida opciones, Skin skin)
        {
            PartidaRed p = new PartidaRed
            {
                EsAnfitrion = true,
                mundo = mundo,
                Puerto = opciones.Puerto,
                NombreLocal = nombreJugador,
                IdLocal = IdAnfitrion,
                NombreMundo = mundo.Nombre,
                NombreAnfitrion = nombreJugador,
                contrasena = opciones.Contrasena ?? string.Empty,
                Pvp = opciones.Pvp,
                maximoJugadores = Math.Max(2, opciones.MaximoJugadores),
                skinLocal = SkinSegura(skin),
                skinLocalDelgada = skin != null && skin.Delgada
            };
            p.escucha = new TcpListener(IPAddress.Any, opciones.Puerto);
            p.escucha.Start();
            new Thread(p.BucleAceptar) { IsBackground = true, Name = "BlockForge red (anfitrión)" }.Start();
            p.anunciador = new Anunciador(p);
            return p;
        }

        private void BucleAceptar()
        {
            try
            {
                while (!cerrando)
                {
                    TcpClient tcp = escucha.AcceptTcpClient();
                    ThreadPool.QueueUserWorkItem(_ => Recibir(tcp));
                }
            }
            catch (Exception)
            {
                // El servidor se detuvo.
            }
        }

        /// <summary>
        /// Reconoce qué llegó por el puerto: un jugador de BlockForge directo, o un WebSocket que viene
        /// por un túnel (Cloudflare o ngrok reenvían el tráfico HTTPS a este mismo puerto).
        /// </summary>
        private void Recibir(TcpClient tcp)
        {
            try
            {
                tcp.NoDelay = true;
                NetworkStream ns = tcp.GetStream();
                ns.ReadTimeout = 15000;
                byte[] inicio = new byte[4];
                int leidos = 0;
                while (leidos < 4)
                {
                    int r = ns.Read(inicio, leidos, 4 - leidos);
                    if (r <= 0) throw new IOException();
                    leidos += r;
                }
                Conexion c;
                if (System.Text.Encoding.ASCII.GetString(inicio) == "GET ")
                {
                    System.Net.WebSockets.WebSocket ws = ServidorWebSocket.Aceptar(ns, inicio);
                    if (ws == null)
                    {
                        tcp.Close();
                        return;
                    }
                    ns.ReadTimeout = Timeout.Infinite;
                    c = new Conexion(new FlujoWebSocket(ws), tcp.Close, "túnel");
                }
                else
                {
                    ns.ReadTimeout = Timeout.Infinite;
                    string origen = tcp.Client.RemoteEndPoint != null ? tcp.Client.RemoteEndPoint.ToString() : "?";
                    c = new Conexion(new FlujoConPrefijo(inicio, ns), tcp.Close, origen);
                }
                c.LimiteMensaje = LimiteMensajeCliente;
                if (cerrando) c.Cerrar();
                else c.Iniciar(cola);
            }
            catch (Exception)
            {
                tcp.Close();
            }
        }

        /// <summary>Direcciones IPv4 de este equipo en la red local.</summary>
        public static List<string> DireccionesLocales()
        {
            List<string> r = new List<string>();
            try
            {
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus != OperationalStatus.Up || ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork) r.Add(ip.Address.ToString());
                    }
                }
            }
            catch (Exception)
            {
                // Sin información de red: se devuelve la lista vacía.
            }
            return r;
        }

        // ------------------------------------------------------------------ invitado
        /// <summary>
        /// Se conecta a un anfitrión (IP o nombre de dominio) y descarga el mundo. Es bloqueante
        /// (hasta "espera" ms); lanza una excepción con un mensaje para el usuario si no se pudo entrar.
        /// </summary>
        public static PartidaRed Unirse(string servidor, int puerto, string nombreJugador, string clave, Skin skin, int espera, out Mundo mundoRecibido)
        {
            Conexion c = servidor.Contains("://") ? ConectarTunel(servidor, espera) : ConectarTcp(servidor, puerto, espera);
            c.TiempoLectura = Math.Max(espera, 30000); // por internet el mundo puede tardar en llegar
            try
            {
                byte[] png = SkinSegura(skin);
                c.EnviarDirecto(TipoMensaje.Hola, Protocolo.Armar(w =>
                {
                    w.Write(Protocolo.Version);
                    w.Write(nombreJugador);
                    w.Write(clave ?? string.Empty);
                    w.Write(png != null ? png.Length : 0);
                    if (png != null) w.Write(png);
                    w.Write(skin != null && skin.Delgada);
                }));
                Mensaje m;
                try
                {
                    m = c.LeerDirecto();
                }
                catch (IOException)
                {
                    throw new IOException("El anfitrión cerró la conexión. Puede que sea una versión distinta de BlockForge.");
                }
                if (m.Tipo == TipoMensaje.Rechazo)
                {
                    throw new IOException("El anfitrión rechazó la conexión: " + m.Lector().ReadString());
                }
                if (m.Tipo != TipoMensaje.Bienvenida) throw new IOException("Respuesta inesperada del anfitrión.");

                PartidaRed p = new PartidaRed { EsAnfitrion = false, Puerto = puerto, anfitrion = c };
                using (BinaryReader r = m.Lector())
                {
                    p.IdLocal = r.ReadInt32();
                    p.NombreLocal = r.ReadString();
                    p.NombreMundo = r.ReadString();
                    p.NombreAnfitrion = r.ReadString();
                    int ancho = r.ReadInt32(), alto = r.ReadInt32(), largo = r.ReadInt32();
                    int semilla = r.ReadInt32();
                    bool lagos = r.ReadBoolean();
                    byte[] celdas = Compresion.Descomprimir(r.ReadBytes(r.ReadInt32()));
                    mundoRecibido = new Mundo(ancho, alto, largo) { Nombre = p.NombreMundo, Semilla = semilla, ConLagos = lagos, FechaCreacion = DateTime.Now };
                    mundoRecibido.CargarCeldas(celdas);
                }
                p.mundo = mundoRecibido;
                c.TiempoLectura = 0;
                c.Iniciar(p.cola);
                return p;
            }
            catch (SocketException ex)
            {
                c.Cerrar();
                throw new IOException("Se perdió la conexión mientras se descargaba el mundo: " + ex.Message);
            }
            catch
            {
                c.Cerrar();
                throw;
            }
        }

        private static Conexion ConectarTcp(string servidor, int puerto, int espera)
        {
            TcpClient tcp = new TcpClient();
            try
            {
                if (!tcp.ConnectAsync(servidor, puerto).Wait(espera) || !tcp.Connected)
                {
                    throw new IOException("El anfitrión no respondió. Verifique la dirección y el puerto, y que el anfitrión tenga el puerto abierto.");
                }
            }
            catch (AggregateException ex)
            {
                tcp.Close();
                throw new IOException("No fue posible conectarse a " + servidor + ":" + puerto + ". " + ex.InnerException.Message);
            }
            catch
            {
                tcp.Close();
                throw;
            }
            return new Conexion(tcp);
        }

        /// <summary>Se conecta a una invitación https://... (túnel de Cloudflare o ngrok) mediante WebSocket.</summary>
        private static Conexion ConectarTunel(string url, int espera)
        {
            string direccion = url.Trim();
            if (direccion.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) direccion = "wss://" + direccion.Substring(8);
            else if (direccion.StartsWith("http://", StringComparison.OrdinalIgnoreCase)) direccion = "ws://" + direccion.Substring(7);
            Uri uri;
            if (!Uri.TryCreate(direccion, UriKind.Absolute, out uri)) throw new IOException("La dirección «" + url + "» no es válida.");

            // Una dirección de túnel recién creada tarda hasta un minuto en funcionar: se reintenta mientras tanto.
            DateTime limite = DateTime.UtcNow.AddMilliseconds(Math.Max(espera, 90000));
            Exception ultimo = null;
            while (DateTime.UtcNow < limite)
            {
                System.Net.WebSockets.ClientWebSocket ws = new System.Net.WebSockets.ClientWebSocket();
                ws.Options.KeepAliveInterval = TimeSpan.FromSeconds(20);
                ws.Options.SetRequestHeader("ngrok-skip-browser-warning", "1");
                try
                {
                    using (CancellationTokenSource cts = new CancellationTokenSource(15000))
                    {
                        ws.ConnectAsync(uri, cts.Token).GetAwaiter().GetResult();
                    }
                    return new Conexion(new FlujoWebSocket(ws), () => { }, uri.Host);
                }
                catch (Exception ex)
                {
                    ws.Dispose();
                    ultimo = ex;
                    Thread.Sleep(3000);
                }
            }
            throw new IOException("No fue posible conectarse a " + uri.Host + ". Puede que el anfitrión haya cerrado la partida " +
                                  "(cada vez que la abre, la dirección del túnel cambia). " + (ultimo != null ? ultimo.Message : ""));
        }

        // ------------------------------------------------------------------ procesamiento (hilo del juego)
        public void Procesar()
        {
            Mensaje m;
            int limite = 2000;
            while (limite-- > 0 && cola.TryDequeue(out m))
            {
                try
                {
                    if (EsAnfitrion) ProcesarComoAnfitrion(m);
                    else ProcesarComoInvitado(m);
                }
                catch (Exception)
                {
                    // Un mensaje mal formado no debe detener el juego: se cierra esa conexión.
                    if (m.Origen != null && EsAnfitrion) m.Origen.Cerrar();
                }
            }

            // Conexiones colgadas (por ejemplo, un equipo que perdió internet sin cerrar el juego).
            if (EsAnfitrion)
            {
                foreach (Conexion c in clientes.ToArray())
                {
                    if (c.Abierta && c.SegundosSinRecibir > SegundosSinRespuesta) c.Cerrar();
                }
            }
            else if (anfitrion != null && anfitrion.Abierta && anfitrion.SegundosSinRecibir > SegundosSinRespuesta)
            {
                MotivoDesconexion = "el anfitrión dejó de responder.";
                anfitrion.Cerrar();
            }
        }

        private void ProcesarComoAnfitrion(Mensaje m)
        {
            Conexion c = m.Origen;
            if (m.Tipo == TipoMensaje.Desconexion)
            {
                if (clientes.Remove(c) && c.Aceptada)
                {
                    jugadores.Remove(c.Id);
                    Difundir(TipoMensaje.Sale, Protocolo.Armar(w => w.Write(c.Id)), null);
                    Sistema(c.Nombre + " salió de la partida.");
                }
                return;
            }

            if (!c.Aceptada)
            {
                if (m.Tipo == TipoMensaje.Hola) Aceptar(c, m);
                else c.Cerrar();
                return;
            }

            using (BinaryReader r = m.Lector())
            {
                switch (m.Tipo)
                {
                    case TipoMensaje.Bloque:
                        {
                            int x = r.ReadInt32(), y = r.ReadInt32(), z = r.ReadInt32(), id = r.ReadByte();
                            if (!mundo.EstaDentro(x, y, z) || (id != 0 && CatalogoBloques.Obtener(id) == null)) return;
                            if (BloqueRecibido != null) BloqueRecibido(x, y, z, id);
                            Difundir(TipoMensaje.Bloque, m.Datos, c);
                            break;
                        }
                    case TipoMensaje.Posicion:
                        {
                            r.ReadInt32();
                            float x = r.ReadSingle(), y = r.ReadSingle(), z = r.ReadSingle(), g = r.ReadSingle(), i = r.ReadSingle();
                            int objeto = r.ReadByte();
                            if (float.IsNaN(x + y + z + g + i) || float.IsInfinity(x + y + z + g + i)) return;
                            JugadorRemoto j;
                            if (jugadores.TryGetValue(c.Id, out j)) { Mover(j, x, y, z, g, i); j.Objeto = objeto; }
                            Difundir(TipoMensaje.Posicion, DatosPosicion(c.Id, x, y, z, g, i, objeto), c);
                            break;
                        }
                    case TipoMensaje.Golpe:
                        {
                            JugadorRemoto j;
                            if (jugadores.TryGetValue(c.Id, out j)) j.DesdeGolpe = 0f;
                            Difundir(TipoMensaje.Golpe, Protocolo.Armar(w => w.Write(c.Id)), c);
                            break;
                        }
                    case TipoMensaje.AtacarJugador:
                        {
                            int objetivo = r.ReadInt32();
                            float dano = Math.Max(0f, Math.Min(8f, r.ReadSingle()));
                            JugadorRemoto atacante;
                            if (jugadores.TryGetValue(c.Id, out atacante)) Atacar(c.Id, c.Nombre, atacante.Destino, objetivo, dano);
                            break;
                        }
                    case TipoMensaje.Chat:
                        {
                            string texto = Limpiar(r.ReadString());
                            if (texto.Length == 0) return;
                            string linea = c.Nombre + ": " + texto;
                            Difundir(TipoMensaje.Chat, Protocolo.Armar(w => { w.Write(linea); w.Write(false); }), null);
                            if (ChatRecibido != null) ChatRecibido(linea, false);
                            break;
                        }
                    case TipoMensaje.AtacarMob:
                        {
                            int idMob = r.ReadInt32();
                            float dano = Math.Max(0f, Math.Min(8f, r.ReadSingle()));
                            JugadorRemoto j;
                            if (MobAtacado != null && jugadores.TryGetValue(c.Id, out j)) MobAtacado(c.Id, j.Destino, idMob, dano);
                            break;
                        }
                }
            }
        }

        private void Rechazar(Conexion c, string motivo)
        {
            c.Enviar(TipoMensaje.Rechazo, Protocolo.Armar(w => w.Write(motivo)));
            c.TerminarDespuesDeEnviar();
        }

        private void Aceptar(Conexion c, Mensaje m)
        {
            int version;
            string nombre, clave = string.Empty;
            byte[] skin = null;
            bool delgada = false;
            using (BinaryReader r = m.Lector())
            {
                version = r.ReadInt32();
                nombre = Limpiar(r.ReadString());
                if (version == Protocolo.Version)
                {
                    clave = r.ReadString();
                    int largoSkin = r.ReadInt32();
                    if (largoSkin > 0 && largoSkin <= SkinService.TamanoMaximo) skin = r.ReadBytes(largoSkin);
                    delgada = r.ReadBoolean();
                }
            }
            if (version != Protocolo.Version)
            {
                Rechazar(c, "versión del juego distinta (el anfitrión usa la versión " + Protocolo.Version + " del protocolo).");
                return;
            }
            if (contrasena.Length > 0 && clave != contrasena)
            {
                Rechazar(c, "contraseña incorrecta.");
                return;
            }
            if (CantidadJugadores >= maximoJugadores)
            {
                Rechazar(c, "la partida está llena (" + maximoJugadores + " jugadores).");
                return;
            }
            if (skin != null)
            {
                try { SkinService.Validar(skin); } catch (Exception) { skin = null; }
            }
            if (nombre.Length > 20) nombre = nombre.Substring(0, 20);
            if (nombre.Length == 0) nombre = "Jugador";
            string baseNombre = nombre;
            int n = 2;
            while (nombre == NombreLocal || jugadores.Values.Any(j => j.Nombre == nombre)) nombre = baseNombre + n++;

            c.Id = siguienteId++;
            c.Nombre = nombre;
            c.Aceptada = true;
            clientes.Add(c);

            byte[] celdas = Compresion.Comprimir(mundo.Celdas);
            string nombreFinal = nombre;
            c.Enviar(TipoMensaje.Bienvenida, Protocolo.Armar(w =>
            {
                w.Write(c.Id);
                w.Write(nombreFinal);
                w.Write(mundo.Nombre);
                w.Write(NombreLocal);
                w.Write(mundo.Ancho); w.Write(mundo.Alto); w.Write(mundo.Largo);
                w.Write(mundo.Semilla);
                w.Write(mundo.ConLagos);
                w.Write(celdas.Length);
                w.Write(celdas);
            }));

            // Al nuevo: el anfitrión y los demás jugadores. A los demás: el nuevo.
            c.Enviar(TipoMensaje.Entra, DatosEntra(IdAnfitrion, NombreLocal, true, skinLocal, skinLocalDelgada));
            foreach (JugadorRemoto j in jugadores.Values)
            {
                c.Enviar(TipoMensaje.Entra, DatosEntra(j.Id, j.Nombre, false, j.Skin, j.SkinDelgada));
                c.Enviar(TipoMensaje.Posicion, DatosPosicion(j.Id, j.Destino.X, j.Destino.Y, j.Destino.Z, j.GiroDestino, j.InclinacionDestino, j.Objeto));
            }
            Difundir(TipoMensaje.Entra, DatosEntra(c.Id, nombre, false, skin, delgada), c);

            jugadores[c.Id] = new JugadorRemoto
            {
                Id = c.Id, Nombre = nombre, Camiseta = JugadorRemoto.ColorPara(nombre), Posicion = new Vec3(-1000, -1000, -1000),
                Skin = skin, SkinDelgada = delgada
            };
            Sistema(nombre + " se unió a la partida.");
        }

        private void ProcesarComoInvitado(Mensaje m)
        {
            if (m.Tipo == TipoMensaje.Desconexion)
            {
                jugadores.Clear();
                if (!cerrando && ConexionPerdida != null) ConexionPerdida();
                return;
            }
            using (BinaryReader r = m.Lector())
            {
                switch (m.Tipo)
                {
                    case TipoMensaje.Bloque:
                        {
                            int x = r.ReadInt32(), y = r.ReadInt32(), z = r.ReadInt32(), id = r.ReadByte();
                            if (mundo.EstaDentro(x, y, z) && BloqueRecibido != null) BloqueRecibido(x, y, z, id);
                            break;
                        }
                    case TipoMensaje.Posicion:
                        {
                            int id = r.ReadInt32();
                            float x = r.ReadSingle(), y = r.ReadSingle(), z = r.ReadSingle(), g = r.ReadSingle(), i = r.ReadSingle();
                            int objeto = r.ReadByte();
                            JugadorRemoto j;
                            if (id != IdLocal && jugadores.TryGetValue(id, out j)) { Mover(j, x, y, z, g, i); j.Objeto = objeto; }
                            break;
                        }
                    case TipoMensaje.Golpe:
                        {
                            JugadorRemoto j;
                            if (jugadores.TryGetValue(r.ReadInt32(), out j)) j.DesdeGolpe = 0f;
                            break;
                        }
                    case TipoMensaje.Entra:
                        {
                            int id = r.ReadInt32();
                            string nombre = r.ReadString();
                            bool esAnfitrion = r.ReadBoolean();
                            int largo = r.ReadInt32();
                            byte[] skin = largo > 0 ? r.ReadBytes(largo) : null;
                            bool delgada = r.ReadBoolean();
                            if (id == IdLocal) return;
                            jugadores[id] = new JugadorRemoto
                            {
                                Id = id, Nombre = nombre, Anfitrion = esAnfitrion, Camiseta = JugadorRemoto.ColorPara(nombre),
                                Posicion = new Vec3(-1000, -1000, -1000), Skin = skin, SkinDelgada = delgada
                            };
                            break;
                        }
                    case TipoMensaje.Sale:
                        jugadores.Remove(r.ReadInt32());
                        break;
                    case TipoMensaje.Chat:
                        {
                            string texto = r.ReadString();
                            bool sistema = r.ReadBoolean();
                            if (ChatRecibido != null) ChatRecibido(texto, sistema);
                            break;
                        }
                    case TipoMensaje.Rechazo:
                        MotivoDesconexion = r.ReadString();
                        break;
                    case TipoMensaje.Mobs:
                        if (MobsRecibidos != null) MobsRecibidos(m.Datos);
                        break;
                    case TipoMensaje.Dano:
                        {
                            float dano = r.ReadSingle();
                            Vec3 desde = new Vec3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                            string causa = r.ReadString();
                            bool deJugador = r.ReadBoolean();
                            if (DanoRecibido != null) DanoRecibido(dano, desde, causa, deJugador);
                            break;
                        }
                    case TipoMensaje.Explosion:
                        {
                            Vec3 centro = new Vec3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                            float radio = r.ReadSingle();
                            if (ExplosionRecibida != null) ExplosionRecibida(centro, radio);
                            break;
                        }
                    case TipoMensaje.Recompensa:
                        {
                            string nombre = r.ReadString();
                            int cantidad = r.ReadInt32();
                            if (RecompensaRecibida != null) RecompensaRecibida(nombre, Math.Min(cantidad, 16));
                            break;
                        }
                }
            }
        }

        private static void Mover(JugadorRemoto j, float x, float y, float z, float g, float i)
        {
            bool primera = j.Posicion.Y < -500;
            j.Destino = new Vec3(x, y, z);
            j.GiroDestino = g;
            j.InclinacionDestino = i;
            if (primera)
            {
                j.Posicion = j.Destino;
                j.Giro = g;
                j.Inclinacion = i;
            }
        }

        // ------------------------------------------------------------------ envío desde el juego
        public void EnviarBloque(int x, int y, int z, int id)
        {
            byte[] datos = Protocolo.Armar(w => { w.Write(x); w.Write(y); w.Write(z); w.Write((byte)id); });
            if (EsAnfitrion) Difundir(TipoMensaje.Bloque, datos, null);
            else if (anfitrion != null) anfitrion.Enviar(TipoMensaje.Bloque, datos);
        }

        public void EnviarPosicion(float x, float y, float z, float giro, float inclinacion, int objeto)
        {
            byte[] datos = DatosPosicion(IdLocal, x, y, z, giro, inclinacion, objeto);
            if (EsAnfitrion) Difundir(TipoMensaje.Posicion, datos, null);
            else if (anfitrion != null) anfitrion.Enviar(TipoMensaje.Posicion, datos);
        }

        /// <summary>Anfitrión: estado de los mobs y la hora a todos los invitados.</summary>
        internal void EnviarMobs(byte[] datos)
        {
            if (EsAnfitrion) Difundir(TipoMensaje.Mobs, datos, null);
        }

        /// <summary>Invitado: avisa al anfitrión que golpeó un mob.</summary>
        internal void AtacarMob(int idMob, float dano)
        {
            if (!EsAnfitrion && anfitrion != null) anfitrion.Enviar(TipoMensaje.AtacarMob, Protocolo.Armar(w => { w.Write(idMob); w.Write(dano); }));
        }

        /// <summary>Anfitrión: un invitado recibe daño.</summary>
        internal void EnviarDano(int jugador, float dano, Vec3 desde, string causa = "por un mob", bool deJugador = false)
        {
            if (!EsAnfitrion) return;
            Conexion c = clientes.FirstOrDefault(x => x.Aceptada && x.Id == jugador);
            if (c != null)
            {
                c.Enviar(TipoMensaje.Dano, Protocolo.Armar(w =>
                {
                    w.Write(dano); w.Write(desde.X); w.Write(desde.Y); w.Write(desde.Z); w.Write(causa ?? ""); w.Write(deJugador);
                }));
            }
        }

        /// <summary>El jugador local dio un golpe: los demás ven moverse su brazo.</summary>
        public void EnviarGolpe()
        {
            byte[] datos = Protocolo.Armar(w => w.Write(IdLocal));
            if (EsAnfitrion) Difundir(TipoMensaje.Golpe, datos, null);
            else if (anfitrion != null) anfitrion.Enviar(TipoMensaje.Golpe, datos);
        }

        /// <summary>El jugador local golpeó a otro jugador (PvP). Lo decide el anfitrión.</summary>
        internal void AtacarJugador(int objetivo, float dano, Vec3 desde)
        {
            if (EsAnfitrion) Atacar(IdLocal, NombreLocal, desde, objetivo, dano);
            else if (anfitrion != null) anfitrion.Enviar(TipoMensaje.AtacarJugador, Protocolo.Armar(w => { w.Write(objetivo); w.Write(dano); }));
        }

        /// <summary>Anfitrión: aplica un ataque entre jugadores si el PvP está activo y están cerca.</summary>
        private void Atacar(int atacante, string nombreAtacante, Vec3 desde, int objetivo, float dano)
        {
            if (!Pvp || atacante == objetivo) return;
            string causa = "por " + nombreAtacante;
            if (objetivo == IdAnfitrion)
            {
                // La distancia al anfitrión la comprueba el juego, que conoce su posición.
                if (DanoRecibido != null) DanoRecibido(dano, desde, causa, true);
                return;
            }
            JugadorRemoto j;
            if (!jugadores.TryGetValue(objetivo, out j) || (j.Destino - desde).Longitud > 6f) return;
            j.DesdeDano = 0f;
            EnviarDano(objetivo, dano, desde, causa, true);
        }

        /// <summary>Anfitrión: efectos de una explosión (los bloques llegan como cambios normales).</summary>
        internal void EnviarExplosion(Vec3 centro, float radio)
        {
            if (EsAnfitrion) Difundir(TipoMensaje.Explosion, Protocolo.Armar(w => { w.Write(centro.X); w.Write(centro.Y); w.Write(centro.Z); w.Write(radio); }), null);
        }

        /// <summary>Anfitrión: objetos para el invitado que mató un mob.</summary>
        internal void EnviarRecompensa(int jugador, string tipo, int cantidad)
        {
            if (!EsAnfitrion) return;
            Conexion c = clientes.FirstOrDefault(x => x.Aceptada && x.Id == jugador);
            if (c != null) c.Enviar(TipoMensaje.Recompensa, Protocolo.Armar(w => { w.Write(tipo); w.Write(cantidad); }));
        }

        public void EnviarChat(string texto)
        {
            texto = Limpiar(texto);
            if (texto.Length == 0) return;
            if (EsAnfitrion)
            {
                if (texto.StartsWith("/"))
                {
                    Comando(texto);
                    return;
                }
                string linea = NombreLocal + ": " + texto;
                Difundir(TipoMensaje.Chat, Protocolo.Armar(w => { w.Write(linea); w.Write(false); }), null);
                if (ChatRecibido != null) ChatRecibido(linea, false);
            }
            else if (anfitrion != null)
            {
                anfitrion.Enviar(TipoMensaje.Chat, Protocolo.Armar(w => w.Write(texto)));
            }
        }

        /// <summary>Comandos del anfitrión: /expulsar nombre y /jugadores.</summary>
        private void Comando(string texto)
        {
            string[] partes = texto.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            string orden = partes[0].ToLowerInvariant();
            if (orden == "/expulsar" && partes.Length == 2)
            {
                Conexion c = clientes.FirstOrDefault(x => x.Aceptada && string.Equals(x.Nombre, partes[1].Trim(), StringComparison.OrdinalIgnoreCase));
                if (c == null)
                {
                    Local("No hay ningún jugador llamado «" + partes[1].Trim() + "».");
                    return;
                }
                Rechazar(c, "el anfitrión te expulsó de la partida.");
                Sistema(c.Nombre + " fue expulsado por el anfitrión.");
            }
            else if (orden == "/jugadores")
            {
                Local("Jugadores: " + string.Join(", ", NombresJugadores()));
            }
            else if (orden == "/pvp")
            {
                Pvp = !Pvp;
                Sistema(Pvp ? "PvP activado: los jugadores pueden atacarse." : "PvP desactivado: los jugadores ya no se hacen daño.");
            }
            else
            {
                Local("Comandos del anfitrión: /expulsar <nombre>, /jugadores, /pvp, /dia, /noche, /pacifico");
            }
        }

        private void Local(string texto)
        {
            if (ChatRecibido != null) ChatRecibido(texto, true);
        }

        private void Sistema(string texto)
        {
            Difundir(TipoMensaje.Chat, Protocolo.Armar(w => { w.Write(texto); w.Write(true); }), null);
            if (ChatRecibido != null) ChatRecibido(texto, true);
        }

        private void Difundir(TipoMensaje tipo, byte[] datos, Conexion excepto)
        {
            foreach (Conexion c in clientes)
            {
                if (c != excepto && c.Aceptada) c.Enviar(tipo, datos);
            }
        }

        private static byte[] DatosPosicion(int id, float x, float y, float z, float g, float i, int objeto)
        {
            return Protocolo.Armar(w => { w.Write(id); w.Write(x); w.Write(y); w.Write(z); w.Write(g); w.Write(i); w.Write((byte)Math.Max(0, Math.Min(255, objeto))); });
        }

        private static byte[] DatosEntra(int id, string nombre, bool esAnfitrion, byte[] skin, bool delgada)
        {
            return Protocolo.Armar(w =>
            {
                w.Write(id);
                w.Write(nombre);
                w.Write(esAnfitrion);
                w.Write(skin != null ? skin.Length : 0);
                if (skin != null) w.Write(skin);
                w.Write(delgada);
            });
        }

        private static string Limpiar(string texto)
        {
            texto = (texto ?? string.Empty).Trim();
            if (texto.Length > 120) texto = texto.Substring(0, 120);
            char[] r = texto.Where(ch => !char.IsControl(ch)).ToArray();
            return new string(r);
        }

        public void Dispose()
        {
            cerrando = true;
            if (anunciador != null) anunciador.Dispose();
            if (escucha != null)
            {
                try { escucha.Stop(); } catch (Exception) { }
                escucha = null;
            }
            foreach (Conexion c in clientes.ToArray()) c.Cerrar();
            clientes.Clear();
            if (anfitrion != null) anfitrion.Cerrar();
            jugadores.Clear();
        }

        // ------------------------------------------------------------------ descubrimiento en la red local
        /// <summary>Anuncia la partida por UDP cada 1,5 s para que aparezca en la lista de "Unirse".</summary>
        private sealed class Anunciador : IDisposable
        {
            private readonly PartidaRed partida;
            private readonly UdpClient udp;
            private readonly Timer temporizador;

            public Anunciador(PartidaRed partida)
            {
                this.partida = partida;
                try
                {
                    udp = new UdpClient { EnableBroadcast = true };
                    temporizador = new Timer(Anunciar, null, 0, 1500);
                }
                catch (Exception)
                {
                    // Sin anuncios la partida sigue disponible escribiendo la dirección a mano.
                }
            }

            private void Anunciar(object estado)
            {
                try
                {
                    string texto = "BLOCKFORGE|" + Protocolo.Version + "|" + partida.Puerto + "|" + partida.NombreMundo.Replace("|", " ") +
                                   "|" + partida.NombreLocal.Replace("|", " ") + "|" + partida.CantidadJugadores + "|" + (partida.TieneContrasena ? 1 : 0);
                    byte[] datos = System.Text.Encoding.UTF8.GetBytes(texto);
                    udp.Send(datos, datos.Length, new IPEndPoint(IPAddress.Broadcast, Protocolo.PuertoDescubrimiento));
                    udp.Send(datos, datos.Length, new IPEndPoint(IPAddress.Loopback, Protocolo.PuertoDescubrimiento));
                }
                catch (Exception)
                {
                    // La red puede no permitir difusión; se ignora.
                }
            }

            public void Dispose()
            {
                if (temporizador != null) temporizador.Dispose();
                if (udp != null) udp.Close();
            }
        }
    }

    /// <summary>Partida encontrada en la red local.</summary>
    public class PartidaEncontrada
    {
        public string Direccion { get; set; }
        public int Puerto { get; set; }
        public string Mundo { get; set; }
        public string Anfitrion { get; set; }
        public int Jugadores { get; set; }
        public bool ConContrasena { get; set; }
        public DateTime Visto { get; set; }

        public string Destino
        {
            get { return Direccion + ":" + Puerto; }
        }

        public string Acceso
        {
            get { return ConContrasena ? "Con contraseña" : "Libre"; }
        }
    }

    /// <summary>Escucha los anuncios UDP de las partidas abiertas en la red local.</summary>
    public sealed class BuscadorPartidas : IDisposable
    {
        private readonly UdpClient udp;
        private readonly ConcurrentDictionary<string, PartidaEncontrada> partidas = new ConcurrentDictionary<string, PartidaEncontrada>();
        private volatile bool activo = true;

        public string Error { get; private set; }

        public BuscadorPartidas()
        {
            try
            {
                udp = new UdpClient();
                udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udp.Client.Bind(new IPEndPoint(IPAddress.Any, Protocolo.PuertoDescubrimiento));
                new Thread(Escuchar) { IsBackground = true, Name = "BlockForge red (búsqueda)" }.Start();
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
        }

        private void Escuchar()
        {
            while (activo)
            {
                try
                {
                    IPEndPoint origen = new IPEndPoint(IPAddress.Any, 0);
                    string[] p = System.Text.Encoding.UTF8.GetString(udp.Receive(ref origen)).Split('|');
                    if (p.Length < 6 || p[0] != "BLOCKFORGE" || p[1] != Protocolo.Version.ToString()) continue;
                    PartidaEncontrada e = new PartidaEncontrada
                    {
                        Direccion = IPAddress.IsLoopback(origen.Address) ? "127.0.0.1" : origen.Address.ToString(),
                        Puerto = int.Parse(p[2]),
                        Mundo = p[3],
                        Anfitrion = p[4],
                        Jugadores = int.Parse(p[5]),
                        ConContrasena = p.Length > 6 && p[6] == "1",
                        Visto = DateTime.Now
                    };
                    // Si el mismo anfitrión llega por difusión y por loopback, se conserva una sola entrada.
                    string clave = e.Anfitrion + "|" + e.Mundo + "|" + e.Puerto;
                    PartidaEncontrada previa;
                    if (partidas.TryGetValue(clave, out previa) && previa.Direccion != "127.0.0.1" && e.Direccion == "127.0.0.1")
                    {
                        previa.Visto = e.Visto;
                        previa.Jugadores = e.Jugadores;
                        continue;
                    }
                    partidas[clave] = e;
                }
                catch (Exception)
                {
                    if (!activo) return;
                }
            }
        }

        /// <summary>Partidas vistas en los últimos 5 segundos.</summary>
        public List<PartidaEncontrada> Vigentes()
        {
            DateTime limite = DateTime.Now.AddSeconds(-5);
            return partidas.Values.Where(p => p.Visto >= limite).OrderBy(p => p.Mundo).ToList();
        }

        public void Dispose()
        {
            activo = false;
            if (udp != null) udp.Close();
        }
    }
}
