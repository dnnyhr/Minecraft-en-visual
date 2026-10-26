# BlockForge: traspaso a Claude Code

- Proyecto: Windows Forms, .NET 10 (net10.0-windows), ImplicitUsings/Nullable desactivados.
- Datos: SQL Server Express LocalDB `(localdb)\MSSQLLocalDB`, base BlockForgeDB. Paquete único Microsoft.Data.SqlClient 7.1.1. Cadena de conexión en Data/Conexion.cs.
- La base se crea sola al iniciar (Database/01_CrearBaseDatos.sql, esquema versión 2). Si existe una base versión 1 (2D) se migra con Database/02_MigrarVersion2.sql dentro de una transacción.
- Hash de contraseñas: SHA-256(sal + contraseña UTF-8). Cuentas: admin/Admin2026, jugador1/Jugador2026.
- Capas: Forms (8 formularios), Models, Services, Data, Helpers, Controls/VistaMundo3D.cs y Motor/ (OpenGL 1.1 por P/Invoke, sin paquetes).

## Estado (versión 2, 3D estilo ClassiCube)
- Compila con `dotnet build` sin advertencias. Probado en Windows 11 con LocalDB 17 y GPU AMD:
  creación y migración de la base, inicio de sesión, creación de mundos, juego (romper, colocar, vuelo),
  guardado con posición del jugador y mundo máximo de 256×256×128 (abre en ~2,5 s).
- Mundos guardados como VARBINARY GZip en Mundo.Bloques (ya no existe la tabla Bloque).

## Pendiente
- Actualizar el documento (01_Documento), el modelo ER, el diagrama de clases y las capturas de pantalla a la versión 3D.
- Enlace de Figma (6.6).
