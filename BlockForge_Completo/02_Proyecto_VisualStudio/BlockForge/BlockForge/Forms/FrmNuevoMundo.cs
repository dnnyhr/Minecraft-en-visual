using System;
using System.Windows.Forms;
using BlockForge.Helpers;
using BlockForge.Models;
using BlockForge.Services;

namespace BlockForge.Forms
{
    /// <summary>Registro de un mundo nuevo (RF-03, RF-04, RN-04 y RN-05).</summary>
    public partial class FrmNuevoMundo : Form
    {
        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Mundo MundoCreado { get; private set; }

        public FrmNuevoMundo()
        {
            InitializeComponent();
        }

        private void FrmNuevoMundo_Load(object sender, EventArgs e)
        {
            Configuracion cfg = Sesion.Configuracion ?? Configuracion.PorDefecto();
            nudAncho.Value = Math.Max(nudAncho.Minimum, Math.Min(nudAncho.Maximum, cfg.AnchoPredeterminado));
            nudLargo.Value = Math.Max(nudLargo.Minimum, Math.Min(nudLargo.Maximum, cfg.LargoPredeterminado));
            nudAlto.Value = Math.Max(nudAlto.Minimum, Math.Min(nudAlto.Maximum, cfg.AltoPredeterminado));
        }

        private void txtNombre_TextChanged(object sender, EventArgs e)
        {
            if (lblErrorNombre.Text.Length > 0)
            {
                lblErrorNombre.Text = string.Empty;
                txtNombre.BackColor = System.Drawing.Color.White;
            }
        }

        private void btnCrear_Click(object sender, EventArgs e)
        {
            string mensaje;
            if (!Validaciones.NombreMundoValido(txtNombre.Text, out mensaje))
            {
                MostrarError(mensaje);
                return;
            }

            try
            {
                Cursor = Cursors.WaitCursor;
                MundoCreado = new MundoService().Crear(txtNombre.Text, (int)nudAncho.Value, (int)nudLargo.Value, (int)nudAlto.Value, chkLagos.Checked);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (ExcepcionValidacion ex)
            {
                MostrarError(ex.Message);
            }
            catch (Exception ex)
            {
                Mensajes.Error("No fue posible crear el mundo.\n\n" + ex.Message);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void MostrarError(string mensaje)
        {
            lblErrorNombre.Text = "⚠ " + mensaje;
            txtNombre.BackColor = System.Drawing.ColorTranslator.FromHtml("#FDECEA");
            txtNombre.Focus();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
