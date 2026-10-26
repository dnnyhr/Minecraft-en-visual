using System;

namespace BlockForge.Motor
{
    /// <summary>Vector de tres componentes.</summary>
    internal struct Vec3
    {
        public float X, Y, Z;

        public Vec3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vec3 operator +(Vec3 a, Vec3 b) { return new Vec3(a.X + b.X, a.Y + b.Y, a.Z + b.Z); }
        public static Vec3 operator -(Vec3 a, Vec3 b) { return new Vec3(a.X - b.X, a.Y - b.Y, a.Z - b.Z); }
        public static Vec3 operator *(Vec3 a, float k) { return new Vec3(a.X * k, a.Y * k, a.Z * k); }

        public float Longitud
        {
            get { return (float)Math.Sqrt(X * X + Y * Y + Z * Z); }
        }
    }

    /// <summary>Matrices 4x4 en orden de columnas, el formato que espera glLoadMatrixf.</summary>
    internal static class Matriz
    {
        public static float[] Identidad()
        {
            return new float[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
        }

        public static float[] Multiplicar(float[] a, float[] b)
        {
            float[] r = new float[16];
            for (int c = 0; c < 4; c++)
            {
                for (int f = 0; f < 4; f++)
                {
                    r[c * 4 + f] = a[f] * b[c * 4] + a[4 + f] * b[c * 4 + 1] + a[8 + f] * b[c * 4 + 2] + a[12 + f] * b[c * 4 + 3];
                }
            }
            return r;
        }

        public static float[] Perspectiva(float campoGrados, float aspecto, float cerca, float lejos)
        {
            float f = 1f / (float)Math.Tan(campoGrados * Math.PI / 360.0);
            float[] m = new float[16];
            m[0] = f / aspecto;
            m[5] = f;
            m[10] = (lejos + cerca) / (cerca - lejos);
            m[11] = -1;
            m[14] = 2 * lejos * cerca / (cerca - lejos);
            return m;
        }

        public static float[] RotacionX(float radianes)
        {
            float c = (float)Math.Cos(radianes), s = (float)Math.Sin(radianes);
            float[] m = Identidad();
            m[5] = c; m[6] = s; m[9] = -s; m[10] = c;
            return m;
        }

        public static float[] RotacionY(float radianes)
        {
            float c = (float)Math.Cos(radianes), s = (float)Math.Sin(radianes);
            float[] m = Identidad();
            m[0] = c; m[2] = -s; m[8] = s; m[10] = c;
            return m;
        }

        public static float[] Traslacion(float x, float y, float z)
        {
            float[] m = Identidad();
            m[12] = x; m[13] = y; m[14] = z;
            return m;
        }

        /// <summary>Cámara en primera persona: giro (yaw) alrededor de Y e inclinación (pitch).</summary>
        public static float[] Vista(Vec3 ojo, float giro, float inclinacion)
        {
            return Multiplicar(RotacionX(-inclinacion), Multiplicar(RotacionY(giro), Traslacion(-ojo.X, -ojo.Y, -ojo.Z)));
        }
    }

    /// <summary>Planos del volumen de visión, para no dibujar los chunks que quedan fuera de la pantalla.</summary>
    internal class Frustum
    {
        private readonly float[,] planos = new float[6, 4];

        public void Actualizar(float[] m)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int k = 0; k < 4; k++)
                {
                    planos[i * 2, k] = m[k * 4 + 3] + m[k * 4 + i];
                    planos[i * 2 + 1, k] = m[k * 4 + 3] - m[k * 4 + i];
                }
            }
        }

        public bool CajaVisible(float x0, float y0, float z0, float x1, float y1, float z1)
        {
            for (int p = 0; p < 6; p++)
            {
                float a = planos[p, 0], b = planos[p, 1], c = planos[p, 2], d = planos[p, 3];
                float x = a >= 0 ? x1 : x0;
                float y = b >= 0 ? y1 : y0;
                float z = c >= 0 ? z1 : z0;
                if (a * x + b * y + c * z + d < 0) return false;
            }
            return true;
        }
    }
}
