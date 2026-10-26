# BlockForge: sistema de construcción por bloques en 3D

Proyecto de Windows Forms (C# y .NET 10) de la asignatura Visual II.
La versión 2 reemplaza la cuadrícula 2D por un mundo 3D en primera persona inspirado en
[ClassiCube](https://github.com/ClassiCube/ClassiCube): terreno por chunks, iluminación por columnas,
niebla, cielo con degradado, nubes, agua translúcida, física con colisiones, vuelo y HUD.
Incluye **multijugador en red local** (chat, jugadores visibles y bloques sincronizados) y una interfaz
renovada: menú principal oscuro con tarjetas, menú de pausa con botones, inventario visual y chat dentro del juego.
La versión 4 agrega **supervivencia**: ciclo de día y noche, vida y hambre, comida, mobs pacíficos y hostiles
(incluido el **creeper**), cuevas con lava, plantas, brotes que crecen hasta ser árboles, más de 40 tipos de bloque
y fabricación; y **internet fácil con un túnel de Cloudflare** (sin abrir puertos ni crear cuentas).

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
| `Ruby`     | `Ruby2026`    | Jugador       |
| `Madai`    | `Madai2026`   | Jugador       |
| `Danilo`   | `Danilo2026`  | Jugador       |

Cada usuario puede cambiar su **nombre** y su **contraseña** desde el menú principal, con el botón
**Cambiar nombre o contraseña** (pide la contraseña actual). El nombre es con el que se inicia sesión y el que
ven los demás en el multijugador: de 4 a 20 letras o números, sin espacios y sin repetir el de otra cuenta.
Conviene que cada uno cambie la contraseña de prueba por una propia.

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
| Clic izquierdo              | Romper el bloque apuntado, o golpear al mob que tengas delante |
| Clic derecho                | Colocar el bloque activo; con comida en la mano, **mantener para comer** |
| Clic central                | Poner en la barra el bloque apuntado                           |
| 1 a 9 / rueda del mouse     | Cambiar el espacio de la barra                                 |
| E                           | Inventario y **fabricación** (clic en un objeto para ponerlo en la barra, clic en una receta para fabricarla) |
| Ctrl + I                    | Inventario en ventana                                          |
| T o Enter                   | Chat y comandos (`/dia`, `/noche`, `/pacifico`, `/ayuda`)      |
| Tab (mantener)              | Lista de jugadores conectados                                  |
| F5                          | Cámara en tercera persona (para ver tu skin)                   |
| Ctrl + S                    | Guardar (también guarda la posición del jugador)               |
| R                           | Volver al punto de aparición                                   |
| F3                          | Información de depuración (FPS, posición, chunks)              |
| F11                         | Pantalla completa                                              |

Los mundos nuevos empiezan con el inventario vacío: primero hay que romper bloques para luego poder construir.
El agua y la lava no se recolectan y la capa inferior del mundo no se puede romper.
El mouse se lee con **Raw Input** (el movimiento crudo del ratón, sin la aceleración de Windows), así que va fluido
tanto con mouse como con trackpad; la sensibilidad se ajusta en **Configuración**.

## Supervivencia

- **Vida (10 corazones) y hambre (10 muslos)**, sobre la barra. Correr, saltar y nadar dan hambre.
  - Los corazones **solo se recuperan con la barra de hambre casi llena** (9 muslos o más): para curarte, come.
  - Con **3 muslos o menos no puedes correr**. Con el hambre en cero pierdes vida (hasta quedar con medio corazón).
  - Hacen daño las caídas de más de 3 bloques, la **lava**, ahogarse (aparecen burbujas bajo el agua) y los mobs.
  - Al morir aparece **Reaparecer**: vuelves al punto de aparición y conservas el inventario.
  - Si quieres solo construir, activa el **vuelo (F)**: volando no recibes daño ni te atacan.
- **Comida:** elige la comida en la barra y **mantén clic derecho** (no se come con la barra de hambre llena).
  Manzana (de las hojas), chuletas, filetes y pollo (de los animales; cocidos llenan mucho más) y carne podrida (de los zombis, a veces da náuseas).
- **Día y noche:** un día dura 20 minutos; el reloj está arriba a la derecha. De noche aparecen mobs hostiles en la
  superficie, y en las cuevas oscuras a cualquier hora. Las **antorchas** y las **lámparas** iluminan.
- **Mobs:**
  | Mob | Tipo | Suelta |
  |---|---|---|
  | Cerdo, vaca, oveja, gallina | Pacíficos (huyen al golpearlos) | Chuleta, filete, lana blanca, pollo |
  | Zombi | Hostil (arde al sol) | Carne podrida |
  | Araña | Hostil, rápida, trepa paredes | — |
  | **Creeper** | Hostil: se acerca en silencio, se infla parpadeando y **explota**, rompiendo bloques | — |
  Con `/pacifico` (en el chat) no aparecen mobs hostiles.
- **Brotes:** al romper hojas a veces cae un **brote**; plántalo sobre pasto o tierra y en 1 a 3 minutos (con luz) crece un árbol.
- **Plantas:** rosas, dientes de león, acianos y hierba alta en el pasto. Las flores sirven para teñir lana.

### Fabricación (tecla E)

| Resultado | Ingredientes |
|---|---|
| 4 Madera | 1 Tronco |
| 4 Antorcha | 1 Carbón + 1 Madera |
| 2 Madera oscura | 2 Madera + 1 Carbón |
| 2 Vidrio | 2 Arena + 1 Carbón |
| 2 Ladrillo / 1 Terracota | 2 Arcilla + 1 Carbón / 1 Arcilla + 1 Carbón |
| 4 Ladrillos de piedra | 4 Piedra |
| 2 Piedra lisa | 2 Piedra + 1 Carbón |
| 1 Estantería | 6 Madera |
| 1 Lámpara | 1 Vidrio + 2 Antorcha |
| Lana roja / amarilla / azul / negra | Lana blanca + Rosa / Diente de león / Aciano / Carbón |
| Chuleta, filete o pollo cocido | La carne cruda + 1 Carbón |
| 1 Lingote de hierro | 1 Mineral de hierro + 1 Carbón |
| Espada de madera / piedra / hierro / diamante | 2 Madera / 2 Roca + 1 Madera / 2 Lingotes + 1 Madera / 2 Diamantes + 1 Madera |
| 1 Bloque de hierro / de diamante | 9 Lingotes / 9 Diamantes |
| 1 Roca · 1 Piedra musgosa · 1 Obsidiana | 1 Piedra · 1 Roca + 1 Hojas · 1 Piedra + 2 Carbón |
| Lanas naranja, verde, celeste, morada, rosa y gris | Mezclando lanas (y hojas para la verde) |

El **carbón** sale del mineral de carbón (vetas oscuras en la piedra), la **arcilla** del fondo de los lagos,
el **hierro** de las vetas beige bajo tierra y el **diamante** (celeste) solo en lo más profundo, cerca de la lava.
Los mundos creados antes de esta versión reciben hierro y diamante bajo tierra al abrirlos.
En el inventario aparecen primero las recetas que puedes fabricar.

### Espadas y daño

| En la mano | Daño por golpe |
|---|---|
| Nada, un bloque u otro objeto | 1 corazón |
| Espada de madera | 2 corazones |
| Espada de piedra | 2,5 corazones |
| Espada de hierro | 3 corazones |
| Espada de diamante | 3,5 corazones |

## Texturas de Minecraft Classic

En el juego, menú **Ver → Texturas**:

- **Descargar las de Minecraft Classic (Mojang):** baja el cliente oficial de Minecraft Classic 0.30 desde
  `launcher.mojang.com` (unos 290 KB), como hace ClassiCube, y guarda solo su `terrain.png` en `%LocalAppData%\BlockForge`.
- **Elegir un terrain.png:** cualquier paquete de texturas con el formato de Classic (16×16 tiles; 256×256 o mayor).
- **Minecraft Classic / BlockForge:** cambia entre esas texturas y el pixel art propio sin salir del mundo.

Los bloques que Classic no tiene (diamante, comida, antorchas, espadas, etc.) usan siempre el pixel art de BlockForge.
Las texturas de Classic son de Mojang: **no se incluyen en el proyecto ni en el repositorio**, cada equipo las descarga.

## Multijugador (red local e internet)

- **Hospedar:** en el menú principal, **Hospedar partida** y elijan un mundo; o, dentro de cualquier mundo,
  **Esc → Abrir a la red / internet** (también en el menú *Multijugador*). Se elige:
  - **Red local:** equipos del mismo Wi-Fi/router. La partida aparece sola en la lista de los demás.
  - **Internet fácil (túnel de Cloudflare) — recomendado:** no hay que tocar el router ni crear cuentas, y funciona
    aunque el proveedor use CGNAT. Solo el **anfitrión** necesita **cloudflared**, el programa oficial y gratuito de
    Cloudflare; se instala una vez desde una terminal (PowerShell) con:
    ```
    winget install --id Cloudflare.cloudflared -e --source winget
    ```
    Al abrir la partida, Cloudflare crea una dirección como `https://palabras-al-azar.trycloudflare.com` y **tarda
    hasta un minuto en publicarla**: el chat dice «Preparando...» y avisa cuando ya funciona. Entonces **Esc → Copiar
    invitación** y mándenla. Los demás la pegan en **Unirse a partida** (si entran justo después, el juego reintenta
    solo durante un minuto y medio). La dirección cambia cada vez que se abre la partida.
  - **Internet abriendo un puerto (avanzado):** BlockForge pide al router que abra el puerto (**UPnP**) y muestra tu
    **IP pública** (por ejemplo `181.45.3.2:25570`).
  - **Contraseña** (recomendada en internet), **puerto** y **máximo de jugadores**.
  La primera vez, Windows pregunta si permite el acceso a la red: acepten.
- **Unirse:** **Unirse a partida**, elijan una de la lista o **peguen la invitación** (`https://...`, IP:puerto o un nombre de dominio)
  y la contraseña si la hay. Para probar en un solo equipo, abran dos veces la aplicación y únanse a `127.0.0.1`.
- El **anfitrión** es dueño del mundo: los cambios de todos se guardan en su base de datos cuando él guarda.
  Los **invitados** empiezan con el inventario vacío y no guardan nada; si el anfitrión sale, la partida termina.
- El anfitrión puede escribir en el chat **/expulsar nombre** o **/jugadores**.
- Los mobs, la hora del día y las explosiones los controla el anfitrión; cada jugador tiene su propia vida y hambre.
- Se ve lo que cada jugador lleva en la mano y su brazo al golpear, romper o construir.
- **PvP:** con clic izquierdo se golpea a otros jugadores (se ponen rojos y retroceden); el daño depende de la espada.
  Se activa o desactiva al abrir la partida o con **/pvp** en el chat del anfitrión. Volando no te atacan los mobs,
  pero sí los jugadores. Al morir, el chat dice quién te mató.
- Puertos: TCP 25570 (si está ocupado se usa el siguiente, hasta 25579) y UDP 25571 para anunciar en la red local.
  El mismo puerto TCP acepta también conexiones WebSocket, que es lo que llega por los túneles.

### Con ngrok u otro túnel propio

Si prefieren **ngrok**, abran la partida en **Red local** y ejecuten `ngrok http 25570` (o `cloudflared tunnel --url http://127.0.0.1:25570`).
Peguen en **Unirse a partida** la dirección `https://...` que muestre. También sirve `ngrok tcp 25570`: en ese caso
la invitación es la dirección `0.tcp.ngrok.io:12345` que da ngrok.

### Si por internet no pueden entrar (opción avanzada)

Si usan la opción de abrir un puerto, el chat dice si el puerto se abrió y si hay algún problema
(si no se resuelve, usen el túnel de Cloudflare):

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
    │   ├── VistaMundo3D.Interfaz.cs  Menú de pausa, inventario y fabricación, chat, jugadores remotos
    │   ├── VistaMundo3D.Juego.cs  Supervivencia: comer, combate, explosiones, brotes, sol y luna, corazones
    │   └── TarjetaMenu.cs         Botones grandes del menú principal
    ├── Red/                       Multijugador
    │   ├── Protocolo.cs           Tipos de mensaje y formato
    │   ├── Conexion.cs            Conexión (TCP o WebSocket) con hilos de lectura y escritura
    │   ├── PartidaRed.cs          Anfitrión, invitado, contraseña, expulsión, mobs y descubrimiento por UDP
    │   ├── WebSockets.cs          WebSocket sobre el mismo puerto (para túneles https)
    │   ├── Tunel.cs               Túnel rápido de Cloudflare (cloudflared)
    │   └── Internet.cs            UPnP (abrir el puerto del router) e IP pública
    ├── Motor/                     Motor 3D
    │   ├── GL.cs                  Enlace con OpenGL 1.1, WGL y Win32 (P/Invoke, Raw Input del mouse)
    │   ├── Atlas.cs, AtlasSprites.cs  Texturas 16×16 generadas por código (bloques, plantas, comida, espadas, HUD)
    │   ├── TexturasClassic.cs     terrain.png de Minecraft Classic (descarga desde Mojang o archivo propio)
    │   ├── RenderMundo.cs         Chunks de 16³, luz del sol con tinte del día y luz de antorchas/lava
    │   ├── CicloDia.cs            Hora, color del cielo y de la niebla
    │   ├── Supervivencia.cs       Vida, hambre, saturación, aire y daño
    │   ├── Mobs.cs, ModeloMob.cs  Inteligencia, aparición y modelos de los mobs
    │   ├── Fisica.cs              Colisiones de cajas (mobs)
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
        ├── 03_MigrarVersion3.sql  Skins de los usuarios
        ├── 04_MigrarVersion4.sql  Bloques nuevos, comida, hora del día, vida y hambre
        ├── 05_MigrarVersion5.sql  Hierro, diamante, espadas y más bloques
        └── 06_MigrarVersion6.sql  Cuentas de Ruby, Madai y Danilo
```

### Cambios en la base de datos (versión 5)

- `TipoBloque.Categoria` pasa a `VARCHAR(12)` y admite `Herramienta`; se agregan 19 tipos (hierro, diamante, espadas,
  roca, piedra musgosa, obsidiana y seis lanas). Script: `Database/05_MigrarVersion5.sql`.

### Cambios en la base de datos (versión 4)

- `TipoBloque` agrega `Categoria` (Bloque, Planta, Liquido, Comida, Material), `Alimento` y `Luz`, y 31 tipos nuevos.
- `Mundo` agrega `HoraDia`, `JugadorVida` y `JugadorHambre` (se guardan con Ctrl+S).
- La migración se aplica sola al abrir la aplicación.

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
- **"Could not load file or assembly ... BlockForge.dll. Una directiva de Control de aplicaciones bloqueó este archivo (0x800711C7)":**
  es **Smart App Control** de Windows 11, que juzga los programas sin firma digital (como cualquier proyecto recién
  compilado). Suele bastar con **Compilar → Recompilar solución** y volver a ejecutar; el registro está en el Visor de
  eventos → Registros de aplicaciones → Microsoft → Windows → CodeIntegrity → Operational. Desactivar Smart App Control
  también lo evita, pero en Windows 11 no se puede volver a activar sin reinstalar, así que no lo recomendamos.
- **"No fue posible iniciar la vista 3D (OpenGL)":** actualicen el controlador de la tarjeta de video. En máquinas virtuales o escritorio remoto sin aceleración, OpenGL puede no estar disponible.
- **"Cannot open database BlockForgeDB requested by the login" o "No se encuentran los archivos de la base de datos":**
  se borraron o se movieron `BlockForgeDB.mdf` y `BlockForgeDB_log.ldf`. Las bases antiguas los tenían sueltos en la
  carpeta del usuario (`C:\Users\nombre`), donde es fácil borrarlos sin querer; las nuevas los guardan en
  `%LocalAppData%\BlockForge\Datos`. Si están en la **Papelera**, restáuralos y abre BlockForge de nuevo para recuperar
  los mundos. Si no, al abrir BlockForge aparece la opción de **crear una base nueva y vacía**.
- **Volver a crear la base desde cero:** en Visual Studio, abran **Ver → Explorador de objetos de SQL Server**, conéctense a `(localdb)\MSSQLLocalDB`, eliminen `BlockForgeDB` y ejecuten la aplicación de nuevo.
- **Usar otro servidor SQL:** cambien la constante `CadenaConexion` en `Data/Conexion.cs`.
- **No aparece la partida en «Unirse a partida»:** escriban la dirección del anfitrión a mano. Si tampoco conecta, revisen que ambos equipos estén en la misma red y que el Firewall de Windows permita BlockForge en redes privadas (Panel de control → Firewall de Windows Defender → Permitir una aplicación).
