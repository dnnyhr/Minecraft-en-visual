using System;
using System.Windows.Forms;
using BlockForge.Helpers;
using BlockForge.Models;
using BlockForge.Services;

namespace BlockForge.Forms
{
    /// <summary>Gestión de partidas del Administrador (RF-12, RF-13, RF-14 y RN-09).</summary>
    public partial class FrmGestionPartidas : Form
    {
        private readonly MundoService servicio = new MundoService();

        public FrmGestionPartidas()
        {
            InitializeComponent();
            Estilos.Tabla(dgvPartidas);
        }

        private void FrmGestionPartidas_Load(object sender, EventArgs e)
        {
            dgvPartidas.AutoGenerateColumns = false;
            dgvPartidas.Columns.Add(FrmCargarMundo.Columna("IdMundo", "ID", 8));
            dgvPartidas.Columns.Add(FrmCargarMundo.Columna("Nombre", "Mundo", 26));
            dgvPartidas.Columns.Add(FrmCargarMundo.Columna("Usuario", "Usuario", 16));
            dgvPartidas.Columns.Add(FrmCargarMundo.Columna("Dimensiones", "Dimensiones", 15));
            DataGridViewTextBoxColumn bloques = FrmCargarMundo.Columna("TotalBloques", "Bloques", 11);
            bloques.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvPartidas.Columns.Add(bloques);
            dgvPartidas.Columns.Add(FrmCargarMundo.Columna("UltimoGuardadoTexto", "Último guardado", 24));
            Cargar();
        }

        private void Cargar()
        {
            try
            {
                dgvPartidas.DataSource = servicio.ListarTodos(txtBuscar.Text);
            }
            catch (Exception ex)
            {
                Mensajes.Error("No fue posible consultar las partidas.\n\n" + ex.Message);
            }
            ActualizarBotones();
        }

        private ResumenMundo Seleccionada
        {
            get { return dgvPartidas.CurrentRow != null ? dgvPartidas.CurrentRow.DataBoundItem as ResumenMundo : null; }
        }

        private void ActualizarBotones()
        {
            bool haySeleccion = Seleccionada != null;
            btnEliminar.Enabled = haySeleccion;
            btnRestablecer.Enabled = haySeleccion;
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            Cargar();
        }

        private void dgvPartidas_SelectionChanged(object sender, EventArgs e)
        {
            ActualizarBotones();
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            ResumenMundo partida = Seleccionada;
            if (partida == null) return;
            if (!Mensajes.Confirmar("¿Desea eliminar la partida «" + partida.Nombre + "»?\nEsta acción no se puede deshacer.",
                                    "Confirmar eliminación")) return;
            try
            {
                servicio.Eliminar(partida.IdMundo);
                Mensajes.Informacion("La partida se eliminó correctamente.");
            }
            catch (Exception ex)
            {
                Mensajes.Error("No fue posible eliminar la partida.\n\n" + ex.Message);
            }
            Cargar();
        }

        private void btnRestablecer_Click(object sender, EventArgs e)
        {
            ResumenMundo partida = Seleccionada;
            if (partida == null) return;
            if (!Mensajes.Confirmar("¿Desea restablecer la partida «" + partida.Nombre + "»?\n" +
                                    "El terreno volverá a su estado inicial y el inventario quedará en cero.",
                                    "Confirmar restablecimiento")) return;
            try
            {
                Cursor = Cursors.WaitCursor;
                servicio.Restablecer(partida.IdMundo);
                Mensajes.Informacion("La partida se restableció correctamente.");
            }
            catch (Exception ex)
            {
                Mensajes.Error("No fue posible restablecer la partida.\n\n" + ex.Message);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
            Cargar();
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
