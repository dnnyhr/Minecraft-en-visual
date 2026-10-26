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
        Indestructible
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
            if (tipo == null || !tipo.EsRecolectable) return ResultadoConstruccion.NoColocable; // RN-07
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
            if (tipo == null || !tipo.EsRecolectable)
            {
                Mundo.EstablecerBloque(x, y, z, 0);
                HayCambios = true;
                return ResultadoConstruccion.RetiradoSinSumar;
            }

            if (Inventario.EstaLleno(idTipo)) return ResultadoConstruccion.InventarioLleno; // RN-08

            Mundo.EstablecerBloque(x, y, z, 0);
            Inventario.AgregarBloque(idTipo);
            HayCambios = true;
            return ResultadoConstruccion.Recolectado;
        }

        private static bool EsLiquido(int idTipo)
        {
            TipoBloque tipo = CatalogoBloques.Obtener(idTipo);
            return tipo != null && !tipo.EsSolido;
        }
    }
}
