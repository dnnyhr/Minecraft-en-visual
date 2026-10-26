using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using BlockForge.Motor;

namespace BlockForge.Forms
{
    /// <summary>
    /// Gráficos de la interfaz: el logotipo, los íconos de bloques (con las mismas texturas del juego)
    /// y el paisaje de bloques de las cabeceras.
    /// </summary>
    internal static class Dibujo
    {
        private static readonly Dictionary<string, Bitmap> cacheIconos = new Dictionary<string, Bitmap>();
        private static readonly Dictionary<Size, Bitmap> cachePaisajes = new Dictionary<Size, Bitmap>();

        /// <summary>Logotipo de BlockForge: un bloque de pasto en perspectiva isométrica.</summary>
        public static void Logotipo(Graphics g, Rectangle area)
        {
            IconoBloque(g, area, "Pasto");
        }

        /// <summary>Cubo isométrico del bloque con sus texturas y el sombreado clásico por cara.</summary>
        public static void IconoBloque(Graphics g, Rectangle area, string nombreBloque)
        {
            int lado = Math.Min(area.Width, area.Height);
            string clave = nombreBloque + "|" + lado;
            Bitmap icono;
            if (!cacheIconos.TryGetValue(clave, out icono))
            {
                icono = CrearIcono(nombreBloque, lado);
                cacheIconos[clave] = icono;
            }
            g.DrawImage(icono, area.X + (area.Width - lado) / 2, area.Y + (area.Height - lado) / 2, lado, lado);
        }

        private static Bitmap CrearIcono(string nombre, int lado)
        {
            int arriba, costado;
            Atlas.TilesPorNombre(nombre, out arriba, out costado);
            Bitmap bmp = new Bitmap(lado, lado, PixelFormat.Format32bppArgb);
            using (Bitmap texArriba = Atlas.BitmapTile(arriba))
            using (Bitmap texLado = Atlas.BitmapTile(costado))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                float r = lado / 2f;
                float cx = r, cy = r;
                // Cara superior (rombo), izquierda y derecha como paralelogramos.
                Cara(g, texArriba, 1f, new PointF(cx, cy - r), new PointF(cx + r * 0.95f, cy - r / 2), new PointF(cx - r * 0.95f, cy - r / 2));
                Cara(g, texLado, 0.8f, new PointF(cx - r * 0.95f, cy - r / 2), new PointF(cx, cy), new PointF(cx - r * 0.95f, cy + r / 2));
                Cara(g, texLado, 0.6f, new PointF(cx, cy), new PointF(cx + r * 0.95f, cy - r / 2), new PointF(cx, cy + r));
            }
            return bmp;
        }

        /// <summary>Dibuja la textura deformada: puntos = esquina superior izquierda, superior derecha e inferior izquierda.</summary>
        private static void Cara(Graphics g, Bitmap textura, float luz, PointF a, PointF b, PointF c)
        {
            using (ImageAttributes atributos = new ImageAttributes())
            {
                ColorMatrix m = new ColorMatrix(new[]
                {
                    new float[] { luz, 0, 0, 0, 0 },
                    new float[] { 0, luz, 0, 0, 0 },
                    new float[] { 0, 0, luz, 0, 0 },
                    new float[] { 0, 0, 0, 1, 0 },
                    new float[] { 0, 0, 0, 0, 1 }
                });
                atributos.SetColorMatrix(m);
                atributos.SetWrapMode(WrapMode.TileFlipXY);
                g.DrawImage(textura, new[] { a, b, c }, new RectangleF(0, 0, textura.Width, textura.Height), GraphicsUnit.Pixel, atributos);
            }
        }

        /// <summary>
        /// Paisaje de bloques (cielo, nubes, colinas, árboles y agua) para las cabeceras.
        /// "escala" es el factor de DPI del control (1 a 96 ppp, 2 a 192 ppp).
        /// </summary>
        public static void Paisaje(Graphics g, Rectangle area, float escala)
        {
            if (area.Width <= 0 || area.Height <= 0) return;
            Bitmap bmp;
            if (!cachePaisajes.TryGetValue(area.Size, out bmp))
            {
                bmp = CrearPaisaje(area.Width, area.Height, Math.Max(8, (int)Math.Round(14 * escala)));
                cachePaisajes[area.Size] = bmp;
            }
            g.DrawImageUnscaled(bmp, area.Location);
        }

        /// <summary>Factor de escala del control según sus puntos por pulgada.</summary>
        public static float Escala(System.Windows.Forms.Control control)
        {
            return control.DeviceDpi / 96f;
        }

        private static Bitmap CrearPaisaje(int ancho, int alto, int b)
        {
            Bitmap bmp = new Bitmap(ancho, alto, PixelFormat.Format32bppArgb);
            Random azar = new Random(7);
            using (Graphics g = Graphics.FromImage(bmp))
            using (Bitmap pastoLado = Atlas.BitmapTile(1))
            using (Bitmap tierra = Atlas.BitmapTile(2))
            using (Bitmap piedra = Atlas.BitmapTile(3))
            using (Bitmap agua = Atlas.BitmapTile(5))
            using (Bitmap arena = Atlas.BitmapTile(6))
            using (Bitmap tronco = Atlas.BitmapTile(7))
            using (Bitmap hojas = Atlas.BitmapTile(9))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                using (LinearGradientBrush cielo = new LinearGradientBrush(new Rectangle(0, 0, ancho, alto),
                           Color.FromArgb(92, 152, 240), Color.FromArgb(196, 224, 255), LinearGradientMode.Vertical))
                {
                    g.FillRectangle(cielo, 0, 0, ancho, alto);
                }
                using (SolidBrush nube = new SolidBrush(Color.FromArgb(230, 255, 255, 255)))
                {
                    for (int i = 0; i < ancho / 140 + 2; i++)
                    {
                        int nx = azar.Next(-40, ancho), ny = azar.Next(6, Math.Max(8, alto / 3));
                        int largo = azar.Next(3, 7);
                        for (int k = 0; k < largo; k++) g.FillRectangle(nube, nx + k * b, ny + (k % 2 == 0 ? 0 : -b / 2), b, b / 2);
                        g.FillRectangle(nube, nx + b, ny + b / 2, (largo - 2) * b, b / 2);
                    }
                }

                int columnas = ancho / b + 1;
                int nivelAgua = alto - 3 * b;
                double fase = azar.NextDouble() * 6;
                for (int c = 0; c < columnas; c++)
                {
                    double onda = Math.Sin(c / 6.0 + fase) * 1.6 + Math.Sin(c / 2.7) * 0.7;
                    int superficie = alto - (int)Math.Round(4.2 + onda) * b;
                    int x = c * b;
                    for (int y = superficie; y < alto; y += b)
                    {
                        Bitmap tex = y == superficie ? (superficie >= nivelAgua - b ? arena : pastoLado) : (y > superficie + 2 * b ? piedra : (superficie >= nivelAgua - b ? arena : tierra));
                        g.DrawImage(tex, x, y, b, b);
                    }
                    if (superficie > nivelAgua)
                    {
                        using (ImageAttributes semitransparente = new ImageAttributes())
                        {
                            ColorMatrix m = new ColorMatrix { Matrix33 = 0.8f };
                            semitransparente.SetColorMatrix(m);
                            for (int y = nivelAgua; y < superficie; y += b)
                            {
                                g.DrawImage(agua, new Rectangle(x, y, b, b), 0, 0, 16, 16, GraphicsUnit.Pixel, semitransparente);
                            }
                        }
                    }
                    else if (c > 1 && c < columnas - 2 && azar.NextDouble() < 0.09)
                    {
                        // Árbol: tronco y copa.
                        for (int t = 1; t <= 3; t++) g.DrawImage(tronco, x, superficie - t * b, b, b);
                        for (int dx = -2; dx <= 2; dx++)
                        {
                            for (int dy = 3; dy <= 5; dy++)
                            {
                                if (Math.Abs(dx) == 2 && dy == 5) continue;
                                g.DrawImage(hojas, x + dx * b, superficie - dy * b, b, b);
                            }
                        }
                    }
                }
                // Oscurece suavemente la parte baja para que el texto se lea mejor.
                using (LinearGradientBrush sombra = new LinearGradientBrush(new Rectangle(0, 0, ancho, alto),
                           Color.FromArgb(0, 0, 0, 0), Color.FromArgb(90, 0, 0, 0), LinearGradientMode.Vertical))
                {
                    g.FillRectangle(sombra, 0, 0, ancho, alto);
                }
            }
            return bmp;
        }

        /// <summary>Texto grande con sombra (para títulos sobre el paisaje).</summary>
        public static void TextoConSombra(Graphics g, string texto, Font fuente, Color color, PointF posicion)
        {
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            using (SolidBrush sombra = new SolidBrush(Color.FromArgb(150, 0, 0, 0)))
            using (SolidBrush pincel = new SolidBrush(color))
            {
                g.DrawString(texto, fuente, sombra, posicion.X + 2, posicion.Y + 2);
                g.DrawString(texto, fuente, pincel, posicion);
            }
        }
    }
}
