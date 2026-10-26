using System.Linq;
using BlockForge.Models;

namespace BlockForge.Services
{
    public enum ResultadoConstruccion
    {
        Colocado,
        Recolectado,
        RetiradoSinSumar,
        FueraDelMundo,
        CeldaOcupada,
        CeldaVacia,
        SinExistencias,
        NoColocable,
        InventarioLleno,
        Indestructible,
        SinSoporte
    }

    /// <summary>
    /// Mecánica de construcción (RF-07, RF-08 y CU-07). Trabaja en memoria;
    /// los cambios se escriben en la base de datos al guardar.
    /// </summary>
    public class ConstruccionService
    {
        public Mundo Mundo { get; private set; }
        public Inventario Inventario { get; private set; }
        public bool HayCambios { get; set; }

        public ConstruccionService(Mundo mundo, Inventario inventario)
        {
            Mundo = mundo;
            Inventario = inventario;
        }

        /// <summary>Clic derecho: coloca un bloque del tipo activo (puede reemplazar agua).</summary>
        public ResultadoConstruccion Colocar(int x, int y, int z, TipoBloque tipo)
        {
            if (!Mundo.EstaDentro(x, y, z)) return ResultadoConstruccion.FueraDelMundo;
            int actual = Mundo.ObtenerBloque(x, y, z);
            if (actual != 0 && !EsLiquido(actual)) return ResultadoConstruccion.CeldaOcupada;
            if (tipo == null || !tipo.EsColocable) return ResultadoConstruccion.NoColocable; // RN-07
            if (tipo.Categoria == TipoBloque.Planta && !TieneSoporte(x, y, z, tipo)) return ResultadoConstruccion.SinSoporte;
            if (!Inventario.RetirarBloque(tipo.IdTipoBloque)) return ResultadoConstruccion.SinExistencias;

            Mundo.EstablecerBloque(x, y, z, tipo.IdTipoBloque);
            HayCambios = true;
            return ResultadoConstruccion.Colocado;
        }

        /// <summary>Clic izquierdo: retira el bloque y lo suma al inventario si es recolectable.</summary>
        public ResultadoConstruccion Eliminar(int x, int y, int z)
        {
            if (!Mundo.EstaDentro(x, y, z)) return ResultadoConstruccion.FueraDelMundo;
            int idTipo = Mundo.ObtenerBloque(x, y, z);
            if (idTipo == 0) return ResultadoConstruccion.CeldaVacia;
            if (y == 0) return ResultadoConstruccion.Indestructible; // la capa inferior sostiene el mundo

            TipoBloque tipo = CatalogoBloques.Obtener(idTipo);
            TipoBloque suelta = Suelta(tipo);
            if (suelta == null)
            {
                Mundo.EstablecerBloque(x, y, z, 0);
                HayCambios = true;
                return ResultadoConstruccion.RetiradoSinSumar;
            }

            if (Inventario.EstaLleno(suelta.IdTipoBloque)) return ResultadoConstruccion.InventarioLleno; // RN-08

            Mundo.EstablecerBloque(x, y, z, 0);
            Inventario.AgregarBloque(suelta.IdTipoBloque);
            UltimoRecolectado = suelta;
            HayCambios = true;
            return ResultadoConstruccion.Recolectado;
        }

        /// <summary>Lo último que se sumó al inventario al romper un bloque.</summary>
        public TipoBloque UltimoRecolectado { get; private set; }

        /// <summary>Qué deja un bloque al romperse: el mineral de carbón da carbón; la hierba alta, nada.</summary>
        private static TipoBloque Suelta(TipoBloque tipo)
        {
            if (tipo == null || !tipo.EsRecolectable) return null;
            switch (tipo.Nombre)
            {
                case "Mineral de carbón": return CatalogoBloques.Tipos.FirstOrDefault(t => t.Nombre == "Carbón") ?? tipo;
                case "Hierba alta": return null;
                default: return tipo;
            }
        }

        /// <summary>Suma un objeto al inventario (lo que dejan los mobs, las hojas o la fabricación).</summary>
        public bool Dar(TipoBloque tipo, int cantidad)
        {
            if (tipo == null || cantidad <= 0) return false;
            int actual = Inventario.ObtenerCantidad(tipo.IdTipoBloque);
            if (actual >= Inventario.CapacidadMaxima) return false;
            Inventario.EstablecerCantidad(tipo.IdTipoBloque, actual + cantidad);
            HayCambios = true;
            return true;
        }

        /// <summary>Gasta una unidad (al comer).</summary>
        public bool Gastar(TipoBloque tipo)
        {
            if (tipo == null || !Inventario.RetirarBloque(tipo.IdTipoBloque)) return false;
            HayCambios = true;
            return true;
        }

        /// <summary>Las plantas necesitan pasto o tierra debajo; las antorchas, cualquier bloque sólido.</summary>
        private bool TieneSoporte(int x, int y, int z, TipoBloque tipo)
        {
            TipoBloque abajo = CatalogoBloques.Obtener(Mundo.ObtenerBloque(x, y - 1, z));
            if (abajo == null) return false;
            if (tipo.Luz > 0) return abajo.EsSolido && abajo.Categoria == TipoBloque.Bloque;
            return abajo.Nombre == "Pasto" || abajo.Nombre == "Tierra";
        }

        private static bool EsLiquido(int idTipo)
        {
            TipoBloque tipo = CatalogoBloques.Obtener(idTipo);
            return tipo != null && !tipo.EsSolido;
        }
    }
}
