using Microsoft.Data.SqlClient;

namespace BlockForge.Data
{
    /// <summary>
    /// Crea conexiones a BlockForgeDB. Para usar otro servidor de SQL Server,
    /// basta con cambiar la constante CadenaConexion.
    /// </summary>
    public static class Conexion
    {
        public const string CadenaConexion =
            @"Server=(localdb)\MSSQLLocalDB;Database=BlockForgeDB;Integrated Security=true;TrustServerCertificate=true;Connect Timeout=60";

        public static SqlConnection Abrir()
        {
            SqlConnection conexion = new SqlConnection(CadenaConexion);
            conexion.Open();
            return conexion;
        }

        /// <summary>Conexión a la base master de la misma instancia (para crear la base de datos).</summary>
        public static SqlConnection AbrirMaster()
        {
            SqlConnectionStringBuilder constructor = new SqlConnectionStringBuilder(CadenaConexion);
            constructor.InitialCatalog = "master";
            SqlConnection conexion = new SqlConnection(constructor.ConnectionString);
            conexion.Open();
            return conexion;
        }

        public static string NombreBaseDatos
        {
            get { return new SqlConnectionStringBuilder(CadenaConexion).InitialCatalog; }
        }
    }
}
