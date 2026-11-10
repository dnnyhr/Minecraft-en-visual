// Plugin de Figma: reconstruye las pantallas REALES de BlockForge a partir de la exportación
// "BlockForge.exe --exportar-figma" (pantallas.json + capturas + imágenes).
//
// Cada control de Windows Forms se convierte en un frame con su nombre real (btnIngresar, txtUsuario...)
// y dentro sus rectángulos, bordes y textos editables, con las coordenadas, colores y fuentes exactos
// que leyó el exportador. Lo que el programa dibuja a mano (paisaje, íconos de bloques, vista 3D OpenGL)
// llega como imagen 1:1. Al lado se coloca la captura real, para compararlas.

figma.showUI(__html__, { width: 360, height: 400 });

// Navegación del prototipo (la misma que hace el código de los formularios):
// pantalla → { control: pantalla destino }.
const NAVEGACION = {
  '01_FrmAcceso': { btnIngresar: '02_FrmMenuPrincipal' },
  '02_FrmMenuPrincipal': {
    btnNuevoMundo: '03_FrmNuevoMundo', btnCargarMundo: '04_FrmCargarMundo', btnHospedar: '13_FrmOpcionesPartida',
    btnUnirse: '14_FrmUnirse', btnGestionPartidas: '09_FrmGestionPartidas', btnConfiguracion: '10_FrmConfiguracion',
    btnSkin: '12_FrmSkin', btnCuenta: '11_FrmCuenta', btnCerrarSesion: '01_FrmAcceso'
  },
  '03_FrmNuevoMundo': { btnCrear: '05_FrmMundo_Juego', btnCancelar: '02_FrmMenuPrincipal' },
  '04_FrmCargarMundo': { btnAbrir: '05_FrmMundo_Juego', btnCancelar: '02_FrmMenuPrincipal' },
  '05_FrmMundo_Juego': { vistaMundo: '06_FrmMundo_Pausa', mnuMundo: '08_FrmInventario', mnuArchivo: '02_FrmMenuPrincipal' },
  '06_FrmMundo_Pausa': { vistaMundo: '07_FrmMundo_Inventario', mnuArchivo: '02_FrmMenuPrincipal' },
  '07_FrmMundo_Inventario': { vistaMundo: '05_FrmMundo_Juego' },
  '08_FrmInventario': { btnCerrar: '05_FrmMundo_Juego' },
  '09_FrmGestionPartidas': { btnCerrar: '02_FrmMenuPrincipal' },
  '10_FrmConfiguracion': { btnGuardar: '02_FrmMenuPrincipal', btnCancelar: '02_FrmMenuPrincipal' },
  '11_FrmCuenta': { btnGuardar: '02_FrmMenuPrincipal', btnCancelar: '02_FrmMenuPrincipal' },
  '12_FrmSkin': { btnGuardar: '02_FrmMenuPrincipal', btnCancelar: '02_FrmMenuPrincipal' },
  '13_FrmOpcionesPartida': { btnAbrir: '05_FrmMundo_Juego', btnCancelar: '02_FrmMenuPrincipal' },
  '14_FrmUnirse': { btnUnirse: '05_FrmMundo_Juego', btnCancelar: '02_FrmMenuPrincipal' }
};

// Familias por orden de preferencia: la real (Segoe UI, viene con Windows), Open Sans (incluida en
// Figma y del mismo estilo humanista) e Inter. Con una sustituta se ajusta el espaciado para que cada
// texto mida exactamente lo mismo que en Windows (campo "anchos" de la exportación).
const FAMILIAS = ['Segoe UI', 'Open Sans', 'Inter'];
const ESTILOS_INTER = { Regular: 'Regular', Bold: 'Bold', Semibold: 'Semi Bold', Light: 'Light', Semilight: 'Light', Italic: 'Italic', 'Bold Italic': 'Bold Italic' };

let disponibles = null;
const usadas = new Set();
const sustituciones = new Set();

function avisar(texto, error, fin) {
  figma.ui.postMessage({ texto, error: !!error, fin: !!fin });
}

function color(hex) {
  const n = parseInt(hex.slice(1), 16);
  return { r: (n >> 16) / 255, g: ((n >> 8) & 255) / 255, b: (n & 255) / 255 };
}

function solido(hex, opacidad) {
  return { type: 'SOLID', color: color(hex), opacity: opacidad === undefined ? 1 : opacidad };
}

/** Devuelve la fuente de Figma más parecida a la que usa el control y la deja cargada. */
async function fuente(f) {
  if (!disponibles) {
    disponibles = new Set((await figma.listAvailableFontsAsync()).map((x) => x.fontName.family + '|' + x.fontName.style));
  }
  const candidatos = [];
  for (const familia of [f.familia].concat(FAMILIAS)) {
    candidatos.push({ family: familia, style: familia === 'Inter' ? (ESTILOS_INTER[f.estilo] || 'Regular') : f.estilo });
    if (f.estilo === 'Semibold') candidatos.push({ family: familia, style: 'SemiBold' }, { family: familia, style: 'Semi Bold' });
    if (f.estilo === 'Light' || f.estilo === 'Semilight') candidatos.push({ family: familia, style: 'Light' });
    if (f.estilo === 'Semilight') candidatos.push({ family: familia, style: 'SemiLight' }, { family: familia, style: 'Light' });
  }
  for (const c of candidatos) {
    if (!disponibles.has(c.family + '|' + c.style)) continue;
    const clave = c.family + '|' + c.style;
    if (!usadas.has(clave)) {
      await figma.loadFontAsync(c);
      usadas.add(clave);
    }
    if (c.family !== f.familia) sustituciones.add(f.familia + ' ' + f.estilo + ' → ' + c.family + ' ' + c.style);
    return c;
  }
  await figma.loadFontAsync({ family: 'Inter', style: 'Regular' });
  sustituciones.add(f.familia + ' ' + f.estilo + ' → Inter Regular');
  return { family: 'Inter', style: 'Regular' };
}

/** Crea el nodo de Figma; (ox, oy) es la esquina del padre, k la escala de DPI de la exportación. */
async function crear(n, padre, ox, oy, k, imagenes, porNombre) {
  const x = (n.x - ox) / k, y = (n.y - oy) / k;
  const w = Math.max(n.w / k, 0.01), h = Math.max(n.h / k, 0.01);

  if (n.tipo === 'grupo') {
    const marco = figma.createFrame();
    marco.name = n.nombre;
    marco.fills = [];
    marco.clipsContent = !!n.recortar;
    marco.resize(w, h);
    marco.x = x; marco.y = y;
    padre.appendChild(marco);
    for (const hijo of n.hijos) await crear(hijo, marco, n.x, n.y, k, imagenes, porNombre);
    if (!porNombre[n.nombre]) porNombre[n.nombre] = marco;
    return;
  }

  if (n.tipo === 'rect') {
    const r = figma.createRectangle();
    r.name = n.nombre;
    r.resize(w, h);
    r.x = x; r.y = y;
    r.fills = n.relleno ? [solido(n.relleno, n.opacidad)] : [];
    if (n.borde) {
      r.strokes = [solido(n.borde, n.bordeOpacidad)];
      r.strokeWeight = n.grosor / k;
      r.strokeAlign = 'INSIDE';
    }
    if (n.radio) r.cornerRadius = n.radio / k;
    padre.appendChild(r);
    return;
  }

  if (n.tipo === 'imagen') {
    const bytes = imagenes[n.archivo];
    const r = figma.createRectangle();
    r.name = n.nombre;
    r.resize(w, h);
    r.x = x; r.y = y;
    if (bytes) {
      r.fills = [{ type: 'IMAGE', imageHash: figma.createImage(bytes).hash, scaleMode: 'FILL' }];
    } else {
      r.fills = [solido('#FF00FF')];
      avisar('Falta la imagen ' + n.archivo, true);
    }
    padre.appendChild(r);
    return;
  }

  if (n.tipo === 'texto') {
    const t = figma.createText();
    t.name = n.nombre;
    t.fontName = await fuente(n.fuente);
    t.fontSize = (/^●+$/.test(n.texto) && t.fontName.family !== n.fuente.familia ? 0.62 : 1) * n.fuente.px / k;
    t.lineHeight = { value: n.altoLinea / k, unit: 'PIXELS' };
    // Los renglones ya vienen partidos tal como los muestra Windows: el texto no vuelve a ajustarse.
    t.textAutoResize = 'WIDTH_AND_HEIGHT';
    t.characters = n.texto;
    t.fills = [solido(n.color, n.opacidad)];
    t.textAlignHorizontal = { izquierda: 'LEFT', centro: 'CENTER', derecha: 'RIGHT' }[n.alinear];
    if (n.sombra) {
      t.effects = [{
        type: 'DROP_SHADOW', visible: true, blendMode: 'NORMAL', radius: 0, spread: 0, showShadowBehindNode: false,
        offset: { x: n.sombra.dx / k, y: n.sombra.dy / k },
        color: Object.assign(color(n.sombra.color), { a: n.sombra.opacidad })
      }];
    }
    padre.appendChild(t);
    // Con una fuente sustituta, el renglón más largo se lleva al ancho exacto que tenía en Windows.
    if (t.fontName.family !== n.fuente.familia && n.anchos) {
      const lineas = n.texto.split('\n');
      let i = 0;
      for (let j = 1; j < lineas.length; j++) if (n.anchos[j] > n.anchos[i]) i = j;
      if (n.anchos[i] > 0 && lineas[i].length > 1) {
        const muestra = figma.createText();
        muestra.fontName = t.fontName; muestra.fontSize = t.fontSize; muestra.textAutoResize = 'WIDTH_AND_HEIGHT';
        muestra.characters = lineas[i];
        t.letterSpacing = { value: (n.anchos[i] / k - muestra.width) / lineas[i].length, unit: 'PIXELS' };
        muestra.remove();
      }
    }
    const libre = w - t.width;
    t.x = n.alinear === 'centro' ? x + libre / 2 : n.alinear === 'derecha' ? x + libre : x;
    t.y = y;
  }
}

async function enlazar(origen, destino) {
  const reaccion = [{
    trigger: { type: 'ON_CLICK' },
    actions: [{
      type: 'NODE', destinationId: destino.id, navigation: 'NAVIGATE', preserveScrollPosition: false,
      transition: { type: 'DISSOLVE', easing: { type: 'EASE_OUT' }, duration: 0.2 }
    }]
  }];
  if (origen.setReactionsAsync) await origen.setReactionsAsync(reaccion);
  else origen.reactions = reaccion;
}

async function importar(m) {
  const datos = m.datos;
  const pagina = figma.createPage();
  pagina.name = 'Pantallas reales · ' + datos.aplicacion + ' ' + String(datos.version).split('+')[0];
  await figma.setCurrentPageAsync(pagina);

  const pantallas = {};
  const controles = {};
  let y = 0;
  const separacion = 80;

  for (const p of datos.pantallas) {
    const k = p.escala || 1;
    const ancho = p.ancho / k, alto = p.alto / k;
    avisar('Creando ' + p.id + ' (' + p.nombre + ')…');

    const marco = figma.createFrame();
    marco.name = p.id + ' · ' + p.nombre;
    marco.resize(ancho, alto);
    marco.x = 0; marco.y = y;
    marco.clipsContent = true;
    marco.fills = [];
    pagina.appendChild(marco);
    const porNombre = {};
    for (const n of p.nodos) await crear(n, marco, 0, 0, k, m.imagenes, porNombre);
    marco.setPluginData('formulario', p.formulario);
    marco.setPluginData('tituloVentana', p.titulo);
    pantallas[p.id] = marco;
    controles[p.id] = porNombre;

    if (m.referencia && m.imagenes[p.captura]) {
      const ref = figma.createRectangle();
      ref.name = 'Captura real · ' + p.formulario + ' (referencia)';
      ref.resize(ancho, alto);
      ref.x = ancho + separacion; ref.y = y;
      ref.fills = [{ type: 'IMAGE', imageHash: figma.createImage(m.imagenes[p.captura]).hash, scaleMode: 'FILL' }];
      ref.locked = true;
      pagina.appendChild(ref);
    }

    const rotulo = figma.createText();
    await figma.loadFontAsync({ family: 'Inter', style: 'Bold' });
    rotulo.fontName = { family: 'Inter', style: 'Bold' };
    rotulo.fontSize = 16;
    rotulo.characters = p.formulario + ' — "' + p.titulo + '"  (' + ancho + ' × ' + alto + ' px)';
    rotulo.x = 0; rotulo.y = y - 28;
    pagina.appendChild(rotulo);

    y += alto + separacion + 40;
  }

  let enlaces = 0;
  if (m.prototipo) {
    for (const id in NAVEGACION) {
      for (const control in NAVEGACION[id]) {
        const origen = controles[id] && controles[id][control];
        const destino = pantallas[NAVEGACION[id][control]];
        if (origen && destino) {
          await enlazar(origen, destino);
          enlaces++;
        }
      }
    }
    if (pantallas['01_FrmAcceso']) pagina.flowStartingPoints = [{ nodeId: pantallas['01_FrmAcceso'].id, name: 'BlockForge' }];
  }

  figma.viewport.scrollAndZoomIntoView(Object.values(pantallas));
  avisar('Listo: ' + datos.pantallas.length + ' pantallas' + (m.prototipo ? ', ' + enlaces + ' enlaces de prototipo' : '') + '.');
  avisar('Fuentes usadas: ' + Array.from(usadas).map((u) => u.replace('|', ' ')).join(', '));
  if (sustituciones.size) avisar('Sustituciones (con el ancho ajustado al de Windows): ' + Array.from(sustituciones).join('; '));
  avisar('', false, true);
}

figma.ui.onmessage = async (m) => {
  if (m.tipo !== 'importar') return;
  try {
    await importar(m);
  } catch (e) {
    avisar('Error: ' + (e && e.message ? e.message : e), true, true);
  }
};
