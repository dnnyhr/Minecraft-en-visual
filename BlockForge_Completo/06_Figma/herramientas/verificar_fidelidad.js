// Comprueba la fidelidad de la exportación: vuelve a dibujar cada pantalla SOLO con las capas de
// "pantallas.json" (lo mismo que crea el plugin en Figma) y la compara píxel a píxel con la captura real.
//
// Uso:  node verificar_fidelidad.js <carpeta de exportación> <carpeta de salida> [carpeta con fuentes .ttf]
//
// Necesita Node.js y Playwright (npm i playwright). Por cada pantalla escribe:
//   <id>_capas.png        las capas editables dibujadas por separado
//   <id>_comparacion.png  captura real | capas | diferencias (en rojo)
// y al final un resumen con el porcentaje de píxeles que difieren.
const fs = require('fs');
const path = require('path');
const { chromium } = require('playwright');

const [entrada, salida, fuentes] = process.argv.slice(2);
if (!entrada || !salida) {
  console.error('Uso: node verificar_fidelidad.js <exportación> <salida> [fuentes]');
  process.exit(1);
}
fs.mkdirSync(salida, { recursive: true });
const datos = JSON.parse(fs.readFileSync(path.join(entrada, 'pantallas.json'), 'utf8'));
const uri = (archivo) => 'data:image/png;base64,' + fs.readFileSync(path.join(entrada, archivo)).toString('base64');
const PESOS = { Light: 300, Semilight: 350, Regular: 400, Semibold: 600, Bold: 700 };

function rgba(hex, opacidad) {
  const n = parseInt(hex.slice(1), 16);
  return `rgba(${n >> 16},${(n >> 8) & 255},${n & 255},${opacidad === undefined ? 1 : opacidad})`;
}

// Las mismas reglas que el plugin (code.js): rectángulos con borde interior, textos con los renglones ya partidos.
function html(nodo, ox, oy, k) {
  const x = (nodo.x - ox) / k, y = (nodo.y - oy) / k, w = nodo.w / k, h = nodo.h / k;
  const caja = `position:absolute;left:${x}px;top:${y}px;width:${w}px;height:${h}px;box-sizing:border-box;`;
  if (nodo.tipo === 'grupo') {
    const hijos = nodo.hijos.map((n) => html(n, nodo.x, nodo.y, k)).join('');
    return `<div style="${caja}${nodo.recortar ? 'overflow:hidden;' : ''}">${hijos}</div>`;
  }
  if (nodo.tipo === 'rect') {
    let estilo = caja;
    if (nodo.relleno) estilo += `background:${rgba(nodo.relleno, nodo.opacidad)};`;
    if (nodo.borde) estilo += `border:${nodo.grosor / k}px solid ${rgba(nodo.borde, nodo.bordeOpacidad)};`;
    if (nodo.radio) estilo += `border-radius:${nodo.radio / k}px;`;
    return `<div style="${estilo}"></div>`;
  }
  if (nodo.tipo === 'imagen') {
    return `<img src="${uri(nodo.archivo)}" style="${caja}image-rendering:pixelated;">`;
  }
  if (nodo.tipo === 'texto') {
    const f = nodo.fuente;
    const cursiva = f.estilo.includes('Italic');
    const peso = PESOS[f.estilo.replace(' Italic', '').replace('Italic', 'Regular')] || 400;
    const alinear = { izquierda: 'left', centro: 'center', derecha: 'right' }[nodo.alinear];
    let estilo = `position:absolute;left:${x}px;top:${y}px;width:${w}px;white-space:pre;` +
      `font-family:'${f.familia}';font-size:${f.px / k}px;font-weight:${peso};${cursiva ? 'font-style:italic;' : ''}` +
      `line-height:${nodo.altoLinea / k}px;color:${rgba(nodo.color, nodo.opacidad)};text-align:${alinear};`;
    if (nodo.sombra) estilo += `text-shadow:${nodo.sombra.dx / k}px ${nodo.sombra.dy / k}px 0 ${rgba(nodo.sombra.color, nodo.sombra.opacidad)};`;
    const texto = nodo.texto.replace(/&/g, '&amp;').replace(/</g, '&lt;');
    return `<div style="${estilo}">${texto}</div>`;
  }
  return '';
}

function caras() {
  if (!fuentes) return '';
  const mapa = { 'segoeui.ttf': 400, 'segoeuib.ttf': 700, 'seguisb.ttf': 600, 'segoeuil.ttf': 300, 'segoeuisl.ttf': 350 };
  return Object.entries(mapa).filter(([a]) => fs.existsSync(path.join(fuentes, a))).map(([a, peso]) =>
    `@font-face{font-family:'Segoe UI';font-weight:${peso};src:url(data:font/ttf;base64,${fs.readFileSync(path.join(fuentes, a)).toString('base64')});}`).join('\n');
}

(async () => {
  const navegador = await chromium.launch();
  const resumen = [];
  for (const p of datos.pantallas) {
    const k = p.escala || 1;
    const ancho = Math.round(p.ancho / k), alto = Math.round(p.alto / k);
    const pagina = await navegador.newPage({ viewport: { width: ancho, height: alto }, deviceScaleFactor: k });
    const capas = p.nodos.map((n) => html(n, 0, 0, k)).join('');
    await pagina.setContent(`<style>${caras()} body{margin:0;overflow:hidden;-webkit-font-smoothing:antialiased;}</style>` +
      `<div style="position:relative;width:${ancho}px;height:${alto}px;overflow:hidden;">${capas}</div>`);
    await pagina.evaluate(() => document.fonts.ready);
    const archivoCapas = path.join(salida, p.id + '_capas.png');
    await pagina.screenshot({ path: archivoCapas });

    // Diferencias: un píxel "difiere" si algún canal se aparta más de 48 niveles (ignora el suavizado del texto).
    const r = await pagina.evaluate(async ({ real, capas, w, h }) => {
      const cargar = (src) => new Promise((ok) => { const i = new Image(); i.onload = () => ok(i); i.src = src; });
      const [a, b] = [await cargar(real), await cargar(capas)];
      const lienzo = document.createElement('canvas');
      lienzo.width = w * 3 + 40; lienzo.height = h;
      const g = lienzo.getContext('2d');
      g.fillStyle = '#888'; g.fillRect(0, 0, lienzo.width, h);
      g.drawImage(a, 0, 0); g.drawImage(b, w + 20, 0);
      const da = g.getImageData(0, 0, w, h), db = g.getImageData(w + 20, 0, w, h), dd = g.createImageData(w, h);
      let distintos = 0, suma = 0;
      for (let i = 0; i < da.data.length; i += 4) {
        const d = Math.max(Math.abs(da.data[i] - db.data[i]), Math.abs(da.data[i + 1] - db.data[i + 1]), Math.abs(da.data[i + 2] - db.data[i + 2]));
        suma += d;
        const gris = (da.data[i] + da.data[i + 1] + da.data[i + 2]) / 3 * 0.35 + 160;
        if (d > 48) { distintos++; dd.data[i] = 255; dd.data[i + 1] = 0; dd.data[i + 2] = 0; }
        else { dd.data[i] = dd.data[i + 1] = dd.data[i + 2] = gris; }
        dd.data[i + 3] = 255;
      }
      g.putImageData(dd, 2 * w + 40, 0);
      return { png: lienzo.toDataURL('image/png'), distintos: distintos / (w * h) * 100, media: suma / (w * h) };
    }, { real: uri(p.captura), capas: 'data:image/png;base64,' + fs.readFileSync(archivoCapas).toString('base64'), w: p.ancho, h: p.alto });
    fs.writeFileSync(path.join(salida, p.id + '_comparacion.png'), Buffer.from(r.png.split(',')[1], 'base64'));
    resumen.push({ pantalla: p.id, tamano: `${p.ancho}x${p.alto}`, pixelesDistintos: r.distintos.toFixed(2) + ' %', diferenciaMedia: r.media.toFixed(2) + ' / 255' });
    await pagina.close();
  }
  await navegador.close();
  console.table(resumen);
  fs.writeFileSync(path.join(salida, 'resumen.json'), JSON.stringify(resumen, null, 2));
})();
