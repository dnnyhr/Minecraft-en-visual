using System;
using BlockForge.Models;

namespace BlockForge.Services
{
    /// <summary>
    /// Genera el terreno 3D a partir de la semilla del mundo: la misma semilla produce
    /// siempre el mismo terreno, lo que permite restablecer partidas.
    /// Colinas con ruido fractal, capas de pasto, tierra y piedra, playas de arena,
    /// lagos opcionales hasta el nivel del mar y árboles dispersos.
    /// </summary>
    public static class GeneradorTerreno
    {
        public static void Generar(Mundo mundo)
        {
            int pasto = CatalogoBloques.PorNombre("Pasto").IdTipoBloque;
            int tierra = CatalogoBloques.PorNombre("Tierra").IdTipoBloque;
            int piedra = CatalogoBloques.PorNombre("Piedra").IdTipoBloque;
            int agua = CatalogoBloques.PorNombre("Agua").IdTipoBloque;
            int arena = CatalogoBloques.PorNombre("Arena").IdTipoBloque;
            int grava = CatalogoBloques.PorNombre("Grava").IdTipoBloque;
            int tronco = CatalogoBloques.PorNombre("Tronco").IdTipoBloque;
            int hojas = CatalogoBloques.PorNombre("Hojas").IdTipoBloque;

            Ruido ruido = new Ruido(mundo.Semilla);
            Random azar = new Random(mundo.Semilla);
            int nivelAgua = mundo.NivelAgua;
            double amplitud = mundo.Alto * 0.18;
            int[,] superficies = new int[mundo.Ancho, mundo.Largo];

            mundo.Vaciar();
            for (int x = 0; x < mundo.Ancho; x++)
            {
                for (int z = 0; z < mundo.Largo; z++)
                {
                    double colinas = ruido.Fractal(x / 64.0, z / 64.0, 4);
                    double detalle = ruido.Fractal(x / 16.0 + 311.7, z / 16.0 - 127.3, 2);
                    int superficie = nivelAgua + 1 + (int)Math.Round(colinas * amplitud + detalle * 1.5);
                    superficie = Math.Max(3, Math.Min(mundo.Alto - 10, superficie));
                    superficies[x, z] = superficie;

                    bool playa = mundo.ConLagos && superficie <= nivelAgua + 1;
                    bool fondo = mundo.ConLagos && superficie < nivelAgua - 2;
                    double profundidadTierra = 3 + ruido.Valor(x / 8.0, z / 8.0) * 1.5;

                    for (int y = 0; y <= superficie; y++)
                    {
                        int tipo;
                        if (y == superficie) tipo = fondo ? (ruido.Valor(x / 5.0, z / 5.0) > 0.2 ? grava : arena) : playa ? arena : pasto;
                        else if (y >= superficie - profundidadTierra) tipo = playa || fondo ? arena : tierra;
                        else tipo = piedra;
                        mundo.EstablecerBloque(x, y, z, tipo);
                    }

                    if (mundo.ConLagos)
                    {
                        for (int y = superficie + 1; y <= nivelAgua; y++)
                        {
                            mundo.EstablecerBloque(x, y, z, agua);
                        }
                    }
                }
            }

            // Árboles: tronco de 4 a 6 bloques con una copa de hojas, separados entre sí.
            bool[,] ocupado = new bool[mundo.Ancho, mundo.Largo];
            for (int x = 3; x < mundo.Ancho - 3; x++)
            {
                for (int z = 3; z < mundo.Largo - 3; z++)
                {
                    int superficie = superficies[x, z];
                    if (azar.NextDouble() > 0.014) continue;
                    if (mundo.ObtenerBloque(x, superficie, z) != pasto || ocupado[x, z]) continue;
                    int altura = 4 + azar.Next(3);
                    if (superficie + altura + 2 >= mundo.Alto) continue;

                    for (int dx = -3; dx <= 3; dx++)
                    {
                        for (int dz = -3; dz <= 3; dz++)
                        {
                            int ox = x + dx, oz = z + dz;
                            if (ox >= 0 && oz >= 0 && ox < mundo.Ancho && oz < mundo.Largo) ocupado[ox, oz] = true;
                        }
                    }

                    mundo.EstablecerBloque(x, superficie, z, tierra);
                    int cima = superficie + altura;
                    for (int y = cima - 2; y <= cima + 1; y++)
                    {
                        int radio = y <= cima - 1 ? 2 : 1;
                        for (int dx = -radio; dx <= radio; dx++)
                        {
                            for (int dz = -radio; dz <= radio; dz++)
                            {
                                bool esquina = Math.Abs(dx) == radio && Math.Abs(dz) == radio;
                                if (esquina && (y == cima + 1 || azar.Next(2) == 0)) continue;
                                if (mundo.ObtenerBloque(x + dx, y, z + dz) == 0) mundo.EstablecerBloque(x + dx, y, z + dz, hojas);
                            }
                        }
                    }
                    for (int y = superficie + 1; y <= cima; y++)
                    {
                        mundo.EstablecerBloque(x, y, z, tronco);
                    }
                }
            }
        }

        /// <summary>Ruido de valor 2D con interpolación suave, determinado por la semilla.</summary>
        private class Ruido
        {
            private readonly int semilla;

            public Ruido(int semilla)
            {
                this.semilla = semilla;
            }

            private double Aleatorio(int x, int z)
            {
                unchecked
                {
                    int h = semilla * 374761393 + x * 668265263 + z * 1274126177;
                    h = (h ^ (h >> 13)) * 1103515245;
                    h ^= h >> 16;
                    return (h & 0x7FFFFFFF) / (double)0x7FFFFFFF * 2.0 - 1.0;
                }
            }

            /// <summary>Valor entre -1 y 1.</summary>
            public double Valor(double x, double z)
            {
                int x0 = (int)Math.Floor(x), z0 = (int)Math.Floor(z);
                double fx = x - x0, fz = z - z0;
                double sx = fx * fx * (3 - 2 * fx), sz = fz * fz * (3 - 2 * fz);
                double a = Aleatorio(x0, z0), b = Aleatorio(x0 + 1, z0);
                double c = Aleatorio(x0, z0 + 1), d = Aleatorio(x0 + 1, z0 + 1);
                double arriba = a + (b - a) * sx;
                double abajo = c + (d - c) * sx;
                return arriba + (abajo - arriba) * sz;
            }

            /// <summary>Suma de octavas (fBm), normalizada entre -1 y 1.</summary>
            public double Fractal(double x, double z, int octavas)
            {
                double total = 0, amplitud = 1, maximo = 0, frecuencia = 1;
                for (int i = 0; i < octavas; i++)
                {
                    total += Valor(x * frecuencia + i * 17.3, z * frecuencia - i * 9.1) * amplitud;
                    maximo += amplitud;
                    amplitud *= 0.5;
                    frecuencia *= 2;
                }
                return total / maximo;
            }
        }
    }
}
