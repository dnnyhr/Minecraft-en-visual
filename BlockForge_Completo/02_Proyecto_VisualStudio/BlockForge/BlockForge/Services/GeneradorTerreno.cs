using System;
using System.Linq;
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
        private static int Id(string nombre)
        {
            TipoBloque t = CatalogoBloques.Tipos.FirstOrDefault(x => string.Equals(x.Nombre, nombre, StringComparison.OrdinalIgnoreCase));
            return t != null ? t.IdTipoBloque : 0;
        }

        /// <summary>
        /// Hierro y diamante en la piedra. Se usa al generar y también al abrir un mundo creado antes de que
        /// existieran (solo reemplaza piedra bajo tierra, así no toca lo construido en la superficie).
        /// </summary>
        public static void AgregarMinerales(Mundo mundo, Random azar = null, int[,] superficies = null)
        {
            int piedra = Id("Piedra"), hierro = Id("Mineral de hierro"), diamante = Id("Mineral de diamante");
            if (piedra == 0) return;
            azar = azar ?? new Random(mundo.Semilla ^ 0x5A17);
            if (superficies == null)
            {
                superficies = new int[mundo.Ancho, mundo.Largo];
                for (int x = 0; x < mundo.Ancho; x++)
                {
                    for (int z = 0; z < mundo.Largo; z++) superficies[x, z] = mundo.NivelAgua - 4;
                }
            }
            int volumen = mundo.Ancho * mundo.Largo * mundo.Alto;
            if (hierro != 0) Vetas(mundo, azar, superficies, piedra, hierro, volumen / 5200, 3, 6, mundo.Alto * 2 / 5);
            if (diamante != 0) Vetas(mundo, azar, superficies, piedra, diamante, volumen / 26000, 2, 5, Math.Max(6, mundo.Alto / 8));
        }

        /// <summary>¿El mundo ya tiene hierro? (los mundos de versiones anteriores no).</summary>
        public static bool TieneMinerales(Mundo mundo)
        {
            int hierro = Id("Mineral de hierro");
            if (hierro == 0) return true;
            byte[] celdas = mundo.Celdas;
            for (int i = 0; i < celdas.Length; i++)
            {
                if (celdas[i] == hierro) return true;
            }
            return false;
        }

        private static void Vetas(Mundo mundo, Random azar, int[,] superficies, int piedra, int mineral, int cantidad, int minimo, int maximo, int alturaMaxima)
        {
            for (int i = 0; i < cantidad; i++)
            {
                int x = azar.Next(mundo.Ancho), z = azar.Next(mundo.Largo);
                int tope = Math.Min(alturaMaxima, superficies[x, z] - 4);
                if (tope < 2) continue;
                int y = 1 + azar.Next(tope);
                int tamano = minimo + azar.Next(maximo - minimo + 1);
                for (int k = 0; k < tamano; k++)
                {
                    if (mundo.EstaDentro(x, y, z) && mundo.ObtenerBloque(x, y, z) == piedra) mundo.EstablecerBloque(x, y, z, mineral);
                    x += azar.Next(-1, 2); y += azar.Next(-1, 2); z += azar.Next(-1, 2);
                }
            }
        }

        public static void Generar(Mundo mundo)
        {
            int pasto = CatalogoBloques.PorNombre("Pasto").IdTipoBloque;
            int tierra = CatalogoBloques.PorNombre("Tierra").IdTipoBloque;
            int piedra = CatalogoBloques.PorNombre("Piedra").IdTipoBloque;
            int agua = CatalogoBloques.PorNombre("Agua").IdTipoBloque;
            int arena = CatalogoBloques.PorNombre("Arena").IdTipoBloque;
            int grava = CatalogoBloques.PorNombre("Grava").IdTipoBloque;
            int arcilla = Id("Arcilla"), lava = Id("Lava"), carbon = Id("Mineral de carbón");
            int hierba = Id("Hierba alta");
            int[] flores = { Id("Rosa"), Id("Diente de león"), Id("Aciano") };

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
                        if (y == superficie)
                        {
                            if (fondo)
                            {
                                double r = ruido.Valor(x / 5.0, z / 5.0);
                                tipo = r > 0.45 && arcilla != 0 ? arcilla : r > 0.2 ? grava : arena;
                            }
                            else tipo = playa ? arena : pasto;
                        }
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

            // Vetas de carbón dentro de la piedra; hierro más abajo y diamante solo en lo más hondo.
            if (carbon != 0) Vetas(mundo, azar, superficies, piedra, carbon, mundo.Ancho * mundo.Largo * mundo.Alto / 2600, 3, 8, mundo.Alto);
            AgregarMinerales(mundo, azar, superficies);

            // Cuevas: túneles que serpentean bajo tierra; en lo más hondo se llenan de lava.
            int tuneles = Math.Max(3, mundo.Ancho * mundo.Largo / 1100);
            for (int i = 0; i < tuneles; i++)
            {
                double x = azar.Next(mundo.Ancho), z = azar.Next(mundo.Largo);
                double y = 4 + azar.NextDouble() * Math.Max(4, superficies[(int)x, (int)z] - 8);
                double giro = azar.NextDouble() * Math.PI * 2, inclinacion = (azar.NextDouble() - 0.5) * 0.6;
                int largo = 60 + azar.Next(100);
                for (int paso = 0; paso < largo; paso++)
                {
                    double radio = 1.3 + Math.Sin(paso * 0.15 + i) * 0.6 + azar.NextDouble() * 0.5;
                    Excavar(mundo, x, y, z, radio, superficies, agua, lava, nivelAgua);
                    x += Math.Cos(giro) * Math.Cos(inclinacion);
                    z += Math.Sin(giro) * Math.Cos(inclinacion);
                    y += Math.Sin(inclinacion);
                    giro += (azar.NextDouble() - 0.5) * 0.5;
                    inclinacion = Math.Max(-0.7, Math.Min(0.7, inclinacion + (azar.NextDouble() - 0.5) * 0.3));
                    if (y < 2) inclinacion = Math.Abs(inclinacion);
                    if (x < 1 || z < 1 || x >= mundo.Ancho - 1 || z >= mundo.Largo - 1) break;
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
                    if (!PlantarArbol(mundo, x, superficie, z, azar, null)) continue;
                    for (int dx = -3; dx <= 3; dx++)
                    {
                        for (int dz = -3; dz <= 3; dz++)
                        {
                            int ox = x + dx, oz = z + dz;
                            if (ox >= 0 && oz >= 0 && ox < mundo.Ancho && oz < mundo.Largo) ocupado[ox, oz] = true;
                        }
                    }
                }
            }

            // Flores y hierba alta sobre el pasto.
            for (int x = 0; x < mundo.Ancho; x++)
            {
                for (int z = 0; z < mundo.Largo; z++)
                {
                    int y = superficies[x, z];
                    if (y + 1 >= mundo.Alto || mundo.ObtenerBloque(x, y, z) != pasto || mundo.ObtenerBloque(x, y + 1, z) != 0) continue;
                    double p = azar.NextDouble();
                    double grupo = ruido.Valor(x / 6.0 + 50, z / 6.0 - 50);
                    if (p < 0.012 + Math.Max(0, grupo) * 0.03)
                    {
                        int flor = flores[azar.Next(flores.Length)];
                        if (flor != 0) mundo.EstablecerBloque(x, y + 1, z, flor);
                    }
                    else if (p < 0.09 && hierba != 0)
                    {
                        mundo.EstablecerBloque(x, y + 1, z, hierba);
                    }
                }
            }
        }

        private static void Excavar(Mundo mundo, double cx, double cy, double cz, double radio, int[,] superficies, int agua, int lava, int nivelAgua)
        {
            int r = (int)Math.Ceiling(radio);
            for (int x = (int)cx - r; x <= (int)cx + r; x++)
            {
                for (int y = (int)cy - r; y <= (int)cy + r; y++)
                {
                    for (int z = (int)cz - r; z <= (int)cz + r; z++)
                    {
                        if (x < 1 || z < 1 || y < 1 || x >= mundo.Ancho - 1 || z >= mundo.Largo - 1 || y >= mundo.Alto - 1) continue;
                        double d = (x + 0.5 - cx) * (x + 0.5 - cx) + (y + 0.5 - cy) * (y + 0.5 - cy) * 1.4 + (z + 0.5 - cz) * (z + 0.5 - cz);
                        if (d > radio * radio) continue;
                        // No se abren cuevas que inunden los lagos ni que rompan la superficie bajo el agua.
                        if (y >= superficies[x, z] - 1 && superficies[x, z] <= nivelAgua) continue;
                        int arriba = mundo.ObtenerBloque(x, y + 1, z);
                        if (arriba == agua || mundo.ObtenerBloque(x, y, z) == agua) continue;
                        mundo.EstablecerBloque(x, y, z, y <= 5 && lava != 0 ? lava : 0);
                    }
                }
            }
        }

        /// <summary>
        /// Planta un árbol con la base en (x, suelo, z). Se usa al generar el terreno y cuando crece un brote.
        /// "alCambiar" recibe cada celda modificada. Devuelve false si no hay espacio.
        /// </summary>
        public static bool PlantarArbol(Mundo mundo, int x, int suelo, int z, Random azar, Action<int, int, int> alCambiar)
        {
            int tierra = CatalogoBloques.PorNombre("Tierra").IdTipoBloque;
            int tronco = CatalogoBloques.PorNombre("Tronco").IdTipoBloque;
            int hojas = CatalogoBloques.PorNombre("Hojas").IdTipoBloque;
            int altura = 4 + azar.Next(3);
            if (suelo + altura + 2 >= mundo.Alto) return false;
            for (int y = suelo + 1; y <= suelo + altura; y++)
            {
                int id = mundo.ObtenerBloque(x, y, z);
                if (id != 0 && CatalogoBloques.Obtener(id) != null && CatalogoBloques.Obtener(id).Categoria != TipoBloque.Planta) return false;
            }

            Action<int, int, int, int> poner = (px, py, pz, id) =>
            {
                if (!mundo.EstaDentro(px, py, pz)) return;
                mundo.EstablecerBloque(px, py, pz, id);
                if (alCambiar != null) alCambiar(px, py, pz);
            };
            poner(x, suelo, z, tierra);
            int cima = suelo + altura;
            for (int y = cima - 2; y <= cima + 1; y++)
            {
                int radio = y <= cima - 1 ? 2 : 1;
                for (int dx = -radio; dx <= radio; dx++)
                {
                    for (int dz = -radio; dz <= radio; dz++)
                    {
                        bool esquina = Math.Abs(dx) == radio && Math.Abs(dz) == radio;
                        if (esquina && (y == cima + 1 || azar.Next(2) == 0)) continue;
                        if (mundo.ObtenerBloque(x + dx, y, z + dz) == 0) poner(x + dx, y, z + dz, hojas);
                    }
                }
            }
            for (int y = suelo + 1; y <= cima; y++) poner(x, y, z, tronco);
            return true;
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