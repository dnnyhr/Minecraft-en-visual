using BlockForge.Data;
using BlockForge.Helpers;
using BlockForge.Models;

namespace BlockForge.Services
{
    public class ConfiguracionService
    {
        private readonly ConfiguracionRepository repositorio = new ConfiguracionRepository();

        public Configuracion Cargar()
        {
            Sesion.Configuracion = repositorio.ObtenerActual();
            return Sesion.Configuracion;
        }

        /// <summary>RF-15: valida los rangos y guarda una nueva versión de la configuración.</summary>
        public void Guardar(Configuracion configuracion)
        {
            if (!Sesion.EsAdministrador)
            {
                throw new ExcepcionValidacion("Solo el Administrador puede cambiar la configuración.");
            }
            if (!Validaciones.VisualizacionValida(configuracion.DistanciaVision, configuracion.CampoVision, configuracion.Sensibilidad))
            {
                throw new ExcepcionValidacion("Las opciones de visualización están fuera de rango.");
            }
            if (!Validaciones.DimensionesValidas(configuracion.AnchoPredeterminado, configuracion.LargoPredeterminado,
                                                 configuracion.AltoPredeterminado))
            {
                throw new ExcepcionValidacion("Las dimensiones predeterminadas están fuera de rango.");
            }
            repositorio.Guardar(configuracion, Sesion.UsuarioActual.IdUsuario);
            Cargar();
        }
    }
}
