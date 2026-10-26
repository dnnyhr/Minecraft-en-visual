using System;
using System.Windows.Forms;
using BlockForge.Helpers;
using BlockForge.Models;
using BlockForge.Services;

namespace BlockForge.Forms
{
    /// <summary>Consulta del inventario del mundo abierto (RF-09).</summary>
    public partial class FrmInventario : Form
    {
        private readonly string nombreMundo;
        private readonly Inventario inventario;

        public FrmInventario(string nombreMundo, Inventario inventario)
        {
            InitializeComponent();
            this.nombreMundo = nombreMundo;
            this.inventario = inventario;
            Estilos.Tabla(dgvInventario);
        }

        private void FrmInventario_Load(object sender, EventArgs e)
        {
            lblMundo.Text = "Mundo: " + nombreMundo;
            DataGridViewTextBoxColumn color = new DataGridViewTextBoxColumn { HeaderText = "", FillWeight = 8 };
            dgvInventario.Columns.Add(color);
            dgvInventario.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tipo de bloque", FillWeight = 42 });
            DataGridViewTextBoxColumn cantidad = new DataGridViewTextBoxColumn { HeaderText = "Cantidad", FillWeight = 22 };
            cantidad.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvInventario.Columns.Add(cantidad);
            dgvInventario.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Recolectable", FillWeight = 28 });

            foreach (TipoBloque tipo in CatalogoBloques.Tipos)
            {
                int fila = dgvInventario.Rows.Add("", tipo.Nombre, inventario.ObtenerCantidad(tipo.IdTipoBloque),
                    tipo.EsRecolectable ? "Sí" : "No");
                dgvInventario.Rows[fila].Tag = tipo;
            }
            lblTotal.Text = "Total de bloques: " + inventario.Total();
            lblCapacidad.Text = "Capacidad máxima por tipo: " + Inventario.CapacidadMaxima;
            dgvInventario.ClearSelection();
        }

        private void dgvInventario_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex != 0 || e.RowIndex < 0) return;
            TipoBloque tipo = dgvInventario.Rows[e.RowIndex].Tag as TipoBloque;
            if (tipo == null) return;
            e.CellStyle.BackColor = tipo.Color;
            e.CellStyle.SelectionBackColor = tipo.Color;
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
