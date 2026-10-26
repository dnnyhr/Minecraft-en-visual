using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BlockForge.Helpers;
using BlockForge.Models;
using BlockForge.Services;

namespace BlockForge.Forms
{
    /// <summary>Elegir la skin del usuario: desde su cuenta de Minecraft o desde un archivo PNG.</summary>
    public partial class FrmSkin : Form
    {
        private Skin skin;
        private Bitmap imagen;

        public FrmSkin()
        {
            InitializeComponent();
        }

        private void FrmSkin_Load(object sender, EventArgs e)
        {
            Skin actual = Sesion.UsuarioActual != null ? Sesion.UsuarioActual.Skin : null;
            if (actual != null)
            {
                skin = new Skin { Png = actual.Png, Delgada = actual.Delgada, Origen = actual.Origen };
                if (actual.Origen != null && actual.Origen != "archivo") txtNombre.Text = actual.Origen;
            }
            Mostrar();
        }

        private void Mostrar()
        {
            if (imagen != null) imagen.Dispose();
            imagen = null;
            if (skin != null)
            {
                try { imagen = SkinService.Decodificar(skin.Png); } catch (Exception) { imagen = null; }
                chkDelgada.Checked = skin.Delgada;
                lblOrigen.Text = skin.Origen == "archivo" ? "Origen: archivo PNG" : "Origen: cuenta de Minecraft «" + skin.Origen + "»";
                if (imagen != null) lblOrigen.Text += Environment.NewLine + imagen.Width + "×" + imagen.Height + (imagen.Height * 2 == imagen.Width ? " (formato antiguo)" : "");
            }
            else
            {
                lblOrigen.Text = "Sin skin: se usa el aspecto predeterminado de BlockForge.";
            }
            btnQuitar.Enabled = skin != null;
            pnlVista.Invalidate();
        }

        private void Estado(string texto, bool error)
        {
            lblEstado.Text = texto;
            lblEstado.ForeColor = error ? Color.FromArgb(198, 40, 40) : Color.FromArgb(46, 125, 50);
        }

        private async void btnBuscar_Click(object sender, EventArgs e)
        {
            btnBuscar.Enabled = btnArchivo.Enabled = btnGuardar.Enabled = false;
            Cursor = Cursors.WaitCursor;
            Estado("Buscando la skin de «" + txtNombre.Text.Trim() + "» en los servidores de Minecraft...", false);
            try
            {
                skin = await SkinService.DescargarDeMinecraft(txtNombre.Text);
                Estado("¡Listo! Skin descargada. Presiona Guardar para usarla.", false);
                Mostrar();
            }
            catch (ExcepcionValidacion ex)
            {
                Estado(ex.Message, true);
            }
            catch (Exception ex)
            {
                Estado("No fue posible descargar la skin: " + ex.Message, true);
            }
            finally
            {
                if (!IsDisposed)
                {
                    btnBuscar.Enabled = btnArchivo.Enabled = btnGuardar.Enabled = true;
                    Cursor = Cursors.Default;
                }
            }
        }

        private void btnArchivo_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialogo = new OpenFileDialog { Filter = "Skin de Minecraft (*.png)|*.png", Title = "Elegir skin" })
            {
                if (dialogo.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    skin = SkinService.DesdeArchivo(dialogo.FileName, chkDelgada.Checked);
                    Estado("Skin cargada. Presiona Guardar para usarla.", false);
                    Mostrar();
                }
                catch (ExcepcionValidacion ex)
                {
                    Estado(ex.Message, true);
                }
                catch (Exception ex)
                {
                    Estado("No fue posible leer el archivo: " + ex.Message, true);
                }
            }
        }

        private void chkDelgada_CheckedChanged(object sender, EventArgs e)
        {
            if (skin != null) skin.Delgada = chkDelgada.Checked;
            pnlVista.Invalidate();
        }

        private void btnQuitar_Click(object sender, EventArgs e)
        {
            skin = null;
            Estado("Se usará el aspecto predeterminado. Presiona Guardar para confirmar.", false);
            Mostrar();
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                new SkinService().Guardar(skin);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (ExcepcionValidacion ex)
            {
                Estado(ex.Message, true);
            }
            catch (Exception ex)
            {
                Mensajes.Error("No fue posible guardar la skin.\n\n" + ex.Message);
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>Vista previa: el personaje de frente, con la segunda capa.</summary>
        private void pnlVista_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (imagen == null)
            {
                TextRenderer.DrawText(g, "Sin skin", Font, pnlVista.ClientRectangle, Color.FromArgb(180, 180, 180),
                                      TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            float k = imagen.Width / 64f;
            bool antigua = imagen.Height * 2 == imagen.Width;
            int brazo = chkDelgada.Checked ? 3 : 4;
            // Escala para que 32 píxeles de alto ocupen casi todo el panel.
            float p = Math.Min((pnlVista.Width - 20) / 16f, (pnlVista.Height - 20) / 32f);
            float ox = (pnlVista.Width - 16 * p) / 2f, oy = (pnlVista.Height - 32 * p) / 2f;

            Action<int, int, int, int, float, float, bool> parte = (u, v, w, h, x, y, espejo) =>
            {
                RectangleF destino = new RectangleF(ox + x * p, oy + y * p, w * p, h * p);
                RectangleF origen = new RectangleF(u * k, v * k, w * k, h * k);
                if (espejo)
                {
                    destino = new RectangleF(destino.Right, destino.Top, -destino.Width, destino.Height);
                }
                g.DrawImage(imagen, new[] { destino.Location, new PointF(destino.Right, destino.Top), new PointF(destino.Left, destino.Bottom) },
                            origen, GraphicsUnit.Pixel);
            };

            // Primera capa: piernas, cuerpo, brazos y cabeza (vistos de frente; la derecha del personaje a la izquierda).
            parte(4, 20, 4, 12, 4, 20, false);
            if (antigua) parte(4, 20, 4, 12, 8, 20, true); else parte(20, 52, 4, 12, 8, 20, false);
            parte(20, 20, 8, 12, 4, 8, false);
            parte(44, 20, brazo, 12, 4 - brazo, 8, false);
            if (antigua) parte(44, 20, brazo, 12, 12, 8, true); else parte(36, 52, brazo, 12, 12, 8, false);
            parte(8, 8, 8, 8, 4, 0, false);
            // Segunda capa
            if (!antigua)
            {
                parte(4, 36, 4, 12, 4, 20, false);
                parte(4, 52, 4, 12, 8, 20, false);
                parte(20, 36, 8, 12, 4, 8, false);
                parte(44, 36, brazo, 12, 4 - brazo, 8, false);
                parte(52, 52, brazo, 12, 12, 8, false);
            }
            if (SkinService.SombreroVisible(imagen)) parte(40, 8, 8, 8, 4, 0, false);
        }
    }
}
