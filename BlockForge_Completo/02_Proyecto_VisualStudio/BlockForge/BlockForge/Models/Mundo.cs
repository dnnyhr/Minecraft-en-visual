using System;

namespace BlockForge.Models
{
    /// <summary>
    /// Terreno tridimensional de bloques. Cada celda guarda el IdTipoBloque de su posición
    /// (0 significa aire). X recorre el ancho, Y la altura y Z el largo, como en ClassiCube.
    /// </summary>
    public class Mundo
    {
        public const int AnchoMinimo = 32, AnchoMaximo = 256;
        public const int LargoMinimo = 32, LargoMaximo = 256;
        public const int AltoMinimo = 32, AltoMaximo = 128;

        public int IdMundo { get; set; }
        public int IdUsuario { get; set; }
        public string Nombre { get; set; }
        public int Ancho { get; private set; }
        public int Alto { get; private set; }
        public int Largo { get; private set; }
        public int Semilla { get; set; }
        public bool ConLagos { get; set; }
        public DateTime FechaCreacion { get; set; }

        /// <summary>Última posición guardada del jugador (null si nunca se guardó).</summary>
        public float? JugadorX { get; set; }
        public float? JugadorY { get; set; }
        public float? JugadorZ { get; set; }
        public float JugadorGiro { get; set; }
        public float JugadorInclinacion { get; set; }

        private byte[] celdas;

        public Mundo(int ancho, int alto, int largo)
        {
            Ancho = ancho;
            Alto = alto;
            Largo = largo;
            celdas = new byte[ancho * alto * largo];
        }

        /// <summary>Nivel del mar: la altura hasta la que se llenan los lagos.</summary>
        public int NivelAgua
        {
            get { return Alto / 2; }
        }

        /// <summary>Celdas en el orden (y, z, x), el mismo que usan los mapas de ClassiCube.</summary>
        public byte[] Celdas
        {
            get { return celdas; }
        }

        public bool EstaDentro(int x, int y, int z)
        {
            return x >= 0 && y >= 0 && z >= 0 && x < Ancho && y < Alto && z < Largo;
        }

        public int ObtenerBloque(int x, int y, int z)
        {
            return EstaDentro(x, y, z) ? celdas[(y * Largo + z) * Ancho + x] : 0;
        }

        public void EstablecerBloque(int x, int y, int z, int idTipoBloque)
        {
            if (EstaDentro(x, y, z))
            {
                celdas[(y * Largo + z) * Ancho + x] = (byte)idTipoBloque;
            }
        }

        /// <summary>Reemplaza todas las celdas (por ejemplo, al cargar desde la base de datos).</summary>
        public void CargarCeldas(byte[] datos)
        {
            if (datos == null || datos.Length != celdas.Length)
            {
                throw new ArgumentException("Los datos no corresponden a las dimensiones del mundo.");
            }
            celdas = datos;
        }

        public void Vaciar()
        {
            celdas = new byte[Ancho * Alto * Largo];
        }

        public int ContarBloques()
        {
            int total = 0;
            for (int i = 0; i < celdas.Length; i++)
            {
                if (celdas[i] != 0) total++;
            }
            return total;
        }
    }
}
