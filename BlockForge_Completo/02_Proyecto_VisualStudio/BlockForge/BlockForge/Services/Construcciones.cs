using System;
using System.Collections.Generic;
using BlockForge.Models;

namespace BlockForge.Services
{
    /// <summary>
    /// Construcciones prediseñadas. La casita: base de roca, paredes de madera con esquinas de tronco y ventanas,
    /// techo a dos aguas de madera oscura, chimenea de ladrillo, interior amueblado y un jardín con camino,
    /// flores, seto, un estanque y un árbol. El jugador aparece en el camino, mirando la puerta.
    /// </summary>
    public static class Construcciones
    {
        // Casa de 9 × 7 bloques; el terreno que se aplana mide 17 × 19.
        private const int AnchoCasa = 9, FondoCasa = 7;
        private const int MargenLado = 4, MargenFrente = 8, MargenAtras = 4;

        private static Mundo mundo;

        private static void Poner(int x, int y, int z, string nombre)
        {
            if (!mundo.EstaDentro(x, y, z)) return;
            mundo.EstablecerBloque(x, y, z, nombre == null ? 0 : CatalogoBloques.PorNombre(nombre).IdTipoBloque);
        }

        private static bool Es(int x, int y, int z, string nombre)
        {
            return mundo.EstaDentro(x, y, z) && mundo.ObtenerBloque(x, y, z) == CatalogoBloques.PorNombre(nombre).IdTipoBloque;
        }

        /// <summary>
        /// Construye la casita en el mejor lugar plano y seco cerca del centro del mundo y deja ahí
        /// el punto de aparición del jugador. Devuelve false si el mundo es demasiado pequeño.
        /// </summary>
        public static bool ConstruirCasa(Mundo destino, Random azar)
        {
            mundo = destino;
            try
            {
                int anchoLote = AnchoCasa + 2 * MargenLado, fondoLote = FondoCasa + MargenFrente + MargenAtras;
                if (mundo.Ancho < anchoLote + 4 || mundo.Largo < fondoLote + 4 || mundo.Alto < 24) return false;

                int lx, lz, suelo;
                ElegirLote(anchoLote, fondoLote, out lx, out lz, out suelo);
                Aplanar(lx, lz, anchoLote, fondoLote, suelo);

                int hx = lx + MargenLado, hz = lz + MargenFrente, y0 = suelo;
                Casa(hx, hz, y0);
                Jardin(lx, lz, anchoLote, fondoLote, hx, hz, y0, azar);

                mundo.JugadorX = hx + AnchoCasa / 2 + 0.5f;
                mundo.JugadorY = y0 + 1;
                mundo.JugadorZ = lz + 1.5f;
                mundo.JugadorGiro = (float)Math.PI; // mirando hacia la casa (+Z)
                mundo.JugadorInclinacion = -0.1f;
                return true;
            }
            finally
            {
                mundo = null;
            }
        }

        // ------------------------------------------------------------------ terreno
        /// <summary>Altura del suelo de una columna (ignora troncos, hojas y plantas). -1 si hay agua o lava encima.</summary>
        private static int Superficie(int x, int z)
        {
            for (int y = mundo.Alto - 1; y > 0; y--)
            {
                int id = mundo.ObtenerBloque(x, y, z);
                if (id == 0) continue;
                TipoBloque t = CatalogoBloques.Obtener(id);
                if (t == null) continue;
                if (t.Categoria == TipoBloque.Liquido) return -1;
                if (t.Categoria == TipoBloque.Planta || t.Nombre == "Hojas" || t.Nombre == "Tronco") continue;
                return y;
            }
            return 0;
        }

        /// <summary>Busca cerca del centro el lote más parejo y sin agua.</summary>
        private static void ElegirLote(int ancho, int fondo, out int mejorX, out int mejorZ, out int mejorSuelo)
        {
            int cx = mundo.Ancho / 2 - ancho / 2, cz = mundo.Largo / 2 - fondo / 2;
            mejorX = Math.Max(2, Math.Min(mundo.Ancho - ancho - 2, cx));
            mejorZ = Math.Max(2, Math.Min(mundo.Largo - fondo - 2, cz));
            mejorSuelo = Math.Max(mundo.NivelAgua + 1, Superficie(mejorX + ancho / 2, mejorZ + fondo / 2));
            double mejorPuntaje = double.MaxValue;
            int radio = Math.Min(64, Math.Min(mundo.Ancho, mundo.Largo) / 2);
            for (int dz = -radio; dz <= radio; dz += 4)
            {
                for (int dx = -radio; dx <= radio; dx += 4)
                {
                    int x0 = cx + dx, z0 = cz + dz;
                    if (x0 < 2 || z0 < 2 || x0 + ancho >= mundo.Ancho - 2 || z0 + fondo >= mundo.Largo - 2) continue;
                    List<int> alturas = new List<int>();
                    int agua = 0;
                    for (int z = z0; z < z0 + fondo; z += 2)
                    {
                        for (int x = x0; x < x0 + ancho; x += 2)
                        {
                            int h = Superficie(x, z);
                            if (h < 0) agua++;
                            else alturas.Add(h);
                        }
                    }
                    if (alturas.Count == 0) continue;
                    alturas.Sort();
                    int mediana = alturas[alturas.Count / 2];
                    if (mediana <= mundo.NivelAgua) continue;
                    double desnivel = 0;
                    foreach (int h in alturas) desnivel += Math.Abs(h - mediana);
                    double puntaje = desnivel + agua * 40 + Math.Sqrt(dx * dx + dz * dz) * 0.3;
                    if (puntaje < mejorPuntaje)
                    {
                        mejorPuntaje = puntaje;
                        mejorX = x0; mejorZ = z0; mejorSuelo = mediana;
                    }
                }
            }
            mejorSuelo = Math.Min(mejorSuelo, mundo.Alto - 16);
        }

        /// <summary>Deja el lote a la altura "suelo" con pasto encima y nada (ni árboles) por encima.</summary>
        private static void Aplanar(int x0, int z0, int ancho, int fondo, int suelo)
        {
            for (int x = x0; x < x0 + ancho; x++)
            {
                for (int z = z0; z < z0 + fondo; z++)
                {
                    for (int y = suelo + 1; y < mundo.Alto; y++) Poner(x, y, z, null);
                    Poner(x, suelo, z, "Pasto");
                    for (int y = suelo - 1; y >= Math.Max(1, suelo - 4); y--)
                    {
                        int id = mundo.ObtenerBloque(x, y, z);
                        TipoBloque t = id == 0 ? null : CatalogoBloques.Obtener(id);
                        if (t == null || !t.EsSolido) Poner(x, y, z, "Tierra");
                    }
                }
            }
        }

        // ------------------------------------------------------------------ casa
        private static void Casa(int hx, int hz, int y0)
        {
            int x1 = hx + AnchoCasa - 1, z1 = hz + FondoCasa - 1, medio = hx + AnchoCasa / 2;

            // Base de roca y piso de madera, con una alfombra azul en el centro.
            for (int x = hx; x <= x1; x++)
            {
                for (int z = hz; z <= z1; z++)
                {
                    bool borde = x == hx || x == x1 || z == hz || z == z1;
                    Poner(x, y0, z, borde ? "Roca" : "Madera");
                }
            }
            for (int x = medio - 1; x <= medio + 1; x++)
            {
                for (int z = hz + 2; z <= hz + 3; z++) Poner(x, y0, z, "Lana azul");
            }

            // Paredes: troncos en las esquinas, tablones en el resto y una viga de madera oscura arriba.
            for (int y = y0 + 1; y <= y0 + 4; y++)
            {
                for (int x = hx; x <= x1; x++)
                {
                    for (int z = hz; z <= z1; z++)
                    {
                        bool borde = x == hx || x == x1 || z == hz || z == z1;
                        if (!borde) continue;
                        bool esquina = (x == hx || x == x1) && (z == hz || z == z1);
                        Poner(x, y, z, y == y0 + 4 ? "Madera oscura" : esquina ? "Tronco" : "Madera");
                    }
                }
            }

            // Ventanas de vidrio y puerta (dos bloques de alto) con un escalón de piedra lisa.
            foreach (int x in new[] { hx + 2, x1 - 2 })
            {
                Poner(x, y0 + 2, hz, "Vidrio");
                Poner(x, y0 + 2, z1, "Vidrio");
            }
            Poner(medio, y0 + 2, z1, "Vidrio");
            foreach (int z in new[] { hz + 2, z1 - 2 })
            {
                Poner(hx, y0 + 2, z, "Vidrio");
                Poner(x1, y0 + 2, z, "Vidrio");
            }
            Poner(medio, y0 + 1, hz, null);
            Poner(medio, y0 + 2, hz, null);
            Poner(medio, y0, hz - 1, "Piedra lisa");

            // Techo a dos aguas con alero, frontones de madera y una ventana redonda en cada uno.
            int niveles = FondoCasa / 2 + 2; // 0 = alero ... último = cumbrera
            for (int k = 0; k < niveles; k++)
            {
                int y = y0 + 4 + k;
                int za = hz - 1 + k, zb = z1 + 1 - k;
                for (int x = hx - 1; x <= x1 + 1; x++)
                {
                    Poner(x, y, za, "Madera oscura");
                    Poner(x, y, zb, "Madera oscura");
                }
                if (k == 0) continue;
                for (int z = za + 1; z < zb; z++)
                {
                    Poner(hx, y, z, "Madera");
                    Poner(x1, y, z, "Madera");
                }
            }
            Poner(hx, y0 + 6, hz + FondoCasa / 2, "Vidrio");
            Poner(x1, y0 + 6, hz + FondoCasa / 2, "Vidrio");

            // Chimenea de ladrillo en la esquina del fondo, que sale por encima del techo.
            for (int y = y0 + 1; y <= y0 + 4 + niveles + 1; y++) Poner(x1 - 1, y, z1 - 1, "Ladrillo");

            // Interior: cama, estantería, mesa, lámparas colgando de la cumbrera y antorchas.
            Poner(hx + 1, y0 + 1, z1 - 2, "Lana roja");
            Poner(hx + 1, y0 + 1, z1 - 1, "Lana blanca");
            for (int x = medio - 1; x <= medio + 1; x++)
            {
                Poner(x, y0 + 1, z1 - 1, "Estantería");
                Poner(x, y0 + 2, z1 - 1, "Estantería");
            }
            Poner(x1 - 1, y0 + 1, hz + 2, "Madera oscura");
            Poner(x1 - 2, y0 + 1, hz + 2, "Madera oscura");
            foreach (int x in new[] { hx + 2, medio, x1 - 2 }) Poner(x, y0 + 4 + niveles - 2, hz + FondoCasa / 2, "Lámpara");
            Poner(hx + 1, y0 + 1, hz + 1, "Antorcha");
            Poner(x1 - 1, y0 + 1, hz + 1, "Antorcha");
        }

        // ------------------------------------------------------------------ jardín
        private static void Jardin(int lx, int lz, int ancho, int fondo, int hx, int hz, int y0, Random azar)
        {
            int medio = hx + AnchoCasa / 2;

            // Camino de grava desde la entrada del lote hasta el escalón, con antorchas junto a la puerta.
            for (int z = lz; z <= hz - 2; z++) Poner(medio, y0, z, "Grava");
            Poner(medio - 1, y0 + 1, hz - 1, "Antorcha");
            Poner(medio + 1, y0 + 1, hz - 1, "Antorcha");
            Poner(medio - 1, y0 + 1, lz + 1, "Antorcha");
            Poner(medio + 1, y0 + 1, lz + 1, "Antorcha");

            // Canteros de flores frente a la casa y a los lados del camino.
            string[] flores = { "Rosa", "Diente de león", "Aciano" };
            int i = 0;
            for (int x = hx; x <= hx + AnchoCasa - 1; x++)
            {
                if (Math.Abs(x - medio) <= 1) continue;
                Poner(x, y0 + 1, hz - 1, flores[i++ % flores.Length]);
            }
            for (int z = lz + 3; z <= hz - 3; z += 2)
            {
                Poner(medio - 2, y0 + 1, z, flores[i++ % flores.Length]);
                Poner(medio + 2, y0 + 1, z, flores[i++ % flores.Length]);
            }

            // Estanque con orilla de arena.
            int px = hx + AnchoCasa, pz = lz + 2; // dentro del lote, al lado del camino
            for (int x = px - 1; x <= px + 3; x++)
            {
                for (int z = pz - 1; z <= pz + 3; z++) Poner(x, y0, z, "Arena");
            }
            for (int x = px; x <= px + 2; x++)
            {
                for (int z = pz; z <= pz + 2; z++)
                {
                    Poner(x, y0, z, "Agua");
                    Poner(x, y0 - 1, z, "Arcilla");
                }
            }

            // Seto de hojas alrededor del lote, con la entrada abierta frente al camino.
            for (int x = lx; x < lx + ancho; x++)
            {
                if (Math.Abs(x - medio) > 1) Poner(x, y0 + 1, lz, "Hojas");
                Poner(x, y0 + 1, lz + fondo - 1, "Hojas");
            }
            for (int z = lz; z < lz + fondo; z++)
            {
                Poner(lx, y0 + 1, z, "Hojas");
                Poner(lx + ancho - 1, y0 + 1, z, "Hojas");
            }

            // Un árbol en la esquina del fondo y algo de hierba alta.
            GeneradorTerreno.PlantarArbol(mundo, lx + 2, y0, lz + fondo - 3, azar, (x, y, z) => { });
            for (int k = 0; k < 14; k++)
            {
                int x = lx + 1 + azar.Next(ancho - 2), z = lz + 1 + azar.Next(fondo - 2);
                if (Es(x, y0, z, "Pasto") && mundo.ObtenerBloque(x, y0 + 1, z) == 0 &&
                    (x < hx - 1 || x > hx + AnchoCasa || z > hz + FondoCasa)) Poner(x, y0 + 1, z, "Hierba alta");
            }
        }

        /// <summary>Materiales para seguir construyendo en el mundo de la casita.</summary>
        public static Inventario MaterialesIniciales()
        {
            Inventario inventario = new Inventario();
            foreach (string nombre in new[] { "Madera", "Madera oscura", "Tronco", "Vidrio", "Roca", "Ladrillo", "Piedra lisa", "Lana blanca", "Lana roja" })
            {
                inventario.EstablecerCantidad(CatalogoBloques.PorNombre(nombre).IdTipoBloque, 64);
            }
            inventario.EstablecerCantidad(CatalogoBloques.PorNombre("Antorcha").IdTipoBloque, 32);
            inventario.EstablecerCantidad(CatalogoBloques.PorNombre("Manzana").IdTipoBloque, 10);
            inventario.EstablecerCantidad(CatalogoBloques.PorNombre("Pollo cocido").IdTipoBloque, 10);
            return inventario;
        }
    }
}
