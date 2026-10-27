using System;
using System.IO;
using System.Text.RegularExpressions;
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
            @"Server=(localdb)\MSSQLLocalDB;Database=BlockForgeDB;Integrated Security=true;TrustServerCertificate=true;Connect Timeout=30";

        private const string InstanciaLocalDb = "MSSQLLocalDB";

        // Si (localdb)\MSSQLLocalDB no responde pero su servidor sí está corriendo, se usa su tubería directamente.
        private static string servidorAlternativo;

        public static SqlConnection Abrir()
        {
            return AbrirCatalogo(null);
        }

        /// <summary>Conexión a la base master de la misma instancia (para crear la base de datos).</summary>
        public static SqlConnection AbrirMaster()
        {
            return AbrirCatalogo("master");
        }

        public static string NombreBaseDatos
        {
            get { return new SqlConnectionStringBuilder(CadenaConexion).InitialCatalog; }
        }

        private static SqlConnection AbrirCatalogo(string catalogo)
        {
            SqlConnectionStringBuilder constructor = new SqlConnectionStringBuilder(CadenaConexion);
            if (catalogo != null) constructor.InitialCatalog = catalogo;
            bool esLocalDb = constructor.DataSource.StartsWith("(localdb)", StringComparison.OrdinalIgnoreCase);

            // LocalDB a veces queda marcada como "detenida" aunque su servidor siga funcionando, y entonces
            // (localdb)\MSSQLLocalDB no responde (o tarda medio minuto en fallar). Por eso, si ya hay un servidor
            // en marcha, se usa directamente su tubería; si no lo hay, falla al instante y se sigue por el camino normal.
            if (esLocalDb)
            {
                string tuberia = servidorAlternativo ?? TuberiaLocalDbActiva();
                if (tuberia != null)
                {
                    SqlConnectionStringBuilder directo = new SqlConnectionStringBuilder(constructor.ConnectionString)
                    {
                        DataSource = "np:" + tuberia,
                        ConnectTimeout = 5
                    };
                    try
                    {
                        SqlConnection conexion = AbrirCon(directo.ConnectionString);
                        servidorAlternativo = tuberia;
                        return conexion;
                    }
                    catch (SqlException ex)
                    {
                        servidorAlternativo = null;
                        if (!EsServidorNoEncontrado(ex)) throw; // el servidor respondió con otro error (usuario, base...)
                    }
                }
            }
            return AbrirCon(constructor.ConnectionString);
        }

        private static SqlConnection AbrirCon(string cadena)
        {
            SqlConnection conexion = new SqlConnection(cadena);
            try
            {
                conexion.Open();
                return conexion;
            }
            catch
            {
                conexion.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Errores de conexión ("no se encontró el servidor", tiempo agotado, errores de LocalDB), a diferencia de
        /// los que da un servidor que sí respondió (usuario sin permiso, base inexistente...), que son de clase menor a 20.
        /// </summary>
        private static bool EsServidorNoEncontrado(SqlException ex)
        {
            return ex.Class >= 20 || ex.Number == -1 || ex.Number == -2 || ex.Number == 2 || ex.Number == 53 || ex.Number == 26;
        }

        /// <summary>
        /// Tubería del servidor de LocalDB en marcha, según su registro (error.log), o null si no hay ninguno.
        /// </summary>
        private static string TuberiaLocalDbActiva()
        {
            try
            {
                string registro = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Microsoft", "Microsoft SQL Server Local DB", "Instances", InstanciaLocalDb, "error.log");
                if (!File.Exists(registro)) return null;
                string texto;
                using (FileStream flujo = new FileStream(registro, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                using (StreamReader lector = new StreamReader(flujo, true))
                {
                    texto = lector.ReadToEnd();
                }
                // Si el último arranque ya se apagó, la tubería no sirve.
                int arranque = texto.LastIndexOf("is now ready for client connections", StringComparison.Ordinal);
                if (arranque < 0 || texto.IndexOf("shutdown has been initiated", arranque, StringComparison.OrdinalIgnoreCase) >= 0) return null;
                MatchCollection tuberias = Regex.Matches(texto, @"\\\\\.\\pipe\\LOCALDB#[0-9A-F]+\\tsql\\query");
                return tuberias.Count > 0 ? tuberias[tuberias.Count - 1].Value : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Si el error indica que LocalDB no está instalado en este equipo (error 52 de la capa de red de SQL Server;
        /// el 50 es otro: LocalDB está instalado pero no pudo arrancar).
        /// </summary>
        public static bool FaltaLocalDb(Exception ex)
        {
            return Regex.IsMatch(ex.ToString(), @"error:\s*52\b") ||
                   ex.ToString().IndexOf("Unable to locate a Local Database Runtime installation", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
