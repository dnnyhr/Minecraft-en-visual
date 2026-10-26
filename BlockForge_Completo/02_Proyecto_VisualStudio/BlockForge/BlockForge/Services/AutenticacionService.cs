using System;
using BlockForge.Data;
using BlockForge.Helpers;
using BlockForge.Models;

namespace BlockForge.Services
{
    public enum ResultadoAcceso
    {
        Correcto,
        CamposVacios,
        CredencialesInvalidas,
        UsuarioInactivo,
        Bloqueado
    }

    /// <summary>RF-01 y RN-01/RN-02: autenticación con bloqueo tras tres intentos fallidos.</summary>
    public class AutenticacionService
    {
        public const int IntentosPermitidos = 3;
        public const int SegundosBloqueo = 30;

        private static int intentosFallidos;
        private static DateTime bloqueadoHasta = DateTime.MinValue;

        private readonly UsuarioRepository repositorio = new UsuarioRepository();

        public int IntentosRestantes
        {
            get { return Math.Max(0, IntentosPermitidos - intentosFallidos); }
        }

        public int SegundosRestantesBloqueo
        {
            get
            {
                double segundos = (bloqueadoHasta - DateTime.Now).TotalSeconds;
                return segundos > 0 ? (int)Math.Ceiling(segundos) : 0;
            }
        }

        public ResultadoAcceso Autenticar(string nombreUsuario, string contrasena)
        {
            if (SegundosRestantesBloqueo > 0) return ResultadoAcceso.Bloqueado;
            if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrEmpty(contrasena))
            {
                return ResultadoAcceso.CamposVacios;
            }

            CredencialUsuario credencial = repositorio.ObtenerPorNombre(nombreUsuario.Trim());
            bool valido = credencial != null
                && Seguridad.VerificarContrasena(contrasena, credencial.Sal, credencial.ContrasenaHash);

            if (!valido)
            {
                intentosFallidos++;
                if (intentosFallidos >= IntentosPermitidos)
                {
                    bloqueadoHasta = DateTime.Now.AddSeconds(SegundosBloqueo);
                    intentosFallidos = 0;
                    return ResultadoAcceso.Bloqueado;
                }
                return ResultadoAcceso.CredencialesInvalidas;
            }

            if (!credencial.Usuario.Activo) return ResultadoAcceso.UsuarioInactivo;

            intentosFallidos = 0;
            Sesion.UsuarioActual = credencial.Usuario;
            return ResultadoAcceso.Correcto;
        }
    }
}
