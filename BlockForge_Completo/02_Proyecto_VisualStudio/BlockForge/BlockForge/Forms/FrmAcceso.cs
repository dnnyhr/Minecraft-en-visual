using System;
using System.Drawing;
using System.Windows.Forms;
using BlockForge.Helpers;
using BlockForge.Models;
using BlockForge.Services;

namespace BlockForge.Forms
{
    /// <summary>Inicio de sesión (RF-01, RN-01 y RN-02).</summary>
    public partial class FrmAcceso : Form
    {
        private readonly AutenticacionService autenticacion = new AutenticacionService();

        public FrmAcceso()
        {
            InitializeComponent();
        }

        private void FrmAcceso_Load(object sender, EventArgs e)
        {
            txtUsuario.Focus();
        }

        private void pnlLogo_Paint(object sender, PaintEventArgs e)
        {
            // Cabecera: paisaje de bloques con el logotipo y el título.
            Graphics g = e.Graphics;
            float k = Dibujo.Escala(pnlLogo);
            Dibujo.Paisaje(g, pnlLogo.ClientRectangle, k);
            Dibujo.Logotipo(g, new Rectangle((int)(30 * k), (int)(24 * k), (int)(60 * k), (int)(60 * k)));
            using (Font titulo = new Font("Segoe UI", 24F, FontStyle.Bold))
            using (Font subtitulo = new Font("Segoe UI Semibold", 9.75F))
            {
                Dibujo.TextoConSombra(g, "BlockForge", titulo, Color.White, new PointF(98 * k, 14 * k));
                Dibujo.TextoConSombra(g, "Construye, explora y juega con tus amigos", subtitulo, Color.FromArgb(235, 240, 250), new PointF(103 * k, 62 * k));
            }
        }

        private void btnIngresar_Click(object sender, EventArgs e)
        {
            lblMensaje.Text = string.Empty;
            ResultadoAcceso resultado;
            try
            {
                Cursor = Cursors.WaitCursor;
                resultado = autenticacion.Autenticar(txtUsuario.Text, txtContrasena.Text);
            }
            catch (Exception ex)
            {
                Mensajes.Error("No fue posible conectar con la base de datos.\n\n" + ex.Message);
                return;
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            switch (resultado)
            {
                case ResultadoAcceso.Correcto:
                    AbrirMenu();
                    break;
                case ResultadoAcceso.CamposVacios:
                    lblMensaje.Text = "Ingrese el usuario y la contraseña.";
                    (string.IsNullOrWhiteSpace(txtUsuario.Text) ? (Control)txtUsuario : txtContrasena).Focus();
                    break;
                case ResultadoAcceso.CredencialesInvalidas:
                    lblMensaje.Text = "Usuario o contraseña incorrectos. Intentos restantes: " + autenticacion.IntentosRestantes + ".";
                    txtContrasena.SelectAll();
                    txtContrasena.Focus();
                    break;
                case ResultadoAcceso.UsuarioInactivo:
                    lblMensaje.Text = "La cuenta está desactivada. Consulte al Administrador.";
                    break;
                case ResultadoAcceso.Bloqueado:
                    IniciarBloqueo();
                    break;
            }
        }

        private void IniciarBloqueo()
        {
            btnIngresar.Enabled = false;
            ActualizarMensajeBloqueo();
            tmrBloqueo.Start();
        }

        private void tmrBloqueo_Tick(object sender, EventArgs e)
        {
            if (autenticacion.SegundosRestantesBloqueo <= 0)
            {
                tmrBloqueo.Stop();
                btnIngresar.Enabled = true;
                lblMensaje.Text = string.Empty;
                txtContrasena.Focus();
                return;
            }
            ActualizarMensajeBloqueo();
        }

        private void ActualizarMensajeBloqueo()
        {
            lblMensaje.Text = "Demasiados intentos fallidos. Espere " + autenticacion.SegundosRestantesBloqueo + " segundos.";
        }

        private void AbrirMenu()
        {
            try
            {
                new ConfiguracionService().Cargar();
            }
            catch (Exception ex)
            {
                Sesion.Configuracion = Configuracion.PorDefecto();
                Mensajes.Advertencia("No se pudo leer la configuración; se usarán los valores predeterminados.\n\n" + ex.Message);
            }

            Hide();
            bool cerrarSesion;
            using (FrmMenuPrincipal menu = new FrmMenuPrincipal())
            {
                menu.ShowDialog();
                cerrarSesion = menu.CerrarSesion;
            }

            if (cerrarSesion)
            {
                Sesion.Cerrar();
                txtContrasena.Clear();
                lblMensaje.Text = string.Empty;
                Show();
                txtContrasena.Focus();
            }
            else
            {
                Close();
            }
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
