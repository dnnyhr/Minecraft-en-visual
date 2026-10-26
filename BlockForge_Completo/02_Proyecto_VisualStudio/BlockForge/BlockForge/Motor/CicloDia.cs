using System;
using System.Drawing;

namespace BlockForge.Motor
{
    /// <summary>
    /// Ciclo de día y noche. La hora va de 0 a 1: 0 amanecer, 0,25 mediodía, 0,5 atardecer y 0,75 medianoche.
    /// Un día completo dura 20 minutos (14 de día y 6 de noche, aproximadamente).
    /// </summary>
    internal class CicloDia
    {
        public const float DuracionSegundos = 1200f;

        public float Hora { get; set; }

        public CicloDia(float hora)
        {
            Hora = hora;
        }

        public void Avanzar(float dt)
        {
            Hora = (Hora + dt / DuracionSegundos) % 1f;
            if (Hora < 0) Hora += 1f;
        }

        /// <summary>Ángulo del sol: sale por el este (+X) y se pone por el oeste.</summary>
        public float AnguloSol
        {
            get { return Hora * (float)Math.PI * 2; }
        }

        /// <summary>Luz del día de 0,12 (noche) a 1 (día), con transiciones suaves al amanecer y al atardecer.</summary>
        public float Luz
        {
            get
            {
                float s = (float)Math.Sin(AnguloSol);
                return Math.Max(0.12f, Math.Min(1f, 0.55f + s * 1.6f));
            }
        }

        /// <summary>De noche aparecen los mobs hostiles en la superficie.</summary>
        public bool EsDeNoche
        {
            get { return Luz < 0.35f; }
        }

        /// <summary>Qué tan cerca está del amanecer o del atardecer (0 a 1), para teñir el horizonte de naranja.</summary>
        public float Crepusculo
        {
            get
            {
                float s = (float)Math.Sin(AnguloSol);
                return Math.Max(0f, 1f - Math.Abs(s) * 4f);
            }
        }

        public string Texto
        {
            get
            {
                int minutos = (int)(((Hora * 24f + 6f) % 24f) * 60f);
                return string.Format("{0:00}:{1:00}", minutos / 60, minutos % 60);
            }
        }

        private static Color Mezclar(Color a, Color b, float t)
        {
            t = Math.Max(0f, Math.Min(1f, t));
            return Color.FromArgb((int)(a.R + (b.R - a.R) * t), (int)(a.G + (b.G - a.G) * t), (int)(a.B + (b.B - a.B) * t));
        }

        public Color ColorCielo
        {
            get
            {
                float t = (Luz - 0.12f) / 0.88f;
                return Mezclar(Color.FromArgb(8, 12, 34), Color.FromArgb(120, 178, 255), t);
            }
        }

        public Color ColorNiebla
        {
            get
            {
                float t = (Luz - 0.12f) / 0.88f;
                Color c = Mezclar(Color.FromArgb(14, 18, 40), Color.FromArgb(200, 226, 255), t);
                return Mezclar(c, Color.FromArgb(250, 150, 90), Crepusculo * 0.55f);
            }
        }
    }
}
