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
                try
                {
                    InicializadorBaseDatos.Asegurar();
                }
                catch (ArchivosBaseFaltantesException ex)
                {
                    bool nueva = Mensajes.Confirmar(
                        "No se encuentran los archivos de la base de datos BlockForgeDB:\n" + ex.Ruta + "\n\n" +
                        "Probablemente se borraron o se movieron. Si están en la Papelera de reciclaje, cancela, restáuralos " +
                        "(BlockForgeDB y BlockForgeDB_log) y vuelve a abrir BlockForge para recuperar tus mundos.\n\n" +
                        "¿Crear una base de datos nueva y vacía? (Se pierden los mundos y las cuentas creadas; " +
                        "vuelven las cuentas de prueba.)", "BlockForge — Base de datos");
                    if (!nueva) return;
                    InicializadorBaseDatos.OlvidarBaseSinArchivos();
                    InicializadorBaseDatos.Asegurar();
                }
                CatalogoBloques.Cargar();
            }
            catch (Exception ex) when (Conexion.FaltaLocalDb(ex))
            {
                Mensajes.Error("Este equipo no tiene SQL Server Express LocalDB, que BlockForge usa para guardar los mundos.\n\n" +
                    "Para instalarlo:\n" +
                    "• Con Visual Studio: Visual Studio Installer → Modificar → Componentes individuales → «SQL Server Express LocalDB».\n" +
                    "• Sin Visual Studio: descarga «SQL Server Express» de Microsoft y en el instalador elige «Descargar medios» → «LocalDB».\n\n" +
                    "Después vuelve a abrir BlockForge.\n\nDetalle: " + ex.Message);
                return;
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
