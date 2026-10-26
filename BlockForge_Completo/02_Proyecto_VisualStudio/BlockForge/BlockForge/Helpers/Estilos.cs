using System.Drawing;
using System.Windows.Forms;

namespace BlockForge.Helpers
{
    /// <summary>Paleta y tipografía de la identidad visual (sección 6 del documento).</summary>
    public static class Estilos
    {
        public static readonly Color Principal = ColorTranslator.FromHtml("#2F6B3A");
        public static readonly Color PrincipalOscuro = ColorTranslator.FromHtml("#24542D");
        public static readonly Color PrincipalClaro = ColorTranslator.FromHtml("#E3EFE0");
        public static readonly Color Secundario = ColorTranslator.FromHtml("#8B5E34");
        public static readonly Color Exito = ColorTranslator.FromHtml("#2E7D32");
        public static readonly Color Advertencia = ColorTranslator.FromHtml("#F9A825");
        public static readonly Color Error = ColorTranslator.FromHtml("#C62828");
        public static readonly Color Fondo = ColorTranslator.FromHtml("#F4F1EA");
        public static readonly Color Superficie = Color.White;
        public static readonly Color Neutro = ColorTranslator.FromHtml("#9E9E9E");
        public static readonly Color Texto = ColorTranslator.FromHtml("#212121");
        public static readonly Color TextoAuxiliar = ColorTranslator.FromHtml("#5F6368");
        public static readonly Color FilaAlterna = ColorTranslator.FromHtml("#FAF8F3");
        public static readonly Color Cielo = ColorTranslator.FromHtml("#CFE8F7");

        public static void BotonPrincipal(Button boton)
        {
            AplicarBoton(boton, Principal, Color.White, PrincipalOscuro);
        }

        public static void BotonSecundario(Button boton)
        {
            AplicarBoton(boton, Color.White, Texto, Neutro);
        }

        public static void BotonPeligro(Button boton)
        {
            AplicarBoton(boton, Error, Color.White, ColorTranslator.FromHtml("#9F1F1F"));
        }

        public static void BotonAdvertencia(Button boton)
        {
            AplicarBoton(boton, ColorTranslator.FromHtml("#FFF8E1"), ColorTranslator.FromHtml("#7A5A00"), Advertencia);
        }

        private static void AplicarBoton(Button boton, Color fondo, Color texto, Color borde)
        {
            boton.FlatStyle = FlatStyle.Flat;
            boton.BackColor = fondo;
            boton.ForeColor = texto;
            boton.FlatAppearance.BorderColor = borde;
            boton.FlatAppearance.BorderSize = 1;
            boton.Cursor = Cursors.Hand;
        }

        /// <summary>Tablas de solo lectura con encabezado verde y filas alternas.</summary>
        public static void Tabla(DataGridView tabla)
        {
            tabla.ReadOnly = true;
            tabla.AllowUserToAddRows = false;
            tabla.AllowUserToDeleteRows = false;
            tabla.AllowUserToResizeRows = false;
            tabla.MultiSelect = false;
            tabla.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            tabla.RowHeadersVisible = false;
            tabla.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            tabla.BackgroundColor = Color.White;
            tabla.BorderStyle = BorderStyle.FixedSingle;
            tabla.GridColor = ColorTranslator.FromHtml("#ECE8DF");
            tabla.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            tabla.EnableHeadersVisualStyles = false;
            tabla.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            tabla.ColumnHeadersHeight = 34;
            tabla.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            tabla.ColumnHeadersDefaultCellStyle.BackColor = Principal;
            tabla.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            tabla.ColumnHeadersDefaultCellStyle.SelectionBackColor = Principal;
            tabla.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            tabla.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
            tabla.DefaultCellStyle.ForeColor = Texto;
            tabla.DefaultCellStyle.SelectionBackColor = PrincipalClaro;
            tabla.DefaultCellStyle.SelectionForeColor = Texto;
            tabla.AlternatingRowsDefaultCellStyle.BackColor = FilaAlterna;
            tabla.RowTemplate.Height = 30;
        }
    }
}
