using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;

namespace BlockForge.Motor
{
    internal enum TipoMob : byte
    {
        Cerdo, Vaca, Oveja, Gallina, Zombi, Creeper, Arana
    }

    /// <summary>Cómo se mueve una pieza al caminar.</summary>
    internal enum Animacion
    {
        Ninguna, PiernaA, PiernaB, Cabeza, BrazoZombiA, BrazoZombiB, PataArana
    }

    /// <summary>Una caja del modelo, en píxeles (16 píxeles = 1 bloque). El frente mira hacia -Z.</summary>
    internal class Pieza
    {
        public float Px, Py, Pz;              // punto de giro
        public float X0, Y0, Z0;              // esquina de la caja, relativa al punto de giro
        public int W, H, D;                   // tamaño en píxeles
        public float Rx, Ry, Rz;              // giro fijo en grados
        public Animacion Animacion;
        public float Fase;                    // desfase de la animación (patas de araña)
        public Color Color;
        public int Variacion = 10;
        public Action<Pintor, Pieza> Decorar;
        public int U, V;                      // posición en la textura (se calcula sola)

        public Pieza(float px, float py, float pz, float x0, float y0, float z0, int w, int h, int d, Color color, Animacion anim = Animacion.Ninguna)
        {
            Px = px; Py = py; Pz = pz; X0 = x0; Y0 = y0; Z0 = z0; W = w; H = h; D = d; Color = color; Animacion = anim;
        }

        public Rectangle Frente { get { return new Rectangle(U + D, V + D, W, H); } }
        public Rectangle Arriba { get { return new Rectangle(U + D, V, W, D); } }
        public Rectangle LadoDerecho { get { return new Rectangle(U, V + D, D, H); } }
        public Rectangle LadoIzquierdo { get { return new Rectangle(U + D + W, V + D, D, H); } }
        public Rectangle Espalda { get { return new Rectangle(U + 2 * D + W, V + D, W, H); } }
    }

    /// <summary>Pinta la textura de un mob (pixel art por código).</summary>
    internal class Pintor
    {
        public readonly int Ancho, Alto;
        public readonly int[] Pixeles;
        public readonly Random Azar;

        public Pintor(int ancho, int alto, int semilla)
        {
            Ancho = ancho; Alto = alto;
            Pixeles = new int[ancho * alto];
            Azar = new Random(semilla);
        }

        public void Punto(int x, int y, Color c)
        {
            if (x < 0 || y < 0 || x >= Ancho || y >= Alto) return;
            Pixeles[y * Ancho + x] = c.ToArgb();
        }

        public void Rect(Rectangle r, Color c, int variacion)
        {
            for (int y = r.Top; y < r.Bottom; y++)
            {
                for (int x = r.Left; x < r.Right; x++)
                {
                    int d = variacion > 0 ? Azar.Next(-variacion, variacion + 1) : 0;
                    Punto(x, y, Color.FromArgb(Lim(c.R + d), Lim(c.G + d), Lim(c.B + d)));
                }
            }
        }

        public void Rect(int x, int y, int w, int h, Color c)
        {
            Rect(new Rectangle(x, y, w, h), c, 0);
        }

        private static int Lim(int v) { return Math.Max(0, Math.Min(255, v)); }
    }

    /// <summary>Modelo de un tipo de mob: piezas, textura y tamaño.</summary>
    internal class ModeloMob
    {
        public readonly List<Pieza> Piezas = new List<Pieza>();
        public float Escala = 1f / 16f;
        public int AnchoTextura = 64, AltoTextura = 64;
        public int[] Textura;
        public uint IdTextura;

        public void Agregar(Pieza p) { Piezas.Add(p); }

        /// <summary>Acomoda las piezas en la textura y las pinta.</summary>
        public void Pintar(int semilla)
        {
            int x = 0, y = 0, altoFila = 0;
            foreach (Pieza p in Piezas)
            {
                int rw = 2 * (p.D + p.W), rh = p.D + p.H;
                if (x + rw > AnchoTextura) { x = 0; y += altoFila; altoFila = 0; }
                p.U = x; p.V = y;
                x += rw;
                altoFila = Math.Max(altoFila, rh);
            }
            while (y + altoFila > AltoTextura) AltoTextura *= 2;
            Pintor pintor = new Pintor(AnchoTextura, AltoTextura, semilla);
            foreach (Pieza p in Piezas)
            {
                pintor.Rect(new Rectangle(p.U + p.D, p.V, 2 * p.W, p.D), p.Color, p.Variacion);
                pintor.Rect(new Rectangle(p.U, p.V + p.D, 2 * (p.D + p.W), p.H), p.Color, p.Variacion);
                if (p.Decorar != null) p.Decorar(pintor, p);
            }
            Textura = pintor.Pixeles;
        }

        public void Subir()
        {
            if (IdTextura != 0) return;
            uint[] id = new uint[1];
            GL.glGenTextures(1, id);
            GL.glBindTexture(GL.TEXTURE_2D, id[0]);
            GL.glTexParameteri(GL.TEXTURE_2D, GL.TEXTURE_MIN_FILTER, GL.NEAREST);
            GL.glTexParameteri(GL.TEXTURE_2D, GL.TEXTURE_MAG_FILTER, GL.NEAREST);
            GL.glTexParameteri(GL.TEXTURE_2D, GL.TEXTURE_WRAP_S, GL.CLAMP);
            GL.glTexParameteri(GL.TEXTURE_2D, GL.TEXTURE_WRAP_T, GL.CLAMP);
            GCHandle h = GCHandle.Alloc(Textura, GCHandleType.Pinned);
            try
            {
                GL.glTexImage2D(GL.TEXTURE_2D, 0, GL.RGBA, AnchoTextura, AltoTextura, 0, GL.BGRA_EXT, GL.UNSIGNED_BYTE, h.AddrOfPinnedObject());
            }
            finally
            {
                h.Free();
            }
            IdTextura = id[0];
        }

        public void Liberar()
        {
            if (IdTextura != 0) GL.glDeleteTextures(1, new[] { IdTextura });
            IdTextura = 0;
        }

        /// <summary>Dibuja las piezas (sin la transformación del mob, que pone el llamador).</summary>
        public void DibujarPiezas(float balanceo, float inclinacion, float luz, float tiempo)
        {
            GL.glBindTexture(GL.TEXTURE_2D, IdTextura);
            foreach (Pieza p in Piezas)
            {
                GL.glPushMatrix();
                GL.glTranslatef(p.Px * Escala, p.Py * Escala, p.Pz * Escala);
                if (p.Ry != 0) GL.glRotatef(p.Ry, 0, 1, 0);
                float rx = p.Rx;
                switch (p.Animacion)
                {
                    case Animacion.PiernaA: rx += balanceo; break;
                    case Animacion.PiernaB: rx -= balanceo; break;
                    case Animacion.Cabeza: rx += (float)(inclinacion * 180 / Math.PI); break;
                    case Animacion.BrazoZombiA: rx += balanceo * 0.3f + (float)Math.Sin(tiempo * 2) * 3; break;
                    case Animacion.BrazoZombiB: rx -= balanceo * 0.3f + (float)Math.Sin(tiempo * 2) * 3; break;
                    case Animacion.PataArana: GL.glRotatef((float)Math.Sin(p.Fase) * balanceo * 0.6f, 0, 1, 0); break;
                }
                if (p.Rz != 0) GL.glRotatef(p.Rz, 0, 0, 1);
                if (rx != 0) GL.glRotatef(rx, 1, 0, 0);
                GL.glScalef(Escala * 16f, Escala * 16f, Escala * 16f);
                ModeloJugador.Caja(p.X0, p.Y0, p.Z0, p.X0 + p.W, p.Y0 + p.H, p.Z0 + p.D, p.U, p.V, p.W, p.H, p.D, false, 0, luz, AltoTextura, AnchoTextura);
                GL.glPopMatrix();
            }
        }

        // ------------------------------------------------------------------ modelos
        private static readonly Color Negro = Color.FromArgb(20, 20, 20), Blanco = Color.FromArgb(240, 240, 240);

        private static void Ojos(Pintor p, Rectangle cara, int fila, int separacion, Color iris)
        {
            int cx = cara.Left + cara.Width / 2;
            p.Rect(cx - separacion - 2, cara.Top + fila, 2, 1, Blanco);
            p.Rect(cx + separacion, cara.Top + fila, 2, 1, Blanco);
            p.Punto(cx - separacion - 1, cara.Top + fila, iris);
            p.Punto(cx + separacion, cara.Top + fila, iris);
        }

        public static ModeloMob Crear(TipoMob tipo)
        {
            ModeloMob m = new ModeloMob();
            switch (tipo)
            {
                case TipoMob.Cerdo:
                    {
                        Color rosa = Color.FromArgb(236, 160, 158);
                        m.Agregar(new Pieza(0, 0, 0, -5, 6, -8, 10, 8, 16, rosa));
                        m.Agregar(new Pieza(0, 12, -8, -4, -4, -8, 8, 8, 8, rosa, Animacion.Cabeza)
                        {
                            Decorar = (p, s) => { Ojos(p, s.Frente, 3, 2, Negro); }
                        });
                        m.Agregar(new Pieza(0, 12, -8, -2, -3, -9, 4, 3, 1, Color.FromArgb(220, 130, 130), Animacion.Cabeza)
                        {
                            Decorar = (p, s) => { p.Punto(s.Frente.Left + 1, s.Frente.Top + 1, Color.FromArgb(120, 60, 60)); p.Punto(s.Frente.Left + 2, s.Frente.Top + 1, Color.FromArgb(120, 60, 60)); }
                        });
                        Patas(m, 3, 6, 5, 6, rosa, Color.FromArgb(150, 100, 100));
                        break;
                    }
                case TipoMob.Vaca:
                    {
                        Color cafe = Color.FromArgb(84, 58, 40);
                        Action<Pintor, Pieza> manchas = (p, s) =>
                        {
                            for (int i = 0; i < 6; i++)
                            {
                                int x = s.U + p.Azar.Next(2 * (s.D + s.W) - 4), y = s.V + s.D + p.Azar.Next(Math.Max(1, s.H - 3));
                                p.Rect(new Rectangle(x, y, 3 + p.Azar.Next(3), 2 + p.Azar.Next(3)), Color.FromArgb(232, 232, 228), 6);
                            }
                        };
                        m.Agregar(new Pieza(0, 0, 0, -6, 10, -9, 12, 10, 18, cafe) { Decorar = manchas });
                        m.Agregar(new Pieza(0, 18, -9, -4, -4, -6, 8, 8, 6, cafe, Animacion.Cabeza)
                        {
                            Decorar = (p, s) =>
                            {
                                p.Rect(new Rectangle(s.Frente.Left + 2, s.Frente.Top + 5, 4, 3), Color.FromArgb(200, 180, 170), 6);
                                Ojos(p, s.Frente, 2, 2, Negro);
                            }
                        });
                        m.Agregar(new Pieza(0, 18, -9, -5, 3, -4, 1, 3, 1, Color.FromArgb(220, 214, 196), Animacion.Cabeza));
                        m.Agregar(new Pieza(0, 18, -9, 4, 3, -4, 1, 3, 1, Color.FromArgb(220, 214, 196), Animacion.Cabeza));
                        m.Agregar(new Pieza(0, 0, 0, -2, 9, 2, 4, 1, 4, Color.FromArgb(230, 160, 160)));
                        Patas(m, 4, 10, 6, 10, cafe, Color.FromArgb(40, 30, 24));
                        break;
                    }
                case TipoMob.Oveja:
                    {
                        Color lana = Color.FromArgb(232, 232, 226);
                        m.Agregar(new Pieza(0, 0, 0, -5, 9, -8, 10, 10, 16, lana) { Variacion = 14 });
                        m.Agregar(new Pieza(0, 17, -8, -3, -3, -6, 6, 6, 6, Color.FromArgb(214, 190, 170), Animacion.Cabeza)
                        {
                            Decorar = (p, s) => { p.Rect(new Rectangle(s.Arriba.Left, s.Arriba.Top, s.Arriba.Width, s.Arriba.Height), lana, 10); Ojos(p, s.Frente, 2, 1, Negro); }
                        });
                        Patas(m, 3, 9, 5, 9, Color.FromArgb(214, 190, 170), Color.FromArgb(120, 100, 90));
                        break;
                    }
                case TipoMob.Gallina:
                    {
                        Color pluma = Color.FromArgb(244, 244, 240);
                        m.Agregar(new Pieza(0, 0, 0, -3, 4, -4, 6, 6, 8, pluma));
                        m.Agregar(new Pieza(0, 9, -4, -2, 0, -3, 4, 6, 3, pluma, Animacion.Cabeza)
                        {
                            Decorar = (p, s) => { p.Punto(s.Frente.Left, s.Frente.Top + 1, Negro); p.Punto(s.Frente.Right - 1, s.Frente.Top + 1, Negro); }
                        });
                        m.Agregar(new Pieza(0, 9, -4, -2, 2, -5, 4, 2, 2, Color.FromArgb(240, 180, 40), Animacion.Cabeza));
                        m.Agregar(new Pieza(0, 9, -4, -1, 0, -4, 2, 2, 1, Color.FromArgb(220, 40, 40), Animacion.Cabeza));
                        m.Agregar(new Pieza(0, 0, 0, -4, 5, -3, 1, 4, 6, pluma));
                        m.Agregar(new Pieza(0, 0, 0, 3, 5, -3, 1, 4, 6, pluma));
                        m.Agregar(new Pieza(-1.5f, 4, 0, -0.5f, -4, -0.5f, 1, 4, 1, Color.FromArgb(240, 180, 40), Animacion.PiernaA));
                        m.Agregar(new Pieza(1.5f, 4, 0, -0.5f, -4, -0.5f, 1, 4, 1, Color.FromArgb(240, 180, 40), Animacion.PiernaB));
                        break;
                    }
                case TipoMob.Zombi:
                    {
                        Color piel = Color.FromArgb(96, 150, 86), camisa = Color.FromArgb(46, 138, 148), pantalon = Color.FromArgb(58, 54, 132);
                        m.Escala = 0.95f / 16f;
                        m.Agregar(new Pieza(0, 24, 0, -4, 0, -4, 8, 8, 8, piel, Animacion.Cabeza)
                        {
                            Decorar = (p, s) =>
                            {
                                p.Rect(s.Frente.Left + 1, s.Frente.Top + 3, 2, 2, Color.FromArgb(20, 30, 20));
                                p.Rect(s.Frente.Right - 3, s.Frente.Top + 3, 2, 2, Color.FromArgb(20, 30, 20));
                                p.Rect(s.Frente.Left + 2, s.Frente.Top + 6, 4, 1, Color.FromArgb(50, 80, 46));
                            }
                        });
                        m.Agregar(new Pieza(0, 0, 0, -4, 12, -2, 8, 12, 4, camisa));
                        m.Agregar(new Pieza(6, 22, 0, -2, -10, -2, 4, 12, 4, piel, Animacion.BrazoZombiA) { Rx = -90 });
                        m.Agregar(new Pieza(-6, 22, 0, -2, -10, -2, 4, 12, 4, piel, Animacion.BrazoZombiB) { Rx = -90 });
                        m.Agregar(new Pieza(2, 12, 0, -2, -12, -2, 4, 12, 4, pantalon, Animacion.PiernaA));
                        m.Agregar(new Pieza(-2, 12, 0, -2, -12, -2, 4, 12, 4, pantalon, Animacion.PiernaB));
                        break;
                    }
                case TipoMob.Creeper:
                    {
                        Color verde = Color.FromArgb(88, 170, 70);
                        Action<Pintor, Pieza> camuflaje = (p, s) =>
                        {
                            Color[] tonos = { Color.FromArgb(60, 140, 50), Color.FromArgb(96, 186, 76), Color.FromArgb(130, 200, 110), Color.FromArgb(70, 120, 60) };
                            for (int y = s.V; y < s.V + s.D + s.H; y++)
                            {
                                for (int x = s.U; x < s.U + 2 * (s.D + s.W); x++)
                                {
                                    if (y < s.V + s.D && (x < s.U + s.D || x >= s.U + s.D + 2 * s.W)) continue;
                                    p.Punto(x, y, tonos[p.Azar.Next(tonos.Length)]);
                                }
                            }
                        };
                        m.Agregar(new Pieza(0, 18, 0, -4, 0, -4, 8, 8, 8, verde, Animacion.Ninguna)
                        {
                            Decorar = (p, s) =>
                            {
                                camuflaje(p, s);
                                Rectangle c = s.Frente;
                                Color o = Color.FromArgb(16, 16, 16);
                                p.Rect(c.Left + 1, c.Top + 2, 2, 2, o);
                                p.Rect(c.Left + 5, c.Top + 2, 2, 2, o);
                                p.Rect(c.Left + 3, c.Top + 4, 2, 2, o);
                                p.Rect(c.Left + 2, c.Top + 5, 1, 2, o);
                                p.Rect(c.Left + 5, c.Top + 5, 1, 2, o);
                            }
                        });
                        m.Agregar(new Pieza(0, 0, 0, -4, 6, -2, 8, 12, 4, verde) { Decorar = camuflaje });
                        m.Agregar(new Pieza(2, 6, -4, -2, -6, -2, 4, 6, 4, verde, Animacion.PiernaA) { Decorar = camuflaje });
                        m.Agregar(new Pieza(-2, 6, -4, -2, -6, -2, 4, 6, 4, verde, Animacion.PiernaB) { Decorar = camuflaje });
                        m.Agregar(new Pieza(2, 6, 4, -2, -6, -2, 4, 6, 4, verde, Animacion.PiernaB) { Decorar = camuflaje });
                        m.Agregar(new Pieza(-2, 6, 4, -2, -6, -2, 4, 6, 4, verde, Animacion.PiernaA) { Decorar = camuflaje });
                        break;
                    }
                case TipoMob.Arana:
                    {
                        Color cuerpo = Color.FromArgb(52, 44, 40);
                        m.Agregar(new Pieza(0, 9, 3, -5, -4, 0, 10, 8, 12, cuerpo));
                        m.Agregar(new Pieza(0, 9, 0, -3, -3, -3, 6, 6, 6, Color.FromArgb(70, 60, 54)));
                        m.Agregar(new Pieza(0, 9, -3, -4, -4, -8, 8, 8, 8, cuerpo, Animacion.Cabeza)
                        {
                            Decorar = (p, s) =>
                            {
                                Color rojo = Color.FromArgb(220, 30, 30);
                                p.Rect(s.Frente.Left + 1, s.Frente.Top + 3, 2, 1, rojo);
                                p.Rect(s.Frente.Right - 3, s.Frente.Top + 3, 2, 1, rojo);
                                p.Punto(s.Frente.Left + 3, s.Frente.Top + 2, rojo);
                                p.Punto(s.Frente.Right - 4, s.Frente.Top + 2, rojo);
                            }
                        });
                        float[] zs = { -2f, -0.7f, 0.7f, 2f };
                        float[] abre = { -35f, -12f, 12f, 35f };
                        for (int i = 0; i < 4; i++)
                        {
                            m.Agregar(new Pieza(3, 9, zs[i], 0, -1, -1, 15, 2, 2, Color.FromArgb(40, 34, 30), Animacion.PataArana) { Ry = abre[i], Rz = -32, Fase = i * 1.7f });
                            m.Agregar(new Pieza(-3, 9, zs[i], -15, -1, -1, 15, 2, 2, Color.FromArgb(40, 34, 30), Animacion.PataArana) { Ry = -abre[i], Rz = 32, Fase = i * 1.7f + 1.5f });
                        }
                        break;
                    }
            }
            m.Pintar((int)tipo * 977 + 13);
            return m;
        }

        private static void Patas(ModeloMob m, float x, float alto, float z, int largo, Color color, Color pezuna)
        {
            Action<Pintor, Pieza> deco = (p, s) => p.Rect(new Rectangle(s.U, s.V + s.D + s.H - 1, 2 * (s.D + s.W), 1), pezuna, 4);
            m.Agregar(new Pieza(x, alto, -z, -2, -largo, -2, 4, largo, 4, color, Animacion.PiernaA) { Decorar = deco });
            m.Agregar(new Pieza(-x, alto, -z, -2, -largo, -2, 4, largo, 4, color, Animacion.PiernaB) { Decorar = deco });
            m.Agregar(new Pieza(x, alto, z, -2, -largo, -2, 4, largo, 4, color, Animacion.PiernaB) { Decorar = deco });
            m.Agregar(new Pieza(-x, alto, z, -2, -largo, -2, 4, largo, 4, color, Animacion.PiernaA) { Decorar = deco });
        }
    }
}
