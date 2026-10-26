using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BlockForge.Helpers;
using BlockForge.Models;
using BlockForge.Red;

namespace BlockForge.Forms
{
    /// <summary>Busca partidas en la red local y se conecta a la elegida (o a una dirección escrita a mano).</summary>
    public partial class FrmUnirse : Form
    {
        private BuscadorPartidas buscador;
        private string ultimaLista = string.Empty;

        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public PartidaRed Partida { get; private set; }

        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Mundo MundoRecibido { get; private set; }

        public FrmUnirse()
        {
            InitializeComponent();
            Estilos.Tabla(dgvPartidas);
        }

        private string NombreJugador
        {
            get { return Sesion.UsuarioActual != null ? Sesion.UsuarioActual.NombreUsuario : "Jugador"; }
        }

        private void FrmUnirse_Load(object sender, EventArgs e)
        {
            dgvPartidas.AutoGenerateColumns = false;
            dgvPartidas.Columns.Add(FrmCargarMundo.Columna("Mundo", "Mundo", 30));
            dgvPartidas.Columns.Add(FrmCargarMundo.Columna("Anfitrion", "Anfitrión", 22));
            DataGridViewTextBoxColumn jugadores = FrmCargarMundo.Columna("Jugadores", "Jugadores", 20);
            jugadores.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvPartidas.Columns.Add(jugadores);
            dgvPartidas.Columns.Add(FrmCargarMundo.Columna("Acceso", "Acceso", 20));
            dgvPartidas.Columns.Add(FrmCargarMundo.Columna("Destino", "Dirección", 28));
            lblJugador.Text = "Entrarás como: " + NombreJugador + ". Puerto predeterminado: " + Protocolo.PuertoPredeterminado + ".";

            buscador = new BuscadorPartidas();
            if (buscador.Error != null)
            {
                lblDescripcion.Text = "No se pueden buscar partidas automáticamente; escribe la dirección del anfitrión.";
            }
            tmrBuscar.Start();
            ActualizarLista();
        }

        private void tmrBuscar_Tick(object sender, EventArgs e)
        {
            ActualizarLista();
        }

        private void ActualizarLista()
        {
            List<PartidaEncontrada> partidas = buscador.Vigentes();
            string firma = string.Join(";", partidas.Select(p => p.Destino + "/" + p.Mundo + "/" + p.Jugadores));
            if (firma == ultimaLista) return;
            ultimaLista = firma;

            string seleccion = dgvPartidas.CurrentRow != null && dgvPartidas.CurrentRow.DataBoundItem is PartidaEncontrada
                ? ((PartidaEncontrada)dgvPartidas.CurrentRow.DataBoundItem).Destino : null;
            dgvPartidas.DataSource = partidas;
            foreach (DataGridViewRow fila in dgvPartidas.Rows)
            {
                if (((PartidaEncontrada)fila.DataBoundItem).Destino == seleccion) fila.Selected = true;
            }
            if (seleccion == null) dgvPartidas.ClearSelection();
        }

        private void dgvPartidas_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvPartidas.SelectedRows.Count == 0) return;
            PartidaEncontrada p = dgvPartidas.SelectedRows[0].DataBoundItem as PartidaEncontrada;
            if (p != null) txtDireccion.Text = p.Destino;
        }

        private void dgvPartidas_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) btnUnirse_Click(sender, e);
        }

        /// <summary>
        /// Separa "host" o "host:puerto". Acepta una invitación pegada con texto alrededor
        /// (por ejemplo "Únete a 181.45.3.2:25570"): se toma la parte que parece una dirección.
        /// </summary>
        private static bool Interpretar(string texto, out string servidor, out int puerto)
        {
            servidor = (texto ?? string.Empty).Trim();
            // Invitación por túnel (Cloudflare o ngrok): una dirección https://... que se usa tal cual.
            System.Text.RegularExpressions.Match url = System.Text.RegularExpressions.Regex.Match(servidor, @"(?i)\b(https?|wss?)://[A-Za-z0-9.\-]+(:\d{1,5})?/?");
            if (!url.Success) url = System.Text.RegularExpressions.Regex.Match(servidor, @"(?i)\b[a-z0-9\-]+\.(trycloudflare\.com|ngrok-free\.app|ngrok\.app|ngrok\.io)\b(?!:)");
            if (url.Success && !servidor.Contains("tcp.ngrok"))
            {
                servidor = url.Value.Contains("://") ? url.Value : "https://" + url.Value;
                puerto = 0;
                return true;
            }
            System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(servidor, @"[A-Za-z0-9.\-]+:\d{1,5}");
            if (m.Success) servidor = m.Value;
            else if (servidor.Contains(" ")) servidor = servidor.Split(' ').Last(t => t.Length > 0);
            puerto = Protocolo.PuertoPredeterminado;
            if (servidor.Length == 0) return false;
            int dosPuntos = servidor.LastIndexOf(':');
            if (dosPuntos > 0 && servidor.IndexOf(':') == dosPuntos)
            {
                if (!int.TryParse(servidor.Substring(dosPuntos + 1), out puerto) || puerto < 1 || puerto > 65535) return false;
                servidor = servidor.Substring(0, dosPuntos);
            }
            return servidor.Length > 0;
        }

        private async void btnUnirse_Click(object sender, EventArgs e)
        {
            lblError.Text = string.Empty;
            string servidor;
            int puerto;
            if (!Interpretar(txtDireccion.Text, out servidor, out puerto))
            {
                lblError.Text = "Escribe o pega la dirección del anfitrión, por ejemplo 181.45.3.2:" + Protocolo.PuertoPredeterminado + ".";
                txtDireccion.Focus();
                return;
            }

            try
            {
                Cursor = Cursors.WaitCursor;
                btnUnirse.Enabled = false;
                lblError.Text = "Conectando con " + (puerto > 0 ? servidor + ":" + puerto : servidor) + "..." +
                                (puerto == 0 ? " Si el anfitrión acaba de abrir el túnel, puede tardar hasta un minuto." : "");
                Skin skin = Sesion.UsuarioActual != null ? Sesion.UsuarioActual.Skin : null;
                string clave = txtClave.Text;
                Mundo mundo = null;
                PartidaRed partida = await System.Threading.Tasks.Task.Run(() =>
                {
                    Mundo m;
                    PartidaRed p = PartidaRed.Unirse(servidor, puerto, NombreJugador, clave, skin, 15000, out m);
                    mundo = m;
                    return p;
                });
                if (IsDisposed || DialogResult == DialogResult.Cancel)
                {
                    partida.Dispose(); // se canceló mientras se conectaba
                    return;
                }
                Partida = partida;
                MundoRecibido = mundo;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                lblError.Text = ex.Message;
            }
            finally
            {
                Cursor = Cursors.Default;
                btnUnirse.Enabled = true;
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void FrmUnirse_FormClosed(object sender, FormClosedEventArgs e)
        {
            tmrBuscar.Stop();
            if (buscador != null) buscador.Dispose();
        }
    }
}
