using System;

namespace BlockForge.Motor
{
    /// <summary>Movimiento con colisiones de una caja (mobs). Misma lógica que la del jugador, con tamaño variable.</summary>
    internal static class Fisica
    {
        public static void Mover(ref Vec3 pos, ref Vec3 vel, float medioAncho, float alto, float dt, IConsultaMundo mundo,
                                 out bool enSuelo, out bool choco)
        {
            Vec3 delta = vel * dt;
            float mayor = Math.Max(Math.Abs(delta.X), Math.Max(Math.Abs(delta.Y), Math.Abs(delta.Z)));
            int pasos = Math.Max(1, (int)Math.Ceiling(mayor / 0.4f));
            Vec3 paso = delta * (1f / pasos);
            enSuelo = false;
            choco = false;
            for (int i = 0; i < pasos; i++)
            {
                Eje(ref pos, ref vel, 0, paso.X, medioAncho, alto, mundo, ref enSuelo, ref choco);
                Eje(ref pos, ref vel, 2, paso.Z, medioAncho, alto, mundo, ref enSuelo, ref choco);
                Eje(ref pos, ref vel, 1, paso.Y, medioAncho, alto, mundo, ref enSuelo, ref choco);
            }
        }

        private static void Eje(ref Vec3 pos, ref Vec3 vel, int eje, float d, float m, float alto, IConsultaMundo mundo, ref bool enSuelo, ref bool choco)
        {
            if (d == 0) return;
            if (eje == 0) pos.X += d; else if (eje == 1) pos.Y += d; else pos.Z += d;
            if (!Colisiona(pos, m, alto, mundo)) return;
            const float margen = 0.001f;
            if (eje == 0)
            {
                pos.X = d > 0 ? (float)Math.Floor(pos.X + m) - m - margen : (float)Math.Floor(pos.X - m) + 1 + m + margen;
                vel.X = 0;
                choco = true;
            }
            else if (eje == 2)
            {
                pos.Z = d > 0 ? (float)Math.Floor(pos.Z + m) - m - margen : (float)Math.Floor(pos.Z - m) + 1 + m + margen;
                vel.Z = 0;
                choco = true;
            }
            else
            {
                if (d < 0) { pos.Y = (float)Math.Floor(pos.Y) + 1; enSuelo = true; }
                else pos.Y = (float)Math.Floor(pos.Y + alto) - alto - margen;
                vel.Y = 0;
            }
        }

        public static bool Colisiona(Vec3 p, float m, float alto, IConsultaMundo mundo)
        {
            const float e = 0.0001f;
            int ax = (int)Math.Floor(p.X - m), bx = (int)Math.Floor(p.X + m - e);
            int ay = (int)Math.Floor(p.Y), by = (int)Math.Floor(p.Y + alto - e);
            int az = (int)Math.Floor(p.Z - m), bz = (int)Math.Floor(p.Z + m - e);
            for (int y = ay; y <= by; y++)
            {
                for (int z = az; z <= bz; z++)
                {
                    for (int x = ax; x <= bx; x++)
                    {
                        if (mundo.EsSolido(x, y, z)) return true;
                    }
                }
            }
            return false;
        }

        /// <summary>Distancia a la que un rayo entra en una caja (o -1 si no la toca).</summary>
        public static float RayoCaja(Vec3 o, Vec3 d, float x0, float y0, float z0, float x1, float y1, float z1)
        {
            float tmin = 0, tmax = float.MaxValue;
            if (!Slab(o.X, d.X, x0, x1, ref tmin, ref tmax)) return -1;
            if (!Slab(o.Y, d.Y, y0, y1, ref tmin, ref tmax)) return -1;
            if (!Slab(o.Z, d.Z, z0, z1, ref tmin, ref tmax)) return -1;
            return tmin;
        }

        private static bool Slab(float o, float d, float a, float b, ref float tmin, ref float tmax)
        {
            if (Math.Abs(d) < 1e-6f) return o >= a && o <= b;
            float t1 = (a - o) / d, t2 = (b - o) / d;
            if (t1 > t2) { float t = t1; t1 = t2; t2 = t; }
            tmin = Math.Max(tmin, t1);
            tmax = Math.Min(tmax, t2);
            return tmin <= tmax;
        }
    }
}
