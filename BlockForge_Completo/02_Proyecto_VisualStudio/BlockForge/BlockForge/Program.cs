using System;
using System.Windows.Forms;
using BlockForge.Data;
using BlockForge.Forms;
using BlockForge.Helpers;
using BlockForge.Services;

namespace BlockForge
{
    internal static class Program
    {
        /// <summary>Punto de entrada de la aplicación.</summary>
        [STAThread]
        private static void Main()
        {
#if !MONO
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
#endif
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                // Crea BlockForgeDB la primera vez y carga el catálogo de bloques.
                InicializadorBaseDatos.Asegurar();
                CatalogoBloques.Cargar();
            }
            catch (Exception ex)
            {
                Mensajes.Error("No fue posible preparar la base de datos BlockForgeDB.\n\n" +
                    "Verifique que SQL Server Express LocalDB esté instalado (se incluye con Visual Studio) " +
                    "y revise la cadena de conexión en Data\\Conexion.cs.\n\nDetalle: " + ex.Message);
                return;
            }

            Application.Run(new FrmAcceso());
        }
    }
}
