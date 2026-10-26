using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Sockets;
using System.Windows.Forms;
using BlockForge.Controls;
using BlockForge.Helpers;
using BlockForge.Models;
using BlockForge.Red;
using BlockForge.Services;

namespace BlockForge.Forms
{
    /// <summary>Área de juego 3D: construcción, inventario, guardado (RF-06 a RF-11) y multijugador en red local.</summary>
    public partial class FrmMundo : Form
    {
        private readonly ConstruccionService juego;
        private readonly bool esInvitado;
        private PartidaRed red;
        private Upnp upnp;
        private int puertoUpnp;
        private FormWindowState estadoAnterior;
        private bool pantallaCompleta, conexionPerdida;

        /// <summary>Si tiene valor, el mundo se abre a la red con estas opciones al iniciar ("Hospedar partida").</summary>
        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public OpcionesPartida HospedarAlIniciar { get; set; }

        /// <summary>Partida de un jugador (o anfitrión, si después se abre a la red).</summary>
        public FrmMundo(Mundo mundo, Inventario inventario)
        {
            InitializeComponent();
            juego = new ConstruccionService(mundo, inventario);
        }

        /// <summary>Invitado en el mundo de otro jugador: no se guarda en la base de datos local.</summary>
        public FrmMundo(Mundo mundo, PartidaRed partida) : this(mundo, new Inventario())
        {
            esInvitado = true;
            red = partida;
        }

        private void FrmMundo_Load(object sender, EventArgs e)
        {
            vistaMundo.Iniciar(juego, Sesion.Configuracion ?? Configuracion.PorDefecto());
            vistaMundo.EsInvitado = esInvitado;
            if (Sesion.UsuarioActual != null) vistaMundo.SkinLocal = Sesion.UsuarioActual.Skin;
            if (esInvitado)
            {
                Text = "BlockForge — " + juego.Mundo.Nombre + " (invitado de " + red.NombreAnfitrion + ")";
                mnuGuardar.Enabled = false;
                mnuAbrirRed.Enabled = false;
                red.ConexionPerdida += AlPerderConexion;
                vistaMundo.ConectarRed(red);
                vistaMundo.InfoConexion = "Conectado a " + red.NombreAnfitrion + " · jugando como " + red.NombreLocal;
                vistaMundo.AgregarChat("Te uniste al mundo «" + red.NombreMundo + "» de " + red.NombreAnfitrion + ". Presiona T para chatear.",
                                       Color.FromArgb(255, 225, 120));
            }
            else
            {
                Text = "BlockForge — " + juego.Mundo.Nombre;
            }
            ActualizarEstado();
        }

        private void FrmMundo_Shown(object sender, EventArgs e)
        {
            if (vistaMundo.ErrorInicio != null)
            {
                Mensajes.Error("No fue posible iniciar la vista 3D (OpenGL).\n\n" +
                               "Actualice el controlador de la tarjeta de video e intente de nuevo.\n\nDetalle: " + vistaMundo.ErrorInicio);
            }
            if (HospedarAlIniciar != null) AbrirRed(HospedarAlIniciar);
            vistaMundo.Focus();
        }

        private void FrmMundo_Deactivate(object sender, EventArgs e)
        {
            vistaMundo.LiberarMouse();
        }

        private void vistaMundo_EstadoCambiado(object sender, EventArgs e)
        {
            ActualizarEstado();
        }

        private void tmrEstado_Tick(object sender, EventArgs e)
        {
            lblEstadoPosicion.Text = "Posición: " + vistaMundo.TextoPosicion;
            vistaMundo.MantenerRed();
            ActualizarEstado();
        }

        private void ActualizarEstado()
        {
            TipoBloque tipo = vistaMundo.TipoActivo;
            lblEstadoMundo.Text = "Mundo: " + juego.Mundo.Nombre;
            lblEstadoBloque.Text = "Bloque activo: " + (tipo != null
                ? tipo.Nombre + " (" + juego.Inventario.ObtenerCantidad(tipo.IdTipoBloque) + ")"
                : "-");
            if (esInvitado)
            {
                lblEstadoCambios.Text = "Invitado (no se guarda)";
                lblEstadoCambios.ForeColor = Estilos.TextoAuxiliar;
            }
            else
            {
                lblEstadoCambios.Text = juego.HayCambios ? "● Cambios sin guardar" : "Sin cambios";
                lblEstadoCambios.ForeColor = juego.HayCambios ? Color.FromArgb(160, 110, 0) : Estilos.Texto;
            }
            if (red == null) lblEstadoRed.Text = "Un jugador";
            else if (!red.Conectado) lblEstadoRed.Text = "Sin conexión";
            else lblEstadoRed.Text = (red.EsAnfitrion ? "Anfitrión · puerto " + red.Puerto : "Invitado") + " · " +
                                     red.CantidadJugadores + (red.CantidadJugadores == 1 ? " jugador" : " jugadores");
            mnuVuelo.Checked = vistaMundo.Volando;
            mnuAbrirRed.Enabled = !esInvitado && red == null;
        }

        // ------------------------------------------------------------------ multijugador
        /// <summary>Pide las opciones y abre el mundo a la red local o a internet.</summary>
        private void AbrirRed()
        {
            if (esInvitado || red != null) return;
            vistaMundo.LiberarMouse();
            using (FrmOpcionesPartida dialogo = new FrmOpcionesPartida())
            {
                if (dialogo.ShowDialog(this) == DialogResult.OK) AbrirRed(dialogo.Opciones);
            }
            vistaMundo.Focus();
        }

        private void AbrirRed(OpcionesPartida opciones)
        {
            if (esInvitado || red != null) return;
            string nombre = Sesion.UsuarioActual != null ? Sesion.UsuarioActual.NombreUsuario : "Anfitrión";
            Skin skin = Sesion.UsuarioActual != null ? Sesion.UsuarioActual.Skin : null;
            int primero = opciones.Puerto;
            for (int puerto = primero; puerto < primero + 10 && puerto <= 65535; puerto++)
            {
                try
                {
                    opciones.Puerto = puerto;
                    red = PartidaRed.Hospedar(juego.Mundo, nombre, opciones, skin);
                    break;
                }
                catch (SocketException)
                {
                    // Puerto ocupado (por ejemplo, otra partida en este equipo): se prueba el siguiente.
                }
            }
            if (red == null)
            {
                Mensajes.Error("No fue posible abrir la partida: los puertos " + primero + " a " + (primero + 9) + " están ocupados.");
                return;
            }

            vistaMundo.ConectarRed(red);
            Color amarillo = Color.FromArgb(255, 225, 120);
            List<string> ips = PartidaRed.DireccionesLocales();
            string local = (ips.Count > 0 ? ips[0] : "127.0.0.1") + ":" + red.Puerto;
            vistaMundo.InfoConexion = "Red local: " + local + (red.TieneContrasena ? " · con contraseña" : "");
            vistaMundo.Invitacion = local;
            vistaMundo.AgregarChat("Partida abierta en tu red local (" + local + ").", amarillo);
            if (opciones.Internet) PrepararInternet(red.Puerto, local);
            else vistaMundo.AgregarChat("En la misma red pueden entrar con «Unirse a partida».", amarillo);
            ActualizarEstado();
        }

        /// <summary>
        /// Abre el puerto en el router (UPnP), averigua la IP pública y explica qué hacer si no se puede
        /// (por ejemplo, con CGNAT del proveedor de internet).
        /// </summary>
        private async void PrepararInternet(int puerto, string local)
        {
            Color amarillo = Color.FromArgb(255, 225, 120), rojo = Color.FromArgb(255, 150, 130), verde = Color.FromArgb(150, 235, 150);
            vistaMundo.AgregarChat("Preparando la partida para internet...", amarillo);
            string errorUpnp = null, ipRouter = null;
            try
            {
                upnp = await Upnp.Buscar();
                if (upnp != null)
                {
                    errorUpnp = await upnp.AbrirPuerto(puerto, "BlockForge");
                    if (errorUpnp == null) puertoUpnp = puerto;
                    ipRouter = await upnp.IpExterna();
                }
            }
            catch (Exception ex)
            {
                errorUpnp = ex.Message;
            }
            string ipPublica = await Internet.IpPublica();
            if (IsDisposed || red == null) return;

            string ipLocal = upnp != null ? upnp.IpLocal : local.Split(':')[0];
            if (upnp == null)
            {
                vistaMundo.AgregarChat("Tu router no permite abrir puertos automáticamente (UPnP desactivado).", rojo);
                vistaMundo.AgregarChat("Abre a mano el puerto TCP " + puerto + " hacia " + ipLocal + " en tu router, o usa una VPN (ver LEEME).", rojo);
            }
            else if (errorUpnp != null)
            {
                vistaMundo.AgregarChat("El router «" + upnp.NombreRouter + "» no aceptó abrir el puerto: " + errorUpnp, rojo);
                vistaMundo.AgregarChat("Abre a mano el puerto TCP " + puerto + " hacia " + ipLocal + ", o usa una VPN (ver LEEME).", rojo);
            }
            else
            {
                vistaMundo.AgregarChat("Puerto " + puerto + " abierto en el router «" + upnp.NombreRouter + "».", verde);
            }

            bool cgnat = ipRouter != null && Internet.EsPrivada(ipRouter);
            if (cgnat)
            {
                vistaMundo.AgregarChat("Tu router no tiene IP pública (" + ipRouter + "): tu proveedor usa CGNAT o hay otro router delante.", rojo);
                vistaMundo.AgregarChat("Desde internet no podrán entrar con tu IP. Usen Radmin VPN, ZeroTier o Tailscale (ver LEEME).", rojo);
            }

            if (ipPublica == null)
            {
                vistaMundo.AgregarChat("No se pudo averiguar tu IP pública (¿hay conexión a internet?).", rojo);
                return;
            }
            string direccion = ipPublica + ":" + puerto;
            vistaMundo.Invitacion = direccion;
            vistaMundo.InfoConexion = "Internet: " + direccion + (red.TieneContrasena ? " · con contraseña" : "") + "\nRed local: " + local;
            vistaMundo.AgregarChat("Por internet tus amigos entran con la dirección " + direccion + " (Esc → Copiar invitación).", verde);
            if (!cgnat) vistaMundo.AgregarChat("Nota: desde tu propia red usa " + local + "; la IP pública suele funcionar solo desde fuera.", amarillo);
        }

        private void AlPerderConexion()
        {
            if (conexionPerdida) return;
            conexionPerdida = true;
            vistaMundo.LiberarMouse();
            string motivo = red != null && red.MotivoDesconexion != null ? red.MotivoDesconexion : "se perdió la conexión con el anfitrión.";
            BeginInvoke(new Action(() =>
            {
                Mensajes.Advertencia("Saliste de la partida: " + motivo);
                Close();
            }));
        }

        private void mnuAbrirRed_Click(object sender, EventArgs e)
        {
            AbrirRed();
        }

        private void mnuChat_Click(object sender, EventArgs e)
        {
            vistaMundo.MostrarChat();
        }

        private void vistaMundo_AccionSolicitada(object sender, AccionEventArgs e)
        {
            switch (e.Accion)
            {
                case AccionEventArgs.Guardar: mnuGuardar_Click(sender, e); break;
                case AccionEventArgs.AbrirRed: AbrirRed(); break;
                case AccionEventArgs.Controles: mnuControles_Click(sender, e); break;
                case AccionEventArgs.Salir: Close(); break;
            }
        }

        // ------------------------------------------------------------------ guardar y salir
        private bool Guardar()
        {
            if (esInvitado) return false;
            try
            {
                Cursor = Cursors.WaitCursor;
                vistaMundo.GuardarPosicionEnMundo();
                new MundoService().Guardar(juego.Mundo, juego.Inventario);
                juego.HayCambios = false;
                ActualizarEstado();
                return true;
            }
            catch (Exception ex)
            {
                vistaMundo.LiberarMouse();
                Mensajes.Error("No fue posible guardar. Se conservó el estado anterior.\n\n" + ex.Message);
                return false;
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void mnuGuardar_Click(object sender, EventArgs e)
        {
            if (Guardar()) vistaMundo.Avisar("Progreso guardado correctamente.", Color.FromArgb(160, 240, 160));
        }

        private void mnuInventario_Click(object sender, EventArgs e)
        {
            vistaMundo.LiberarMouse();
            using (FrmInventario dialogo = new FrmInventario(juego.Mundo.Nombre, juego.Inventario))
            {
                dialogo.ShowDialog(this);
            }
            vistaMundo.Focus();
        }

        private void mnuReaparecer_Click(object sender, EventArgs e)
        {
            vistaMundo.Reaparecer();
        }

        private void mnuVuelo_Click(object sender, EventArgs e)
        {
            vistaMundo.Volando = !vistaMundo.Volando;
            ActualizarEstado();
        }

        private void mnuDepuracion_Click(object sender, EventArgs e)
        {
            vistaMundo.MostrarDepuracion = mnuDepuracion.Checked;
        }

        private void mnuPantallaCompleta_Click(object sender, EventArgs e)
        {
            pantallaCompleta = !pantallaCompleta;
            SuspendLayout();
            if (pantallaCompleta)
            {
                estadoAnterior = WindowState;
                mnsPrincipal.Visible = false;
                stsEstado.Visible = false;
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Normal;
                WindowState = FormWindowState.Maximized;
            }
            else
            {
                FormBorderStyle = FormBorderStyle.Sizable;
                WindowState = estadoAnterior;
                mnsPrincipal.Visible = true;
                stsEstado.Visible = true;
            }
            ResumeLayout();
            mnuPantallaCompleta.Checked = pantallaCompleta;
            vistaMundo.Focus();
        }

        private void mnuControles_Click(object sender, EventArgs e)
        {
            vistaMundo.LiberarMouse();
            Mensajes.Informacion("Controles de BlockForge:\n\n" +
                "Clic en la vista: capturar el mouse (Esc abre el menú de pausa)\n" +
                "Mouse: mirar alrededor\n" +
                "W, A, S, D: moverse  ·  Shift: correr\n" +
                "Espacio: saltar o nadar\n" +
                "F: activar o desactivar el vuelo (Espacio sube, Shift baja)\n\n" +
                "Clic izquierdo: romper y recolectar un bloque\n" +
                "Clic derecho: colocar el bloque activo (descuenta una unidad)\n" +
                "Clic central: elegir el bloque apuntado\n" +
                "Teclas 1 a 9 o rueda del mouse: cambiar el bloque activo\n" +
                "E: inventario  ·  Ctrl + S: guardar\n\n" +
                "Multijugador: T o Enter abre el chat, Tab muestra los jugadores.\n" +
                "F5: cámara en tercera persona (para ver tu skin).\n" +
                "Abre tu mundo con «Abrir a la red» (menú de pausa o menú Multijugador), en tu red o por internet.\n" +
                "El anfitrión puede escribir /expulsar nombre en el chat.\n\n" +
                "R: volver al punto de aparición\n" +
                "F3: información de depuración  ·  F11: pantalla completa");
        }

        private void mnuSalir_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>RF-11 y RN-10: avisa si hay cambios sin guardar antes de salir.</summary>
        private void FrmMundo_FormClosing(object sender, FormClosingEventArgs e)
        {
            vistaMundo.LiberarMouse();
            if (esInvitado || !juego.HayCambios) return;
            string extra = red != null && red.CantidadJugadores > 1 ? "\n\nAl salir, los demás jugadores se desconectarán." : "";
            DialogResult respuesta = Mensajes.PreguntarGuardar(
                "Hay cambios sin guardar en «" + juego.Mundo.Nombre + "».\n\n¿Desea guardarlos antes de salir?" + extra);
            if (respuesta == DialogResult.Cancel) e.Cancel = true;
            else if (respuesta == DialogResult.Yes && !Guardar()) e.Cancel = true;
        }

        private void FrmMundo_FormClosed(object sender, FormClosedEventArgs e)
        {
            vistaMundo.DesconectarRed();
            if (red != null)
            {
                red.ConexionPerdida -= AlPerderConexion;
                red.Dispose();
                red = null;
            }
            if (upnp != null && puertoUpnp != 0)
            {
                // Cierra el reenvío del router (sin esperar más de dos segundos).
                int puerto = puertoUpnp;
                Upnp router = upnp;
                try { System.Threading.Tasks.Task.Run(() => router.CerrarPuerto(puerto)).Wait(2000); } catch (Exception) { }
                puertoUpnp = 0;
            }
        }
    }
}
