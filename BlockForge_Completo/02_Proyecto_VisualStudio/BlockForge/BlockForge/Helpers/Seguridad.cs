using System;
using System.Security.Cryptography;
using System.Text;

namespace BlockForge.Helpers
{
    /// <summary>Hash de contraseñas: SHA-256(sal + contraseña), igual que el script SQL.</summary>
    public static class Seguridad
    {
        public static byte[] GenerarSal()
        {
            byte[] sal = new byte[16];
            using (RandomNumberGenerator generador = RandomNumberGenerator.Create())
            {
                generador.GetBytes(sal);
            }
            return sal;
        }

        public static byte[] CalcularHash(byte[] sal, string contrasena)
        {
            byte[] texto = Encoding.UTF8.GetBytes(contrasena ?? string.Empty);
            byte[] datos = new byte[sal.Length + texto.Length];
            Buffer.BlockCopy(sal, 0, datos, 0, sal.Length);
            Buffer.BlockCopy(texto, 0, datos, sal.Length, texto.Length);
            using (SHA256 sha = SHA256.Create())
            {
                return sha.ComputeHash(datos);
            }
        }

        /// <summary>Compara en tiempo constante para no revelar información por la duración.</summary>
        public static bool VerificarContrasena(string contrasena, byte[] sal, byte[] hashGuardado)
        {
            byte[] calculado = CalcularHash(sal, contrasena);
            if (hashGuardado == null || calculado.Length != hashGuardado.Length) return false;
            int diferencia = 0;
            for (int i = 0; i < calculado.Length; i++)
            {
                diferencia |= calculado[i] ^ hashGuardado[i];
            }
            return diferencia == 0;
        }
    }
}
