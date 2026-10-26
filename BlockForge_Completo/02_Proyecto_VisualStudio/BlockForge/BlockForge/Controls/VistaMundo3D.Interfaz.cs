using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using BlockForge.Models;
using BlockForge.Motor;
using BlockForge.Red;
using BlockForge.Services;

namespace BlockForge.Controls
{
    /// <summary>Opción elegida en el menú de pausa.</summary>
    public class AccionEventArgs : EventArgs
    {
        public const string Guardar = "guardar", AbrirRed = "red", Controles = "controles", Salir = "salir", Inventario = "inventario";

        public string Accion { get; private set; }

        public AccionEventArgs(string accion)
        {
            Accion = accion;
        }
    }

    /// <summary>
    /// Interfaz dentro del juego (dibujada con OpenGL): menú de pausa con botones, inventario visual,
    /// chat, lista de jugadores y nombres sobre los otros jugadores; y la conexión con la partida en red.
    /// </summary>
    public partial class VistaMundo3D
    {
        private class LineaChat
        {
            public string Texto;
            public Color Color;
            public float Edad;
        }

        private class BotonHud
        {
            public RectangleF Area;
            public string Texto, Accion;
            public bool Habilitado = true;
        }

        private PartidaRed red;
        private float tiempoPosicion;
        private Vec3 ultimaPosicionEnviada = new Vec3(float.NaN, 0, 0);
        private float ultimoGiroEnviado, ultimaInclinacionEnviada;
        private bool chatAbierto, inventarioAbierto;
        private readonly StringBuilder entradaChat = new StringBuilder();
        private readonly List<LineaChat> lineasChat = new List<LineaChat>();
        private readonly List<BotonHud> botones = new List<BotonHud>();
        private readonly List<KeyValuePair<RectangleF, TipoBloque>> celdasInventario = new List<KeyValuePair<RectangleF, TipoBloque>>();
        private RectangleF panelInventario;
        private Point posicionMouse;
        private float[] matrizVista = Matriz.Identidad();
        private double ultimaRed;

        // Skins: texturas de los jugadores remotos (por id) y la propia (para la tercera persona).
        private readonly Dictionary<int, SkinGL> skinsRemotas = new Dictionary<int, SkinGL>();
        private readonly Dictionary<int, byte[]> origenSkins = new Dictionary<int, byte[]>();
        private SkinGL skinPropia;
        private Skin skinLocal;
        private bool skinPropiaPendiente;
        private bool terceraPersona;
        private float caminataLocal;

        /// <summary>Skin del jugador local (se ve en tercera persona y se envía a los demás).</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Skin SkinLocal
        {
            get { return skinLocal; }
            set { skinLocal = value; skinPropiaPendiente = true; }
        }

        /// <summary>Texto que se copia con el botón "Copiar invitación" del menú de pausa.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Invitacion { get; set; }

        /// <summary>
        /// Procesa la red aunque no se esté dibujando (ventana minimizada o un diálogo abierto),
        /// para que las conexiones no se corten. Lo llama un temporizador del formulario.
        /// </summary>
        public void MantenerRed()
        {
            if (red != null && reloj.Elapsed.TotalSeconds - ultimaRed > 0.5) ActualizarRed(0.25f);
        }

        /// <summary>El jugador está en el mundo de otro (no puede guardar ni abrir la partida a la red).</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool EsInvitado { get; set; }

        /// <summary>Texto con la dirección de la partida, para el menú de pausa.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string InfoConexion { get; set; }

        [Browsable(false)]
        public PartidaRed Red
        {
            get { return red; }
        }

        // ------------------------------------------------------------------ red
        public void ConectarRed(PartidaRed partida)
        {
            DesconectarRed();
            red = partida;
            red.BloqueRecibido += AlRecibirBloque;
            red.ChatRecibido += AlRecibirChat;
            ultimaPosicionEnviada = new Vec3(float.NaN, 0, 0);
        }

        public void DesconectarRed()
        {
            if (red == null) return;
            red.BloqueRecibido -= AlRecibirBloque;
            red.ChatRecibido -= AlRecibirChat;
            red = null;
        }

        /// <summary>Libera las texturas de skins (necesita el contexto GL activo).</summary>
        private void LiberarSkins()
        {
            foreach (SkinGL s in skinsRemotas.Values) s.Dispose();
            skinsRemotas.Clear();
            origenSkins.Clear();
            if (skinPropia != null) skinPropia.Dispose();
            skinPropia = null;
            skinPropiaPendiente = skinLocal != null;
        }

        private SkinGL SkinDe(JugadorRemoto j)
        {
            byte[] origen;
            SkinGL s;
            if (skinsRemotas.TryGetValue(j.Id, out s) && origenSkins.TryGetValue(j.Id, out origen) && origen == j.Skin) return s;
            if (s != null) s.Dispose();
            s = SkinGL.Crear(j.Skin, j.SkinDelgada);
            skinsRemotas[j.Id] = s;
            origenSkins[j.Id] = j.Skin;
            return s;
        }

        /// <summary>F5: alterna entre primera y tercera persona.</summary>
        public void AlternarCamara()
        {
            terceraPersona = !terceraPersona;
            Avisar(terceraPersona ? "Cámara en tercera persona (F5 para volver)" : "Cámara en primera persona", Color.White, 2f);
        }

        /// <summary>Posición de la cámara detrás del jugador, sin atravesar bloques.</summary>
        private Vec3 CamaraTercera()
        {
            Vec3 ojo = jugador.Ojo, dir = jugador.Direccion;
            float distancia = 0;
            for (float d = 0.1f; d <= 4f; d += 0.1f)
            {
                Vec3 p = ojo - dir * d;
                bool choca = false;
                for (int i = 0; i < 8 && !choca; i++)
                {
                    float ox = (i & 1) == 0 ? -0.15f : 0.15f, oy = (i & 2) == 0 ? -0.15f : 0.15f, oz = (i & 4) == 0 ? -0.15f : 0.15f;
                    int bx = (int)Math.Floor(p.X + ox), by = (int)Math.Floor(p.Y + oy), bz = (int)Math.Floor(p.Z + oz);
                    if (mundo.EstaDentro(bx, by, bz) && EsSolido(bx, by, bz)) choca = true;
                }
                if (choca) break;
                distancia = d;
            }
            return ojo - dir * distancia;
        }

        private void ActualizarRed(float dt)
        {
            if (red == null) return;
            ultimaRed = reloj.Elapsed.TotalSeconds;
            red.Procesar();
            if (red == null) return; // el formulario pudo cerrar la partida al perder la conexión
            foreach (JugadorRemoto j in red.Jugadores) j.Interpolar(dt);

            tiempoPosicion -= dt;
            Vec3 p = jugador.Posicion;
            bool cambio = float.IsNaN(ultimaPosicionEnviada.X) || (p - ultimaPosicionEnviada).Longitud > 0.01f ||
                          Math.Abs(jugador.Giro - ultimoGiroEnviado) > 0.01f || Math.Abs(jugador.Inclinacion - ultimaInclinacionEnviada) > 0.01f;
            if (tiempoPosicion <= 0 && (cambio || tiempoPosicion < -1f))
            {
                red.EnviarPosicion(p.X, p.Y, p.Z, jugador.Giro, jugador.Inclinacion);
                ultimaPosicionEnviada = p;
                ultimoGiroEnviado = jugador.Giro;
                ultimaInclinacionEnviada = jugador.Inclinacion;
                tiempoPosicion = 0.05f;
            }
        }

        private void AlRecibirBloque(int x, int y, int z, int id)
        {
            int anterior = mundo.ObtenerBloque(x, y, z);
            if (anterior == id) return;
            mundo.EstablecerBloque(x, y, z, id);
            if (id == 0 && anterior != 0)
            {
                float luz = render.EstaIluminado(x, y + 1, z) ? 1f : 0.65f;
                particulas.Romper(x, y, z, Atlas.Obtener(anterior).Lado, luz);
            }
            render.BloqueCambiado(x, y, z);
            if (red != null && red.EsAnfitrion) juego.HayCambios = true;
            NotificarEstado();
        }

        private void AlRecibirChat(string mensaje, bool sistema)
        {
            AgregarChat(mensaje, sistema ? Color.FromArgb(255, 225, 120) : Color.White);
        }

        public void AgregarChat(string mensaje, Color color)
        {
            lineasChat.Add(new LineaChat { Texto = mensaje, Color = color });
            if (lineasChat.Count > 60) lineasChat.RemoveAt(0);
        }

        private void ActualizarInterfaz(float dt)
        {
            foreach (LineaChat l in lineasChat) l.Edad += dt;
            Vec3 v = jugador.Velocidad;
            caminataLocal += (float)Math.Sqrt(v.X * v.X + v.Z * v.Z) * dt * 2.2f;
        }

        // ------------------------------------------------------------------ chat
        /// <summary>Abre la línea de chat (desde el menú Multijugador).</summary>
        public void MostrarChat()
        {
            Focus();
            AbrirChat();
        }

        private void AbrirChat()
        {
            if (red == null)
            {
                Avisar("El chat funciona en multijugador: abre el mundo a la red o a internet desde el menú de pausa (Esc).", Color.FromArgb(255, 225, 120), 5f);
                return;
            }
            chatAbierto = true;
            entradaChat.Clear();
            botonIzquierdo = botonDerecho = false;
            CapturarMouse();
        }

        private void TeclaChat(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                chatAbierto = false;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                if (entradaChat.Length > 0 && red != null) red.EnviarChat(entradaChat.ToString());
                chatAbierto = false;
            }
            else if (e.KeyCode == Keys.Back && entradaChat.Length > 0)
            {
                entradaChat.Length--;
            }
        }

        private void CaracterChat(char c)
        {
            if (!char.IsControl(c) && entradaChat.Length < 100) entradaChat.Append(c);
        }

        // ------------------------------------------------------------------ inventario visual
        private void AbrirInventario()
        {
            inventarioAbierto = true;
            LiberarMouse();
        }

        private void CerrarInventario()
        {
            inventarioAbierto = false;
            CapturarMouse();
        }

        /// <summary>Clic con el mouse libre: botones del menú de pausa o celdas del inventario.</summary>
        private bool ClicInterfaz(Point p, MouseButtons boton)
        {
            if (boton != MouseButtons.Left) return true;
            if (inventarioAbierto)
            {
                foreach (KeyValuePair<RectangleF, TipoBloque> celda in celdasInventario)
                {
                    if (!celda.Key.Contains(p)) continue;
                    int indice = ranuras.FindIndex(t => t.IdTipoBloque == celda.Value.IdTipoBloque);
                    if (indice >= 0)
                    {
                        SeleccionarRanura(indice);
                        CerrarInventario();
                    }
                    else
                    {
                        Avisar("El " + celda.Value.Nombre.ToLowerInvariant() + " no se puede colocar.", Color.FromArgb(255, 200, 110));
                    }
                    return true;
                }
                if (!panelInventario.Contains(p)) CerrarInventario();
                return true;
            }

            foreach (BotonHud b in botones)
            {
                if (!b.Area.Contains(p)) continue;
                if (!b.Habilitado) return true;
                if (b.Accion == "continuar") CapturarMouse();
                else if (b.Accion == "copiar")
                {
                    try
                    {
                        Clipboard.SetText(Invitacion);
                        Avisar("Invitación copiada: pégala en el chat con tus amigos.", Color.FromArgb(160, 240, 160), 4f);
                    }
                    catch (Exception)
                    {
                        Avisar("No se pudo copiar al portapapeles.", Color.FromArgb(255, 200, 110));
                    }
                }
                else if (b.Accion == AccionEventArgs.Inventario) AbrirInventario();
                else if (AccionSolicitada != null) AccionSolicitada(this, new AccionEventArgs(b.Accion));
                return true;
            }
            return false;
        }

        // ------------------------------------------------------------------ jugadores remotos
        private float LuzEn(Vec3 p)
        {
            return render.EstaIluminado((int)Math.Floor(p.X), (int)Math.Floor(p.Y + 1), (int)Math.Floor(p.Z)) ? 1f : 0.65f;
        }

        private void DibujarJugadoresRemotos()
        {
            if (terceraPersona)
            {
                if (skinPropiaPendiente)
                {
                    if (skinPropia != null) skinPropia.Dispose();
                    skinPropia = skinLocal != null ? SkinGL.Crear(skinLocal.Png, skinLocal.Delgada) : null;
                    skinPropiaPendiente = false;
                }
                Vec3 v = jugador.Velocidad;
                float velocidad = (float)Math.Sqrt(v.X * v.X + v.Z * v.Z);
                string nombre = Sesion.UsuarioActual != null ? Sesion.UsuarioActual.NombreUsuario : "";
                ModeloJugador.Dibujar(jugador.Posicion, jugador.Giro, jugador.Inclinacion, caminataLocal, velocidad,
                                      JugadorRemoto.ColorPara(nombre), LuzEn(jugador.Posicion), skinPropia);
            }
            if (red == null) return;

            List<int> presentes = new List<int>();
            foreach (JugadorRemoto j in red.Jugadores)
            {
                presentes.Add(j.Id);
                if (j.Posicion.Y < -500) continue;
                ModeloJugador.Dibujar(j.Posicion, j.Giro, j.Inclinacion, j.Caminata, j.Velocidad, j.Camiseta, LuzEn(j.Posicion), SkinDe(j));
            }
            // Texturas de jugadores que ya salieron.
            foreach (int id in new List<int>(skinsRemotas.Keys))
            {
                if (presentes.Contains(id)) continue;
                if (skinsRemotas[id] != null) skinsRemotas[id].Dispose();
                skinsRemotas.Remove(id);
                origenSkins.Remove(id);
            }
            GL.glEnable(GL.TEXTURE_2D);
            GL.glBindTexture(GL.TEXTURE_2D, Atlas.Textura);
        }

        private void DibujarNombresJugadores(int ancho, int alto, float escala)
        {
            if (red == null) return;
            foreach (JugadorRemoto j in red.Jugadores)
            {
                if (j.Posicion.Y < -500) continue;
                if ((j.Posicion - jugador.Posicion).Longitud > 96) continue;
                float x = j.Posicion.X, y = j.Posicion.Y + 2.1f, z = j.Posicion.Z;
                float[] m = matrizVista;
                float cx = m[0] * x + m[4] * y + m[8] * z + m[12];
                float cy = m[1] * x + m[5] * y + m[9] * z + m[13];
                float cw = m[3] * x + m[7] * y + m[11] * z + m[15];
                if (cw < 0.1f) continue;
                float sx = (cx / cw * 0.5f + 0.5f) * ancho, sy = (1 - (cy / cw * 0.5f + 0.5f)) * alto;
                float t = 14 * escala;
                Size medida = texto.Medir(j.Nombre, t, true);
                Rectangulo(sx - medida.Width / 2f - 4, sy - medida.Height - 2, sx + medida.Width / 2f + 4, sy + 2, Color.FromArgb(110, 0, 0, 0));
                Escribir(j.Nombre, sx, sy - medida.Height, t, j.Anfitrion ? Color.FromArgb(255, 225, 120) : Color.White, true, 1);
            }
        }

        // ------------------------------------------------------------------ capas del HUD
        private void Escribir(string cadena, float x, float y, float tamano, Color color, bool negrita = false, int alineacion = 0)
        {
            GL.glEnable(GL.TEXTURE_2D);
            texto.Dibujar(cadena, x, y, tamano, color, negrita, alineacion);
        }

        private void DibujarCapasInterfaz(int ancho, int alto, float escala)
        {
            DibujarChat(ancho, alto, escala);
            Form formulario = FindForm();
            bool tab = red != null && !chatAbierto && formulario != null && Form.ActiveForm == formulario && Win32.TeclaPresionada(Keys.Tab);
            if (tab) DibujarListaJugadores(ancho, alto, escala);

            botones.Clear();
            if (inventarioAbierto) DibujarInventario(ancho, alto, escala);
            else if (!capturado) DibujarMenuPausa(ancho, alto, escala);
        }

        private void DibujarChat(int ancho, int alto, float escala)
        {
            float t = 14 * escala, alturaLinea = t * 1.45f;
            float x = 12, yBase = alto - 96 * escala;
            if (chatAbierto)
            {
                float yEntrada = yBase;
                Rectangulo(x - 4, yEntrada - 3, Math.Min(ancho - 12, x + 620 * escala), yEntrada + alturaLinea, Color.FromArgb(160, 0, 0, 0));
                bool cursor = ((int)(reloj.Elapsed.TotalSeconds * 2)) % 2 == 0;
                Escribir("> " + entradaChat + (cursor ? "_" : ""), x, yEntrada, t, Color.White);
                yBase -= alturaLinea + 6;
            }

            int maximo = chatAbierto ? 14 : 8;
            int mostradas = 0;
            for (int i = lineasChat.Count - 1; i >= 0 && mostradas < maximo; i--)
            {
                LineaChat l = lineasChat[i];
                float alfa = chatAbierto ? 1f : Math.Max(0f, Math.Min(1f, 10f - l.Edad));
                if (alfa <= 0) break;
                float y = yBase - alturaLinea * (mostradas + 1);
                Size medida = texto.Medir(l.Texto, t);
                Rectangulo(x - 4, y - 1, x + medida.Width + 4, y + alturaLinea - 1, Color.FromArgb((int)(alfa * 110), 0, 0, 0));
                Escribir(l.Texto, x, y, t, Color.FromArgb((int)(alfa * 255), l.Color));
                mostradas++;
            }
        }

        private void DibujarListaJugadores(int ancho, int alto, float escala)
        {
            List<string> nombres = red.NombresJugadores();
            float t = 15 * escala, alturaLinea = t * 1.6f;
            float w = 320 * escala, h = alturaLinea * (nombres.Count + 1.6f);
            float x = (ancho - w) / 2f, y = 60 * escala;
            Panel(x, y, x + w, y + h, Color.FromArgb(185, 18, 20, 26));
            Escribir("Jugadores en «" + red.NombreMundo + "» (" + nombres.Count + ")", ancho / 2f, y + t * 0.5f, t, Color.FromArgb(255, 225, 120), true, 1);
            for (int i = 0; i < nombres.Count; i++)
            {
                Escribir(nombres[i], x + 18 * escala, y + alturaLinea * (i + 1.3f), t, i == 0 ? Color.FromArgb(160, 230, 160) : Color.White);
            }
        }

        private void DibujarMenuPausa(int ancho, int alto, float escala)
        {
            Rectangulo(0, 0, ancho, alto, Color.FromArgb(140, 0, 0, 0));
            float t = 15 * escala;
            float y = alto * 0.16f;
            Escribir("Juego en pausa", ancho / 2f, y, t * 2.2f, Color.White, true, 1);
            string sub = mundo.Nombre + (EsInvitado ? " · invitado de " + (red != null ? red.NombreAnfitrion : "?") : "");
            Escribir(sub, ancho / 2f, y + t * 3.1f, t, Color.FromArgb(210, 210, 210), false, 1);
            float extra = 0;
            if (!string.IsNullOrEmpty(InfoConexion))
            {
                foreach (string linea in InfoConexion.Split('\n'))
                {
                    Escribir(linea, ancho / 2f, y + t * 4.6f + extra, t * 0.95f, Color.FromArgb(140, 235, 140), true, 1);
                    extra += t * 1.4f;
                }
                extra -= t * 1.4f;
            }

            List<BotonHud> lista = new List<BotonHud>
            {
                new BotonHud { Texto = "Volver al juego", Accion = "continuar" },
                new BotonHud { Texto = "Inventario", Accion = AccionEventArgs.Inventario }
            };
            if (!EsInvitado)
            {
                lista.Add(new BotonHud { Texto = juego.HayCambios ? "Guardar partida  ●" : "Guardar partida", Accion = AccionEventArgs.Guardar });
                lista.Add(red == null
                    ? new BotonHud { Texto = "Abrir a la red / internet", Accion = AccionEventArgs.AbrirRed }
                    : new BotonHud { Texto = "Partida abierta en la red", Accion = AccionEventArgs.AbrirRed, Habilitado = false });
            }
            if (!string.IsNullOrEmpty(Invitacion)) lista.Add(new BotonHud { Texto = "Copiar invitación", Accion = "copiar" });
            lista.Add(new BotonHud { Texto = "Controles", Accion = AccionEventArgs.Controles });
            lista.Add(new BotonHud { Texto = EsInvitado ? "Desconectarse" : "Salir al menú", Accion = AccionEventArgs.Salir });

            float bw = 320 * escala, bh = 40 * escala, separacion = 10 * escala;
            float by = y + t * 6.6f + extra;
            // Si no caben entre el título y la barra de bloques (más la línea de ayuda), se achican.
            float limite = alto - 96 * escala - t * 2f;
            float necesario = lista.Count * bh + (lista.Count - 1) * separacion;
            if (by + necesario > limite && necesario > 0)
            {
                float k = Math.Max(0.55f, (limite - by) / necesario);
                bh *= k;
                separacion *= k;
            }
            foreach (BotonHud b in lista)
            {
                b.Area = new RectangleF((ancho - bw) / 2f, by, bw, bh);
                DibujarBoton(b, t);
                botones.Add(b);
                by += bh + separacion;
            }

            Escribir("Esc: pausa   ·   E: inventario   ·   T: chat   ·   Tab: jugadores   ·   F5: cámara   ·   F1: controles", ancho / 2f, by + 8 * escala, t * 0.85f,
                     Color.FromArgb(190, 190, 190), false, 1);
        }

        private void DibujarBoton(BotonHud b, float t)
        {
            bool encima = b.Habilitado && b.Area.Contains(posicionMouse);
            RectangleF r = b.Area;
            Color fondo = !b.Habilitado ? Color.FromArgb(150, 40, 44, 52) : encima ? Color.FromArgb(235, 72, 130, 82) : Color.FromArgb(215, 46, 50, 60);
            Color borde = !b.Habilitado ? Color.FromArgb(120, 90, 90, 90) : encima ? Color.FromArgb(255, 170, 230, 170) : Color.FromArgb(200, 110, 116, 128);
            Rectangulo(r.Left, r.Top, r.Right, r.Bottom, borde);
            Rectangulo(r.Left + 2, r.Top + 2, r.Right - 2, r.Bottom - 2, fondo);
            // Brillo superior, como los botones de los juegos de bloques.
            Rectangulo(r.Left + 2, r.Top + 2, r.Right - 2, r.Top + 4, Color.FromArgb(encima ? 90 : 50, 255, 255, 255));
            Size medida = texto.Medir(b.Texto, t, true);
            Escribir(b.Texto, r.Left + r.Width / 2f, r.Top + (r.Height - medida.Height) / 2f, t,
                     b.Habilitado ? Color.White : Color.FromArgb(150, 150, 150), true, 1);
        }

        private void DibujarInventario(int ancho, int alto, float escala)
        {
            Rectangulo(0, 0, ancho, alto, Color.FromArgb(120, 0, 0, 0));
            IList<TipoBloque> tipos = CatalogoBloques.Tipos;
            const int columnas = 5;
            float t = 14 * escala;
            float cw = 104 * escala, ch = 112 * escala, sep = 8 * escala;
            int filas = (tipos.Count + columnas - 1) / columnas;
            float pw = columnas * cw + (columnas - 1) * sep + 40 * escala;
            float ph = filas * ch + (filas - 1) * sep + 120 * escala;
            float px = (ancho - pw) / 2f, py = (alto - ph) / 2f;
            panelInventario = new RectangleF(px, py, pw, ph);
            Panel(px, py, px + pw, py + ph, Color.FromArgb(235, 28, 31, 38));
            Escribir("Inventario", px + 20 * escala, py + 14 * escala, t * 1.6f, Color.White, true);
            Escribir("Haz clic en un bloque para usarlo · E o Esc para cerrar", px + pw - 20 * escala, py + 20 * escala, t * 0.85f,
                     Color.FromArgb(180, 180, 180), false, 2);

            celdasInventario.Clear();
            float gx = px + 20 * escala, gy = py + 64 * escala;
            for (int i = 0; i < tipos.Count; i++)
            {
                TipoBloque tipo = tipos[i];
                float x = gx + (i % columnas) * (cw + sep), y = gy + (i / columnas) * (ch + sep);
                RectangleF celda = new RectangleF(x, y, cw, ch);
                celdasInventario.Add(new KeyValuePair<RectangleF, TipoBloque>(celda, tipo));
                bool activo = TipoActivo != null && TipoActivo.IdTipoBloque == tipo.IdTipoBloque;
                bool encima = celda.Contains(posicionMouse);
                int cantidad = juego.Inventario.ObtenerCantidad(tipo.IdTipoBloque);
                Rectangulo(x, y, x + cw, y + ch, activo ? Color.FromArgb(255, 150, 220, 150) : encima ? Color.FromArgb(200, 140, 150, 170) : Color.FromArgb(160, 70, 76, 90));
                Rectangulo(x + 2, y + 2, x + cw - 2, y + ch - 2, encima ? Color.FromArgb(240, 58, 64, 78) : Color.FromArgb(240, 40, 44, 54));

                GL.glEnable(GL.TEXTURE_2D);
                GL.glEnable(GL.ALPHA_TEST);
                GL.glBindTexture(GL.TEXTURE_2D, Atlas.Textura);
                DibujarIcono(Atlas.Obtener(tipo.IdTipoBloque), x + cw / 2f, y + ch * 0.36f, cw * 0.22f, cantidad > 0 || !tipo.EsRecolectable ? 1f : 0.45f);
                GL.glDisable(GL.ALPHA_TEST);
                Escribir(tipo.Nombre, x + cw / 2f, y + ch * 0.62f, t, Color.White, true, 1);
                Escribir(tipo.EsRecolectable ? "× " + cantidad : "no recolectable", x + cw / 2f, y + ch * 0.8f, t * 0.85f,
                         tipo.EsRecolectable ? (cantidad > 0 ? Color.FromArgb(160, 230, 160) : Color.FromArgb(160, 160, 160)) : Color.FromArgb(140, 170, 220), false, 1);
            }

            Escribir("Total de bloques: " + juego.Inventario.Total() + "   ·   Capacidad máxima por tipo: " + Inventario.CapacidadMaxima,
                     px + 20 * escala, py + ph - 34 * escala, t * 0.9f, Color.FromArgb(200, 200, 200));
        }

        /// <summary>Panel con borde claro y fondo oscuro.</summary>
        private static void Panel(float x0, float y0, float x1, float y1, Color fondo)
        {
            Rectangulo(x0, y0, x1, y1, Color.FromArgb(Math.Min(255, fondo.A + 20), 95, 100, 112));
            Rectangulo(x0 + 2, y0 + 2, x1 - 2, y1 - 2, fondo);
        }
    }
}
