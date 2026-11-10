# 06_Figma · Pantallas reales de BlockForge en Figma

Archivo de Figma con las 14 pantallas ya importadas (capas editables y prototipo navegable):
**https://www.figma.com/design/GKkvjZEH7FOK1mEm53ZHDZ**

Las pantallas **no se dibujaron a mano**: se exportaron desde el programa real y se reconstruyeron en Figma
con las mismas medidas, colores, fuentes y textos que usa Windows Forms.

## Cómo funciona

1. **Exportador** (`02_Proyecto_VisualStudio/.../Herramientas/ExportadorFigma.cs`). Al ejecutar

   ```
   BlockForge.exe --exportar-figma <carpeta>
   ```

   BlockForge abre de verdad cada formulario con datos de demostración (sin tocar la base de datos), le toma
   una captura y recorre sus controles. Así escribe `pantallas.json` con la posición, el tamaño, el color,
   la fuente y el texto exactos de cada control, y con el ancho real de cada renglón medido con el motor de
   texto de Windows. Lo que el programa dibuja a mano se guarda como imagen en `img/`: el paisaje de bloques,
   los íconos isométricos, la vista 3D de OpenGL y algunos controles nativos (casillas, deslizadores y
   flechas numéricas).
2. **Plugin de Figma** (`plugin/`). Lee esa carpeta y crea cada pantalla como frames, rectángulos y textos
   editables, con los nombres reales de los controles (`btnIngresar`, `txtUsuario`…). También enlaza los
   botones como prototipo navegable (32 enlaces) y, si se marca la opción, coloca al lado la captura real
   como referencia.
3. **Verificación** (`herramientas/`).
   - `verificar_fidelidad.js` redibuja las capas del JSON y las compara píxel a píxel con las capturas.
   - `probar_plugin_simulado.js` ejecuta el plugin contra una simulación estricta de la API de Figma.

## Instalar y usar el plugin

1. En **Figma de escritorio**: *Plugins → Development → Import plugin from manifest…* y elegir `plugin/manifest.json`.
2. Ejecutarlo (*Plugins → Development → BlockForge – Importar pantallas reales*) y elegir la carpeta `exportacion/`.
3. Fuentes: si el equipo tiene **Segoe UI** (viene con Windows), se usa esa. Si no, se usa **Open Sans** y se
   ajusta el espaciado para que cada texto mida exactamente lo mismo que en Windows.

## Fidelidad comprobada

`verificacion/` tiene, para cada pantalla, la imagen **real | Figma** lado a lado y la tabla
`resultados.md`. La diferencia media entre los renders de Figma y las capturas reales va de 0,6 a 8 sobre 255.
Los píxeles que cambian están casi todos en el suavizado de las letras. La geometría, los colores y los
textos son los mismos.

Transparencia sobre cómo se generó la exportación incluida aquí:

- Se ejecutó `BlockForge.exe` (compilado para Windows) con **Wine** en Linux. El campo `entorno` de
  `pantallas.json` lo indica.
- En lugar de Segoe UI (que no se puede redistribuir) se usó **Selawik**, la fuente libre de Microsoft pensada
  como sustituta de Segoe UI, ajustada a sus métricas. Con eso los formularios salen con los tamaños exactos
  del diseñador de Visual Studio (460 × 416, 820 × 596…).
- En Figma, las pantallas usan Open Sans con el ancho de cada texto ajustado, porque Segoe UI no está
  disponible en Figma web.

Para regenerarla en un Windows real con Segoe UI: compilar el proyecto y ejecutar
`BlockForge.exe --exportar-figma C:\ruta\exportacion`. Después, volver a importar con el plugin.
