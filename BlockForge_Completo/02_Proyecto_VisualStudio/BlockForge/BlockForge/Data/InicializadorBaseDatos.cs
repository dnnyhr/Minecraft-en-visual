using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Data.SqlClient;

namespace BlockForge.Data
{
    /// <summary>
    /// Crea BlockForgeDB la primera vez que se ejecuta la aplicación (Database\01_CrearBaseDatos.sql)
    /// y migra las bases de la versión 1 (mundos 2D) al esquema actual (Database\02_MigrarVersion2.sql).
    /// </summary>
    /// <summary>La base está registrada en LocalDB pero sus archivos se borraron o se movieron.</summary>
    public class ArchivosBaseFaltantesException : Exception
    {
        public string Ruta { get; private set; }

        public ArchivosBaseFaltantesException(string ruta)
            : base("Los archivos de la base de datos no están en " + ruta + ".")
        {
            Ruta = ruta;
        }
    }

    public static class InicializadorBaseDatos
    {
        /// <summary>
        /// Carpeta de los archivos de las bases nuevas. Antes LocalDB los dejaba sueltos en la carpeta del usuario
        /// (C:\Users\nombre), donde es fácil borrarlos sin querer.
        /// </summary>
        public static string CarpetaDatos
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BlockForge", "Datos"); }
        }

        public static void Asegurar()
        {
            bool existe;
            using (SqlConnection master = Conexion.AbrirMaster())
            {
                SqlCommand consulta = new SqlCommand("SELECT physical_name FROM sys.master_files WHERE database_id = DB_ID(@nombre) AND type = 0", master);
                consulta.Parameters.AddWithValue("@nombre", Conexion.NombreBaseDatos);
                object archivo = consulta.ExecuteScalar();
                existe = archivo != null && archivo != DBNull.Value;
                if (existe && !File.Exists((string)archivo)) throw new ArchivosBaseFaltantesException((string)archivo);

                if (!existe)
                {
                    Directory.CreateDirectory(CarpetaDatos);
                    foreach (string lote in ObtenerLotes(LeerScript("01_CrearBaseDatos.sql")))
                    {
                        string sql = lote;
                        if (lote.Trim().StartsWith("CREATE DATABASE", StringComparison.OrdinalIgnoreCase))
                        {
                            string datos = Path.Combine(CarpetaDatos, Conexion.NombreBaseDatos);
                            sql = "CREATE DATABASE " + Conexion.NombreBaseDatos +
                                  " ON (NAME = N'" + Conexion.NombreBaseDatos + "', FILENAME = N'" + datos.Replace("'", "''") + ".mdf')" +
                                  " LOG ON (NAME = N'" + Conexion.NombreBaseDatos + "_log', FILENAME = N'" + datos.Replace("'", "''") + "_log.ldf');";
                        }
                        using (SqlCommand comando = new SqlCommand(sql, master))
                        {
                            comando.CommandTimeout = 120;
                            comando.ExecuteNonQuery();
                        }
                    }
                }
            }

            // La base nueva se crea con el esquema 3 y las migraciones posteriores se aplican igual que en una existente.
            MigrarSiHaceFalta();
        }

        /// <summary>
        /// Quita de LocalDB el registro de una base cuyos archivos ya no existen, para poder crear una nueva.
        /// No borra ningún archivo (si estaban en la Papelera, siguen ahí).
        /// </summary>
        public static void OlvidarBaseSinArchivos()
        {
            using (SqlConnection master = Conexion.AbrirMaster())
            {
                try
                {
                    new SqlCommand("DROP DATABASE [" + Conexion.NombreBaseDatos + "]", master).ExecuteNonQuery();
                }
                catch (SqlException)
                {
                    // Avisa que no encontró los archivos, pero el registro se quita igual.
                }
                new SqlCommand("CHECKPOINT", master).ExecuteNonQuery();
            }
            SqlConnection.ClearAllPools();
        }

        /// <summary>Versión actual: la 1 no tenía tabla VersionEsquema.</summary>
        private static int VersionActual(SqlConnection conexion)
        {
            SqlCommand existe = new SqlCommand("SELECT OBJECT_ID('VersionEsquema')", conexion);
            if (existe.ExecuteScalar() == DBNull.Value) return 1;
            object maxima = new SqlCommand("SELECT MAX(Version) FROM VersionEsquema", conexion).ExecuteScalar();
            return maxima == DBNull.Value ? 1 : Convert.ToInt32(maxima);
        }

        private static void MigrarSiHaceFalta()
        {
            using (SqlConnection conexion = Conexion.Abrir())
            {
                int version = VersionActual(conexion);
                if (version < 2) EjecutarMigracion(conexion, "02_MigrarVersion2.sql");
                if (version < 3) EjecutarMigracion(conexion, "03_MigrarVersion3.sql");
                if (version < 4) EjecutarMigracion(conexion, "04_MigrarVersion4.sql");
                if (version < 5) EjecutarMigracion(conexion, "05_MigrarVersion5.sql");
                if (version < 6) EjecutarMigracion(conexion, "06_MigrarVersion6.sql");
            }
        }

        private static void EjecutarMigracion(SqlConnection conexion, string script)
        {
            {
                using (SqlTransaction transaccion = conexion.BeginTransaction())
                {
                    try
                    {
                        foreach (string lote in ObtenerLotes(LeerScript(script)))
                        {
                            using (SqlCommand comando = new SqlCommand(lote, conexion, transaccion))
                            {
                                comando.CommandTimeout = 120;
                                comando.ExecuteNonQuery();
                            }
                        }
                        transaccion.Commit();
                    }
                    catch
                    {
                        transaccion.Rollback();
                        throw;
                    }
                }
            }
        }

        private static string LeerScript(string archivo)
        {
            Assembly ensamblado = Assembly.GetExecutingAssembly();
            string recurso = null;
            foreach (string nombre in ensamblado.GetManifestResourceNames())
            {
                if (nombre.EndsWith(archivo, StringComparison.OrdinalIgnoreCase)) recurso = nombre;
            }
            if (recurso == null)
            {
                throw new InvalidOperationException("No se encontró el script " + archivo + ".");
            }
            using (Stream flujo = ensamblado.GetManifestResourceStream(recurso))
            using (StreamReader lector = new StreamReader(flujo, Encoding.UTF8))
            {
                return lector.ReadToEnd();
            }
        }

        /// <summary>Divide el script en lotes separados por líneas GO, como hace SSMS.</summary>
        private static IEnumerable<string> ObtenerLotes(string script)
        {
            List<string> lotes = new List<string>();
            StringBuilder actual = new StringBuilder();
            foreach (string linea in script.Replace("\r\n", "\n").Split('\n'))
            {
                if (linea.Trim().Equals("GO", StringComparison.OrdinalIgnoreCase))
                {
                    if (actual.ToString().Trim().Length > 0) lotes.Add(actual.ToString());
                    actual.Clear();
                }
                else
                {
                    actual.AppendLine(linea);
                }
            }
            if (actual.ToString().Trim().Length > 0) lotes.Add(actual.ToString());
            return lotes;
        }
    }
}
