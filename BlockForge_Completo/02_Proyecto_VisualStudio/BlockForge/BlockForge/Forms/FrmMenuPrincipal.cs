using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BlockForge.Helpers;
using BlockForge.Models;
using BlockForge.Services;

namespace BlockForge.Forms
{
    /// <summary>Menú principal: muestra las funciones según el rol (RF-02, RN-03).</summary>
    public partial class FrmMenuPrincipal : Form
    {
        /// <summary>Indica si el usuario cerró sesión (true) o cerró la aplicación (false).</summary>
        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public bool CerrarSesion { get; private set; }

        public FrmMenuPrincipal()
        {
            InitializeComponent();
        }

        private void FrmMenuPrincipal_Load(object sender, EventArgs e)
        {
            MostrarUsuario();
            ActualizarAvatar();

            bool admin = Sesion.EsAdministrador;
            lblAdministracion.Visible = admin;
            btnGestionPartidas.Visible = admin;
            btnConfiguracion.Visible = admin;
            lblNotaAdmin.Visible = admin;
            if (!admin)
            {
                // Sin la sección de administración, el formulario se reduce para no dejar espacio vacío.
                ClientSize = new System.Drawing.Size(ClientSize.Width, btnUnirse.Bottom + 26 + stsEstado.Height);
            }
            ActualizarEstado();
        }

        private void MostrarUsuario()
        {
            Usuario usuario = Sesion.UsuarioActual;
            lblUsuario.Text = usuario.NombreUsuario + Environment.NewLine + "Rol: " + usuario.Rol;
        }

        private void btnCuenta_Click(object sender, EventArgs e)
        {
            using (FrmCuenta dialogo = new FrmCuenta())
            {
                if (dialogo.ShowDialog(this) == DialogResult.OK) MostrarUsuario();
            }
        }

        /// <summary>La cara de la skin del usuario como ícono del botón "Mi skin".</summary>
        private void ActualizarAvatar()
        {
            System.Drawing.Image anterior = btnSkin.Image;
            int tam = (int)(24 * Dibujo.Escala(btnSkin));
            Skin skin = Sesion.UsuarioActual != null ? Sesion.UsuarioActual.Skin : null;
            try
            {
                btnSkin.Image = skin != null ? SkinService.Cara(skin.Png, tam) : null;
            }
            catch (Exception)
            {
                btnSkin.Image = null;
            }
            if (anterior != null) anterior.Dispose();
        }

        private void btnSkin_Click(object sender, EventArgs e)
        {
            using (FrmSkin dialogo = new FrmSkin())
            {
                dialogo.ShowDialog(this);
            }
            ActualizarAvatar();
        }

        /// <summary>Cabecera: paisaje de bloques con el logotipo y el título.</summary>
        private void pnlEncabezado_Paint(object sender, PaintEventArgs e)
        {
            float k = Dibujo.Escala(pnlEncabezado);
            Dibujo.Paisaje(e.Graphics, pnlEncabezado.ClientRectangle, k);
            System.Drawing.Rectangle zonaUsuario = System.Drawing.Rectangle.Union(lblUsuario.Bounds, btnCerrarSesion.Bounds);
            zonaUsuario = System.Drawing.Rectangle.Union(zonaUsuario, btnSkin.Bounds);
            zonaUsuario = System.Drawing.Rectangle.Union(zonaUsuario, btnCuenta.Bounds);
            zonaUsuario.Inflate((int)(8 * k), (int)(6 * k));
            using (System.Drawing.SolidBrush fondo = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(120, 20, 22, 26)))
            {
                e.Graphics.FillRectangle(fondo, zonaUsuario);
            }
            Dibujo.Logotipo(e.Graphics, new System.Drawing.Rectangle((int)(26 * k), (int)(30 * k), (int)(72 * k), (int)(72 * k)));
            using (System.Drawing.Font titulo = new System.Drawing.Font("Segoe UI", 28F, System.Drawing.FontStyle.Bold))
            using (System.Drawing.Font subtitulo = new System.Drawing.Font("Segoe UI Semibold", 10.5F))
            {
                Dibujo.TextoConSombra(e.Graphics, "BlockForge", titulo, System.Drawing.Color.White, new System.Drawing.PointF(104 * k, 18 * k));
                Dibujo.TextoConSombra(e.Graphics, "Construye, explora y juega con tus amigos", subtitulo,
                                      System.Drawing.Color.FromArgb(235, 240, 250), new System.Drawing.PointF(110 * k, 76 * k));
            }
        }

        private void btnHospedar_Click(object sender, EventArgs e)
        {
            int idMundo = 0;
            using (FrmCargarMundo dialogo = new FrmCargarMundo())
            {
                dialogo.Text = "BlockForge — Hospedar partida";
                if (dialogo.ShowDialog(this) == DialogResult.OK) idMundo = dialogo.IdMundoSeleccionado;
            }
            if (idMundo == 0) return;
            Red.OpcionesPartida opciones;
            using (FrmOpcionesPartida dialogo = new FrmOpcionesPartida())
            {
                if (dialogo.ShowDialog(this) != DialogResult.OK) return;
                opciones = dialogo.Opciones;
            }
            try
            {
                Inventario inventario;
                Mundo mundo = new MundoService().Abrir(idMundo, out inventario);
                Hide();
                using (FrmMundo juego = new FrmMundo(mundo, inventario) { HospedarAlIniciar = opciones })
                {
                    juego.ShowDialog();
                }
                Show();
                ActualizarEstado();
            }
            catch (Exception ex)
            {
                Show();
                Mensajes.Error("No fue posible abrir el mundo.\n\n" + ex.Message);
            }
        }

        private void btnUnirse_Click(object sender, EventArgs e)
        {
            Red.PartidaRed partida = null;
            Mundo mundo = null;
            using (FrmUnirse dialogo = new FrmUnirse())
            {
                if (dialogo.ShowDialog(this) == DialogResult.OK)
                {
                    partida = dialogo.Partida;
                    mundo = dialogo.MundoRecibido;
                }
            }
            if (partida == null) return;
            Hide();
            using (FrmMundo juego = new FrmMundo(mundo, partida))
            {
                juego.ShowDialog();
            }
            Show();
        }

        private void ActualizarEstado()
        {
            try
            {
                List<ResumenMundo> mundos = new MundoService().ListarMisMundos();
                string texto = mundos.Count + (mundos.Count == 1 ? " mundo guardado" : " mundos guardados");
                if (mundos.Count > 0 && mundos[0].UltimoGuardado.HasValue)
                {
                    texto += "   |   Último guardado: " + mundos[0].UltimoGuardadoTexto;
                }
                lblEstado.Text = texto;
            }
            catch (Exception ex)
            {
                lblEstado.Text = "No se pudo consultar la base de datos: " + ex.Message;
            }
        }

        private void btnNuevoMundo_Click(object sender, EventArgs e)
        {
            Mundo mundo = null;
            using (FrmNuevoMundo dialogo = new FrmNuevoMundo())
            {
                if (dialogo.ShowDialog(this) == DialogResult.OK) mundo = dialogo.MundoCreado;
            }
            if (mundo != null) AbrirMundo(mundo, new Inventario());
        }

        private void btnCargarMundo_Click(object sender, EventArgs e)
        {
            int idMundo = 0;
            using (FrmCargarMundo dialogo = new FrmCargarMundo())
            {
                if (dialogo.ShowDialog(this) == DialogResult.OK) idMundo = dialogo.IdMundoSeleccionado;
            }
            if (idMundo == 0) return;

            try
            {
                Inventario inventario;
                Mundo mundo = new MundoService().Abrir(idMundo, out inventario);
                AbrirMundo(mundo, inventario);
            }
            catch (Exception ex)
            {
                Mensajes.Error("No fue posible cargar el mundo.\n\n" + ex.Message);
            }
        }

        private void AbrirMundo(Mundo mundo, Inventario inventario)
        {
            Hide();
            using (FrmMundo juego = new FrmMundo(mundo, inventario))
            {
                juego.ShowDialog();
            }
            Show();
            ActualizarEstado();
        }

        private void btnGestionPartidas_Click(object sender, EventArgs e)
        {
            using (FrmGestionPartidas dialogo = new FrmGestionPartidas())
            {
                dialogo.ShowDialog(this);
            }
            ActualizarEstado();
        }

        private void btnConfiguracion_Click(object sender, EventArgs e)
        {
            using (FrmConfiguracion dialogo = new FrmConfiguracion())
            {
                dialogo.ShowDialog(this);
            }
        }

        private void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            CerrarSesion = true;
            Close();
        }
    }
}
