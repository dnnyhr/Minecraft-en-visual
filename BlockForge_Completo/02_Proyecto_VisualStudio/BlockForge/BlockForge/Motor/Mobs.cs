using System;
using System.Collections.Generic;
using System.Linq;
using BlockForge.Models;

namespace BlockForge.Motor
{
    /// <summary>Datos fijos de cada tipo de mob.</summary>
    internal class DefMob
    {
        public string Nombre;
        public float MedioAncho, Alto, VidaMaxima, Velocidad;
        public bool Hostil;
        public string Suelta;          // nombre del objeto que deja al morir
        public int SueltaMin, SueltaMax;

        public static readonly Dictionary<TipoMob, DefMob> Todos = new Dictionary<TipoMob, DefMob>
        {
            { TipoMob.Cerdo,   new DefMob { Nombre = "Cerdo",   MedioAncho = 0.45f, Alto = 0.9f, VidaMaxima = 10, Velocidad = 1.2f, Suelta = "Chuleta cruda", SueltaMin = 1, SueltaMax = 2 } },
            { TipoMob.Vaca,    new DefMob { Nombre = "Vaca",    MedioAncho = 0.45f, Alto = 1.4f, VidaMaxima = 10, Velocidad = 1.1f, Suelta = "Filete crudo", SueltaMin = 1, SueltaMax = 2 } },
            { TipoMob.Oveja,   new DefMob { Nombre = "Oveja",   MedioAncho = 0.45f, Alto = 1.3f, VidaMaxima = 8,  Velocidad = 1.1f, Suelta = "Lana blanca", SueltaMin = 1, SueltaMax = 2 } },
            { TipoMob.Gallina, new DefMob { Nombre = "Gallina", MedioAncho = 0.2f,  Alto = 0.7f, VidaMaxima = 4,  Velocidad = 1.2f, Suelta = "Pollo crudo", SueltaMin = 1, SueltaMax = 1 } },
            { TipoMob.Zombi,   new DefMob { Nombre = "Zombi",   MedioAncho = 0.3f,  Alto = 1.9f, VidaMaxima = 20, Velocidad = 2.3f, Hostil = true, Suelta = "Carne podrida", SueltaMin = 0, SueltaMax = 2 } },
            { TipoMob.Creeper, new DefMob { Nombre = "Creeper", MedioAncho = 0.3f,  Alto = 1.7f, VidaMaxima = 20, Velocidad = 2.1f, Hostil = true } },
            { TipoMob.Arana,   new DefMob { Nombre = "Araña",   MedioAncho = 0.7f,  Alto = 0.9f, VidaMaxima = 16, Velocidad = 3.0f, Hostil = true } }
        };
    }

    internal class Mob
    {
        public int Id;
        public TipoMob Tipo;
        public DefMob Def;
        public Vec3 Posicion, Velocidad, Destino;
        public float Giro, GiroDestino, Inclinacion;
        public float Vida;
        public bool EnSuelo;
        public float Caminata, Rapidez;
        public float Mecha;            // creeper: de 0 a 1 antes de explotar
        public float DesdeDano = 99f;
        public float Ardiendo;
        public float Decision, Huir, Ataque, TiempoLava;
        public Vec3 Rumbo;
        public int Objetivo = -1;      // id del jugador que persigue (-1 ninguno)
        public bool Muerto;
        public float Agonia;           // animación de muerte

        public float Distancia(Vec3 p)
        {
            return (Posicion - p).Longitud;
        }
    }

    /// <summary>Jugador al que pueden atacar los mobs (el local tiene id 0 en una partida sin red).</summary>
    internal struct ObjetivoMob
    {
        public int Id;
        public Vec3 Posicion;
    }

    /// <summary>
    /// Mobs pacíficos (cerdos, vacas, ovejas y gallinas) y hostiles (zombis, arañas y creepers).
    /// En multijugador solo el anfitrión los simula; los invitados reciben sus posiciones.
    /// </summary>
    internal class GestorMobs
    {
        public const int MaximoPacificos = 10, MaximoHostiles = 10;
        public readonly List<Mob> Mobs = new List<Mob>();
        private readonly Dictionary<TipoMob, ModeloMob> modelos = new Dictionary<TipoMob, ModeloMob>();
        private readonly Random azar = new Random();
        private int siguienteId = 1;
        private float temporizadorAparicion;

        /// <summary>Sin mobs hostiles (dificultad pacífica).</summary>
        public bool Pacifico { get; set; }

        /// <summary>Un jugador recibe daño: (id del jugador, daño, posición del atacante).</summary>
        public event Action<int, float, Vec3> JugadorAtacado;
        /// <summary>Un creeper explotó: (centro, radio).</summary>
        public event Action<Vec3, float> Explosion;
        /// <summary>Un mob murió por un golpe: (mob, id del jugador que lo mató).</summary>
        public event Action<Mob, int> MobMuerto;

        private ModeloMob Modelo(TipoMob tipo)
        {
            ModeloMob m;
            if (!modelos.TryGetValue(tipo, out m))
            {
                m = ModeloMob.Crear(tipo);
                modelos[tipo] = m;
            }
            return m;
        }

        public Mob Crear(TipoMob tipo, Vec3 pos, int id = 0)
        {
            Mob m = new Mob
            {
                Id = id != 0 ? id : siguienteId++,
                Tipo = tipo,
                Def = DefMob.Todos[tipo],
                Posicion = pos,
                Giro = (float)(azar.NextDouble() * Math.PI * 2)
            };
            m.Vida = m.Def.VidaMaxima;
            m.GiroDestino = m.Giro;
            Mobs.Add(m);
            return m;
        }

        // ------------------------------------------------------------------ simulación (anfitrión o un jugador)
        public void Actualizar(float dt, IConsultaMundo consulta, Mundo mundo, RenderMundo render, CicloDia ciclo, List<ObjetivoMob> jugadores)
        {
            temporizadorAparicion -= dt;
            if (temporizadorAparicion <= 0 && jugadores.Count > 0)
            {
                temporizadorAparicion = 1f;
                Aparecer(mundo, render, ciclo, jugadores[azar.Next(jugadores.Count)].Posicion);
            }

            foreach (Mob m in Mobs.ToArray())
            {
                if (m.Muerto)
                {
                    m.Agonia += dt;
                    if (m.Agonia > 0.8f) Mobs.Remove(m);
                    continue;
                }
                m.DesdeDano += dt;
                Pensar(m, dt, consulta, mundo, render, ciclo, jugadores);
                Moverse(m, dt, consulta);

                // Desaparecen los que quedan muy lejos de todos los jugadores.
                float cerca = jugadores.Count > 0 ? jugadores.Min(j => m.Distancia(j.Posicion)) : 0;
                if (cerca > (m.Def.Hostil ? 64f : 96f) || (Pacifico && m.Def.Hostil) || m.Posicion.Y < -20) Mobs.Remove(m);
            }
        }

        private void Pensar(Mob m, float dt, IConsultaMundo consulta, Mundo mundo, RenderMundo render, CicloDia ciclo, List<ObjetivoMob> jugadores)
        {
            m.Decision -= dt;
            m.Ataque -= dt;
            if (m.Huir > 0) m.Huir -= dt;

            // Lava y sol (los zombis arden de día bajo el cielo).
            int bx = (int)Math.Floor(m.Posicion.X), by = (int)Math.Floor(m.Posicion.Y), bz = (int)Math.Floor(m.Posicion.Z);
            if (EsLava(mundo, bx, by, bz))
            {
                m.TiempoLava -= dt;
                m.Ardiendo = 3f;
                if (m.TiempoLava <= 0) { m.TiempoLava = 0.5f; Danar(m, 4f, m.Posicion, -1); }
            }
            if (m.Tipo == TipoMob.Zombi && ciclo.Luz > 0.6f && render.EstaIluminado(bx, (int)Math.Floor(m.Posicion.Y + m.Def.Alto), bz) && !consulta.EsLiquido(bx, by, bz))
            {
                m.Ardiendo = 1f;
            }
            if (m.Ardiendo > 0)
            {
                float antes = m.Ardiendo;
                m.Ardiendo -= dt;
                if ((int)(antes * 1f) != (int)(m.Ardiendo * 1f) || (antes > 0 && m.Ardiendo <= 0)) Danar(m, 1f, m.Posicion, -1);
            }

            ObjetivoMob? cercano = null;
            float distancia = float.MaxValue;
            foreach (ObjetivoMob j in jugadores)
            {
                float d = m.Distancia(j.Posicion);
                if (d < distancia) { distancia = d; cercano = j; }
            }

            float velocidad = m.Def.Velocidad;
            if (m.Def.Hostil && cercano.HasValue && distancia < 20f && !Pacifico)
            {
                Vec3 objetivo = cercano.Value.Posicion;
                m.Objetivo = cercano.Value.Id;
                Vec3 hacia = objetivo - m.Posicion;
                hacia.Y = 0;
                float h = hacia.Longitud;
                m.Rumbo = h > 0.01f ? hacia * (1f / h) : new Vec3(0, 0, 0);
                m.GiroDestino = (float)Math.Atan2(hacia.X, -hacia.Z);
                float dy = Math.Abs(objetivo.Y - m.Posicion.Y);

                if (m.Tipo == TipoMob.Creeper)
                {
                    if (h < 3f && dy < 2.5f)
                    {
                        m.Mecha += dt / 1.5f;
                        m.Rumbo = new Vec3(0, 0, 0);
                        if (m.Mecha >= 1f)
                        {
                            m.Muerto = true;
                            m.Agonia = 1f;
                            Mobs.Remove(m);
                            if (Explosion != null) Explosion(m.Posicion + new Vec3(0, 0.8f, 0), 3.2f);
                        }
                    }
                    else m.Mecha = Math.Max(0f, m.Mecha - dt / 1.5f);
                }
                else if (h < (m.Tipo == TipoMob.Arana ? 1.6f : 1.3f) && dy < 1.6f && m.Ataque <= 0)
                {
                    m.Ataque = 1f;
                    if (JugadorAtacado != null) JugadorAtacado(cercano.Value.Id, m.Tipo == TipoMob.Arana ? 2f : 3f, m.Posicion);
                }
            }
            else
            {
                m.Objetivo = -1;
                if (m.Tipo == TipoMob.Creeper) m.Mecha = Math.Max(0f, m.Mecha - dt);
                if (m.Huir > 0)
                {
                    velocidad *= 2.2f;
                }
                else if (m.Decision <= 0)
                {
                    // Pasear: elegir un rumbo al azar o quedarse quieto un rato.
                    m.Decision = 2f + (float)azar.NextDouble() * 5f;
                    if (azar.NextDouble() < 0.45) m.Rumbo = new Vec3(0, 0, 0);
                    else
                    {
                        double a = azar.NextDouble() * Math.PI * 2;
                        m.Rumbo = new Vec3((float)Math.Sin(a), 0, -(float)Math.Cos(a));
                        m.GiroDestino = (float)a;
                    }
                }
                // Los pacíficos no se tiran por precipicios ni caminan hacia la lava o el agua honda.
                if (!m.Def.Hostil && m.Rumbo.Longitud > 0 && m.EnSuelo)
                {
                    int ax = (int)Math.Floor(m.Posicion.X + m.Rumbo.X * 0.8f), az = (int)Math.Floor(m.Posicion.Z + m.Rumbo.Z * 0.8f);
                    bool hueco = !consulta.EsSolido(ax, by - 1, az) && !consulta.EsSolido(ax, by - 2, az) && !consulta.EsSolido(ax, by, az);
                    if (hueco || EsLava(mundo, ax, by, az) || EsLava(mundo, ax, by - 1, az))
                    {
                        m.Rumbo = m.Rumbo * -1f;
                        m.GiroDestino += (float)Math.PI;
                        m.Decision = 1.5f;
                    }
                }
            }

            float k = Math.Min(1f, dt * (m.EnSuelo ? 10f : 2f));
            m.Velocidad.X += (m.Rumbo.X * velocidad - m.Velocidad.X) * k;
            m.Velocidad.Z += (m.Rumbo.Z * velocidad - m.Velocidad.Z) * k;
        }

        private static bool EsLava(Mundo mundo, int x, int y, int z)
        {
            int id = mundo.ObtenerBloque(x, y, z);
            return id != 0 && Atlas.Obtener(id).Modo == ModoDibujo.Lava;
        }

        private void Moverse(Mob m, float dt, IConsultaMundo consulta)
        {
            int bx = (int)Math.Floor(m.Posicion.X), bz = (int)Math.Floor(m.Posicion.Z);
            bool enLiquido = consulta.EsLiquido(bx, (int)Math.Floor(m.Posicion.Y + 0.3f), bz);
            if (enLiquido)
            {
                m.Velocidad.Y = Math.Min(m.Velocidad.Y + 14f * dt, 2.5f); // flotan
            }
            else
            {
                m.Velocidad.Y = Math.Max(m.Velocidad.Y - 32f * dt, -50f);
            }

            bool suelo, choco;
            Fisica.Mover(ref m.Posicion, ref m.Velocidad, m.Def.MedioAncho, m.Def.Alto, dt, consulta, out suelo, out choco);
            m.EnSuelo = suelo;
            if (choco && m.Rumbo.Longitud > 0)
            {
                if (m.Tipo == TipoMob.Arana) m.Velocidad.Y = 3.5f;              // las arañas trepan
                else if (suelo || enLiquido) m.Velocidad.Y = enLiquido ? 6f : 8.6f; // saltan un bloque
            }

            float dg = (float)Math.IEEERemainder(m.GiroDestino - m.Giro, Math.PI * 2);
            m.Giro += dg * Math.Min(1f, dt * 6f);
            float rapidez = (float)Math.Sqrt(m.Velocidad.X * m.Velocidad.X + m.Velocidad.Z * m.Velocidad.Z);
            m.Rapidez = rapidez;
            m.Caminata += rapidez * dt * 2.6f;
        }

        // ------------------------------------------------------------------ combate
        /// <summary>Mob alcanzado por un rayo (el golpe del jugador), o null.</summary>
        public Mob Apuntado(Vec3 ojo, Vec3 dir, float alcance, out float distancia)
        {
            distancia = float.MaxValue;
            Mob mejor = null;
            foreach (Mob m in Mobs)
            {
                if (m.Muerto) continue;
                float a = m.Def.MedioAncho;
                float t = Fisica.RayoCaja(ojo, dir, m.Posicion.X - a, m.Posicion.Y, m.Posicion.Z - a, m.Posicion.X + a, m.Posicion.Y + m.Def.Alto, m.Posicion.Z + a);
                if (t >= 0 && t <= alcance && t < distancia) { distancia = t; mejor = m; }
            }
            return mejor;
        }

        /// <summary>Daña un mob y lo empuja. "atacante" es el id del jugador (-1 si fue el entorno).</summary>
        public void Danar(Mob m, float cantidad, Vec3 desde, int atacante)
        {
            if (m.Muerto || (m.DesdeDano < 0.4f && atacante >= 0)) return;
            m.Vida -= cantidad;
            m.DesdeDano = 0f;
            Vec3 empuje = m.Posicion - desde;
            empuje.Y = 0;
            float l = empuje.Longitud;
            if (l > 0.01f && atacante >= 0)
            {
                empuje = empuje * (1f / l);
                m.Velocidad = new Vec3(empuje.X * 6f, 5f, empuje.Z * 6f);
            }
            if (!m.Def.Hostil) { m.Huir = 4f; m.Rumbo = empuje.Longitud > 0 ? empuje * (1f / Math.Max(0.01f, empuje.Longitud)) : m.Rumbo; m.GiroDestino = (float)Math.Atan2(m.Rumbo.X, -m.Rumbo.Z); }
            if (m.Vida <= 0)
            {
                m.Muerto = true;
                if (MobMuerto != null) MobMuerto(m, atacante);
            }
        }

        // ------------------------------------------------------------------ aparición
        private void Aparecer(Mundo mundo, RenderMundo render, CicloDia ciclo, Vec3 cerca)
        {
            int pacificos = Mobs.Count(m => !m.Def.Hostil), hostiles = Mobs.Count(m => m.Def.Hostil);
            double a = azar.NextDouble() * Math.PI * 2;
            float r = 16f + (float)azar.NextDouble() * 24f;
            int x = (int)Math.Floor(cerca.X + Math.Cos(a) * r), z = (int)Math.Floor(cerca.Z + Math.Sin(a) * r);
            if (x < 1 || z < 1 || x >= mundo.Ancho - 1 || z >= mundo.Largo - 1) return;

            // Superficie de la columna
            int suelo = -1;
            for (int y = mundo.Alto - 2; y > 0; y--)
            {
                int id = mundo.ObtenerBloque(x, y, z);
                if (id != 0 && Atlas.Obtener(id).Solido && Atlas.Obtener(id).Modo != ModoDibujo.Planta) { suelo = y; break; }
                if (id != 0 && Atlas.Obtener(id).Modo != ModoDibujo.Planta) break; // agua o lava encima
            }

            if (!ciclo.EsDeNoche && pacificos < MaximoPacificos && suelo > 0 && azar.NextDouble() < 0.5)
            {
                TipoBloque sueloTipo = Services.CatalogoBloques.Obtener(mundo.ObtenerBloque(x, suelo, z));
                int encima = mundo.ObtenerBloque(x, suelo + 1, z);
                bool libre = encima == 0 || Atlas.Obtener(encima).Modo == ModoDibujo.Planta;
                if (sueloTipo != null && sueloTipo.Nombre == "Pasto" && libre)
                {
                    TipoMob[] tipos = { TipoMob.Cerdo, TipoMob.Vaca, TipoMob.Oveja, TipoMob.Gallina };
                    TipoMob tipo = tipos[azar.Next(tipos.Length)];
                    int grupo = 1 + azar.Next(3);
                    for (int i = 0; i < grupo && pacificos + i < MaximoPacificos; i++)
                    {
                        Crear(tipo, new Vec3(x + 0.5f + (float)(azar.NextDouble() - 0.5), suelo + 1, z + 0.5f + (float)(azar.NextDouble() - 0.5)));
                    }
                }
                return;
            }

            if (Pacifico || hostiles >= MaximoHostiles) return;
            // Hostiles: en la superficie de noche, o en cuevas oscuras a cualquier hora.
            int yAparicion = -1;
            if (suelo > 0 && render.BrilloEn(x, suelo + 1, z) < 0.3f) yAparicion = suelo + 1;
            else
            {
                for (int intento = 0; intento < 6 && yAparicion < 0; intento++)
                {
                    int y = 1 + azar.Next(Math.Max(1, suelo > 0 ? suelo - 2 : mundo.Alto - 2));
                    if (mundo.ObtenerBloque(x, y, z) == 0 && mundo.ObtenerBloque(x, y + 1, z) == 0 &&
                        Atlas.Obtener(mundo.ObtenerBloque(x, y - 1, z)).Solido && render.BrilloEn(x, y, z) < 0.3f) yAparicion = y;
                }
            }
            if (yAparicion < 0) return;
            double p = azar.NextDouble();
            TipoMob hostil = p < 0.45 ? TipoMob.Zombi : p < 0.8 ? TipoMob.Creeper : TipoMob.Arana;
            Crear(hostil, new Vec3(x + 0.5f, yAparicion, z + 0.5f));
        }

        // ------------------------------------------------------------------ red
        /// <summary>Anfitrión: hora del día y estado de todos los mobs para enviar a los invitados.</summary>
        public byte[] Estado(float hora)
        {
            return Red.Protocolo.Armar(w =>
            {
                w.Write(hora);
                w.Write((short)Mobs.Count);
                foreach (Mob m in Mobs)
                {
                    w.Write(m.Id);
                    w.Write((byte)m.Tipo);
                    w.Write(m.Posicion.X); w.Write(m.Posicion.Y); w.Write(m.Posicion.Z);
                    w.Write(m.Giro);
                    byte banderas = (byte)((m.Muerto ? 1 : 0) | (m.DesdeDano < 0.35f ? 2 : 0) | (m.Ardiendo > 0 ? 4 : 0));
                    w.Write(banderas);
                    w.Write((byte)(Math.Max(0f, Math.Min(1f, m.Mecha)) * 255));
                }
            });
        }

        /// <summary>Invitado: aplica el estado recibido del anfitrión. Devuelve la hora del día.</summary>
        public float AplicarEstado(System.IO.BinaryReader r)
        {
            float hora = r.ReadSingle();
            int cantidad = r.ReadInt16();
            HashSet<int> vistos = new HashSet<int>();
            for (int i = 0; i < cantidad; i++)
            {
                int id = r.ReadInt32();
                TipoMob tipo = (TipoMob)r.ReadByte();
                Vec3 pos = new Vec3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                float giro = r.ReadSingle();
                byte banderas = r.ReadByte();
                float mecha = r.ReadByte() / 255f;
                if (!DefMob.Todos.ContainsKey(tipo)) continue;
                vistos.Add(id);
                Mob m = Mobs.FirstOrDefault(x => x.Id == id);
                if (m == null)
                {
                    m = Crear(tipo, pos, id);
                    m.Giro = giro;
                }
                m.Destino = pos;
                m.GiroDestino = giro;
                if ((banderas & 1) != 0) m.Muerto = true;
                if ((banderas & 2) != 0) m.DesdeDano = 0f;
                m.Ardiendo = (banderas & 4) != 0 ? 1f : 0f;
                m.Mecha = mecha;
            }
            Mobs.RemoveAll(m => !vistos.Contains(m.Id) && !m.Muerto);
            return hora;
        }

        /// <summary>Invitado: suaviza el movimiento de los mobs entre actualizaciones.</summary>
        public void Interpolar(float dt)
        {
            foreach (Mob m in Mobs.ToArray())
            {
                if (m.Muerto)
                {
                    m.Agonia += dt;
                    if (m.Agonia > 0.8f) Mobs.Remove(m);
                    continue;
                }
                m.DesdeDano += dt;
                Vec3 antes = m.Posicion;
                float k = Math.Min(1f, dt * 12f);
                m.Posicion = m.Posicion + (m.Destino - m.Posicion) * k;
                if ((m.Destino - m.Posicion).Longitud > 6) m.Posicion = m.Destino;
                float dg = (float)Math.IEEERemainder(m.GiroDestino - m.Giro, Math.PI * 2);
                m.Giro += dg * k;
                Vec3 d = m.Posicion - antes;
                float rapidez = (float)Math.Sqrt(d.X * d.X + d.Z * d.Z) / Math.Max(dt, 0.001f);
                m.Rapidez += (Math.Min(rapidez, 8f) - m.Rapidez) * Math.Min(1f, dt * 10f);
                m.Caminata += m.Rapidez * dt * 2.6f;
            }
        }

        // ------------------------------------------------------------------ dibujo
        public void Dibujar(RenderMundo render, float tiempo)
        {
            GL.glEnable(GL.TEXTURE_2D);
            foreach (Mob m in Mobs)
            {
                ModeloMob modelo = Modelo(m.Tipo);
                modelo.Subir();
                float luz = render.BrilloEn((int)Math.Floor(m.Posicion.X), (int)Math.Floor(m.Posicion.Y + 0.5f), (int)Math.Floor(m.Posicion.Z));
                float balanceo = (float)Math.Sin(m.Caminata) * Math.Min(1f, m.Rapidez / 2f) * 40f;
                GL.glPushMatrix();
                GL.glTranslatef(m.Posicion.X, m.Posicion.Y, m.Posicion.Z);
                GL.glRotatef((float)(-m.Giro * 180 / Math.PI), 0, 1, 0);
                if (m.Muerto) GL.glRotatef(Math.Min(1f, m.Agonia * 2.5f) * 90f, 0, 0, 1); // cae de lado
                if (m.Tipo == TipoMob.Creeper && m.Mecha > 0)
                {
                    float s = 1f + m.Mecha * 0.25f;
                    GL.glScalef(s, 1f + m.Mecha * 0.1f, s);
                }
                modelo.DibujarPiezas(balanceo, m.Inclinacion, luz, tiempo);

                // Tintes: rojo al recibir daño, blanco parpadeante con la mecha, naranja al arder.
                float rojo = m.DesdeDano < 0.35f || m.Muerto ? 0.5f : 0f;
                bool destello = m.Tipo == TipoMob.Creeper && m.Mecha > 0 && ((int)(tiempo * (4 + m.Mecha * 10))) % 2 == 0;
                if (rojo > 0 || destello || m.Ardiendo > 0)
                {
                    GL.glDisable(GL.TEXTURE_2D);
                    GL.glEnable(GL.BLEND);
                    GL.glBlendFunc(GL.SRC_ALPHA, GL.ONE);
                    GL.glDepthFunc(GL.LEQUAL);
                    GL.glDepthMask(0);
                    if (destello) TintarPiezas(modelo, balanceo, m.Inclinacion, tiempo, 1f, 1f, 1f, 0.55f);
                    if (rojo > 0) TintarPiezas(modelo, balanceo, m.Inclinacion, tiempo, 1f, 0f, 0f, rojo);
                    if (m.Ardiendo > 0) TintarPiezas(modelo, balanceo, m.Inclinacion, tiempo, 1f, 0.45f, 0.05f, 0.3f + 0.2f * (float)Math.Sin(tiempo * 20));
                    GL.glDepthMask(1);
                    GL.glBlendFunc(GL.SRC_ALPHA, GL.ONE_MINUS_SRC_ALPHA);
                    GL.glDisable(GL.BLEND);
                    GL.glEnable(GL.TEXTURE_2D);
                }
                GL.glPopMatrix();
            }
        }

        private static void TintarPiezas(ModeloMob modelo, float balanceo, float inclinacion, float tiempo, float r, float g, float b, float a)
        {
            // Se vuelve a dibujar el modelo con un color plano encima (mezcla aditiva).
            ModeloJugador.TinteR = r; ModeloJugador.TinteG = g; ModeloJugador.TinteB = b; ModeloJugador.TinteA = a;
            modelo.DibujarPiezas(balanceo, inclinacion, 1f, tiempo);
            ModeloJugador.TinteR = ModeloJugador.TinteG = ModeloJugador.TinteB = ModeloJugador.TinteA = 1f;
        }

        public void Liberar()
        {
            foreach (ModeloMob m in modelos.Values) m.Liberar();
        }
    }
}
