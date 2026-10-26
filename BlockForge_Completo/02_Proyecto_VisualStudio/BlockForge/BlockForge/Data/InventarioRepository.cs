using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BlockForge.Models;

namespace BlockForge.Data
{
    public class InventarioRepository
    {
        public Inventario Obtener(int idMundo)
        {
            Inventario inventario = new Inventario();
            const string sql = "SELECT IdTipoBloque, Cantidad FROM ItemInventario WHERE IdMundo = @id";
            using (SqlConnection conexion = Conexion.Abrir())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.AddWithValue("@id", idMundo);
                using (SqlDataReader lector = comando.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        inventario.EstablecerCantidad(lector.GetInt32(0), lector.GetInt32(1));
                    }
                }
            }
            return inventario;
        }

        /// <summary>Guarda un registro por cada tipo de bloque (UQ_Item_Mundo_Tipo).</summary>
        public void Guardar(SqlConnection conexion, SqlTransaction transaccion, int idMundo,
                            Inventario inventario, IEnumerable<TipoBloque> tipos)
        {
            using (SqlCommand borrar = new SqlCommand("DELETE FROM ItemInventario WHERE IdMundo = @id", conexion, transaccion))
            {
                borrar.Parameters.AddWithValue("@id", idMundo);
                borrar.ExecuteNonQuery();
            }

            const string sql = @"INSERT INTO ItemInventario (IdMundo, IdTipoBloque, Cantidad)
                                 VALUES (@mundo, @tipo, @cantidad)";
            foreach (TipoBloque tipo in tipos)
            {
                using (SqlCommand insertar = new SqlCommand(sql, conexion, transaccion))
                {
                    insertar.Parameters.AddWithValue("@mundo", idMundo);
                    insertar.Parameters.AddWithValue("@tipo", tipo.IdTipoBloque);
                    insertar.Parameters.AddWithValue("@cantidad", inventario.ObtenerCantidad(tipo.IdTipoBloque));
                    insertar.ExecuteNonQuery();
                }
            }
        }
    }
}
