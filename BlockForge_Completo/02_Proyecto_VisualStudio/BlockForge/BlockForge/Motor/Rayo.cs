using System;

namespace BlockForge.Motor
{
    /// <summary>Bloque apuntado por la mira y la cara por la que entró el rayo.</summary>
    internal struct Impacto
    {
        public bool Hay;
        public int X, Y, Z;
        /// <summary>Normal de la cara: la celda adyacente donde se colocaría un bloque.</summary>
        public int NX, NY, NZ;
    }

    /// <summary>Recorrido de celdas con el algoritmo DDA de Amanatides y Woo.</summary>
    internal static class Rayo
    {
        public static Impacto Lanzar(Vec3 origen, Vec3 dir, float alcance, Func<int, int, int, bool> seleccionable)
        {
            Impacto r = new Impacto();
            int x = (int)Math.Floor(origen.X), y = (int)Math.Floor(origen.Y), z = (int)Math.Floor(origen.Z);
            int pasoX = Math.Sign(dir.X), pasoY = Math.Sign(dir.Y), pasoZ = Math.Sign(dir.Z);
            float tMaxX = Limite(origen.X, dir.X), tMaxY = Limite(origen.Y, dir.Y), tMaxZ = Limite(origen.Z, dir.Z);
            float tDeltaX = pasoX != 0 ? Math.Abs(1f / dir.X) : float.PositiveInfinity;
            float tDeltaY = pasoY != 0 ? Math.Abs(1f / dir.Y) : float.PositiveInfinity;
            float tDeltaZ = pasoZ != 0 ? Math.Abs(1f / dir.Z) : float.PositiveInfinity;
            int nx = 0, ny = 0, nz = 0;
            float t = 0;

            while (t <= alcance)
            {
                if (seleccionable(x, y, z))
                {
                    r.Hay = true;
                    r.X = x; r.Y = y; r.Z = z;
                    r.NX = nx; r.NY = ny; r.NZ = nz;
                    return r;
                }
                if (tMaxX < tMaxY && tMaxX < tMaxZ)
                {
                    x += pasoX; t = tMaxX; tMaxX += tDeltaX;
                    nx = -pasoX; ny = 0; nz = 0;
                }
                else if (tMaxY < tMaxZ)
                {
                    y += pasoY; t = tMaxY; tMaxY += tDeltaY;
                    nx = 0; ny = -pasoY; nz = 0;
                }
                else
                {
                    z += pasoZ; t = tMaxZ; tMaxZ += tDeltaZ;
                    nx = 0; ny = 0; nz = -pasoZ;
                }
            }
            return r;
        }

        private static float Limite(float s, float ds)
        {
            if (ds > 0) return ((float)Math.Floor(s) + 1 - s) / ds;
            if (ds < 0) return (s - (float)Math.Floor(s)) / -ds;
            return float.PositiveInfinity;
        }
    }
}
