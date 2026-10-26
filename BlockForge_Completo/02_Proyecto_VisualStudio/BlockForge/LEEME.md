# BlockForge: sistema de construcción por bloques en 3D

Proyecto de Windows Forms (C# y .NET 10) de la asignatura Visual II.
La versión 2 reemplaza la cuadrícula 2D por un mundo 3D en primera persona inspirado en
[ClassiCube](https://github.com/ClassiCube/ClassiCube): terreno por chunks, iluminación por columnas,
niebla, cielo con degradado, nubes, agua translúcida, física con colisiones, vuelo y HUD.
Incluye **multijugador en red local** (chat, jugadores visibles y bloques sincronizados) y una interfaz
renovada: menú principal oscuro con tarjetas, menú de pausa con botones, inventario visual y chat dentro del juego.

## Requisitos

1. **Visual Studio 2026** (Community es suficiente) con la carga de trabajo **Desarrollo de escritorio de .NET**.
2. **SQL Server Express LocalDB**, que se instala con Visual Studio. Si no lo tienen:
   - Abran el **Visual Studio Installer** y elijan **Modificar**.
   - En **Componentes individuales**, marquen **SQL Server Express LocalDB** e instálenlo.
3. Una tarjeta de video con **OpenGL** (cualquier equipo con controladores actualizados lo tiene).
4. Conexión a internet la **primera vez** que se compila. Visual Studio descarga el paquete NuGet
   **Microsoft.Data.SqlClient** desde nuget.org (el archivo `NuGet.Config` de la solución ya apunta a ese origen).
   El motor 3D no usa paquetes: llama directamente a `opengl32.dll`, que viene con Windows.

## Cómo ejecutarlo

1. Descompriman la carpeta y abran **BlockForge.sln** con Visual Studio.
2. Presionen **F5** (o el botón ▶ BlockForge).
3. La primera vez, la aplicación crea sola la base de datos **BlockForgeDB** en LocalDB, con sus tablas y datos iniciales.
   Si encuentra una base de la versión 1 (mundos 2D), la migra sola al esquema nuevo; los mundos antiguos se
   regeneran en 3D con su misma semilla al abrirlos.

### Cuentas de prueba

| Usuario    | Contraseña    | Rol           |
|------------|---------------|---------------|
| `admin`    | `Admin2026`   | Administrador |
| `jugador1` | `Jugador2026` | Jugador       |

## Controles del juego

Hagan **clic en la vista** para capturar el mouse; **Esc** lo libera y abre el **menú de pausa**
(Volver al juego, Inventario, Guardar, Abrir a la red / internet, Copiar invitación, Controles, Salir).

| Tecla / botón               | Acción                                                         |
|-----------------------------|----------------------------------------------------------------|
| Mouse                       | Mirar alrededor                                                |
| W A S D                     | Moverse                                                        |
| Espacio                     | Saltar (en el agua, nadar hacia arriba)                        |
| Shift                       | Correr (en vuelo, bajar)                                       |
| F                           | Activar o desactivar el vuelo                                  |
| Clic izquierdo              | Romper el bloque apuntado y recolectarlo (se puede mantener)   |
| Clic derecho                | Colocar el bloque activo (descuenta una unidad)                |
| Clic central                | Elegir como activo el bloque apuntado                          |
| 1 a 9 / rueda del mouse     | Cambiar el bloque activo                                       |
| E                           | Inventario visual (clic en un bloque para usarlo)              |
| Ctrl + I                    | Inventario en ventana                                          |
| T o Enter                   | Chat (multijugador)                                            |
| Tab (mantener)              | Lista de jugadores conectados                                  |
| F5                          | Cámara en tercera persona (para ver tu skin)                   |
| Ctrl + S                    | Guardar (también guarda la posición del jugador)               |
| R                           | Volver al punto de aparición                                   |
| F3                          | Información de depuración (FPS, posición, chunks)              |
| F11                         | Pantalla completa                                              |

Los mundos nuevos empiezan con el inventario vacío: primero hay que romper bloques para luego poder construir.
El agua no se recolecta y la capa inferior del mundo no se puede romper.

## Multijugador (red local e internet)

- **Hospedar:** en el menú principal, **Hospedar partida** y elijan un mundo; o, dentro de cualquier mundo,
  **Esc → Abrir a la red / internet** (también en el menú *Multijugador*). Se elige:
  - **Red local:** equipos del mismo Wi-Fi/router. La partida aparece sola en la lista de los demás.
  - **Internet:** BlockForge pide al router que abra el puerto (**UPnP**) y muestra tu **IP pública**.
    En el menú de pausa, **Copiar invitación** copia la dirección (por ejemplo `181.45.3.2:25570`) para mandarla por chat.
  - **Contraseña** (recomendada en internet), **puerto** y **máximo de jugadores**.
  La primera vez, Windows pregunta si permite el acceso a la red: acepten.
- **Unirse:** **Unirse a partida**, elijan una de la lista o **peguen la invitación** (IP:puerto o un nombre de dominio)
  y la contraseña si la hay. Para probar en un solo equipo, abran dos veces la aplicación y únanse a `127.0.0.1`.
- El **anfitrión** es dueño del mundo: los cambios de todos se guardan en su base de datos cuando él guarda.
  Los **invitados** empiezan con el inventario vacío y no guardan nada; si el anfitrión sale, la partida termina.
- El anfitrión puede escribir en el chat **/expulsar nombre** o **/jugadores**.
- Puertos: TCP 25570 (si está ocupado se usa el siguiente, hasta 25579) y UDP 25571 para anunciar en la red local.

### Si por internet no pueden entrar

Al abrir la partida a internet, el chat dice si el puerto se abrió y si hay algún problema:

1. **"Tu router no permite abrir puertos automáticamente":** el router tiene UPnP desactivado. Actívenlo en la
   configuración del router, o creen a mano un **reenvío de puerto** (*port forwarding*) TCP 25570 hacia la IP local
   del anfitrión (la que muestra el chat).
2. **"Tu router no tiene IP pública (CGNAT)":** el proveedor de internet comparte la IP entre varios clientes y no se
   puede abrir ningún puerto. Usen una **red privada virtual gratuita**: todos instalan **Radmin VPN**, **ZeroTier**
   o **Tailscale**, entran a la misma red y juegan como si fuera **red local** (unirse con la IP que muestre la VPN).
   Otra opción es un túnel como **playit.gg**, que da una dirección pública tipo `algo.playit.gg:12345` que se pega
   en **Unirse a partida**.
3. Desde la misma casa del anfitrión, usen la dirección de **red local**: la IP pública suele funcionar solo desde fuera.

## Skins de Minecraft

En el menú principal, **Mi skin**:

- **Cuenta de Minecraft:** escriban el nombre de una cuenta de Minecraft Java y presionen **Buscar**: BlockForge
  descarga esa skin de los servidores públicos de Mojang (también detecta el modelo de brazos delgados "Alex").
- **Archivo PNG:** carguen cualquier skin en formato de Minecraft (64×64, o 64×32 del formato antiguo).
- La skin se guarda en su usuario (tabla `Usuario`, columnas `SkinPng`, `SkinDelgada`, `SkinOrigen`), se ve en
  **tercera persona (F5)** y se envía a los demás jugadores en las partidas multijugador. Se dibuja con el mapeo de
  Minecraft, incluida la segunda capa (sombrero, chaqueta, mangas y pantalón).

## Estructura

```
BlockForge/
├── BlockForge.sln
├── NuGet.Config
└── BlockForge/
    ├── Program.cs                 Punto de entrada (crea o migra la BD)
    ├── Forms/                     9 formularios (cada uno con su .Designer.cs) y Dibujo.cs (íconos y paisajes)
    ├── Controls/
    │   ├── VistaMundo3D.cs        Control OpenGL: bucle del juego, entrada, cámara y HUD
    │   ├── VistaMundo3D.Interfaz.cs  Menú de pausa, inventario visual, chat, jugadores remotos
    │   └── TarjetaMenu.cs         Botones grandes del menú principal
    ├── Red/                       Multijugador
    │   ├── Protocolo.cs           Tipos de mensaje y formato
    │   ├── Conexion.cs            Conexión TCP con hilos de lectura y escritura
    │   ├── PartidaRed.cs          Anfitrión, invitado, contraseña, expulsión y descubrimiento por UDP
    │   └── Internet.cs            UPnP (abrir el puerto del router) e IP pública
    ├── Motor/                     Motor 3D
    │   ├── GL.cs                  Enlace con OpenGL 1.1, WGL y Win32 (P/Invoke)
    │   ├── Atlas.cs               Texturas 16×16 generadas por código y apariencia de cada bloque
    │   ├── RenderMundo.cs         Chunks de 16³ en display lists, sombras y bordes del mapa
    │   ├── Jugador.cs             Física: gravedad, salto, colisiones, nado y vuelo
    │   ├── Rayo.cs                Selección del bloque apuntado (DDA)
    │   ├── Particulas.cs          Fragmentos al romper bloques
    │   ├── ModeloJugador.cs       Personaje con skin de Minecraft (mapeo oficial, segunda capa, Alex)
    │   ├── TextoGL.cs             Texto del HUD
    │   └── Matematica.cs          Vectores, matrices y frustum
    ├── Models/                    Clases de datos (Mundo es un arreglo 3D de bytes)
    ├── Services/                  Reglas del sistema (GeneradorTerreno crea colinas, lagos, playas y árboles)
    ├── Data/                      Repositorios con ADO.NET (la cadena de conexión está en Conexion.cs)
    ├── Helpers/                   Seguridad, validaciones, mensajes y estilos
    └── Database/
        ├── 01_CrearBaseDatos.sql  Esquema versión 2
        ├── 02_MigrarVersion2.sql  Migración desde la versión 1 (2D)
        └── 03_MigrarVersion3.sql  Skins de los usuarios
```

### Cambios en la base de datos (versión 2)

- `Mundo` tiene `Ancho` (X), `Largo` (Z) y `Alto` (Y), y guarda los bloques en `Bloques` (VARBINARY comprimido con GZip,
  un byte por celda, como los mapas de ClassiCube) junto con la posición del jugador. Desaparece la tabla `Bloque`:
  un mundo 3D tiene cientos de miles de celdas y guardarlas fila por fila era demasiado lento.
- `Configuracion` reemplaza el tamaño de celda y la cuadrícula por **distancia de visión**, **campo de visión**
  y **sensibilidad del mouse**, y agrega el largo predeterminado.
- `TipoBloque` agrega Arena, Tronco, Hojas y Grava.
- Nueva tabla `VersionEsquema`.

## Problemas comunes

- **Triángulos amarillos en Dependencias → Paquetes, o errores como "no se encontró el tipo SqlConnection":** los paquetes NuGet no se descargaron.
  1. Verifiquen que haya internet.
  2. Clic derecho sobre la solución → **Restaurar paquetes NuGet**.
  3. **Compilar → Recompilar solución**.
  4. Si sigue igual: **Herramientas → Opciones → Administrador de paquetes NuGet → Orígenes de paquetes**, y comprueben que **nuget.org** (`https://api.nuget.org/v3/index.json`) esté marcado.
- **Error NETSDK1045 ("no admite .NET 10.0"):** su Visual Studio es anterior a 2026. Actualícenlo o, como alternativa, cambien `net10.0-windows` por `net8.0-windows` en `BlockForge.csproj`.
- **"No fue posible preparar la base de datos":** LocalDB no está instalado o no ha arrancado. Abran una terminal y ejecuten `sqllocaldb info`. Si aparece `MSSQLLocalDB`, ejecuten `sqllocaldb start MSSQLLocalDB` y vuelvan a intentar. La primera conexión puede tardar unos segundos mientras LocalDB arranca.
- **"No fue posible iniciar la vista 3D (OpenGL)":** actualicen el controlador de la tarjeta de video. En máquinas virtuales o escritorio remoto sin aceleración, OpenGL puede no estar disponible.
- **Volver a crear la base desde cero:** en Visual Studio, abran **Ver → Explorador de objetos de SQL Server**, conéctense a `(localdb)\MSSQLLocalDB`, eliminen `BlockForgeDB` y ejecuten la aplicación de nuevo.
- **Usar otro servidor SQL:** cambien la constante `CadenaConexion` en `Data/Conexion.cs`.
- **No aparece la partida en «Unirse a partida»:** escriban la dirección del anfitrión a mano. Si tampoco conecta, revisen que ambos equipos estén en la misma red y que el Firewall de Windows permita BlockForge en redes privadas (Panel de control → Firewall de Windows Defender → Permitir una aplicación).
