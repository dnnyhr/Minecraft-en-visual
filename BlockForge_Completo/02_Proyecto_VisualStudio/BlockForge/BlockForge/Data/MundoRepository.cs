using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BlockForge.Models;

namespace BlockForge.Data
{
    public class MundoRepository
    {
        private const string SelectResumen = @"
            SELECT m.IdMundo, m.Nombre, u.NombreUsuario, m.Ancho, m.Alto,
                   ISNULL(p.TotalBloques, 0) AS TotalBloques,
                   p.FechaGuardado, m.Largo
            FROM Mundo m
            INNER JOIN Usuario u ON u.IdUsuario = m.IdUsuario
            OUTER APPLY (SELECT TOP 1 TotalBloques, FechaGuardado FROM Partida
                         WHERE IdMundo = m.IdMundo ORDER BY FechaGuardado DESC, IdPartida DESC) p ";

        public bool ExisteNombre(int idUsuario, string nombre)
        {
            const string sql = "SELECT COUNT(*) FROM Mundo WHERE IdUsuario = @usuario AND Nombre = @nombre";
            using (SqlConnection conexion = Conexion.Abrir())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.AddWithValue("@usuario", idUsuario);
                comando.Parameters.AddWithValue("@nombre", nombre);
                return (int)comando.ExecuteScalar() > 0;
            }
        }

        public int Insertar(SqlConnection conexion, SqlTransaction transaccion, Mundo mundo)
        {
            const string sql = @"INSERT INTO Mundo (IdUsuario, Nombre, Ancho, Largo, Alto, Semilla, ConLagos)
                                 OUTPUT INSERTED.IdMundo
                                 VALUES (@usuario, @nombre, @ancho, @largo, @alto, @semilla, @lagos)";
            using (SqlCommand comando = new SqlCommand(sql, conexion, transaccion))
            {
                comando.Parameters.AddWithValue("@usuario", mundo.IdUsuario);
                comando.Parameters.AddWithValue("@nombre", mundo.Nombre);
                comando.Parameters.AddWithValue("@ancho", mundo.Ancho);
                comando.Parameters.AddWithValue("@largo", mundo.Largo);
                comando.Parameters.AddWithValue("@alto", mundo.Alto);
                comando.Parameters.AddWithValue("@semilla", mundo.Semilla);
                comando.Parameters.AddWithValue("@lagos", mundo.ConLagos);
                return (int)comando.ExecuteScalar();
            }
        }

        /// <summary>Obtiene los datos generales del mundo (sin los bloques).</summary>
        public Mundo ObtenerPorId(int idMundo)
        {
            const string sql = @"SELECT IdMundo, IdUsuario, Nombre, Ancho, Alto, Largo, Semilla, ConLagos, FechaCreacion,
                                        JugadorX, JugadorY, JugadorZ, JugadorGiro, JugadorInclinacion
                                 FROM Mundo WHERE IdMundo = @id";
            using (SqlConnection conexion = Conexion.Abrir())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.AddWithValue("@id", idMundo);
                using (SqlDataReader lector = comando.ExecuteReader())
                {
                    if (!lector.Read()) return null;
                    Mundo mundo = new Mundo(lector.GetInt32(3), lector.GetInt32(4), lector.GetInt32(5))
                    {
                        IdMundo = lector.GetInt32(0),
                        IdUsuario = lector.GetInt32(1),
                        Nombre = lector.GetString(2),
                        Semilla = lector.GetInt32(6),
                        ConLagos = lector.GetBoolean(7),
                        FechaCreacion = lector.GetDateTime(8)
                    };
                    if (!lector.IsDBNull(9) && !lector.IsDBNull(10) && !lector.IsDBNull(11))
                    {
                        mundo.JugadorX = lector.GetFloat(9);
                        mundo.JugadorY = lector.GetFloat(10);
                        mundo.JugadorZ = lector.GetFloat(11);
                        mundo.JugadorGiro = lector.IsDBNull(12) ? 0f : lector.GetFloat(12);
                        mundo.JugadorInclinacion = lector.IsDBNull(13) ? 0f : lector.GetFloat(13);
                    }
                    return mundo;
                }
            }
        }

        public List<ResumenMundo> ListarPorUsuario(int idUsuario)
        {
            string sql = SelectResumen + "WHERE m.IdUsuario = @usuario ORDER BY COALESCE(p.FechaGuardado, m.FechaCreacion) DESC";
            using (SqlConnection conexion = Conexion.Abrir())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.AddWithValue("@usuario", idUsuario);
                return LeerResumenes(comando);
            }
        }

        /// <summary>Lista los mundos de todos los usuarios, filtrando por nombre de mundo o de usuario.</summary>
        public List<ResumenMundo> ListarTodos(string filtro)
        {
            string sql = SelectResumen +
                "WHERE (@filtro = '' OR m.Nombre LIKE '%' + @filtro + '%' OR u.NombreUsuario LIKE '%' + @filtro + '%') " +
                "ORDER BY m.IdMundo";
            using (SqlConnection conexion = Conexion.Abrir())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.AddWithValue("@filtro", (filtro ?? string.Empty).Trim());
                return LeerResumenes(comando);
            }
        }

        public void Eliminar(int idMundo)
        {
            // Bloque, ItemInventario y Partida se eliminan en cascada (ON DELETE CASCADE).
            using (SqlConnection conexion = Conexion.Abrir())
            using (SqlCommand comando = new SqlCommand("DELETE FROM Mundo WHERE IdMundo = @id", conexion))
            {
                comando.Parameters.AddWithValue("@id", idMundo);
                comando.ExecuteNonQuery();
            }
        }

        private static List<ResumenMundo> LeerResumenes(SqlCommand comando)
        {
            List<ResumenMundo> lista = new List<ResumenMundo>();
            using (SqlDataReader lector = comando.ExecuteReader())
            {
                while (lector.Read())
                {
                    lista.Add(new ResumenMundo
                    {
                        IdMundo = lector.GetInt32(0),
                        Nombre = lector.GetString(1),
                        Usuario = lector.GetString(2),
                        Ancho = lector.GetInt32(3),
                        Alto = lector.GetInt32(4),
                        TotalBloques = lector.GetInt32(5),
                        UltimoGuardado = lector.IsDBNull(6) ? (DateTime?)null : lector.GetDateTime(6),
                        Largo = lector.GetInt32(7)
                    });
                }
            }
            return lista;
        }
    }
}
