using System;
using System.Collections.Generic;
using System.Drawing;

namespace BlockForge.Motor
{
    /// <summary>
    /// Pixel art de los bloques de construcción, plantas, comida e íconos del HUD.
    /// Todo se dibuja por código: los íconos pequeños con plantillas de 8×8 (cada carácter es un píxel doble).
    /// </summary>
    internal static partial class Atlas
    {
        private static void DibujarNuevos()
        {
            DibujarLava(Lava);
            DibujarMineral(MineralCarbon, Color.FromArgb(32, 32, 34), 401);
            DibujarRuido(Arcilla, Color.FromArgb(160, 166, 180), 8, 402);
            DibujarTablones(MaderaOscura, Color.FromArgb(88, 58, 32), 403);
            DibujarLadrillo(Ladrillo);
            DibujarLadrillosPiedra(LadrillosPiedra);
            DibujarPiedraLisa(PiedraLisa);
            DibujarRuido(Terracota, Color.FromArgb(160, 88, 58), 6, 404);
            DibujarVidrio(Vidrio);
            DibujarLana(LanaBlanca, Color.FromArgb(234, 236, 236), 405);
            DibujarLana(LanaRoja, Color.FromArgb(160, 38, 34), 406);
            DibujarLana(LanaAmarilla, Color.FromArgb(230, 198, 44), 407);
            DibujarLana(LanaAzul, Color.FromArgb(52, 58, 158), 408);
            DibujarLana(LanaNegra, Color.FromArgb(28, 28, 32), 409);
            DibujarEstanteria(EstanteriaLado);
            DibujarLampara(Lampara);
            DibujarAntorcha(Antorcha);
            DibujarFlor(Rosa, Color.FromArgb(200, 30, 40), Color.FromArgb(240, 90, 90));
            DibujarFlor(DienteLeon, Color.FromArgb(240, 205, 30), Color.FromArgb(255, 240, 120));
            DibujarFlor(Aciano, Color.FromArgb(60, 96, 216), Color.FromArgb(130, 160, 255));
            DibujarHierbaAlta(HierbaAlta);
            DibujarBrote(Brote);

            Dictionary<char, Color> carne = new Dictionary<char, Color>
            {
                { 'k', Color.FromArgb(60, 30, 30) }, { 'p', Color.FromArgb(232, 140, 136) }, { 'w', Color.FromArgb(250, 200, 196) },
                { 'f', Color.FromArgb(250, 236, 226) }
            };
            string[] chuleta =
            {
                "........",
                "..kkkkk.",
                ".kppppfk",
                "kppwppfk",
                "kpppppfk",
                "kpppppk.",
                ".kkkkk..",
                "........"
            };
            string[] muslo =
            {
                "...kkk..",
                "..kpppk.",
                ".kpwpppk",
                ".kppppk.",
                "..kppk..",
                "..fk....",
                ".ff.....",
                "ff......"
            };
            Plantilla(ChuletaCruda, chuleta, carne);
            Plantilla(ChuletaCocida, chuleta, Paleta(carne, 'p', Color.FromArgb(176, 112, 60), 'w', Color.FromArgb(214, 160, 100), 'f', Color.FromArgb(236, 210, 170)));
            Plantilla(FileteCrudo, chuleta, Paleta(carne, 'p', Color.FromArgb(196, 60, 52), 'w', Color.FromArgb(236, 120, 110), 'f', Color.FromArgb(240, 220, 210)));
            Plantilla(FileteCocido, chuleta, Paleta(carne, 'p', Color.FromArgb(118, 70, 40), 'w', Color.FromArgb(160, 104, 64), 'f', Color.FromArgb(200, 160, 120)));
            Plantilla(PolloCrudo, muslo, Paleta(carne, 'p', Color.FromArgb(240, 200, 180), 'w', Color.FromArgb(255, 230, 220), 'f', Color.FromArgb(250, 250, 240)));
            Plantilla(PolloCocido, muslo, Paleta(carne, 'p', Color.FromArgb(208, 150, 88), 'w', Color.FromArgb(240, 196, 130), 'f', Color.FromArgb(250, 250, 240)));
            Plantilla(CarnePodrida, chuleta, Paleta(carne, 'p', Color.FromArgb(120, 136, 70), 'w', Color.FromArgb(160, 170, 100), 'f', Color.FromArgb(150, 110, 90)));

            Plantilla(Manzana, new[]
            {
                "....g...",
                "...kg...",
                ".krrkrr.",
                "krwrrrrk",
                "krrrrrrk",
                "krrrrrrk",
                ".krrrrk.",
                "..kkkk.."
            }, new Dictionary<char, Color> { { 'k', Color.FromArgb(70, 20, 20) }, { 'r', Color.FromArgb(214, 46, 40) }, { 'w', Color.FromArgb(255, 160, 150) }, { 'g', Color.FromArgb(70, 140, 40) } });
            Plantilla(Carbon, new[]
            {
                "........",
                "..kkkk..",
                ".kgjkgk.",
                "kgkgjgkk",
                "kjgkgkgk",
                ".kgggkk.",
                "..kkkk..",
                "........"
            }, new Dictionary<char, Color> { { 'k', Color.FromArgb(14, 14, 16) }, { 'g', Color.FromArgb(52, 52, 56) }, { 'j', Color.FromArgb(90, 90, 96) } });

            string[] corazon =
            {
                ".kk..kk.",
                "kAAkkBBk",
                "kAwABBBk",
                "kAAABBBk",
                ".kAABBk.",
                "..kABk..",
                "...kk...",
                "........"
            };
            Color rojo = Color.FromArgb(220, 30, 40), oscuro = Color.FromArgb(60, 20, 24), borde = Color.FromArgb(24, 8, 10);
            Plantilla(CorazonLleno, corazon, new Dictionary<char, Color> { { 'k', borde }, { 'A', rojo }, { 'B', rojo }, { 'w', Color.FromArgb(255, 190, 190) } });
            Plantilla(CorazonMedio, corazon, new Dictionary<char, Color> { { 'k', borde }, { 'A', rojo }, { 'B', oscuro }, { 'w', Color.FromArgb(255, 190, 190) } });
            Plantilla(CorazonVacio, corazon, new Dictionary<char, Color> { { 'k', borde }, { 'A', oscuro }, { 'B', oscuro }, { 'w', oscuro } });

            string[] comida =
            {
                "....kkk.",
                "...kAABk",
                "...kAABB",
                "...kAABk",
                "..wkABk.",
                ".ww.kk..",
                "www.....",
                ".w......"
            };
            Color carneHud = Color.FromArgb(196, 110, 56), vacia = Color.FromArgb(60, 40, 28), hueso = Color.FromArgb(240, 236, 220);
            Plantilla(MusloLleno, comida, new Dictionary<char, Color> { { 'k', Color.FromArgb(40, 20, 10) }, { 'A', carneHud }, { 'B', carneHud }, { 'w', hueso } });
            Plantilla(MusloMedio, comida, new Dictionary<char, Color> { { 'k', Color.FromArgb(40, 20, 10) }, { 'A', vacia }, { 'B', carneHud }, { 'w', hueso } });
            Plantilla(MusloVacio, comida, new Dictionary<char, Color> { { 'k', Color.FromArgb(40, 20, 10) }, { 'A', vacia }, { 'B', vacia }, { 'w', Color.FromArgb(90, 86, 80) } });

            Plantilla(Burbuja, new[]
            {
                "........",
                "..kkkk..",
                ".kwbbbk.",
                ".kbbbbk.",
                ".kbbbbk.",
                ".kbbbbk.",
                "..kkkk..",
                "........"
            }, new Dictionary<char, Color> { { 'k', Color.FromArgb(30, 60, 140) }, { 'b', Color.FromArgb(110, 170, 250) }, { 'w', Color.White } });

            DibujarAstro(Sol, Color.FromArgb(255, 236, 120), Color.FromArgb(255, 255, 210), 0);
            DibujarAstro(Luna, Color.FromArgb(200, 206, 220), Color.FromArgb(236, 240, 250), 1);
        }

        private static Dictionary<char, Color> Paleta(Dictionary<char, Color> base_, params object[] cambios)
        {
            Dictionary<char, Color> r = new Dictionary<char, Color>(base_);
            for (int i = 0; i + 1 < cambios.Length; i += 2) r[(char)cambios[i]] = (Color)cambios[i + 1];
            return r;
        }

        /// <summary>Dibuja una plantilla de 8×8 caracteres ampliada al doble ('.' es transparente).</summary>
        private static void Plantilla(int tile, string[] filas, Dictionary<char, Color> paleta)
        {
            for (int y = 0; y < Tile; y++)
            {
                for (int x = 0; x < Tile; x++) Poner(tile, x, y, 0, 0, 0, 0);
            }
            for (int fy = 0; fy < filas.Length; fy++)
            {
                for (int fx = 0; fx < filas[fy].Length; fx++)
                {
                    Color c;
                    if (!paleta.TryGetValue(filas[fy][fx], out c)) continue;
                    for (int dy = 0; dy < 2; dy++)
                    {
                        for (int dx = 0; dx < 2; dx++) Poner(tile, fx * 2 + dx, fy * 2 + dy, c.R, c.G, c.B);
                    }
                }
            }
        }

        private static void DibujarRuido(int tile, Color c, int variacion, int semilla)
        {
            Random azar = new Random(semilla);
            for (int y = 0; y < Tile; y++)
            {
                for (int x = 0; x < Tile; x++) PonerVariado(tile, x, y, c, variacion, azar);
            }
        }

        private static void DibujarLava(int tile)
        {
            Random azar = new Random(301);
            for (int y = 0; y < Tile; y++)
            {
                for (int x = 0; x < Tile; x++)
                {
                    double onda = Math.Sin(x * 0.9 + y * 0.5) + Math.Cos(y * 0.7 - x * 0.3);
                    int r = 230 + azar.Next(-10, 15), g = 96 + (int)(onda * 30) + azar.Next(-15, 15), b = 20 + azar.Next(0, 15);
                    if (azar.NextDouble() < 0.08) { g += 70; b += 40; }
                    Poner(tile, x, y, r, g, b);
                }
            }
        }

        private static void DibujarMineral(int tile, Color mineral, int semilla)
        {
            Random azar = new Random(semilla);
            for (int y = 0; y < Tile; y++)
            {
                for (int x = 0; x < Tile; x++)
                {
                    double manchas = Math.Sin(x * 1.3 + y * 0.4) * Math.Cos(y * 1.1 - x * 0.7);
                    int v = 122 + (int)(manchas * 14) + azar.Next(-9, 10);
                    Poner(tile, x, y, v, v, v);
                }
            }
            for (int i = 0; i < 6; i++)
            {
                int cx = azar.Next(1, Tile - 3), cy = azar.Next(1, Tile - 3);
                for (int dy = 0; dy < 2; dy++)
                {
                    for (int dx = 0; dx < 3; dx++)
                    {
                        if (azar.NextDouble() < 0.8) PonerVariado(tile, cx + dx, cy + dy, mineral, 8, azar);
                    }
                }
            }
        }

        private static void DibujarTablones(int tile, Color baseColor, int semilla)
        {
            Random azar = new Random(semilla);
            for (int fila = 0; fila < 4; fila++)
            {
                int tono = azar.Next(-8, 9);
                int junta = (fila % 2 == 0) ? 3 : 11;
                for (int y = fila * 4; y < fila * 4 + 4; y++)
                {
                    for (int x = 0; x < Tile; x++)
                    {
                        int d = tono + azar.Next(-5, 6);
                        if (y % 4 == 3) d -= 30;
                        else if (x == junta) d -= 22;
                        Poner(tile, x, y, baseColor.R + d, baseColor.G + d, baseColor.B + d);
                    }
                }
            }
        }

        private static void DibujarLadrillo(int tile)
        {
            Random azar = new Random(310);
            Color ladrillo = Color.FromArgb(156, 74, 54), mortero = Color.FromArgb(176, 168, 156);
            for (int y = 0; y < Tile; y++)
            {
                int fila = y / 4;
                for (int x = 0; x < Tile; x++)
                {
                    bool junta = y % 4 == 3 || ((x + (fila % 2 == 0 ? 0 : 4)) % 8 == 7);
                    PonerVariado(tile, x, y, junta ? mortero : ladrillo, junta ? 6 : 14, azar);
                }
            }
        }

        private static void DibujarLadrillosPiedra(int tile)
        {
            Random azar = new Random(311);
            for (int y = 0; y < Tile; y++)
            {
                int fila = y / 8;
                for (int x = 0; x < Tile; x++)
                {
                    int xx = (x + (fila == 1 ? 8 : 0)) % Tile;
                    bool borde = y % 8 == 7 || xx % 16 == 15;
                    bool luz = y % 8 == 0 || xx % 16 == 0;
                    int v = 122 + azar.Next(-8, 9) + (borde ? -40 : luz ? 18 : 0);
                    Poner(tile, x, y, v, v, v + 2);
                }
            }
        }

        private static void DibujarPiedraLisa(int tile)
        {
            Random azar = new Random(312);
            for (int y = 0; y < Tile; y++)
            {
                for (int x = 0; x < Tile; x++)
                {
                    bool borde = x == 0 || y == 0 || x == Tile - 1 || y == Tile - 1;
                    int v = 168 + azar.Next(-4, 5) + (borde ? -26 : 0);
                    Poner(tile, x, y, v, v, v);
                }
            }
        }

        private static void DibujarVidrio(int tile)
        {
            for (int y = 0; y < Tile; y++)
            {
                for (int x = 0; x < Tile; x++)
                {
                    bool marco = x == 0 || y == 0 || x == Tile - 1 || y == Tile - 1;
                    bool reflejo = (x - y == 3 || x - y == 4 || x - y == -6) && x > 1 && y > 1 && x < 14 && y < 14;
                    if (marco) Poner(tile, x, y, 210, 236, 245);
                    else if (reflejo) Poner(tile, x, y, 240, 250, 255);
                    else Poner(tile, x, y, 0, 0, 0, 0);
                }
            }
        }

        private static void DibujarLana(int tile, Color c, int semilla)
        {
            Random azar = new Random(semilla);
            for (int y = 0; y < Tile; y++)
            {
                for (int x = 0; x < Tile; x++)
                {
                    int d = azar.Next(-7, 8) + ((x + y) % 4 == 0 ? -10 : 0) + ((x * 3 + y * 5) % 7 == 0 ? 8 : 0);
                    Poner(tile, x, y, c.R + d, c.G + d, c.B + d);
                }
            }
        }

        private static void DibujarEstanteria(int tile)
        {
            Random azar = new Random(313);
            Color[] libros = { Color.FromArgb(150, 40, 40), Color.FromArgb(40, 80, 150), Color.FromArgb(50, 120, 60), Color.FromArgb(160, 120, 40), Color.FromArgb(110, 60, 130) };
            for (int y = 0; y < Tile; y++)
            {
                for (int x = 0; x < Tile; x++)
                {
                    bool tabla = y < 2 || y > 13 || y == 7 || y == 8;
                    if (tabla) PonerVariado(tile, x, y, Color.FromArgb(150, 112, 64), 8, azar);
                    else Poner(tile, x, y, 40, 26, 14);
                }
            }
            for (int estante = 0; estante < 2; estante++)
            {
                int y0 = estante == 0 ? 2 : 9, y1 = estante == 0 ? 6 : 13;
                for (int x = 1; x < Tile - 1; x += 2)
                {
                    if (azar.NextDouble() < 0.15) continue;
                    Color c = libros[azar.Next(libros.Length)];
                    int alto = azar.Next(0, 2);
                    for (int y = y0 + alto; y <= y1; y++)
                    {
                        PonerVariado(tile, x, y, c, 10, azar);
                        if (x + 1 < Tile - 1) PonerVariado(tile, x + 1, y, Color.FromArgb(c.R * 3 / 4, c.G * 3 / 4, c.B * 3 / 4), 6, azar);
                    }
                }
            }
        }

        private static void DibujarLampara(int tile)
        {
            Random azar = new Random(314);
            for (int y = 0; y < Tile; y++)
            {
                for (int x = 0; x < Tile; x++)
                {
                    bool marco = x == 0 || y == 0 || x == Tile - 1 || y == Tile - 1 || x == 7 || y == 7;
                    if (marco) Poner(tile, x, y, 110, 80, 40);
                    else
                    {
                        double brillo = 1 - (Math.Abs((x % 8) - 3.5) + Math.Abs((y % 8) - 3.5)) / 10.0;
                        Poner(tile, x, y, 255, (int)(200 + brillo * 50) + azar.Next(-8, 8), (int)(110 + brillo * 80) + azar.Next(-10, 10));
                    }
                }
            }
        }

        private static void Transparente(int tile)
        {
            for (int y = 0; y < Tile; y++)
            {
                for (int x = 0; x < Tile; x++) Poner(tile, x, y, 0, 0, 0, 0);
            }
        }

        private static void DibujarAntorcha(int tile)
        {
            Transparente(tile);
            for (int y = 6; y < Tile; y++)
            {
                Poner(tile, 7, y, 120, 84, 44);
                Poner(tile, 8, y, 96, 66, 34);
            }
            Poner(tile, 7, 5, 255, 220, 120); Poner(tile, 8, 5, 255, 200, 90);
            Poner(tile, 7, 4, 255, 240, 160); Poner(tile, 8, 4, 255, 170, 60);
            Poner(tile, 7, 3, 255, 150, 40);
            Poner(tile, 6, 5, 255, 140, 40, 200); Poner(tile, 9, 5, 255, 140, 40, 200);
        }

        private static void DibujarFlor(int tile, Color petalo, Color luz)
        {
            Transparente(tile);
            Color tallo = Color.FromArgb(60, 130, 40);
            for (int y = 8; y < Tile; y++) Poner(tile, 7, y, tallo.R, tallo.G, tallo.B);
            Poner(tile, 6, 12, tallo.R, tallo.G, tallo.B); Poner(tile, 5, 11, tallo.R, tallo.G, tallo.B);
            Poner(tile, 8, 13, tallo.R, tallo.G, tallo.B); Poner(tile, 9, 12, tallo.R, tallo.G, tallo.B);
            int[,] forma = { { 6, 4 }, { 7, 4 }, { 8, 4 }, { 5, 5 }, { 6, 5 }, { 7, 5 }, { 8, 5 }, { 9, 5 }, { 5, 6 }, { 6, 6 }, { 8, 6 }, { 9, 6 }, { 6, 7 }, { 7, 7 }, { 8, 7 }, { 7, 3 } };
            for (int i = 0; i < forma.GetLength(0); i++) Poner(tile, forma[i, 0], forma[i, 1], petalo.R, petalo.G, petalo.B);
            Poner(tile, 7, 6, 250, 220, 80);
            Poner(tile, 6, 4, luz.R, luz.G, luz.B);
        }

        private static void DibujarHierbaAlta(int tile)
        {
            Transparente(tile);
            Random azar = new Random(320);
            for (int i = 0; i < 9; i++)
            {
                int x = 1 + azar.Next(Tile - 2);
                int alto = 5 + azar.Next(10);
                for (int y = Tile - 1; y > Tile - 1 - alto; y--)
                {
                    int d = azar.Next(-20, 20);
                    Poner(tile, x, y, 80 + d / 2, 150 + d, 50 + d / 2);
                    if (azar.NextDouble() < 0.25) x = Math.Max(0, Math.Min(Tile - 1, x + (azar.Next(2) == 0 ? -1 : 1)));
                }
            }
        }

        private static void DibujarBrote(int tile)
        {
            Transparente(tile);
            Random azar = new Random(321);
            for (int y = 9; y < Tile; y++) Poner(tile, 7, y, 100, 74, 40);
            for (int y = 2; y < 11; y++)
            {
                for (int x = 3; x < 13; x++)
                {
                    double d = Math.Sqrt((x - 7.5) * (x - 7.5) + (y - 6) * (y - 6));
                    if (d < 4.5 && azar.NextDouble() < 0.85) PonerVariado(tile, x, y, Color.FromArgb(60, 140, 40), 25, azar);
                }
            }
        }

        private static void DibujarAstro(int tile, Color c, Color centro, int tipo)
        {
            Random azar = new Random(330 + tipo);
            for (int y = 0; y < Tile; y++)
            {
                for (int x = 0; x < Tile; x++)
                {
                    bool dentro = x >= 2 && y >= 2 && x <= 13 && y <= 13;
                    if (!dentro) { Poner(tile, x, y, 0, 0, 0, 0); continue; }
                    bool nucleo = x >= 5 && y >= 5 && x <= 10 && y <= 10;
                    Color k = nucleo ? centro : c;
                    if (tipo == 1 && azar.NextDouble() < 0.15) k = Color.FromArgb(160, 166, 180);
                    Poner(tile, x, y, k.R, k.G, k.B);
                }
            }
        }
    }
}
