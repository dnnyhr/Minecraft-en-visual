using System;
using System.Collections.Generic;

namespace BlockForge.Motor
{
    /// <summary>Fragmentos que saltan al romper un bloque.</summary>
    internal class Particulas
    {
        private class Particula
        {
            public Vec3 Posicion, Velocidad;
            public float Vida, Tamano, U, V;
            public float Luz;
        }

        private readonly List<Particula> lista = new List<Particula>();
        private readonly Random azar = new Random();

        public void Romper(int x, int y, int z, int tile, float luz)
        {
            float u0, v0, u1, v1;
            Atlas.Uv(tile, out u0, out v0, out u1, out v1);
            for (int i = 0; i < 14; i++)
            {
                Particula p = new Particula
                {
                    Posicion = new Vec3(x + 0.15f + (float)azar.NextDouble() * 0.7f, y + 0.15f + (float)azar.NextDouble() * 0.7f,
                                        z + 0.15f + (float)azar.NextDouble() * 0.7f),
                    Velocidad = new Vec3(((float)azar.NextDouble() - 0.5f) * 4f, (float)azar.NextDouble() * 4.5f + 1f,
                                         ((float)azar.NextDouble() - 0.5f) * 4f),
                    Vida = 0.5f + (float)azar.NextDouble() * 0.5f,
                    Tamano = 0.08f + (float)azar.NextDouble() * 0.08f,
                    U = u0 + (float)azar.Next(12) / Atlas.Tile * (u1 - u0),
                    V = v0 + (float)azar.Next(12) / Atlas.Tile * (v1 - v0),
                    Luz = luz
                };
                lista.Add(p);
            }
        }

        public void Actualizar(float dt, IConsultaMundo mundo)
        {
            for (int i = lista.Count - 1; i >= 0; i--)
            {
                Particula p = lista[i];
                p.Vida -= dt;
                if (p.Vida <= 0)
                {
                    lista.RemoveAt(i);
                    continue;
                }
                p.Velocidad.Y -= 22f * dt;
                Vec3 siguiente = p.Posicion + p.Velocidad * dt;
                if (mundo.EsSolido((int)Math.Floor(siguiente.X), (int)Math.Floor(siguiente.Y), (int)Math.Floor(siguiente.Z)))
                {
                    p.Velocidad = new Vec3(p.Velocidad.X * 0.4f, 0, p.Velocidad.Z * 0.4f);
                }
                else
                {
                    p.Posicion = siguiente;
                }
            }
        }

        /// <summary>Dibuja cada partícula como un cuadrado orientado hacia la cámara.</summary>
        public void Dibujar(float giro, float inclinacion)
        {
            if (lista.Count == 0) return;
            Vec3 derecha = new Vec3((float)Math.Cos(giro), 0, (float)Math.Sin(giro));
            float si = (float)Math.Sin(inclinacion), ci = (float)Math.Cos(inclinacion);
            Vec3 arriba = new Vec3(-(float)Math.Sin(giro) * si, ci, (float)Math.Cos(giro) * si);
            const float paso = 4f / Atlas.Tamano;

            GL.glBindTexture(GL.TEXTURE_2D, Atlas.Textura);
            GL.glBegin(GL.QUADS);
            foreach (Particula p in lista)
            {
                Vec3 r = derecha * p.Tamano, a = arriba * p.Tamano;
                GL.glColor3f(p.Luz, p.Luz, p.Luz);
                Vec3 c = p.Posicion;
                GL.glTexCoord2f(p.U, p.V + paso); Vert(c - r - a);
                GL.glTexCoord2f(p.U + paso, p.V + paso); Vert(c + r - a);
                GL.glTexCoord2f(p.U + paso, p.V); Vert(c + r + a);
                GL.glTexCoord2f(p.U, p.V); Vert(c - r + a);
            }
            GL.glEnd();
        }

        private static void Vert(Vec3 v)
        {
            GL.glVertex3f(v.X, v.Y, v.Z);
        }
    }
}
