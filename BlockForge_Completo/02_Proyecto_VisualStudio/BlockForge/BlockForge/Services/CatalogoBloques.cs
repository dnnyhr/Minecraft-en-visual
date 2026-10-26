using System;
using System.Collections.Generic;
using BlockForge.Data;
using BlockForge.Models;

namespace BlockForge.Services
{
    /// <summary>Catálogo de tipos de bloque cargado una sola vez al iniciar la aplicación.</summary>
    public static class CatalogoBloques
    {
        private static List<TipoBloque> tipos = new List<TipoBloque>();

        public static IList<TipoBloque> Tipos
        {
            get { return tipos.AsReadOnly(); }
        }

        public static void Cargar()
        {
            Establecer(new TipoBloqueRepository().ObtenerTodos());
        }

        public static void Establecer(List<TipoBloque> lista)
        {
            tipos = lista ?? new List<TipoBloque>();
        }

        public static TipoBloque Obtener(int idTipoBloque)
        {
            return tipos.Find(t => t.IdTipoBloque == idTipoBloque);
        }

        public static TipoBloque PorNombre(string nombre)
        {
            TipoBloque tipo = tipos.Find(t => string.Equals(t.Nombre, nombre, StringComparison.OrdinalIgnoreCase));
            if (tipo == null)
            {
                throw new InvalidOperationException("El catálogo no contiene el tipo de bloque " + nombre + ".");
            }
            return tipo;
        }
    }
}
