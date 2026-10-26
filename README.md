# Minecraft-en-visual

**BlockForge**: un juego de construcción por bloques en 3D, estilo Minecraft clásico, hecho en C# con Windows Forms
(.NET 10) y SQL Server LocalDB para la asignatura Visual II. El motor 3D está inspirado en
[ClassiCube](https://github.com/ClassiCube/ClassiCube) y usa OpenGL directamente, sin paquetes adicionales.

## Qué incluye

- Mundo 3D por chunks con iluminación, niebla, cielo, nubes, agua, árboles y física (salto, nado y vuelo).
- Inicio de sesión con roles, mundos guardados en la base de datos, inventario y administración de partidas.
- **Multijugador** en red local o por internet: chat, jugadores visibles, contraseña de partida y `/expulsar`.
- **Skins de Minecraft**: por nombre de cuenta (servidores públicos de Mojang) o desde un archivo PNG.

## Cómo ejecutarlo

Abrir `BlockForge_Completo/02_Proyecto_VisualStudio/BlockForge/BlockForge.sln` con Visual Studio 2026 y presionar **F5**.
La base de datos se crea sola la primera vez. Cuentas de prueba: `admin` / `Admin2026` y `jugador1` / `Jugador2026`.

Las instrucciones completas (requisitos, controles, multijugador por internet y problemas comunes) están en
[`LEEME.md`](BlockForge_Completo/02_Proyecto_VisualStudio/BlockForge/LEEME.md).

## Estructura

| Carpeta | Contenido |
|---|---|
| `BlockForge_Completo/01_Documento` | Documento del proyecto (APA 7) |
| `BlockForge_Completo/02_Proyecto_VisualStudio` | Solución de Visual Studio |
| `BlockForge_Completo/03_Diagramas` | Diagramas ER, de clases, casos de uso, actividad, navegación y Gantt |
| `BlockForge_Completo/04_Pantallas_Prototipo` | Prototipos de las pantallas |
| `BlockForge_Completo/05_Base_de_datos` | Script SQL de la base de datos |
