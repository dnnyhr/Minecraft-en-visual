using System.Collections.Generic;

namespace BlockForge.Models
{
    /// <summary>Cantidades de cada tipo de bloque del mundo abierto.</summary>
    public class Inventario
    {
        public const int CapacidadMaxima = 999;

        private readonly Dictionary<int, int> cantidades = new Dictionary<int, int>();

        public int ObtenerCantidad(int idTipoBloque)
        {
            int cantidad;
            return cantidades.TryGetValue(idTipoBloque, out cantidad) ? cantidad : 0;
        }

        public void EstablecerCantidad(int idTipoBloque, int cantidad)
        {
            if (cantidad < 0) cantidad = 0;
            if (cantidad > CapacidadMaxima) cantidad = CapacidadMaxima;
            cantidades[idTipoBloque] = cantidad;
        }

        public bool EstaLleno(int idTipoBloque)
        {
            return ObtenerCantidad(idTipoBloque) >= CapacidadMaxima;
        }

        /// <summary>Suma una unidad. Devuelve false si ya se alcanzó la capacidad máxima.</summary>
        public bool AgregarBloque(int idTipoBloque)
        {
            if (EstaLleno(idTipoBloque)) return false;
            EstablecerCantidad(idTipoBloque, ObtenerCantidad(idTipoBloque) + 1);
            return true;
        }

        /// <summary>Resta una unidad. Devuelve false si no hay existencias.</summary>
        public bool RetirarBloque(int idTipoBloque)
        {
            int actual = ObtenerCantidad(idTipoBloque);
            if (actual <= 0) return false;
            EstablecerCantidad(idTipoBloque, actual - 1);
            return true;
        }

        public int Total()
        {
            int total = 0;
            foreach (int cantidad in cantidades.Values) total += cantidad;
            return total;
        }

        public void Vaciar()
        {
            cantidades.Clear();
        }
    }
}
