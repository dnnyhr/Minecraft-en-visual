using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using BlockForge.Models;
using BlockForge.Motor;
using BlockForge.Red;
using BlockForge.Services;

namespace BlockForge.Controls
{
    /// <summary>
    /// Modo supervivencia: ciclo de día y noche, vida y hambre, comida, mobs, explosiones,
    /// brotes que crecen hasta ser árboles y lo que dejan las hojas al romperse.
    /// </summary>
    public partial class VistaMundo3D
    {
        private const float TiempoComer = 1.4f;
        private const float AlcanceGolpe = 4f;

        private CicloDia ciclo = new CicloDia(0.05f);
        private readonly Supervivencia vida = new Supervivencia();
        private readonly GestorMobs mobs = new GestorMobs();
        private readonly Dictionary<long, float> brotes = new Dictionary<long, float>();
        private readonly Random azarJuego = new Random();
        private float comiendo;
        private float temporizadorBrotes;
        private float sacudida;
        private bool mobsIniciales;

        /// <summary>Sin mobs hostiles ni daño de los mobs (para construir tranquilo).</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Pacifico
        {
            get { return mobs.Pacifico; }
            set
            {
                mobs.Pacifico = value;
                Avisar(value ? "Modo pacífico: no aparecen mobs hostiles." : "Modo normal: de noche y en las cuevas aparecen mobs hostiles.", Color.White);
            }
        }

        private bool EsComidaActiva
        {
            get { return TipoActivo != null && TipoActivo.EsComida; }
        }

        private void IniciarJuego()
        {
            ciclo = new CicloDia(mundo.HoraDia);
            vida.Revivir();
            if (mundo.JugadorVida.HasValue && mundo.JugadorVida.Value > 0) vida.Vida = mundo.JugadorVida.Value;
            if (mundo.JugadorHambre.HasValue) vida.Hambre = mundo.JugadorHambre.Value;
            vida.Murio -= AlMorir;
            vida.Murio += AlMorir;
            mobs.JugadorAtacado -= AlAtacarJugador;
            mobs.JugadorAtacado += AlAtacarJugador;
            mobs.Explosion -= AlExplotar;
            mobs.Explosion += AlExplotar;
            mobs.MobMuerto -= AlMorirMob;
            mobs.MobMuerto += AlMorirMob;
            mobs.Mobs.Clear();
            mobsIniciales = false;

            // Barra: primero lo que hay en el inventario y después bloques básicos.
            Array.Clear(barra, 0, barra.Length);
            int i = 0;
            foreach (TipoBloque t in CatalogoBloques.Tipos.Where(t => t.EsRecolectable && juego.Inventario.ObtenerCantidad(t.IdTipoBloque) > 0))
            {
                if (i < barra.Length) barra[i++] = t;
            }
            foreach (string nombre in new[] { "Madera", "Piedra", "Tierra", "Tronco", "Antorcha", "Vidrio", "Ladrillo", "Arena", "Manzana" })
            {
                TipoBloque t = CatalogoBloques.Tipos.FirstOrDefault(x => x.Nombre == nombre);
                if (t != null && i < barra.Length && !barra.Contains(t)) barra[i++] = t;
            }
            ranuraActiva = 0;

            // Brotes que ya estaban plantados.
            brotes.Clear();
            TipoBloque brote = CatalogoBloques.Tipos.FirstOrDefault(t => t.Nombre == "Brote");
            if (brote != null)
            {
                byte[] celdas = mundo.Celdas;
                for (int k = 0; k < celdas.Length; k++)
                {
                    if (celdas[k] != brote.IdTipoBloque) continue;
                    int x = k % mundo.Ancho, z = (k / mundo.Ancho) % mundo.Largo, y = k / (mundo.Ancho * mundo.Largo);
                    RegistrarBrote(x, y, z);
                }
            }
        }

        private bool Simulo
        {
            get { return red == null || red.EsAnfitrion; }
        }

        private int IdLocal
        {
            get { return red != null ? red.IdLocal : 0; }
        }

        private void ActualizarJuego(float dt, EntradaMovimiento entrada, float recorrido, bool salto)
        {
            ciclo.Avanzar(dt);
            int bx = (int)Math.Floor(jugador.Posicion.X), bz = (int)Math.Floor(jugador.Posicion.Z);
            bool enLava = EsLava(bx, (int)Math.Floor(jugador.Posicion.Y + 0.2f), bz) || EsLava(bx, (int)Math.Floor(jugador.Posicion.Y + 1.2f), bz);
            vida.Actualizar(dt, jugador, enLava, entrada.Correr, recorrido, salto, jugador.Volando);
            if (sacudida > 0) sacudida = Math.Max(0, sacudida - dt * 2f);

            // Comer: mantener clic derecho con comida en la mano.
            if (capturado && botonDerecho && EsComidaActiva && !vida.Muerto)
            {
                if (vida.Hambre >= Supervivencia.HambreMaxima)
                {
                    if (comiendo == 0) Avisar("No tienes hambre.", Color.FromArgb(255, 220, 140), 1.5f);
                    comiendo = 0.001f;
                }
                else
                {
                    comiendo += dt;
                    balanceo = Math.Max(balanceo, 0.3f + 0.2f * (float)Math.Sin(comiendo * 18));
                    if (comiendo >= TiempoComer)
                    {
                        TipoBloque comida = TipoActivo;
                        if (juego.Gastar(comida))
                        {
                            vida.Comer(comida.Alimento);
                            Avisar("Comiste " + comida.Nombre.ToLowerInvariant() + ".", Color.FromArgb(170, 240, 140), 2f);
                            if (comida.Nombre == "Carne podrida" && azarJuego.NextDouble() < 0.5)
                            {
                                vida.Hambre = Math.Max(0, vida.Hambre - 2);
                                Avisar("¡Puaj! La carne podrida te dio náuseas.", Color.FromArgb(200, 230, 120), 2.5f);
                            }
                            NotificarEstado();
                        }
                        else Avisar("No te queda " + comida.Nombre.ToLowerInvariant() + ".", Color.FromArgb(255, 200, 110));
                        comiendo = 0;
                    }
                }
            }
            else comiendo = 0;

            // Mobs: los simula quien juega solo o el anfitrión; los invitados los reciben por la red.
            if (Simulo)
            {
                List<ObjetivoMob> jugadores = new List<ObjetivoMob>();
                if (!vida.Muerto && !jugador.Volando) jugadores.Add(new ObjetivoMob { Id = IdLocal, Posicion = jugador.Posicion });
                if (red != null)
                {
                    foreach (JugadorRemoto j in red.Jugadores)
                    {
                        if (j.Posicion.Y > -500) jugadores.Add(new ObjetivoMob { Id = j.Id, Posicion = j.Posicion });
                    }
                }
                if (jugadores.Count == 0) jugadores.Add(new ObjetivoMob { Id = -99, Posicion = jugador.Posicion });
                if (!mobsIniciales)
                {
                    mobsIniciales = true;
                    for (int i = 0; i < 12; i++) mobs.Actualizar(1.05f, this, mundo, render, ciclo, jugadores); // primeros animales
                }
                mobs.Actualizar(dt, this, mundo, render, ciclo, jugadores);
            }

            // Brotes: crecen hasta ser árboles después de un rato.
            temporizadorBrotes -= dt;
            if (temporizadorBrotes <= 0 && Simulo)
            {
                temporizadorBrotes = 1f;
                CrecerBrotes(1f);
            }
        }

        // ------------------------------------------------------------------ barra e inventario
        /// <summary>Pone un tipo en el espacio activo de la barra (si ya estaba en otro, los intercambia).</summary>
        private void PonerEnBarra(TipoBloque tipo)
        {
            if (tipo == null) return;
            int existente = Array.FindIndex(barra, t => t != null && t.IdTipoBloque == tipo.IdTipoBloque);
            if (existente >= 0)
            {
                SeleccionarRanura(existente);
                return;
            }
            barra[ranuraActiva] = tipo;
            SeleccionarRanura(ranuraActiva);
        }

        /// <summary>Al recolectar algo nuevo se acomoda solo en un espacio libre de la barra.</summary>
        private void AlRecolectar(TipoBloque tipo)
        {
            if (tipo == null || barra.Any(t => t != null && t.IdTipoBloque == tipo.IdTipoBloque)) return;
            int libre = Array.FindIndex(barra, t => t == null);
            if (libre < 0) libre = Array.FindIndex(barra, t => t != null && juego.Inventario.ObtenerCantidad(t.IdTipoBloque) == 0);
            if (libre >= 0) barra[libre] = tipo;
        }

        private void Dar(string nombre, int cantidad, bool avisar)
        {
            TipoBloque t = CatalogoBloques.Tipos.FirstOrDefault(x => x.Nombre == nombre);
            if (t == null || cantidad <= 0) return;
            if (juego.Dar(t, cantidad))
            {
                AlRecolectar(t);
                if (avisar) Avisar("+" + cantidad + " " + t.Nombre, Color.FromArgb(170, 240, 140), 2f);
                NotificarEstado();
            }
        }

        /// <summary>Lo que pasa al romper un bloque: hojas que sueltan brotes o manzanas, plantas que se caen.</summary>
        private void BloqueRoto(int id, int x, int y, int z)
        {
            TipoBloque tipo = CatalogoBloques.Obtener(id);
            if (tipo == null) return;
            if (tipo.Nombre == "Hojas")
            {
                double p = azarJuego.NextDouble();
                if (p < 0.12) Dar("Brote", 1, true);
                else if (p < 0.20) Dar("Manzana", 1, true);
            }
            if (tipo.Nombre == "Brote") brotes.Remove(Clave(x, y, z));
            // Si se rompe el soporte de una planta o antorcha, también se recoge.
            int arriba = mundo.ObtenerBloque(x, y + 1, z);
            TipoBloque planta = CatalogoBloques.Obtener(arriba);
            if (planta != null && planta.Categoria == TipoBloque.Planta)
            {
                mundo.EstablecerBloque(x, y + 1, z, 0);
                CambioLocal(x, y + 1, z);
                brotes.Remove(Clave(x, y + 1, z));
                if (planta.Nombre != "Hierba alta") Dar(planta.Nombre, 1, false);
            }
        }

        // ------------------------------------------------------------------ brotes
        private long Clave(int x, int y, int z)
        {
            return ((long)y * mundo.Largo + z) * mundo.Ancho + x;
        }

        private void RegistrarBrote(int x, int y, int z)
        {
            brotes[Clave(x, y, z)] = 60f + (float)azarJuego.NextDouble() * 120f; // de 1 a 3 minutos
        }

        private void CrecerBrotes(float dt)
        {
            if (brotes.Count == 0) return;
            TipoBloque brote = CatalogoBloques.Tipos.FirstOrDefault(t => t.Nombre == "Brote");
            foreach (long clave in brotes.Keys.ToList())
            {
                brotes[clave] -= dt;
                if (brotes[clave] > 0) continue;
                int x = (int)(clave % mundo.Ancho), z = (int)((clave / mundo.Ancho) % mundo.Largo), y = (int)(clave / ((long)mundo.Ancho * mundo.Largo));
                if (brote == null || mundo.ObtenerBloque(x, y, z) != brote.IdTipoBloque) { brotes.Remove(clave); continue; }
                // Necesita luz para crecer.
                if (render.BrilloEn(x, y, z) < 0.45f) { brotes[clave] = 20f; continue; }
                mundo.EstablecerBloque(x, y, z, 0);
                List<int[]> cambios = new List<int[]>();
                if (GeneradorTerreno.PlantarArbol(mundo, x, y - 1, z, azarJuego, (cx, cy, cz) => cambios.Add(new[] { cx, cy, cz })))
                {
                    brotes.Remove(clave);
                    CambioLocal(x, y, z);
                    foreach (int[] c in cambios) CambioLocal(c[0], c[1], c[2]);
                    juego.HayCambios = true;
                }
                else
                {
                    mundo.EstablecerBloque(x, y, z, brote.IdTipoBloque); // sin espacio: espera
                    brotes[clave] = 30f;
                }
            }
        }

        // ------------------------------------------------------------------ combate
        /// <summary>Golpea al mob apuntado si está más cerca que el bloque. Devuelve true si golpeó.</summary>
        private bool GolpearMob()
        {
            float distancia;
            Mob m = mobs.Apuntado(jugador.Ojo, jugador.Direccion, AlcanceGolpe, out distancia);
            if (m == null) return false;
            if (objetivo.Hay)
            {
                Vec3 centro = new Vec3(objetivo.X + 0.5f, objetivo.Y + 0.5f, objetivo.Z + 0.5f);
                if ((centro - jugador.Ojo).Longitud < distancia - 0.5f) return false;
            }
            balanceo = 1f;
            vida.Agotamiento += 0.1f;
            float dano = 4f;
            if (Simulo) mobs.Danar(m, dano, jugador.Posicion, IdLocal);
            else red.AtacarMob(m.Id, dano);
            return true;
        }

        private void AlMorirMob(Mob m, int atacante)
        {
            if (m.Def.Suelta == null) return;
            int cantidad = m.Def.SueltaMin + azarJuego.Next(m.Def.SueltaMax - m.Def.SueltaMin + 1);
            if (cantidad <= 0) return;
            if (atacante == IdLocal) Dar(m.Def.Suelta, cantidad, true);
            else if (red != null && atacante >= 0) red.EnviarRecompensa(atacante, m.Def.Suelta, cantidad);
        }

        private void AlAtacarJugador(int id, float dano, Vec3 desde)
        {
            if (Pacifico) return;
            if (id == IdLocal) RecibirDano(dano, desde, "por un mob");
            else if (red != null) red.EnviarDano(id, dano, desde);
        }

        /// <summary>Daño al jugador local con retroceso.</summary>
        private void RecibirDano(float dano, Vec3 desde, string causa)
        {
            if (vida.Muerto || jugador.Volando) return;
            float antes = vida.Vida;
            vida.Danar(dano, causa);
            if (vida.Vida < antes)
            {
                Vec3 empuje = jugador.Posicion - desde;
                empuje.Y = 0;
                float l = empuje.Longitud;
                if (l > 0.01f) jugador.Velocidad = new Vec3(empuje.X / l * 7f, 5f, empuje.Z / l * 7f);
                sacudida = Math.Max(sacudida, 0.3f);
            }
        }

        private void AlExplotar(Vec3 centro, float radio)
        {
            Explotar(centro, radio, true);
        }

        /// <summary>Explosión de creeper: rompe bloques, daña a todos y empuja.</summary>
        private void Explotar(Vec3 centro, float radio, bool autoritativo)
        {
            sacudida = Math.Max(sacudida, 1f);
            Random azar = azarJuego;
            for (int i = 0; i < 40; i++)
            {
                int x = (int)Math.Floor(centro.X + (azar.NextDouble() - 0.5) * radio * 2);
                int y = (int)Math.Floor(centro.Y + (azar.NextDouble() - 0.5) * radio * 2);
                int z = (int)Math.Floor(centro.Z + (azar.NextDouble() - 0.5) * radio * 2);
                int id = mundo.ObtenerBloque(x, y, z);
                if (id != 0) particulas.Romper(x, y, z, Atlas.Obtener(id).Lado, 1f);
            }
            if (autoritativo)
            {
                int r = (int)Math.Ceiling(radio);
                for (int x = (int)centro.X - r; x <= (int)centro.X + r; x++)
                {
                    for (int y = (int)centro.Y - r; y <= (int)centro.Y + r; y++)
                    {
                        for (int z = (int)centro.Z - r; z <= (int)centro.Z + r; z++)
                        {
                            if (!mundo.EstaDentro(x, y, z) || y == 0) continue;
                            float d = (new Vec3(x + 0.5f, y + 0.5f, z + 0.5f) - centro).Longitud;
                            if (d > radio - (float)azar.NextDouble() * 0.8f) continue;
                            int id = mundo.ObtenerBloque(x, y, z);
                            if (id == 0) continue;
                            ModoDibujo modo = Atlas.Obtener(id).Modo;
                            if (modo == ModoDibujo.Liquido || modo == ModoDibujo.Lava) continue;
                            mundo.EstablecerBloque(x, y, z, 0);
                            render.BloqueCambiado(x, y, z);
                            if (red != null) red.EnviarBloque(x, y, z, 0);
                        }
                    }
                }
                juego.HayCambios = true;
                render.ReconstruirPendientes(int.MaxValue);

                // Daño según la distancia (a todos los jugadores y mobs cercanos).
                float alcance = radio * 2f;
                float dLocal = (jugador.Posicion + new Vec3(0, 0.9f, 0) - centro).Longitud;
                if (dLocal < alcance) RecibirDano((float)Math.Round((1 - dLocal / alcance) * 18f), centro, "por la explosión de un creeper");
                if (red != null)
                {
                    foreach (JugadorRemoto j in red.Jugadores)
                    {
                        float d = (j.Posicion + new Vec3(0, 0.9f, 0) - centro).Longitud;
                        if (d < alcance) red.EnviarDano(j.Id, (float)Math.Round((1 - d / alcance) * 18f), centro);
                    }
                    red.EnviarExplosion(centro, radio);
                }
                foreach (Mob m in mobs.Mobs.ToArray())
                {
                    float d = (m.Posicion - centro).Longitud;
                    if (d < alcance) mobs.Danar(m, (1 - d / alcance) * 18f, centro, -1);
                }
            }
        }

        private void AlMorir(string causa)
        {
            comiendo = 0;
            botonIzquierdo = botonDerecho = false;
            LiberarMouse();
            if (red != null) red.EnviarChat("(murió " + causa + ")");
        }

        private void Revivir()
        {
            vida.Revivir();
            Reaparecer();
            CapturarMouse();
        }

        // ------------------------------------------------------------------ cielo
        /// <summary>Sol y luna girando alrededor del mundo, y estrellas de noche.</summary>
        private void DibujarAstros(Vec3 ojo)
        {
            float a = ciclo.AnguloSol;
            Vec3 sol = new Vec3((float)Math.Cos(a), (float)Math.Sin(a), 0.25f);
            float l = sol.Longitud;
            sol = sol * (1f / l);
            GL.glDisable(GL.FOG);
            GL.glDepthMask(0);

            // Estrellas (solo de noche)
            float noche = Math.Max(0f, 1f - ciclo.Luz * 2.2f);
            if (noche > 0.02f)
            {
                GL.glDisable(GL.TEXTURE_2D);
                GL.glEnable(GL.BLEND);
                GL.glPointSize(2f);
                GL.glBegin(GL.POINTS);
                Random r = new Random(1234);
                for (int i = 0; i < 300; i++)
                {
                    double t = r.NextDouble() * Math.PI * 2, f = Math.Acos(r.NextDouble());
                    float brillo = (0.5f + (float)r.NextDouble() * 0.5f) * noche;
                    GL.glColor4f(1f, 1f, 1f, brillo);
                    GL.glVertex3f(ojo.X + (float)(Math.Cos(t) * Math.Sin(f)) * 150f, ojo.Y + (float)Math.Cos(f) * 150f, ojo.Z + (float)(Math.Sin(t) * Math.Sin(f)) * 150f);
                }
                GL.glEnd();
                GL.glDisable(GL.BLEND);
            }

            GL.glEnable(GL.TEXTURE_2D);
            GL.glEnable(GL.BLEND);
            GL.glBlendFunc(GL.SRC_ALPHA, GL.ONE);
            GL.glDisable(GL.CULL_FACE);
            GL.glBindTexture(GL.TEXTURE_2D, Atlas.Textura);
            Astro(ojo, sol, Atlas.Sol, 22f, 1f);
            Astro(ojo, sol * -1f, Atlas.Luna, 16f, 0.9f);
            GL.glBlendFunc(GL.SRC_ALPHA, GL.ONE_MINUS_SRC_ALPHA);
            GL.glDisable(GL.BLEND);
            GL.glEnable(GL.CULL_FACE);
            GL.glDepthMask(1);
            GL.glEnable(GL.FOG);
        }

        private static void Astro(Vec3 ojo, Vec3 dir, int tile, float tamano, float alfa)
        {
            Vec3 centro = ojo + dir * 160f;
            // Dos ejes perpendiculares a la dirección.
            Vec3 a = Math.Abs(dir.Y) < 0.9f ? Normal(Cruz(dir, new Vec3(0, 1, 0))) : new Vec3(1, 0, 0);
            Vec3 b = Normal(Cruz(a, dir));
            a = a * tamano; b = b * tamano;
            float u0, v0, u1, v1;
            Atlas.Uv(tile, out u0, out v0, out u1, out v1);
            GL.glColor4f(1, 1, 1, alfa);
            GL.glBegin(GL.QUADS);
            GL.glTexCoord2f(u0, v0); Vertice(centro - a - b);
            GL.glTexCoord2f(u1, v0); Vertice(centro + a - b);
            GL.glTexCoord2f(u1, v1); Vertice(centro + a + b);
            GL.glTexCoord2f(u0, v1); Vertice(centro - a + b);
            GL.glEnd();
        }

        private static Vec3 Cruz(Vec3 a, Vec3 b)
        {
            return new Vec3(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
        }

        private static Vec3 Normal(Vec3 v)
        {
            float l = v.Longitud;
            return l > 0 ? v * (1f / l) : v;
        }

        private static void Vertice(Vec3 v)
        {
            GL.glVertex3f(v.X, v.Y, v.Z);
        }

        // ------------------------------------------------------------------ HUD de supervivencia
        /// <summary>Corazones a la izquierda, comida a la derecha y burbujas de aire, sobre la barra.</summary>
        private void DibujarVidaYHambre(int ancho, int alto, float escala)
        {
            if (jugador.Volando && !vida.Muerto) return;
            float lado = 46 * escala, separacion = 4 * escala;
            float total = 9 * lado + 8 * separacion;
            float x0 = (ancho - total) / 2f, yBarra = alto - lado - 12 * escala;
            float icono = 18 * escala, paso = 17 * escala;
            float y = yBarra - 8 * escala - icono;

            GL.glEnable(GL.TEXTURE_2D);
            GL.glEnable(GL.ALPHA_TEST);
            GL.glBindTexture(GL.TEXTURE_2D, Atlas.Textura);
            bool parpadeo = vida.DesdeDano < 0.6f && ((int)(vida.DesdeDano * 10)) % 2 == 0;
            float temblor = vida.Vida <= 4 ? (float)Math.Sin(reloj.Elapsed.TotalSeconds * 30) * 1.5f * escala : 0;
            for (int i = 0; i < 10; i++)
            {
                float v = vida.Vida - i * 2;
                int tile = v >= 2 ? Atlas.CorazonLleno : v >= 1 ? Atlas.CorazonMedio : Atlas.CorazonVacio;
                float yy = y + (i % 2 == 0 ? temblor : -temblor);
                Sprite(tile, x0 + i * paso, yy, icono, parpadeo ? 1.6f : 1f);
            }
            for (int i = 0; i < 10; i++)
            {
                float h = vida.Hambre - i * 2;
                int tile = h >= 2 ? Atlas.MusloLleno : h >= 1 ? Atlas.MusloMedio : Atlas.MusloVacio;
                Sprite(tile, x0 + total - icono - i * paso, y, icono, 1f);
            }
            if (vida.Aire < Supervivencia.AireMaximo && jugador.CabezaEnAgua)
            {
                int burbujas = (int)Math.Ceiling(vida.Aire);
                for (int i = 0; i < burbujas; i++) Sprite(Atlas.Burbuja, x0 + total - icono - i * paso, y - icono - 2 * escala, icono, 1f);
            }
            GL.glDisable(GL.ALPHA_TEST);
        }

        private static void Sprite(int tile, float x, float y, float tamano, float brillo)
        {
            float u0, v0, u1, v1;
            Atlas.Uv(tile, out u0, out v0, out u1, out v1);
            GL.glColor3f(Math.Min(1f, brillo), Math.Min(1f, brillo), Math.Min(1f, brillo));
            GL.glBegin(GL.QUADS);
            GL.glTexCoord2f(u0, v0); GL.glVertex2f(x, y);
            GL.glTexCoord2f(u0, v1); GL.glVertex2f(x, y + tamano);
            GL.glTexCoord2f(u1, v1); GL.glVertex2f(x + tamano, y + tamano);
            GL.glTexCoord2f(u1, v0); GL.glVertex2f(x + tamano, y);
            GL.glEnd();
        }

        /// <summary>Pantalla roja al morir, con el botón para reaparecer.</summary>
        private void DibujarMuerte(int ancho, int alto, float escala)
        {
            Rectangulo(0, 0, ancho, alto, Color.FromArgb(150, 120, 0, 0));
            float t = 15 * escala;
            Escribir("¡Has muerto!", ancho / 2f, alto * 0.28f, t * 2.6f, Color.White, true, 1);
            Escribir("Moriste " + (vida.CausaMuerte ?? "") + ".", ancho / 2f, alto * 0.28f + t * 4f, t * 1.1f, Color.FromArgb(255, 220, 220), false, 1);
            Escribir("Tu inventario se conserva.", ancho / 2f, alto * 0.28f + t * 5.6f, t * 0.9f, Color.FromArgb(240, 200, 200), false, 1);
            BotonHud b = new BotonHud { Texto = "Reaparecer", Accion = "reaparecer" };
            float bw = 300 * escala, bh = 42 * escala;
            b.Area = new RectangleF((ancho - bw) / 2f, alto * 0.28f + t * 8f, bw, bh);
            DibujarBoton(b, t);
            botones.Add(b);
        }

        /// <summary>Vibración de la cámara (golpes y explosiones).</summary>
        private Vec3 Sacudida()
        {
            if (sacudida <= 0) return new Vec3(0, 0, 0);
            float s = sacudida * 0.15f;
            double tt = reloj.Elapsed.TotalSeconds * 40;
            return new Vec3((float)Math.Sin(tt) * s, (float)Math.Sin(tt * 1.3) * s, (float)Math.Cos(tt * 0.7) * s);
        }
    }
}
