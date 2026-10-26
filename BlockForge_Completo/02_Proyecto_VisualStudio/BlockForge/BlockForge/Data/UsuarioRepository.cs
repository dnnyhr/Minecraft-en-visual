using System;
using Microsoft.Data.SqlClient;
using BlockForge.Models;

namespace BlockForge.Data
{
    /// <summary>Datos de acceso de un usuario, incluidos el hash y la sal.</summary>
    public class CredencialUsuario
    {
        public Usuario Usuario { get; set; }
        public byte[] ContrasenaHash { get; set; }
        public byte[] Sal { get; set; }
    }

    public class UsuarioRepository
    {
        public CredencialUsuario ObtenerPorNombre(string nombreUsuario)
        {
            const string sql = @"SELECT IdUsuario, NombreUsuario, ContrasenaHash, Sal, Rol, Activo, SkinPng, SkinDelgada, SkinOrigen
                                 FROM Usuario WHERE NombreUsuario = @nombre";
            using (SqlConnection conexion = Conexion.Abrir())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.AddWithValue("@nombre", nombreUsuario);
                using (SqlDataReader lector = comando.ExecuteReader())
                {
                    if (!lector.Read()) return null;
                    Usuario usuario = new Usuario
                    {
                        IdUsuario = lector.GetInt32(0),
                        NombreUsuario = lector.GetString(1),
                        Rol = lector.GetString(4),
                        Activo = lector.GetBoolean(5)
                    };
                    if (!lector.IsDBNull(6))
                    {
                        usuario.Skin = new Skin
                        {
                            Png = (byte[])lector[6],
                            Delgada = lector.GetBoolean(7),
                            Origen = lector.IsDBNull(8) ? null : lector.GetString(8)
                        };
                    }
                    return new CredencialUsuario
                    {
                        Usuario = usuario,
                        ContrasenaHash = (byte[])lector[2],
                        Sal = (byte[])lector[3]
                    };
                }
            }
        }

        /// <summary>Guarda o quita (skin = null) el aspecto del usuario.</summary>
        public void GuardarSkin(int idUsuario, Skin skin)
        {
            const string sql = "UPDATE Usuario SET SkinPng = @png, SkinDelgada = @delgada, SkinOrigen = @origen WHERE IdUsuario = @id";
            using (SqlConnection conexion = Conexion.Abrir())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@png", System.Data.SqlDbType.VarBinary, -1).Value = skin != null ? (object)skin.Png : DBNull.Value;
                comando.Parameters.AddWithValue("@delgada", skin != null && skin.Delgada);
                comando.Parameters.AddWithValue("@origen", skin != null && skin.Origen != null ? (object)skin.Origen : DBNull.Value);
                comando.Parameters.AddWithValue("@id", idUsuario);
                comando.ExecuteNonQuery();
            }
        }
    }
}
