using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BlockForge.Forms;

namespace BlockForge.Controls
{
    /// <summary>
    /// Botón grande del menú principal: ícono de bloque, título, descripción y franja de color.
    /// Se ilumina al pasar el mouse y se puede usar con el teclado (Tab y Enter).
    /// </summary>
    public class TarjetaMenu : Button
    {
        private string descripcion = string.Empty;
        private string bloque = "Pasto";
        private Color acento = Color.FromArgb(76, 175, 80);
        private bool encima, presionado;

        public TarjetaMenu()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            Cursor = Cursors.Hand;
            Size = new Size(368, 78);
            Font = new Font("Segoe UI", 9.75F);
        }

        [Category("Apariencia")]
        [DefaultValue("")]
        public string Descripcion
        {
            get { return descripcion; }
            set { descripcion = value ?? string.Empty; Invalidate(); }
        }

        /// <summary>Nombre del bloque que se dibuja como ícono (Pasto, Madera, Piedra...).</summary>
        [Category("Apariencia")]
        [DefaultValue("Pasto")]
        public string Bloque
        {
            get { return bloque; }
            set { bloque = value; Invalidate(); }
        }

        [Category("Apariencia")]
        public Color Acento
        {
            get { return acento; }
            set { acento = value; Invalidate(); }
        }

        private bool ShouldSerializeAcento()
        {
            return acento != Color.FromArgb(76, 175, 80);
        }

        protected override void OnMouseEnter(EventArgs e) { encima = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { encima = false; presionado = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { presionado = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { presionado = false; Invalidate(); base.OnMouseUp(e); }
        protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
        protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent != null ? Parent.BackColor : Color.FromArgb(30, 31, 34));
            Rectangle r = new Rectangle(0, 0, Width - 1, Height - 1);
            Color fondo = !Enabled ? Color.FromArgb(40, 42, 46) : presionado ? Color.FromArgb(30, 32, 36) : encima ? Color.FromArgb(54, 57, 64) : Color.FromArgb(43, 45, 50);
            using (GraphicsPath forma = Redondeado(r, 8))
            using (SolidBrush pincel = new SolidBrush(fondo))
            using (Pen borde = new Pen(encima || Focused ? Color.FromArgb(140, acento) : Color.FromArgb(62, 65, 72)))
            {
                g.FillPath(pincel, forma);
                g.DrawPath(borde, forma);
            }
            using (GraphicsPath franja = Redondeado(new Rectangle(0, 0, 10, Height - 1), 8))
            using (SolidBrush pincel = new SolidBrush(Enabled ? acento : Color.Gray))
            {
                g.SetClip(new Rectangle(0, 0, 5, Height));
                g.FillPath(pincel, franja);
                g.ResetClip();
            }

            float k = DeviceDpi / 96f; // las medidas están pensadas a 96 ppp
            int icono = Math.Min((int)(48 * k), Height - (int)(24 * k));
            int desplazamiento = presionado ? (int)Math.Max(1, k) : 0;
            int margen = (int)(20 * k);
            Dibujo.IconoBloque(g, new Rectangle(margen, (Height - icono) / 2 + desplazamiento, icono, icono), bloque);

            int xTexto = margen + icono + (int)(16 * k);
            Color colorTitulo = Enabled ? Color.White : Color.FromArgb(130, 130, 130);
            using (Font titulo = new Font("Segoe UI Semibold", 12F))
            {
                int altoTitulo = TextRenderer.MeasureText(g, "Ag", titulo).Height;
                int yTitulo = (int)(10 * k) + desplazamiento;
                TextRenderer.DrawText(g, Text, titulo, new Rectangle(xTexto, yTitulo, Width - xTexto - (int)(12 * k), altoTitulo), colorTitulo,
                                      TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.EndEllipsis);
                int yDescripcion = yTitulo + altoTitulo;
                TextRenderer.DrawText(g, descripcion, Font, new Rectangle(xTexto, yDescripcion, Width - xTexto - (int)(12 * k), Height - yDescripcion - (int)(4 * k)),
                                      Color.FromArgb(178, 182, 190), TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis);
            }
        }

        private static GraphicsPath Redondeado(Rectangle r, int radio)
        {
            GraphicsPath p = new GraphicsPath();
            int d = radio * 2;
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
    }
}
