using System;
using System.Collections.Generic;
using System.Linq;
using BlockForge.Models;

namespace BlockForge.Services
{
    /// <summary>Receta de fabricación: ingredientes (nombre y cantidad) que se convierten en un resultado.</summary>
    public class Receta
    {
        public KeyValuePair<string, int>[] Ingredientes { get; private set; }
        public string Resultado { get; private set; }
        public int Cantidad { get; private set; }

        public Receta(string resultado, int cantidad, params object[] ingredientes)
        {
            Resultado = resultado;
            Cantidad = cantidad;
            List<KeyValuePair<string, int>> lista = new List<KeyValuePair<string, int>>();
            for (int i = 0; i + 1 < ingredientes.Length; i += 2) lista.Add(new KeyValuePair<string, int>((string)ingredientes[i], (int)ingredientes[i + 1]));
            Ingredientes = lista.ToArray();
        }

        public string Descripcion
        {
            get { return string.Join(" + ", Ingredientes.Select(i => i.Value + " " + i.Key)); }
        }
    }

    /// <summary>
    /// Fabricación sencilla (sin mesa de trabajo): el carbón hace de combustible para hornear y teñir.
    /// Con estas recetas se consiguen todos los bloques de construcción a partir de lo que da el mundo.
    /// </summary>
    public static class Recetas
    {
        public static readonly Receta[] Todas =
        {
            new Receta("Madera", 4, "Tronco", 1),
            new Receta("Antorcha", 4, "Carbón", 1, "Madera", 1),
            new Receta("Madera oscura", 2, "Madera", 2, "Carbón", 1),
            new Receta("Vidrio", 2, "Arena", 2, "Carbón", 1),
            new Receta("Ladrillo", 2, "Arcilla", 2, "Carbón", 1),
            new Receta("Terracota", 1, "Arcilla", 1, "Carbón", 1),
            new Receta("Ladrillos de piedra", 4, "Piedra", 4),
            new Receta("Piedra lisa", 2, "Piedra", 2, "Carbón", 1),
            new Receta("Estantería", 1, "Madera", 6),
            new Receta("Lámpara", 1, "Vidrio", 1, "Antorcha", 2),
            new Receta("Lana roja", 1, "Lana blanca", 1, "Rosa", 1),
            new Receta("Lana amarilla", 1, "Lana blanca", 1, "Diente de león", 1),
            new Receta("Lana azul", 1, "Lana blanca", 1, "Aciano", 1),
            new Receta("Lana negra", 1, "Lana blanca", 1, "Carbón", 1),
            new Receta("Chuleta cocida", 1, "Chuleta cruda", 1, "Carbón", 1),
            new Receta("Filete cocido", 1, "Filete crudo", 1, "Carbón", 1),
            new Receta("Pollo cocido", 1, "Pollo crudo", 1, "Carbón", 1)
        };

        private static TipoBloque Tipo(string nombre)
        {
            return CatalogoBloques.Tipos.FirstOrDefault(t => string.Equals(t.Nombre, nombre, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>Recetas cuyos tipos existen en el catálogo.</summary>
        public static IEnumerable<Receta> Disponibles()
        {
            return Todas.Where(r => Tipo(r.Resultado) != null && r.Ingredientes.All(i => Tipo(i.Key) != null));
        }

        public static bool SePuede(Receta r, Inventario inventario)
        {
            TipoBloque resultado = Tipo(r.Resultado);
            if (resultado == null || inventario.ObtenerCantidad(resultado.IdTipoBloque) + r.Cantidad > Inventario.CapacidadMaxima) return false;
            foreach (KeyValuePair<string, int> i in r.Ingredientes)
            {
                TipoBloque t = Tipo(i.Key);
                if (t == null || inventario.ObtenerCantidad(t.IdTipoBloque) < i.Value) return false;
            }
            return true;
        }

        /// <summary>Fabrica una vez. Devuelve false si faltan ingredientes.</summary>
        public static bool Fabricar(Receta r, Inventario inventario)
        {
            if (!SePuede(r, inventario)) return false;
            foreach (KeyValuePair<string, int> i in r.Ingredientes)
            {
                TipoBloque t = Tipo(i.Key);
                inventario.EstablecerCantidad(t.IdTipoBloque, inventario.ObtenerCantidad(t.IdTipoBloque) - i.Value);
            }
            TipoBloque resultado = Tipo(r.Resultado);
            inventario.EstablecerCantidad(resultado.IdTipoBloque, inventario.ObtenerCantidad(resultado.IdTipoBloque) + r.Cantidad);
            return true;
        }
    }
}
