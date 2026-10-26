using System;
using Microsoft.Data.SqlClient;
using BlockForge.Helpers;
using BlockForge.Models;

namespace BlockForge.Data
{
    /// <summary>
    /// Guarda y lee los bloques del mundo. Un mundo 3D tiene cientos de miles de celdas,
    /// así que se guardan como un arreglo de bytes comprimido con GZip en Mundo.Bloques
    /// (el mismo enfoque de los mapas .cw de ClassiCube) en lugar de una fila por bloque.
    /// </summary>
    public class BloqueRepository
    {
        /// <summary>Llena las celdas del mundo. Devuelve false si el mundo no tiene bloques guardados.</summary>
        public bool CargarEnMundo(Mundo mundo)
        {
            const string sql = "SELECT Bloques FROM Mundo WHERE IdMundo = @id";
            byte[] comprimido;
            using (SqlConnection conexion = Conexion.Abrir())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.AddWithValue("@id", mundo.IdMundo);
                object valor = comando.ExecuteScalar();
                comprimido = valor as byte[];
            }
            if (comprimido == null) return false;

            byte[] celdas = Compresion.Descomprimir(comprimido);
            if (celdas.Length != mundo.Celdas.Length) return false;
            mundo.CargarCeldas(celdas);
            return true;
        }

        /// <summary>Reemplaza los bloques y la posición del jugador dentro de la transacción recibida.</summary>
        public void Reemplazar(SqlConnection conexion, SqlTransaction transaccion, Mundo mundo)
        {
            const string sql = @"UPDATE Mundo SET Bloques = @bloques, JugadorX = @x, JugadorY = @y, JugadorZ = @z,
                                        JugadorGiro = @giro, JugadorInclinacion = @inclinacion
                                 WHERE IdMundo = @id";
            using (SqlCommand comando = new SqlCommand(sql, conexion, transaccion))
            {
                comando.Parameters.Add("@bloques", System.Data.SqlDbType.VarBinary, -1).Value = Compresion.Comprimir(mundo.Celdas);
                comando.Parameters.AddWithValue("@x", (object)mundo.JugadorX ?? DBNull.Value);
                comando.Parameters.AddWithValue("@y", (object)mundo.JugadorY ?? DBNull.Value);
                comando.Parameters.AddWithValue("@z", (object)mundo.JugadorZ ?? DBNull.Value);
                comando.Parameters.AddWithValue("@giro", mundo.JugadorGiro);
                comando.Parameters.AddWithValue("@inclinacion", mundo.JugadorInclinacion);
                comando.Parameters.AddWithValue("@id", mundo.IdMundo);
                comando.ExecuteNonQuery();
            }
        }
    }
}
