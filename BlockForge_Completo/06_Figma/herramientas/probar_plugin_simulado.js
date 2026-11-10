// Prueba el plugin (plugin/code.js) sin abrir Figma: simula la parte de la API de Figma que usa y
// falla como Figma en los errores típicos (tamaños de 0, texto sin fuente cargada, colores fuera de rango...).
// Uso: node probar_plugin_simulado.js <carpeta de exportación>
const fs = require('fs');
const path = require('path');
const vm = require('vm');

const carpeta = process.argv[2];
const datos = JSON.parse(fs.readFileSync(path.join(carpeta, 'pantallas.json'), 'utf8'));
const cargadas = new Set();
let id = 0, nodos = 0, textos = 0, imagenesCreadas = 0;
const mensajes = [];
const falla = (m) => { throw new Error(m); };
const revisarColor = (p) => {
  for (const c of ['r', 'g', 'b']) if (!(p.color[c] >= 0 && p.color[c] <= 1)) falla('color fuera de rango');
  if (p.opacity !== undefined && !(p.opacity >= 0 && p.opacity <= 1)) falla('opacidad fuera de rango');
};

function nodo(tipo) {
  nodos++;
  const n = { id: '1:' + (++id), type: tipo, children: [], x: 0, y: 0, width: 100, height: 100, fills: [], strokes: [], pluginData: {} };
  n.resize = (w, h) => { if (!(w >= 0.01 && h >= 0.01)) falla(`resize(${w}, ${h}) en ${n.name}`); n.width = w; n.height = h; };
  n.appendChild = (h) => { if (!['FRAME', 'PAGE'].includes(n.type)) falla('appendChild en ' + n.type); n.children.push(h); h.parent = n; };
  n.setPluginData = (k, v) => { n.pluginData[k] = v; };
  n.remove = () => { if (n.parent) n.parent.children.splice(n.parent.children.indexOf(n), 1); };
  n.setReactionsAsync = async (r) => { if (!r[0].actions[0].destinationId) falla('reacción sin destino'); n.reactions = r; };
  return new Proxy(n, {
    set(o, k, v) {
      if (k === 'fills' || k === 'strokes') v.forEach((p) => { if (p.type === 'SOLID') revisarColor(p); else if (p.type === 'IMAGE' && !p.imageHash) falla('imagen sin hash'); });
      if (o.type === 'TEXT' && ['characters', 'fontSize', 'lineHeight', 'textAlignHorizontal'].includes(k)) {
        if (!o.fontName || !cargadas.has(o.fontName.family + '|' + o.fontName.style)) falla(`${k} sin cargar la fuente (${o.name})`);
      }
      if (k === 'characters') { o.width = Math.max(...v.split('\n').map((l) => l.length)) * o.fontSize * 0.55; o.height = v.split('\n').length * 16; }
      if ((k === 'x' || k === 'y') && !Number.isFinite(v)) falla(`${k} no es un número en ${o.name}`);
      if (k === 'textAlignHorizontal' && !['LEFT', 'CENTER', 'RIGHT'].includes(v)) falla('alineación inválida ' + v);
      o[k] = v;
      return true;
    }
  });
}

const figma = {
  showUI() {},
  ui: { postMessage: (m) => mensajes.push(m), onmessage: null },
  createPage: () => nodo('PAGE'),
  createFrame: () => nodo('FRAME'),
  createRectangle: () => nodo('RECTANGLE'),
  createText: () => { textos++; return nodo('TEXT'); },
  createImage: (b) => { if (!(b instanceof Uint8Array) || b.length < 8) falla('imagen inválida'); imagenesCreadas++; return { hash: 'h' + imagenesCreadas }; },
  setCurrentPageAsync: async () => {},
  listAvailableFontsAsync: async () => ['Regular', 'Bold', 'Semibold', 'Light', 'Semilight'].map((s) => ({ fontName: { family: 'Segoe UI', style: s } }))
    .concat(['Regular', 'Bold', 'Semi Bold', 'Light'].map((s) => ({ fontName: { family: 'Inter', style: s } }))),
  loadFontAsync: async (f) => { cargadas.add(f.family + '|' + f.style); },
  viewport: { scrollAndZoomIntoView() {} }
};

const imagenes = {};
const usar = (ruta) => { imagenes[ruta] = new Uint8Array(fs.readFileSync(path.join(carpeta, ruta))); };
const recorrer = (n) => { if (n.archivo) usar(n.archivo); (n.hijos || []).forEach(recorrer); };
datos.pantallas.forEach((p) => { usar(p.captura); p.nodos.forEach(recorrer); });

vm.runInNewContext(fs.readFileSync(path.join(__dirname, '..', 'plugin', 'code.js'), 'utf8'), { figma, __html__: '', console });
figma.ui.onmessage({ tipo: 'importar', datos, imagenes, prototipo: true, referencia: true }).then(() => {
  mensajes.filter((m) => m.texto).forEach((m) => console.log((m.error ? 'ERROR ' : '') + m.texto));
  console.log(`Nodos: ${nodos} · textos: ${textos} · imágenes: ${imagenesCreadas}`);
  process.exit(mensajes.some((m) => m.error && m.texto.startsWith('Error')) ? 1 : 0);
});
