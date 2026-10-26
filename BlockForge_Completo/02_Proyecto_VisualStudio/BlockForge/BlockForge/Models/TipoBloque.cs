using System.Drawing;

namespace BlockForge.Models
{
    /// <summary>Material del catálogo (Pasto, Tierra, Piedra, Madera, Agua).</summary>
    public class TipoBloque
    {
        public int IdTipoBloque { get; set; }
        public string Nombre { get; set; }
        public string ColorHex { get; set; }
        public bool EsSolido { get; set; }
        public bool EsRecolectable { get; set; }

        public Color Color
        {
            get { return ColorTranslator.FromHtml(ColorHex); }
        }

        public override string ToString()
        {
            return Nombre;
        }
    }
}
