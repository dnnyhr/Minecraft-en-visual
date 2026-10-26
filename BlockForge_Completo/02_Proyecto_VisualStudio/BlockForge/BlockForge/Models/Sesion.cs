namespace BlockForge.Models
{
    /// <summary>Datos de la sesión activa, disponibles para todos los formularios.</summary>
    public static class Sesion
    {
        public static Usuario UsuarioActual { get; set; }
        public static Configuracion Configuracion { get; set; }

        public static bool EsAdministrador
        {
            get { return UsuarioActual != null && UsuarioActual.EsAdministrador; }
        }

        public static void Cerrar()
        {
            UsuarioActual = null;
        }
    }
}
