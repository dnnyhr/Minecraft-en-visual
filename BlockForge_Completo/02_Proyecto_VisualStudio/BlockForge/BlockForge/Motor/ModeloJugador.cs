using System;
using System.Drawing;
using System.Drawing.Imaging;
using BlockForge.Services;

namespace BlockForge.Motor
{
    /// <summary>Textura de una skin de Minecraft ya subida a la tarjeta de video.</summary>
    internal sealed class SkinGL : IDisposable
    {
        public uint Textura { get; private set; }
        /// <summary>Formato antiguo 64×32: sin segunda capa y con las extremidades izquierdas espejadas.</summary>
        public bool Antigua { get; private set; }
        public bool Delgada { get; private set; }
        /// <summary>En skins antiguas el sombrero solo se dibuja si tiene píxeles transparentes (igual que Minecraft).</summary>
        public bool Sombrero { get; private set; }

        public static SkinGL Crear(byte[] png, bool delgada)
        {
            if (png == null) return null;
            try
            {
                using (Bitmap bmp = SkinService.Decodificar(png))
                {
                    SkinGL s = new SkinGL { Antigua = bmp.Height * 2 == bmp.Width, Delgada = delgada };
                    s.Sombrero = SkinService.SombreroVisible(bmp);
                    BitmapData d = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                    uint[] id = new uint[1];
                    GL.glGenTextures(1, id);
                    GL.glBindTexture(GL.TEXTURE_2D, id[0]);
                    GL.glTexParameteri(GL.TEXTURE_2D, GL.TEXTURE_MIN_FILTER, GL.NEAREST);
                    GL.glTexParameteri(GL.TEXTURE_2D, GL.TEXTURE_MAG_FILTER, GL.NEAREST);
                    GL.glTexParameteri(GL.TEXTURE_2D, GL.TEXTURE_WRAP_S, GL.CLAMP);
                    GL.glTexParameteri(GL.TEXTURE_2D, GL.TEXTURE_WRAP_T, GL.CLAMP);
                    GL.glTexImage2D(GL.TEXTURE_2D, 0, GL.RGBA, bmp.Width, bmp.Height, 0, GL.BGRA_EXT, GL.UNSIGNED_BYTE, d.Scan0);
                    bmp.UnlockBits(d);
                    s.Textura = id[0];
                    return s;
                }
            }
            catch (Exception)
            {
                return null; // una skin dañada no debe impedir jugar: se usa el aspecto predeterminado
            }
        }

        public void Dispose()
        {
            if (Textura != 0) GL.glDeleteTextures(1, new uint[] { Textura });
            Textura = 0;
        }
    }

    /// <summary>
    /// Personaje humanoide: cabeza, cuerpo, brazos y piernas con balanceo al caminar.
    /// Con una skin usa el mapeo de texturas de Minecraft (incluida la segunda capa);
    /// sin skin se dibuja con colores planos.
    /// </summary>
    internal static class ModeloJugador
    {
        private static readonly Color Piel = Color.FromArgb(226, 180, 140);
        private static readonly Color Pelo = Color.FromArgb(78, 52, 34);
        private static readonly Color Pantalon = Color.FromArgb(52, 64, 140);
        private static readonly Color Zapatos = Color.FromArgb(60, 60, 60);
        private const float P = 1f / 16f; // un píxel de la skin en unidades del modelo

        /// <summary>Color que multiplica a las cajas con textura (para teñir mobs de rojo, blanco o naranja).</summary>
        internal static float TinteR = 1f, TinteG = 1f, TinteB = 1f, TinteA = 1f;

        /// <summary>Dibuja el modelo con los pies en "pos" mirando según giro e inclinación.</summary>
        public static void Dibujar(Vec3 pos, float giro, float inclinacion, float caminata, float velocidad, Color camiseta, float luz, SkinGL skin)
        {
            const float escala = 0.9f; // el modelo mide 2 unidades; el jugador 1,8 bloques
            float balanceo = (float)Math.Sin(caminata) * Math.Min(1f, velocidad / 4f) * 38f;

            GL.glPushMatrix();
            GL.glTranslatef(pos.X, pos.Y, pos.Z);
            GL.glRotatef((float)(-giro * 180 / Math.PI), 0, 1, 0);
            GL.glScalef(escala, escala, escala);
            if (skin != null && skin.Textura != 0) ConSkin(skin, inclinacion, balanceo, luz);
            else SinSkin(inclinacion, balanceo, camiseta, luz);
            GL.glPopMatrix();
        }

        // ------------------------------------------------------------------ con skin
        private static void ConSkin(SkinGL s, float inclinacion, float balanceo, float luz)
        {
            float alto = s.Antigua ? 32f : 64f;
            float brazo = s.Delgada ? 3 : 4;
            GL.glEnable(GL.TEXTURE_2D);
            GL.glBindTexture(GL.TEXTURE_2D, s.Textura);

            // Primera capa (opaca)
            GL.glDisable(GL.ALPHA_TEST);
            Piezas(s, inclinacion, balanceo, luz, alto, brazo, false);
            // Segunda capa (sombrero, chaqueta, mangas y pantalón) con transparencia
            GL.glEnable(GL.ALPHA_TEST);
            Piezas(s, inclinacion, balanceo, luz, alto, brazo, true);
            GL.glDisable(GL.ALPHA_TEST);
        }

        private static void Piezas(SkinGL s, float inclinacion, float balanceo, float luz, float alto, float brazo, bool capa)
        {
            bool moderna = !s.Antigua;
            float inflar = capa ? 0.25f * P : 0;

            // Pierna derecha (+X) e izquierda (-X)
            Pieza(0.125f, 0.75f, -balanceo, () =>
            {
                if (!capa) Caja(-2, -12, -2, 2, 0, 2, 0, 16, 4, 12, 4, false, 0, luz, alto);
                else if (moderna) Caja(-2, -12, -2, 2, 0, 2, 0, 32, 4, 12, 4, false, inflar, luz, alto);
            });
            Pieza(-0.125f, 0.75f, balanceo, () =>
            {
                if (!capa) Caja(-2, -12, -2, 2, 0, 2, moderna ? 16 : 0, moderna ? 48 : 16, 4, 12, 4, !moderna, 0, luz, alto);
                else if (moderna) Caja(-2, -12, -2, 2, 0, 2, 0, 48, 4, 12, 4, false, inflar, luz, alto);
            });

            // Cuerpo
            if (!capa) Caja(-4, 12, -2, 4, 24, 2, 16, 16, 8, 12, 4, false, 0, luz, alto);
            else if (moderna) Caja(-4, 12, -2, 4, 24, 2, 16, 32, 8, 12, 4, false, inflar, luz, alto);

            // Brazo derecho (+X) e izquierdo (-X): el hombro gira 2 píxeles bajo la parte superior.
            float cx = (4 + brazo / 2f) * P;
            Pieza(cx, 22 * P, balanceo, () =>
            {
                if (!capa) Caja(-brazo / 2, -10, -2, brazo / 2, 2, 2, 40, 16, brazo, 12, 4, false, 0, luz, alto);
                else if (moderna) Caja(-brazo / 2, -10, -2, brazo / 2, 2, 2, 40, 32, brazo, 12, 4, false, inflar, luz, alto);
            });
            Pieza(-cx, 22 * P, -balanceo, () =>
            {
                if (!capa) Caja(-brazo / 2, -10, -2, brazo / 2, 2, 2, moderna ? 32 : 40, moderna ? 48 : 16, brazo, 12, 4, !moderna, 0, luz, alto);
                else if (moderna) Caja(-brazo / 2, -10, -2, brazo / 2, 2, 2, 48, 48, brazo, 12, 4, false, inflar, luz, alto);
            });

            // Cabeza con inclinación
            GL.glPushMatrix();
            GL.glTranslatef(0, 1.5f, 0);
            GL.glRotatef((float)(inclinacion * 180 / Math.PI), 1, 0, 0);
            if (!capa) Caja(-4, 0, -4, 4, 8, 4, 0, 0, 8, 8, 8, false, 0, luz, alto);
            else if (s.Sombrero) Caja(-4, 0, -4, 4, 8, 4, 32, 0, 8, 8, 8, false, 0.5f * P, luz, alto);
            GL.glPopMatrix();
        }

        private static void Pieza(float x, float y, float angulo, Action dibujar)
        {
            GL.glPushMatrix();
            GL.glTranslatef(x, y, 0);
            GL.glRotatef(angulo, 1, 0, 0);
            dibujar();
            GL.glPopMatrix();
        }

        /// <summary>
        /// Caja con el mapeo de Minecraft. Coordenadas en píxeles de skin (relativas a la pieza);
        /// (u, v) es la esquina de la plantilla y w, h, d el ancho, alto y fondo de la caja.
        /// El frente del personaje mira hacia -Z y su derecha es +X.
        /// </summary>
        internal static void Caja(float px0, float py0, float pz0, float px1, float py1, float pz1,
                                  float u, float v, float w, float h, float d, bool espejo, float inflar, float luz, float alto, float ancho = 64f)
        {
            float x0 = px0 * P - inflar, y0 = py0 * P - inflar, z0 = pz0 * P - inflar;
            float x1 = px1 * P + inflar, y1 = py1 * P + inflar, z1 = pz1 * P + inflar;
            float a = ancho;

            GL.glBegin(GL.QUADS);
            // Arriba (+Y): la parte de abajo de la plantilla toca el frente.
            Cara(luz, a, alto, espejo, u + d, v, u + d + w, v + d,
                 x1, y1, z1, x0, y1, z1, x0, y1, z0, x1, y1, z0);
            // Abajo (-Y): la parte de arriba de la plantilla toca el frente.
            Cara(luz * 0.5f, a, alto, espejo, u + d + w, v, u + d + 2 * w, v + d,
                 x1, y0, z0, x0, y0, z0, x0, y0, z1, x1, y0, z1);
            // Frente (-Z): izquierda de la plantilla = derecha del personaje (+X).
            Cara(luz * 0.8f, a, alto, espejo, u + d, v + d, u + d + w, v + d + h,
                 x1, y1, z0, x0, y1, z0, x0, y0, z0, x1, y0, z0);
            // Espalda (+Z)
            Cara(luz * 0.8f, a, alto, espejo, u + 2 * d + w, v + d, u + 2 * d + 2 * w, v + d + h,
                 x0, y1, z1, x1, y1, z1, x1, y0, z1, x0, y0, z1);
            // Lados: en una pieza espejada se intercambian las plantillas derecha e izquierda.
            float derechoU0 = espejo ? u + d + w : u, izquierdoU0 = espejo ? u : u + d + w;
            // Derecha del personaje (+X): la plantilla va de la espalda al frente.
            Cara(luz * 0.6f, a, alto, espejo, derechoU0, v + d, derechoU0 + d, v + d + h,
                 x1, y1, z1, x1, y1, z0, x1, y0, z0, x1, y0, z1);
            // Izquierda del personaje (-X): la plantilla va del frente a la espalda.
            Cara(luz * 0.6f, a, alto, espejo, izquierdoU0, v + d, izquierdoU0 + d, v + d + h,
                 x0, y1, z0, x0, y1, z1, x0, y0, z1, x0, y0, z0);
            GL.glEnd();
        }

        /// <summary>
        /// Una cara de la caja. Los vértices van en el orden: arriba-izquierda, arriba-derecha,
        /// abajo-derecha y abajo-izquierda de la plantilla (vistos desde fuera, en sentido horario de la textura).
        /// </summary>
        private static void Cara(float luz, float ancho, float alto, bool espejo, float u0, float v0, float u1, float v1,
                                 float ax, float ay, float az, float bx, float by, float bz,
                                 float cx, float cy, float cz, float dx, float dy, float dz)
        {
            if (espejo) { float t = u0; u0 = u1; u1 = t; }
            u0 /= ancho; u1 /= ancho; v0 /= alto; v1 /= alto;
            GL.glColor4f(luz * TinteR, luz * TinteG, luz * TinteB, TinteA);
            // Orden de envío antihorario visto desde fuera (a, d, c, b).
            GL.glTexCoord2f(u0, v0); GL.glVertex3f(ax, ay, az);
            GL.glTexCoord2f(u0, v1); GL.glVertex3f(dx, dy, dz);
            GL.glTexCoord2f(u1, v1); GL.glVertex3f(cx, cy, cz);
            GL.glTexCoord2f(u1, v0); GL.glVertex3f(bx, by, bz);
        }

        // ------------------------------------------------------------------ sin skin (colores planos)
        private static void SinSkin(float inclinacion, float balanceo, Color camiseta, float luz)
        {
            GL.glDisable(GL.TEXTURE_2D);
            Parte(-0.125f, 0.75f, 0, balanceo, () => { CajaColor(-0.125f, -0.75f, -0.125f, 0.125f, 0, 0.125f, Pantalon, luz); CajaColor(-0.126f, -0.75f, -0.126f, 0.126f, -0.62f, 0.126f, Zapatos, luz); });
            Parte(0.125f, 0.75f, 0, -balanceo, () => { CajaColor(-0.125f, -0.75f, -0.125f, 0.125f, 0, 0.125f, Pantalon, luz); CajaColor(-0.126f, -0.75f, -0.126f, 0.126f, -0.62f, 0.126f, Zapatos, luz); });
            CajaColor(-0.25f, 0.75f, -0.125f, 0.25f, 1.5f, 0.125f, camiseta, luz);
            Parte(-0.375f, 1.45f, 0, -balanceo, () => { CajaColor(-0.125f, -0.4f, -0.125f, 0.125f, 0.05f, 0.125f, camiseta, luz); CajaColor(-0.125f, -0.7f, -0.125f, 0.125f, -0.4f, 0.125f, Piel, luz); });
            Parte(0.375f, 1.45f, 0, balanceo, () => { CajaColor(-0.125f, -0.4f, -0.125f, 0.125f, 0.05f, 0.125f, camiseta, luz); CajaColor(-0.125f, -0.7f, -0.125f, 0.125f, -0.4f, 0.125f, Piel, luz); });

            GL.glPushMatrix();
            GL.glTranslatef(0, 1.5f, 0);
            GL.glRotatef((float)(inclinacion * 180 / Math.PI), 1, 0, 0);
            CajaColor(-0.25f, 0, -0.25f, 0.25f, 0.5f, 0.25f, Piel, luz);
            CajaColor(-0.26f, 0.38f, -0.26f, 0.26f, 0.52f, 0.26f, Pelo, luz);
            CajaColor(-0.26f, 0.12f, 0.12f, 0.26f, 0.4f, 0.26f, Pelo, luz);
            GL.glBegin(GL.QUADS);
            Rasgo(-0.17f, 0.24f, -0.06f, 0.31f, Color.White, luz);
            Rasgo(0.06f, 0.24f, 0.17f, 0.31f, Color.White, luz);
            Rasgo(-0.12f, 0.24f, -0.06f, 0.31f, Color.FromArgb(50, 70, 150), luz);
            Rasgo(0.06f, 0.24f, 0.12f, 0.31f, Color.FromArgb(50, 70, 150), luz);
            Rasgo(-0.08f, 0.1f, 0.08f, 0.14f, Color.FromArgb(150, 90, 70), luz);
            GL.glEnd();
            GL.glPopMatrix();
            GL.glEnable(GL.TEXTURE_2D);
        }

        private static void Parte(float x, float y, float z, float angulo, Action dibujar)
        {
            GL.glPushMatrix();
            GL.glTranslatef(x, y, z);
            GL.glRotatef(angulo, 1, 0, 0);
            dibujar();
            GL.glPopMatrix();
        }

        /// <summary>Rectángulo en el plano z = -0,252 de la cabeza (justo delante de la cara).</summary>
        private static void Rasgo(float x0, float y0, float x1, float y1, Color c, float luz)
        {
            const float z = -0.252f;
            GL.glColor3f(c.R / 255f * luz * 0.8f, c.G / 255f * luz * 0.8f, c.B / 255f * luz * 0.8f);
            GL.glVertex3f(x1, y0, z);
            GL.glVertex3f(x0, y0, z);
            GL.glVertex3f(x0, y1, z);
            GL.glVertex3f(x1, y1, z);
        }

        private static void Color3(Color c, float k)
        {
            GL.glColor3f(c.R / 255f * k, c.G / 255f * k, c.B / 255f * k);
        }

        private static void CajaColor(float x0, float y0, float z0, float x1, float y1, float z1, Color c, float luz)
        {
            GL.glBegin(GL.QUADS);
            Color3(c, luz);
            GL.glVertex3f(x0, y1, z0); GL.glVertex3f(x0, y1, z1); GL.glVertex3f(x1, y1, z1); GL.glVertex3f(x1, y1, z0);
            Color3(c, luz * 0.5f);
            GL.glVertex3f(x0, y0, z0); GL.glVertex3f(x1, y0, z0); GL.glVertex3f(x1, y0, z1); GL.glVertex3f(x0, y0, z1);
            Color3(c, luz * 0.8f);
            GL.glVertex3f(x1, y0, z0); GL.glVertex3f(x0, y0, z0); GL.glVertex3f(x0, y1, z0); GL.glVertex3f(x1, y1, z0);
            GL.glVertex3f(x0, y0, z1); GL.glVertex3f(x1, y0, z1); GL.glVertex3f(x1, y1, z1); GL.glVertex3f(x0, y1, z1);
            Color3(c, luz * 0.6f);
            GL.glVertex3f(x0, y0, z0); GL.glVertex3f(x0, y0, z1); GL.glVertex3f(x0, y1, z1); GL.glVertex3f(x0, y1, z0);
            GL.glVertex3f(x1, y0, z1); GL.glVertex3f(x1, y0, z0); GL.glVertex3f(x1, y1, z0); GL.glVertex3f(x1, y1, z1);
            GL.glEnd();
        }
    }
}
