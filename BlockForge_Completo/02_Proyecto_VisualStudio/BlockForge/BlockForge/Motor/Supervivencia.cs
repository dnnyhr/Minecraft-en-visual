using System;

namespace BlockForge.Motor
{
    /// <summary>
    /// Vida (10 corazones), hambre (10 muslos), saturación y aire, con reglas parecidas a las de Minecraft:
    /// la vida solo se recupera con la barra de hambre casi llena (hay que comer), con hambre baja no se
    /// puede correr, sin comida se pierde vida, y hay daño por caídas, lava y ahogamiento.
    /// </summary>
    internal class Supervivencia
    {
        public const float VidaMaxima = 20f, HambreMaxima = 20f, AireMaximo = 10f;

        public float Vida = VidaMaxima, Hambre = HambreMaxima, Saturacion = 5f, Aire = AireMaximo;
        public float Agotamiento;
        public bool Muerto;
        public string CausaMuerte;
        /// <summary>Segundos desde el último daño (para el destello rojo y la invulnerabilidad breve).</summary>
        public float DesdeDano = 99f;

        private float regeneracion, hambreCero, temporizadorLava, temporizadorAhogo;
        private bool enElAire;
        private float alturaMaxima;

        /// <summary>Con 3 muslos o menos ya no se puede correr.</summary>
        public bool PuedeCorrer
        {
            get { return Hambre > 6f; }
        }

        public event Action<string> Murio;
        public event Action<float> Herido;

        /// <summary>Actualiza las reglas. "recorrido" son los bloques caminados en este cuadro.</summary>
        public void Actualizar(float dt, Jugador j, bool enLava, bool corriendo, float recorrido, bool salto, bool sinDano)
        {
            if (Muerto) return;
            DesdeDano += dt;

            // Cansancio por moverse; al llegar a 4 se gasta saturación y después hambre.
            Agotamiento += recorrido * (corriendo ? 0.1f : 0.01f) + (salto ? (corriendo ? 0.2f : 0.05f) : 0f) + dt * 0.02f;
            while (Agotamiento >= 4f)
            {
                Agotamiento -= 4f;
                if (Saturacion > 0) Saturacion = Math.Max(0f, Saturacion - 1f);
                else Hambre = Math.Max(0f, Hambre - 1f);
            }

            // Regeneración: solo con el hambre casi llena (más rápida si está llena y hay saturación).
            if (Hambre >= 18f && Vida < VidaMaxima)
            {
                regeneracion += dt;
                float intervalo = Hambre >= HambreMaxima && Saturacion > 0 ? 1f : 4f;
                if (regeneracion >= intervalo)
                {
                    regeneracion = 0;
                    Vida = Math.Min(VidaMaxima, Vida + 1f);
                    Agotamiento += 6f;
                }
            }
            else regeneracion = 0;

            if (sinDano)
            {
                enElAire = false;
                Aire = AireMaximo;
                return;
            }

            // Inanición: sin comida se pierde vida hasta quedar con medio corazón.
            if (Hambre <= 0f)
            {
                hambreCero += dt;
                if (hambreCero >= 4f)
                {
                    hambreCero = 0;
                    if (Vida > 1f) Danar(1f, "de hambre");
                }
            }
            else hambreCero = 0;

            // Caídas: se mide la altura máxima del salto o la caída.
            if (j.Volando || j.EnAgua || enLava)
            {
                enElAire = false;
            }
            else if (!j.EnSuelo)
            {
                if (!enElAire) { enElAire = true; alturaMaxima = j.Posicion.Y; }
                alturaMaxima = Math.Max(alturaMaxima, j.Posicion.Y);
            }
            else if (enElAire)
            {
                enElAire = false;
                float caida = alturaMaxima - j.Posicion.Y;
                if (caida > 3.5f) Danar((float)Math.Floor(caida - 3f), "por una caída");
            }

            // Lava
            if (enLava)
            {
                temporizadorLava -= dt;
                if (temporizadorLava <= 0) { temporizadorLava = 0.5f; Danar(4f, "en la lava"); }
            }
            else temporizadorLava = 0;

            // Aire bajo el agua
            if (j.CabezaEnAgua)
            {
                Aire = Math.Max(0f, Aire - dt);
                if (Aire <= 0f)
                {
                    temporizadorAhogo -= dt;
                    if (temporizadorAhogo <= 0) { temporizadorAhogo = 1f; Danar(2f, "ahogado"); }
                }
            }
            else
            {
                Aire = Math.Min(AireMaximo, Aire + dt * 5f);
                temporizadorAhogo = 0;
            }
        }

        public void Danar(float cantidad, string causa)
        {
            if (Muerto || cantidad <= 0) return;
            if (DesdeDano < 0.45f && causa != "en la lava") return; // invulnerabilidad breve tras un golpe
            Vida = Math.Max(0f, Vida - cantidad);
            DesdeDano = 0f;
            Agotamiento += 0.1f;
            if (Herido != null) Herido(cantidad);
            if (Vida <= 0f)
            {
                Muerto = true;
                CausaMuerte = causa;
                if (Murio != null) Murio(causa);
            }
        }

        /// <summary>Come un alimento. Devuelve false si no tiene hambre.</summary>
        public bool Comer(int alimento)
        {
            if (Hambre >= HambreMaxima) return false;
            Hambre = Math.Min(HambreMaxima, Hambre + alimento);
            Saturacion = Math.Min(Hambre, Saturacion + alimento * 0.6f);
            return true;
        }

        public void Revivir()
        {
            Vida = VidaMaxima;
            Hambre = HambreMaxima;
            Saturacion = 5f;
            Aire = AireMaximo;
            Agotamiento = 0;
            Muerto = false;
            enElAire = false;
            DesdeDano = 99f;
        }
    }
}
