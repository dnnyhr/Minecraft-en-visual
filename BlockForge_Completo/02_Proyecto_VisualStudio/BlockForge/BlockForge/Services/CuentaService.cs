using System;
using BlockForge.Data;
using BlockForge.Helpers;
using BlockForge.Models;

namespace BlockForge.Services
{
    /// <summary>
    /// Cambios en la cuenta del usuario que inició sesión: nombre de usuario (con el que entra y aparece
    /// en el multijugador) y contraseña. Siempre se pide la contraseña actual.
    /// </summary>
    public class CuentaService
    {
        private readonly UsuarioRepository repositorio = new UsuarioRepository();

        /// <summary>
        /// Aplica los cambios. contrasenaNueva vacía = conservar la actual. Lanza ExcepcionValidacion
        /// con un mensaje para el usuario si algo no es válido.
        /// </summary>
        public void Actualizar(string nombreNuevo, string contrasenaActual, string contrasenaNueva, string confirmacion)
        {
            Usuario usuario = Sesion.UsuarioActual;
            if (usuario == null) throw new ExcepcionValidacion("No hay ninguna sesión iniciada.");

            string mensaje;
            nombreNuevo = (nombreNuevo ?? string.Empty).Trim();
            if (!Validaciones.NombreUsuarioValido(nombreNuevo, out mensaje)) throw new ExcepcionValidacion(mensaje);

            bool cambiaContrasena = !string.IsNullOrEmpty(contrasenaNueva) || !string.IsNullOrEmpty(confirmacion);
            if (cambiaContrasena)
            {
                if (!Validaciones.ContrasenaValida(contrasenaNueva, out mensaje)) throw new ExcepcionValidacion(mensaje);
                if (contrasenaNueva != confirmacion) throw new ExcepcionValidacion("La contraseña nueva y su confirmación no coinciden.");
            }
            if (nombreNuevo == usuario.NombreUsuario && !cambiaContrasena) throw new ExcepcionValidacion("No hay cambios que guardar.");

            CredencialUsuario credencial = repositorio.ObtenerPorNombre(usuario.NombreUsuario);
            if (credencial == null || !Seguridad.VerificarContrasena(contrasenaActual, credencial.Sal, credencial.ContrasenaHash))
            {
                throw new ExcepcionValidacion("La contraseña actual no es correcta.");
            }
            if (repositorio.NombreEnUso(nombreNuevo, usuario.IdUsuario))
            {
                throw new ExcepcionValidacion("Ya existe una cuenta llamada «" + nombreNuevo + "». Elige otro nombre.");
            }

            byte[] sal = null, hash = null;
            if (cambiaContrasena)
            {
                sal = Seguridad.GenerarSal();
                hash = Seguridad.CalcularHash(sal, contrasenaNueva);
            }
            repositorio.ActualizarCuenta(usuario.IdUsuario, nombreNuevo, hash, sal);
            usuario.NombreUsuario = nombreNuevo;
        }
    }
}
