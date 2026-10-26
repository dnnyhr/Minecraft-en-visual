using System.Text.RegularExpressions;
using BlockForge.Models;

namespace BlockForge.Helpers
{
    /// <summary>Reglas de negocio de validación (sección 10 del documento).</summary>
    public static class Validaciones
    {
        private static readonly Regex PatronNombre = new Regex(@"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ0-9 \-]+$");

        /// <summary>RN-04: nombre obligatorio, 3 a 40 caracteres, letras, números, espacios y guiones.</summary>
        public static bool NombreMundoValido(string nombre, out string mensaje)
        {
            nombre = (nombre ?? string.Empty).Trim();
            if (nombre.Length == 0)
            {
                mensaje = "El nombre es obligatorio (de 3 a 40 caracteres).";
                return false;
            }
            if (nombre.Length < 3 || nombre.Length > 40)
            {
                mensaje = "El nombre debe tener de 3 a 40 caracteres.";
                return false;
            }
            if (!PatronNombre.IsMatch(nombre))
            {
                mensaje = "Use solo letras, números, espacios y guiones.";
                return false;
            }
            mensaje = string.Empty;
            return true;
        }

        private static readonly Regex PatronUsuario = new Regex(@"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ0-9_.\-]+$");

        /// <summary>
        /// Nombre de usuario: de 4 a 20 caracteres (es también el nombre en el multijugador),
        /// letras, números, puntos, guiones y guiones bajos, sin espacios.
        /// </summary>
        public static bool NombreUsuarioValido(string nombre, out string mensaje)
        {
            nombre = (nombre ?? string.Empty).Trim();
            if (nombre.Length < 4 || nombre.Length > 20)
            {
                mensaje = "El nombre de usuario debe tener de 4 a 20 caracteres.";
                return false;
            }
            if (!PatronUsuario.IsMatch(nombre))
            {
                mensaje = "Use solo letras, números, puntos y guiones, sin espacios.";
                return false;
            }
            mensaje = string.Empty;
            return true;
        }

        /// <summary>Contraseña nueva: de 6 a 30 caracteres, con al menos una letra y un número.</summary>
        public static bool ContrasenaValida(string contrasena, out string mensaje)
        {
            contrasena = contrasena ?? string.Empty;
            if (contrasena.Length < 6 || contrasena.Length > 30)
            {
                mensaje = "La contraseña debe tener de 6 a 30 caracteres.";
                return false;
            }
            if (!Regex.IsMatch(contrasena, @"\p{L}") || !Regex.IsMatch(contrasena, @"\d"))
            {
                mensaje = "La contraseña debe tener al menos una letra y un número.";
                return false;
            }
            mensaje = string.Empty;
            return true;
        }

        /// <summary>RN-05: rangos de dimensiones del mundo.</summary>
        public static bool DimensionesValidas(int ancho, int largo, int alto)
        {
            return ancho >= Mundo.AnchoMinimo && ancho <= Mundo.AnchoMaximo
                && largo >= Mundo.LargoMinimo && largo <= Mundo.LargoMaximo
                && alto >= Mundo.AltoMinimo && alto <= Mundo.AltoMaximo;
        }

        /// <summary>RN-11: opciones de visualización dentro de sus rangos.</summary>
        public static bool VisualizacionValida(int distancia, int campo, int sensibilidad)
        {
            return distancia >= Configuracion.DistanciaMinima && distancia <= Configuracion.DistanciaMaxima
                && campo >= Configuracion.CampoMinimo && campo <= Configuracion.CampoMaximo
                && sensibilidad >= Configuracion.SensibilidadMinima && sensibilidad <= Configuracion.SensibilidadMaxima;
        }
    }
}
