using System;

namespace BlockForge.Motor
{
    /// <summary>Teclas de movimiento leídas en el cuadro actual.</summary>
    internal struct EntradaMovimiento
    {
        public bool Adelante, Atras, Izquierda, Derecha, Saltar, Bajar, Correr;
    }

    /// <summary>Consulta del mundo que necesita la física: si una celda es sólida o líquida.</summary>
    internal interface IConsultaMundo
    {
        bool EsSolido(int x, int y, int z);
        bool EsLiquido(int x, int y, int z);
        float LimiteSuperior { get; }
    }

    /// <summary>
    /// Jugador en primera persona con una caja de colisión de 0,6 × 1,8 bloques, gravedad, salto,
    /// nado y modo vuelo (valores similares a los del modo clásico de ClassiCube).
    /// </summary>
    internal class Jugador
    {
        public const float MedioAncho = 0.3f, Altura = 1.8f, AlturaOjos = 1.62f;
        private const float Gravedad = 32f, ImpulsoSalto = 9.2f;

        public Vec3 Posicion;
        public Vec3 Velocidad;
        public float Giro, Inclinacion;
        public bool EnSuelo, Volando, EnAgua, CabezaEnAgua;
        private bool chocoHorizontal;

        public Vec3 Ojo
        {
            get { return new Vec3(Posicion.X, Posicion.Y + AlturaOjos, Posicion.Z); }
        }

        public Vec3 Direccion
        {
            get
            {
                float c = (float)Math.Cos(Inclinacion);
                return new Vec3((float)Math.Sin(Giro) * c, (float)Math.Sin(Inclinacion), -(float)Math.Cos(Giro) * c);
            }
        }

        public void Girar(float dx, float dy)
        {
            Giro += dx;
            Inclinacion -= dy;
            const float limite = (float)(Math.PI / 2 - 0.001);
            if (Inclinacion > limite) Inclinacion = limite;
            if (Inclinacion < -limite) Inclinacion = -limite;
            Giro = (float)Math.IEEERemainder(Giro, Math.PI * 2);
        }

        public void Actualizar(float dt, EntradaMovimiento e, IConsultaMundo mundo)
        {
            Vec3 adelante = new Vec3((float)Math.Sin(Giro), 0, -(float)Math.Cos(Giro));
            Vec3 derecha = new Vec3((float)Math.Cos(Giro), 0, (float)Math.Sin(Giro));
            Vec3 deseo = adelante * ((e.Adelante ? 1 : 0) - (e.Atras ? 1 : 0)) + derecha * ((e.Derecha ? 1 : 0) - (e.Izquierda ? 1 : 0));
            float largo = deseo.Longitud;
            if (largo > 0) deseo = deseo * (1f / largo);

            ActualizarAgua(mundo);
            float velocidad;
            if (Volando) velocidad = e.Correr ? 22f : 10f;
            else if (EnAgua) velocidad = 2.6f;
            else velocidad = e.Correr ? 5.9f : 4.3f;

            float respuesta = Volando ? 10f : EnSuelo ? 20f : EnAgua ? 6f : 5f;
            float k = Math.Min(1f, respuesta * dt);
            Velocidad.X += (deseo.X * velocidad - Velocidad.X) * k;
            Velocidad.Z += (deseo.Z * velocidad - Velocidad.Z) * k;

            if (Volando)
            {
                float vertical = (e.Saltar ? 1 : 0) - (e.Bajar ? 1 : 0);
                Velocidad.Y += (vertical * velocidad * 0.8f - Velocidad.Y) * k;
            }
            else if (EnAgua)
            {
                Velocidad.Y -= 9f * dt;
                if (e.Saltar) Velocidad.Y += 28f * dt;
                if (e.Saltar && chocoHorizontal) Velocidad.Y = Math.Max(Velocidad.Y, 5.5f); // salir del agua trepando
                Velocidad.Y = Math.Max(-3.5f, Math.Min(Velocidad.Y, 3.8f));
            }
            else
            {
                Velocidad.Y -= Gravedad * dt;
                if (Velocidad.Y < -55f) Velocidad.Y = -55f;
                if (e.Saltar && EnSuelo) Velocidad.Y = ImpulsoSalto;
            }

            Mover(Velocidad * dt, mundo);

            if (Posicion.Y > mundo.LimiteSuperior)
            {
                Posicion.Y = mundo.LimiteSuperior;
                if (Velocidad.Y > 0) Velocidad.Y = 0;
            }
        }

        private void ActualizarAgua(IConsultaMundo mundo)
        {
            int x = (int)Math.Floor(Posicion.X), z = (int)Math.Floor(Posicion.Z);
            EnAgua = mundo.EsLiquido(x, (int)Math.Floor(Posicion.Y + 0.4f), z);
            Vec3 ojo = Ojo;
            CabezaEnAgua = mundo.EsLiquido(x, (int)Math.Floor(ojo.Y), z);
        }

        private void Mover(Vec3 delta, IConsultaMundo mundo)
        {
            float mayor = Math.Max(Math.Abs(delta.X), Math.Max(Math.Abs(delta.Y), Math.Abs(delta.Z)));
            int pasos = Math.Max(1, (int)Math.Ceiling(mayor / 0.4f));
            Vec3 paso = delta * (1f / pasos);
            EnSuelo = false;
            chocoHorizontal = false;
            for (int i = 0; i < pasos; i++)
            {
                MoverEje(0, paso.X, mundo);
                MoverEje(2, paso.Z, mundo);
                MoverEje(1, paso.Y, mundo);
            }
        }

        private void MoverEje(int eje, float d, IConsultaMundo mundo)
        {
            if (d == 0) return;
            if (eje == 0) Posicion.X += d;
            else if (eje == 1) Posicion.Y += d;
            else Posicion.Z += d;
            if (!Colisiona(mundo)) return;

            const float margen = 0.001f;
            if (eje == 0)
            {
                Posicion.X = d > 0 ? (float)Math.Floor(Posicion.X + MedioAncho) - MedioAncho - margen
                                   : (float)Math.Floor(Posicion.X - MedioAncho) + 1 + MedioAncho + margen;
                Velocidad.X = 0;
                chocoHorizontal = true;
            }
            else if (eje == 2)
            {
                Posicion.Z = d > 0 ? (float)Math.Floor(Posicion.Z + MedioAncho) - MedioAncho - margen
                                   : (float)Math.Floor(Posicion.Z - MedioAncho) + 1 + MedioAncho + margen;
                Velocidad.Z = 0;
                chocoHorizontal = true;
            }
            else
            {
                if (d < 0)
                {
                    Posicion.Y = (float)Math.Floor(Posicion.Y) + 1;
                    EnSuelo = true;
                }
                else
                {
                    Posicion.Y = (float)Math.Floor(Posicion.Y + Altura) - Altura - margen;
                }
                Velocidad.Y = 0;
            }
        }

        public bool Colisiona(IConsultaMundo mundo)
        {
            return CajaColisiona(mundo, Posicion.X - MedioAncho, Posicion.Y, Posicion.Z - MedioAncho,
                                 Posicion.X + MedioAncho, Posicion.Y + Altura, Posicion.Z + MedioAncho);
        }

        /// <summary>Indica si la caja del jugador se cruza con el bloque (x, y, z).</summary>
        public bool OcupaBloque(int x, int y, int z)
        {
            return Posicion.X + MedioAncho > x && Posicion.X - MedioAncho < x + 1
                && Posicion.Y + Altura > y && Posicion.Y < y + 1
                && Posicion.Z + MedioAncho > z && Posicion.Z - MedioAncho < z + 1;
        }

        private static bool CajaColisiona(IConsultaMundo mundo, float x0, float y0, float z0, float x1, float y1, float z1)
        {
            const float e = 0.0001f;
            int ax = (int)Math.Floor(x0), bx = (int)Math.Floor(x1 - e);
            int ay = (int)Math.Floor(y0), by = (int)Math.Floor(y1 - e);
            int az = (int)Math.Floor(z0), bz = (int)Math.Floor(z1 - e);
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
    }
}
