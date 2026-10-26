using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace BlockForge.Motor
{
    /// <summary>
    /// Texto para el HUD: cada cadena se dibuja una vez con GDI+ (con sombra) en una textura
    /// y se reutiliza mientras siga en pantalla.
    /// </summary>
    internal class TextoGL : IDisposable
    {
        private class Entrada
        {
            public uint Textura;
            public int Ancho, Alto, AnchoTextura, AltoTextura;
            public int UltimoUso;
        }

        private readonly Dictionary<string, Entrada> cache = new Dictionary<string, Entrada>();
        private int cuadro;

        public void NuevoCuadro()
        {
            cuadro++;
            if (cache.Count < 160) return;
            // Libera las cadenas que no se usaron en los últimos cuadros (por ejemplo, contadores de FPS viejos).
            List<string> viejas = new List<string>();
            foreach (KeyValuePair<string, Entrada> par in cache)
            {
                if (cuadro - par.Value.UltimoUso > 120) viejas.Add(par.Key);
            }
            foreach (string clave in viejas)
            {
                GL.glDeleteTextures(1, new uint[] { cache[clave].Textura });
                cache.Remove(clave);
            }
        }

        private Entrada Obtener(string texto, float tamano, bool negrita)
        {
            string clave = tamano.ToString("0.0") + (negrita ? "b|" : "n|") + texto;
            Entrada e;
            if (cache.TryGetValue(clave, out e))
            {
                e.UltimoUso = cuadro;
                return e;
            }

            using (Font fuente = new Font("Segoe UI", tamano, negrita ? FontStyle.Bold : FontStyle.Regular, GraphicsUnit.Pixel))
            {
                Size medida;
                using (Bitmap prueba = new Bitmap(1, 1))
                using (Graphics g = Graphics.FromImage(prueba))
                {
                    SizeF s = g.MeasureString(texto, fuente, PointF.Empty, StringFormat.GenericTypographic);
                    medida = new Size((int)Math.Ceiling(s.Width) + 4, (int)Math.Ceiling(fuente.GetHeight(g)) + 4);
                }
                int at = Potencia2(medida.Width), ht = Potencia2(medida.Height);
                using (Bitmap bmp = new Bitmap(at, ht, PixelFormat.Format32bppArgb))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.Clear(Color.Transparent);
                        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                        using (SolidBrush sombra = new SolidBrush(Color.FromArgb(170, 0, 0, 0)))
                        {
                            g.DrawString(texto, fuente, sombra, 2, 2, StringFormat.GenericTypographic);
                        }
                        g.DrawString(texto, fuente, Brushes.White, 1, 1, StringFormat.GenericTypographic);
                    }
                    BitmapData datos = bmp.LockBits(new Rectangle(0, 0, at, ht), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                    uint[] id = new uint[1];
                    GL.glGenTextures(1, id);
                    GL.glBindTexture(GL.TEXTURE_2D, id[0]);
                    GL.glTexParameteri(GL.TEXTURE_2D, GL.TEXTURE_MIN_FILTER, GL.LINEAR);
                    GL.glTexParameteri(GL.TEXTURE_2D, GL.TEXTURE_MAG_FILTER, GL.LINEAR);
                    GL.glTexParameteri(GL.TEXTURE_2D, GL.TEXTURE_WRAP_S, GL.CLAMP);
                    GL.glTexParameteri(GL.TEXTURE_2D, GL.TEXTURE_WRAP_T, GL.CLAMP);
                    GL.glTexImage2D(GL.TEXTURE_2D, 0, GL.RGBA, at, ht, 0, GL.BGRA_EXT, GL.UNSIGNED_BYTE, datos.Scan0);
                    bmp.UnlockBits(datos);
                    e = new Entrada { Textura = id[0], Ancho = medida.Width, Alto = medida.Height, AnchoTextura = at, AltoTextura = ht, UltimoUso = cuadro };
                }
            }
            cache[clave] = e;
            return e;
        }

        public Size Medir(string texto, float tamano, bool negrita = false)
        {
            Entrada e = Obtener(texto, tamano, negrita);
            return new Size(e.Ancho, e.Alto);
        }

        /// <summary>Dibuja el texto en coordenadas de pantalla. alineacion: 0 izquierda, 1 centro, 2 derecha.</summary>
        public void Dibujar(string texto, float x, float y, float tamano, Color color, bool negrita = false, int alineacion = 0)
        {
            if (string.IsNullOrEmpty(texto)) return;
            Entrada e = Obtener(texto, tamano, negrita);
            if (alineacion == 1) x -= e.Ancho / 2f;
            else if (alineacion == 2) x -= e.Ancho;
            x = (float)Math.Round(x);
            y = (float)Math.Round(y);
            float u = (float)e.Ancho / e.AnchoTextura, v = (float)e.Alto / e.AltoTextura;
            GL.glBindTexture(GL.TEXTURE_2D, e.Textura);
            GL.glColor4f(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
            GL.glBegin(GL.QUADS);
            GL.glTexCoord2f(0, 0); GL.glVertex2f(x, y);
            GL.glTexCoord2f(0, v); GL.glVertex2f(x, y + e.Alto);
            GL.glTexCoord2f(u, v); GL.glVertex2f(x + e.Ancho, y + e.Alto);
            GL.glTexCoord2f(u, 0); GL.glVertex2f(x + e.Ancho, y);
            GL.glEnd();
        }

        private static int Potencia2(int n)
        {
            int p = 1;
            while (p < n) p <<= 1;
            return p;
        }

        public void Dispose()
        {
            foreach (Entrada e in cache.Values) GL.glDeleteTextures(1, new uint[] { e.Textura });
            cache.Clear();
        }
    }
}
