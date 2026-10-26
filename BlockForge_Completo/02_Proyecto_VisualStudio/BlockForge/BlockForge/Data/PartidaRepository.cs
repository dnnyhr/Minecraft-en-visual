using Microsoft.Data.SqlClient;

namespace BlockForge.Data
{
    public class PartidaRepository
    {
        public void Registrar(SqlConnection conexion, SqlTransaction transaccion, int idMundo, int totalBloques)
        {
            const string sql = "INSERT INTO Partida (IdMundo, TotalBloques) VALUES (@mundo, @total)";
            using (SqlCommand comando = new SqlCommand(sql, conexion, transaccion))
            {
                comando.Parameters.AddWithValue("@mundo", idMundo);
                comando.Parameters.AddWithValue("@total", totalBloques);
                comando.ExecuteNonQuery();
            }
        }
    }
}
