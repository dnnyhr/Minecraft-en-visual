using System;
using System.Windows.Forms;
using BlockForge.Helpers;
using BlockForge.Models;
using BlockForge.Services;

namespace BlockForge.Forms
{
    /// <summary>Consulta y apertura de los mundos guardados del usuario (RF-05).</summary>
    public partial class FrmCargarMundo : Form
    {
        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public int IdMundoSeleccionado { get; private set; }

        public FrmCargarMundo()
        {
            InitializeComponent();
            Estilos.Tabla(dgvMundos);
        }

        private void FrmCargarMundo_Load(object sender, EventArgs e)
        {
            dgvMundos.AutoGenerateColumns = false;
            dgvMundos.Columns.Add(Columna("Nombre", "Nombre", 45));
            dgvMundos.Columns.Add(Columna("Dimensiones", "Dimensiones", 25));
            dgvMundos.Columns.Add(Columna("UltimoGuardadoTexto", "Último guardado", 30));
            try
            {
                dgvMundos.DataSource = new MundoService().ListarMisMundos();
            }
            catch (Exception ex)
            {
                Mensajes.Error("No fue posible consultar los mundos.\n\n" + ex.Message);
            }
            btnAbrir.Enabled = dgvMundos.Rows.Count > 0;
            if (dgvMundos.Rows.Count == 0) lblDescripcion.Text = "Todavía no tienes mundos guardados. Crea uno desde el menú principal.";
        }

        internal static DataGridViewTextBoxColumn Columna(string propiedad, string encabezado, int peso)
        {
            return new DataGridViewTextBoxColumn
            {
                DataPropertyName = propiedad,
                HeaderText = encabezado,
                FillWeight = peso,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
        }

        private void dgvMundos_SelectionChanged(object sender, EventArgs e)
        {
            btnAbrir.Enabled = dgvMundos.CurrentRow != null;
        }

        private void dgvMundos_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) btnAbrir_Click(sender, e);
        }

        private void btnAbrir_Click(object sender, EventArgs e)
        {
            if (dgvMundos.CurrentRow == null) return;
            ResumenMundo seleccionado = (ResumenMundo)dgvMundos.CurrentRow.DataBoundItem;
            IdMundoSeleccionado = seleccionado.IdMundo;
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
