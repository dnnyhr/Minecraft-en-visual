using System;
using System.Windows.Forms;
using BlockForge.Red;

namespace BlockForge.Forms
{
    /// <summary>Opciones para abrir una partida: red local o internet, puerto, contraseña y cupo.</summary>
    public partial class FrmOpcionesPartida : Form
    {
        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public OpcionesPartida Opciones { get; private set; }

        public FrmOpcionesPartida()
        {
            InitializeComponent();
        }

        private void btnAbrir_Click(object sender, EventArgs e)
        {
            Opciones = new OpcionesPartida
            {
                Internet = rdoInternet.Checked,
                Tunel = rdoTunel.Checked,
                Pvp = chkPvp.Checked,
                Puerto = (int)nudPuerto.Value,
                MaximoJugadores = (int)nudMaximo.Value,
                Contrasena = txtContrasena.Text.Trim()
            };
            if ((Opciones.Internet || Opciones.Tunel) && Opciones.Contrasena.Length == 0 &&
                MessageBox.Show("La partida estará abierta a internet sin contraseña: cualquiera que conozca tu dirección podrá entrar.\n\n¿Continuar sin contraseña?",
                                "BlockForge", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            {
                txtContrasena.Focus();
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
