using System;
using System.Windows.Forms;
using BlockForge.Helpers;
using BlockForge.Models;
using BlockForge.Services;

namespace BlockForge.Forms
{
    /// <summary>Mi cuenta: cambiar el nombre de usuario y la contraseña (pide la contraseña actual).</summary>
    public partial class FrmCuenta : Form
    {
        public FrmCuenta()
        {
            InitializeComponent();
        }

        private void FrmCuenta_Load(object sender, EventArgs e)
        {
            txtNombre.Text = Sesion.UsuarioActual != null ? Sesion.UsuarioActual.NombreUsuario : string.Empty;
            lblError.Text = string.Empty;
            txtNombre.SelectAll();
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            lblError.Text = string.Empty;
            try
            {
                string anterior = Sesion.UsuarioActual.NombreUsuario;
                new CuentaService().Actualizar(txtNombre.Text, txtActual.Text, txtNueva.Text, txtConfirmar.Text);
                string cambios = Sesion.UsuarioActual.NombreUsuario != anterior
                    ? "Ahora te llamas «" + Sesion.UsuarioActual.NombreUsuario + "»: usa ese nombre para iniciar sesión."
                    : string.Empty;
                if (txtNueva.Text.Length > 0) cambios += (cambios.Length > 0 ? "\n" : "") + "La contraseña se cambió.";
                Mensajes.Informacion(cambios);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (ExcepcionValidacion ex)
            {
                lblError.Text = ex.Message;
            }
            catch (Exception ex)
            {
                lblError.Text = "No se pudo guardar: " + ex.Message;
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
