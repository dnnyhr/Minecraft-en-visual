using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BlockForge.Models;

namespace BlockForge.Data
{
    public class TipoBloqueRepository
    {
        public List<TipoBloque> ObtenerTodos()
        {
            List<TipoBloque> tipos = new List<TipoBloque>();
            const string sql = @"SELECT IdTipoBloque, Nombre, ColorHex, EsSolido, EsRecolectable
                                 FROM TipoBloque ORDER BY IdTipoBloque";
            using (SqlConnection conexion = Conexion.Abrir())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            using (SqlDataReader lector = comando.ExecuteReader())
            {
                while (lector.Read())
                {
                    tipos.Add(new TipoBloque
                    {
                        IdTipoBloque = lector.GetInt32(0),
                        Nombre = lector.GetString(1),
                        ColorHex = lector.GetString(2).Trim(),
                        EsSolido = lector.GetBoolean(3),
                        EsRecolectable = lector.GetBoolean(4)
                    });
                }
            }
            return tipos;
        }
    }
}
