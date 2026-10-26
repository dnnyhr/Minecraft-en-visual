using System.Drawing;

namespace BlockForge.Models
{
    /// <summary>Tipo del catálogo: bloque, planta, líquido, comida o material.</summary>
    public class TipoBloque
    {
        public const string Bloque = "Bloque", Planta = "Planta", Liquido = "Liquido", Comida = "Comida", Material = "Material",
                            Herramienta = "Herramienta";

        public int IdTipoBloque { get; set; }
        public string Nombre { get; set; }
        public string ColorHex { get; set; }
        public bool EsSolido { get; set; }
        public bool EsRecolectable { get; set; }
        public string Categoria { get; set; }
        /// <summary>Puntos de hambre que recupera al comerlo (0 si no es comida).</summary>
        public int Alimento { get; set; }
        /// <summary>Luz que emite (0 a 15): antorchas, lava, lámparas.</summary>
        public int Luz { get; set; }

        public TipoBloque()
        {
            Categoria = Bloque;
        }

        public Color Color
        {
            get { return ColorTranslator.FromHtml(ColorHex); }
        }

        /// <summary>Se puede colocar en el mundo (bloques y plantas).</summary>
        public bool EsColocable
        {
            get { return EsRecolectable && (Categoria == Bloque || Categoria == Planta); }
        }

        public bool EsComida
        {
            get { return Categoria == Comida && Alimento > 0; }
        }

        /// <summary>Daño al golpear con este objeto en la mano (espadas; la mano vacía o un bloque hace 2).</summary>
        public float Dano
        {
            get
            {
                switch (Nombre)
                {
                    case "Espada de madera": return 4f;
                    case "Espada de piedra": return 5f;
                    case "Espada de hierro": return 6f;
                    case "Espada de diamante": return 7f;
                    default: return 2f;
                }
            }
        }

        public override string ToString()
        {
            return Nombre;
        }
    }
}
