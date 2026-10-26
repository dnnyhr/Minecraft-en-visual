using Microsoft.Data.SqlClient;
using BlockForge.Models;

namespace BlockForge.Data
{
    public class ConfiguracionRepository
    {
        /// <summary>Devuelve la configuración más reciente (cada cambio se guarda como una fila nueva).</summary>
        public Configuracion ObtenerActual()
        {
            const string sql = @"SELECT TOP 1 c.IdConfiguracion, c.DistanciaVision, c.CampoVision, c.Sensibilidad,
                                        c.AnchoPredeterminado, c.LargoPredeterminado, c.AltoPredeterminado,
                                        c.FechaModificacion, u.NombreUsuario
                                 FROM Configuracion c
                                 LEFT JOIN Usuario u ON u.IdUsuario = c.IdUsuarioModifica
                                 ORDER BY c.FechaModificacion DESC, c.IdConfiguracion DESC";
            using (SqlConnection conexion = Conexion.Abrir())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            using (SqlDataReader lector = comando.ExecuteReader())
            {
                if (!lector.Read()) return Configuracion.PorDefecto();
                return new Configuracion
                {
                    IdConfiguracion = lector.GetInt32(0),
                    DistanciaVision = lector.GetInt32(1),
                    CampoVision = lector.GetInt32(2),
                    Sensibilidad = lector.GetInt32(3),
                    AnchoPredeterminado = lector.GetInt32(4),
                    LargoPredeterminado = lector.GetInt32(5),
                    AltoPredeterminado = lector.GetInt32(6),
                    FechaModificacion = lector.GetDateTime(7),
                    UsuarioModifica = lector.IsDBNull(8) ? null : lector.GetString(8)
                };
            }
        }

        public void Guardar(Configuracion configuracion, int idUsuario)
        {
            const string sql = @"INSERT INTO Configuracion (IdUsuarioModifica, DistanciaVision, CampoVision, Sensibilidad,
                                                            AnchoPredeterminado, LargoPredeterminado, AltoPredeterminado)
                                 VALUES (@usuario, @distancia, @campo, @sensibilidad, @ancho, @largo, @alto)";
            using (SqlConnection conexion = Conexion.Abrir())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.AddWithValue("@usuario", idUsuario);
                comando.Parameters.AddWithValue("@distancia", configuracion.DistanciaVision);
                comando.Parameters.AddWithValue("@campo", configuracion.CampoVision);
                comando.Parameters.AddWithValue("@sensibilidad", configuracion.Sensibilidad);
                comando.Parameters.AddWithValue("@ancho", configuracion.AnchoPredeterminado);
                comando.Parameters.AddWithValue("@largo", configuracion.LargoPredeterminado);
                comando.Parameters.AddWithValue("@alto", configuracion.AltoPredeterminado);
                comando.ExecuteNonQuery();
            }
        }
    }
}
