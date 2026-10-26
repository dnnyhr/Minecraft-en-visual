using System;
using System.Windows.Forms;
using BlockForge.Helpers;
using BlockForge.Models;
using BlockForge.Services;

namespace BlockForge.Forms
{
    /// <summary>Configuración general (RF-15, RN-05 y RN-11).</summary>
    public partial class FrmConfiguracion : Form
    {
        public FrmConfiguracion()
        {
            InitializeComponent();
        }

        private void FrmConfiguracion_Load(object sender, EventArgs e)
        {
            Configuracion cfg = Sesion.Configuracion ?? Configuracion.PorDefecto();
            MostrarValores(cfg);
            lblModificacion.Text = "Última modificación: " + cfg.FechaModificacion.ToString("dd/MM/yyyy HH:mm") +
                (string.IsNullOrEmpty(cfg.UsuarioModifica) ? " (valores iniciales)" : " por " + cfg.UsuarioModifica);
        }

        private static void Asignar(TrackBar barra, int valor)
        {
            barra.Value = Math.Max(barra.Minimum, Math.Min(barra.Maximum, valor));
        }

        private static void Asignar(NumericUpDown campo, int valor)
        {
            campo.Value = Math.Max(campo.Minimum, Math.Min(campo.Maximum, valor));
        }

        private void MostrarValores(Configuracion cfg)
        {
            Asignar(trkDistancia, cfg.DistanciaVision);
            Asignar(trkCampo, cfg.CampoVision);
            Asignar(trkSensibilidad, cfg.Sensibilidad);
            Asignar(nudAncho, cfg.AnchoPredeterminado);
            Asignar(nudLargo, cfg.LargoPredeterminado);
            Asignar(nudAlto, cfg.AltoPredeterminado);
            ActualizarEtiquetas();
        }

        private void ActualizarEtiquetas()
        {
            lblValorDistancia.Text = trkDistancia.Value + " bloques";
            lblValorCampo.Text = trkCampo.Value + "°";
            lblValorSensibilidad.Text = trkSensibilidad.Value.ToString();
        }

        private void trk_ValueChanged(object sender, EventArgs e)
        {
            ActualizarEtiquetas();
        }

        private void btnRestablecer_Click(object sender, EventArgs e)
        {
            MostrarValores(Configuracion.PorDefecto());
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            Configuracion nueva = new Configuracion
            {
                DistanciaVision = trkDistancia.Value,
                CampoVision = trkCampo.Value,
                Sensibilidad = trkSensibilidad.Value,
                AnchoPredeterminado = (int)nudAncho.Value,
                LargoPredeterminado = (int)nudLargo.Value,
                AltoPredeterminado = (int)nudAlto.Value
            };
            try
            {
                new ConfiguracionService().Guardar(nueva);
                Mensajes.Informacion("La configuración se guardó correctamente. Se aplicará al abrir el siguiente mundo.");
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (ExcepcionValidacion ex)
            {
                Mensajes.Advertencia(ex.Message);
            }
            catch (Exception ex)
            {
                Mensajes.Error("No fue posible guardar la configuración.\n\n" + ex.Message);
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
