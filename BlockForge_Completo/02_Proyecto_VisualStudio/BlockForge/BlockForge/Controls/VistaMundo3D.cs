using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Media;
using System.Windows.Forms;
using BlockForge.Models;
using BlockForge.Motor;
using BlockForge.Services;

namespace BlockForge.Controls
{
    /// <summary>
    /// Vista 3D en primera persona del mundo, dibujada con OpenGL. Contiene el bucle del juego:
    /// lee el teclado y el mouse, mueve al jugador, aplica las acciones de construcción
    /// y dibuja el terreno, el cielo, las nubes y el HUD (mira, barra de bloques y mensajes).
    /// </summary>
    public partial class VistaMundo3D : Control, IConsultaMundo
    {
        private const float Alcance = 5f;
        private static readonly Color ColorCielo = Color.FromArgb(120, 178, 255);
        private static readonly Color ColorNiebla = Color.FromArgb(200, 226, 255);

        private IntPtr hdc, contexto;
        private bool listo;
        private string errorInicio;

        private ConstruccionService juego;
        private Mundo mundo;
        private Configuracion cfg = Configuracion.PorDefecto();
        private RenderMundo render;
        private readonly Jugador jugador = new Jugador();
        private readonly TextoGL texto = new TextoGL();
        private readonly Particulas particulas = new Particulas();
        private readonly Frustum frustum = new Frustum();
        /// <summary>Barra de 9 espacios; cada uno guarda un tipo del inventario (o null).</summary>
        private readonly TipoBloque[] barra = new TipoBloque[9];
        private int ranuraActiva;
        private Impacto objetivo;

        private readonly Stopwatch reloj = Stopwatch.StartNew();
        private double ultimoCuadro;
        private int cuadrosSegundo, fps;
        private double inicioSegundo;
        private bool sincronizacionVertical;

        private bool capturado, cursorOculto;
        private bool rawInput, rawRecibido;
        private int rawX, rawY;
        private bool botonIzquierdo, botonDerecho;
        private float repeticionIzquierdo, repeticionDerecho;
        private float balanceo, tiempoNombre, tiempoNubes;
        private string aviso;
        private Color colorAviso;
        private float tiempoAviso;

        /// <summary>Se produce cuando cambia el inventario, el bloque activo o el estado de guardado.</summary>
        public event EventHandler EstadoCambiado;
        /// <summary>El jugador eligió una opción del menú de pausa (guardar, salir, abrir a la red...).</summary>
        public event EventHandler<AccionEventArgs> AccionSolicitada;

        public VistaMundo3D()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.Opaque | ControlStyles.Selectable, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, false);
            BackColor = Color.Black;
            TabStop = true;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool MostrarDepuracion { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Volando
        {
            get { return jugador.Volando; }
            set
            {
                jugador.Volando = value;
                jugador.Velocidad = new Vec3(0, 0, 0);
                Avisar(value ? "Modo vuelo activado (Espacio sube, Shift baja)" : "Modo vuelo desactivado", Color.White);
            }
        }

        [Browsable(false)]
        public TipoBloque TipoActivo
        {
            get { return barra[ranuraActiva]; }
        }

        [Browsable(false)]
        public string TextoPosicion
        {
            get
            {
                Vec3 p = jugador.Posicion;
                return string.Format("X {0:0.0}  Y {1:0.0}  Z {2:0.0}", p.X, p.Y, p.Z);
            }
        }

        [Browsable(false)]
        public string ErrorInicio
        {
            get { return errorInicio; }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= Win32.CS_OWNDC;
                return cp;
            }
        }

        private bool EnDiseno
        {
            get { return DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime; }
        }

        // ------------------------------------------------------------------ inicialización
        /// <summary>Asocia el mundo que se va a jugar. Se llama desde FrmMundo antes de mostrarse.</summary>
        public void Iniciar(ConstruccionService juego, Configuracion configuracion)
        {
            this.juego = juego;
            mundo = juego.Mundo;
            cfg = configuracion ?? Configuracion.PorDefecto();
            Atlas.Preparar(CatalogoBloques.Tipos);

            IniciarJuego();

            if (mundo.JugadorX.HasValue && mundo.EstaDentro((int)mundo.JugadorX.Value, 0, (int)mundo.JugadorZ.Value))
            {
                jugador.Posicion = new Vec3(mundo.JugadorX.Value, mundo.JugadorY.Value, mundo.JugadorZ.Value);
                jugador.Giro = mundo.JugadorGiro;
                jugador.Inclinacion = mundo.JugadorInclinacion;
                if (jugador.Colisiona(this)) Reaparecer();
            }
            else
            {
                Reaparecer();
            }

            if (listo) CrearRender();
            Avisar("Haz clic para jugar. Rompe bloques, fabrica con E y come para recuperar vida.", Color.White, 7f);
            AgregarChat(Atlas.ClassicActivas
                            ? "Texturas: Minecraft Classic (Ver → Texturas para cambiarlas)."
                            : TexturasClassic.Disponibles
                                ? "Texturas: BlockForge (Ver → Texturas → Minecraft Classic para usar las de Classic)."
                                : "Texturas: BlockForge (Ver → Texturas → Descargar las de Minecraft Classic).",
                        Color.FromArgb(200, 220, 255));
        }

        /// <summary>Copia la posición del jugador al mundo para guardarla con la partida.</summary>
        public void GuardarPosicionEnMundo()
        {
            if (mundo == null) return;
            mundo.JugadorX = jugador.Posicion.X;
            mundo.JugadorY = jugador.Posicion.Y;
            mundo.JugadorZ = jugador.Posicion.Z;
            mundo.JugadorGiro = jugador.Giro;
            mundo.JugadorInclinacion = jugador.Inclinacion;
            mundo.HoraDia = ciclo.Hora;
            mundo.JugadorVida = vida.Muerto ? Supervivencia.VidaMaxima : vida.Vida;
            mundo.JugadorHambre = vida.Hambre;
        }

        /// <summary>Coloca al jugador sobre el terreno cerca del centro del mapa.</summary>
        public void Reaparecer()
        {
            if (mundo == null) return;
            int cxm = mundo.Ancho / 2, czm = mundo.Largo / 2;
            int mejorX = cxm, mejorZ = czm;
            // Busca en espiral la columna más cercana al centro que tenga tierra firme arriba.
            for (int radio = 0; radio < Math.Max(mundo.Ancho, mundo.Largo) / 2; radio++)
            {
                bool hallado = false;
                for (int dx = -radio; dx <= radio && !hallado; dx++)
                {
                    for (int dz = -radio; dz <= radio && !hallado; dz++)
                    {
                        if (Math.Max(Math.Abs(dx), Math.Abs(dz)) != radio) continue;
                        int top = Superficie(cxm + dx, czm + dz);
                        if (top >= 0 && !EsLiquido(cxm + dx, top, czm + dz))
                        {
                            mejorX = cxm + dx; mejorZ = czm + dz; hallado = true;
                        }
                    }
                }
                if (hallado) break;
            }
            int y = Superficie(mejorX, mejorZ) + 1;
            jugador.Posicion = new Vec3(mejorX + 0.5f, Math.Max(1, y), mejorZ + 0.5f);
            jugador.Velocidad = new Vec3(0, 0, 0);
            jugador.Giro = (float)(Math.PI * 0.25);
            jugador.Inclinacion = -0.15f;
        }

        private int Superficie(int x, int z)
        {
            for (int y = mundo.Alto - 1; y >= 0; y--)
            {
                if (mundo.ObtenerBloque(x, y, z) != 0) return y;
            }
            return -1;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (EnDiseno) return;
            try
            {
                IniciarOpenGL();
                listo = true;
                if (mundo != null) CrearRender();
                Application.Idle += AlEstarInactivo;
            }
            catch (Exception ex)
            {
                errorInicio = ex.Message;
            }
            Win32.RAWINPUTDEVICE[] mouse = { new Win32.RAWINPUTDEVICE { usUsagePage = 1, usUsage = 2, dwFlags = 0, hwndTarget = Handle } };
            rawInput = Win32.RegisterRawInputDevices(mouse, 1, (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(Win32.RAWINPUTDEVICE)));
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == Win32.WM_INPUT && capturado)
            {
                int dx, dy;
                if (Win32.LeerMovimientoRaw(m.LParam, out dx, out dy))
                {
                    rawX += dx;
                    rawY += dy;
                    rawRecibido = true;
                }
            }
            base.WndProc(ref m);
        }

        private void IniciarOpenGL()
        {
            Win32.LoadLibrary("opengl32.dll");
            hdc = Win32.GetDC(Handle);
            Win32.PIXELFORMATDESCRIPTOR pfd = new Win32.PIXELFORMATDESCRIPTOR();
            pfd.nSize = (ushort)System.Runtime.InteropServices.Marshal.SizeOf(typeof(Win32.PIXELFORMATDESCRIPTOR));
            pfd.nVersion = 1;
            pfd.dwFlags = Win32.PFD_DRAW_TO_WINDOW | Win32.PFD_SUPPORT_OPENGL | Win32.PFD_DOUBLEBUFFER;
            pfd.cColorBits = 32;
            pfd.cAlphaBits = 8;
            pfd.cDepthBits = 24;
            int formato = Win32.ChoosePixelFormat(hdc, ref pfd);
            if (formato == 0 || !Win32.SetPixelFormat(hdc, formato, ref pfd))
            {
                throw new InvalidOperationException("La tarjeta de video no ofrece un formato de píxel compatible con OpenGL.");
            }
            contexto = GL.wglCreateContext(hdc);
            if (contexto == IntPtr.Zero || !GL.wglMakeCurrent(hdc, contexto))
            {
                throw new InvalidOperationException("No se pudo crear el contexto de OpenGL. Actualice el controlador de video.");
            }

            IntPtr intervalo = GL.wglGetProcAddress("wglSwapIntervalEXT");
            if (intervalo != IntPtr.Zero)
            {
                GL.SwapIntervalEXT f = (GL.SwapIntervalEXT)System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer(intervalo, typeof(GL.SwapIntervalEXT));
                sincronizacionVertical = f(1);
            }

            GL.glEnable(GL.DEPTH_TEST);
            GL.glDepthFunc(GL.LEQUAL);
            GL.glEnable(GL.CULL_FACE);
            GL.glCullFace(GL.BACK);
            GL.glAlphaFunc(GL.GREATER, 0.5f);
            GL.glBlendFunc(GL.SRC_ALPHA, GL.ONE_MINUS_SRC_ALPHA);
            GL.glPixelStorei(GL.UNPACK_ALIGNMENT, 4);
            GL.glEnableClientState(GL.VERTEX_ARRAY);
            GL.glEnableClientState(GL.TEXTURE_COORD_ARRAY);
            GL.glEnableClientState(GL.COLOR_ARRAY);
            if (mundo == null) Atlas.Preparar(CatalogoBloques.Tipos);
            Atlas.SubirTexturas();
        }

        private void CrearRender()
        {
            if (render != null) render.Dispose();
            render = new RenderMundo(mundo, ciclo.Luz);
            render.ReconstruirPendientes(int.MaxValue);
        }

        /// <summary>Vuelve a armar el atlas (al cambiar entre las texturas de Minecraft Classic y las propias).</summary>
        public void RecargarTexturas()
        {
            if (!listo)
            {
                Atlas.Preparar(CatalogoBloques.Tipos);
                return;
            }
            GL.wglMakeCurrent(hdc, contexto);
            Atlas.Recargar(CatalogoBloques.Tipos);
            Avisar(Atlas.ClassicActivas ? "Texturas de Minecraft Classic." : "Texturas de BlockForge.", Color.White, 3f);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            Application.Idle -= AlEstarInactivo;
            LiberarMouse();
            if (listo)
            {
                GL.wglMakeCurrent(hdc, contexto);
                if (render != null) render.Dispose();
                LiberarSkins();
                mobs.Liberar();
                texto.Dispose();
                Atlas.Liberar();
                GL.wglMakeCurrent(IntPtr.Zero, IntPtr.Zero);
                GL.wglDeleteContext(contexto);
                Win32.ReleaseDC(Handle, hdc);
                listo = false;
            }
            base.OnHandleDestroyed(e);
        }

        // ------------------------------------------------------------------ bucle del juego
        private void AlEstarInactivo(object sender, EventArgs e)
        {
            Win32.MSG msg;
            while (listo && !Win32.PeekMessage(out msg, IntPtr.Zero, 0, 0, 0))
            {
                Form formulario = FindForm();
                if (formulario == null || formulario.WindowState == FormWindowState.Minimized || !Visible) return;
                Cuadro();
                if (!sincronizacionVertical) System.Threading.Thread.Sleep(1);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (EnDiseno || !listo)
            {
                e.Graphics.Clear(Color.FromArgb(120, 178, 255));
                string mensaje = errorInicio != null ? "No se pudo iniciar OpenGL: " + errorInicio : "Vista 3D del mundo";
                TextRenderer.DrawText(e.Graphics, mensaje, Font, ClientRectangle, Color.White,
                                      TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
                return;
            }
            Cuadro();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (EnDiseno || !listo) base.OnPaintBackground(e);
        }

        private void Cuadro()
        {
            if (mundo == null || render == null || ClientSize.Width <= 0 || ClientSize.Height <= 0) return;
            double ahora = reloj.Elapsed.TotalSeconds;
            float dt = (float)Math.Min(0.1, ahora - ultimoCuadro);
            ultimoCuadro = ahora;
            cuadrosSegundo++;
            if (ahora - inicioSegundo >= 1)
            {
                fps = cuadrosSegundo;
                cuadrosSegundo = 0;
                inicioSegundo = ahora;
            }

            GL.wglMakeCurrent(hdc, contexto);
            Actualizar(dt);
            Dibujar();
            Win32.SwapBuffers(hdc);
        }

        private void Actualizar(float dt)
        {
            Form formulario = FindForm();
            bool activo = capturado && formulario != null && formulario.ContainsFocus && Form.ActiveForm == formulario;
            if (capturado && !activo) LiberarMouse();

            bool controlando = capturado && !chatAbierto && !vida.Muerto;
            if (capturado)
            {
                Point centro = PointToScreen(new Point(ClientSize.Width / 2, ClientSize.Height / 2));
                Point actual = Cursor.Position;
                float factor = 0.0006f + cfg.Sensibilidad * 0.00045f;
                if (rawInput && rawRecibido)
                {
                    // Raw Input: los conteos del mouse llegan sin aceleración; el cursor solo se mantiene en el centro.
                    if (controlando) jugador.Girar(rawX * factor * 0.75f, rawY * factor * 0.75f);
                    rawX = rawY = 0;
                    if (actual != centro) Cursor.Position = centro;
                }
                else if (actual != centro)
                {
                    if (controlando) jugador.Girar((actual.X - centro.X) * factor, (actual.Y - centro.Y) * factor);
                    Cursor.Position = centro;
                }
            }

            EntradaMovimiento entrada = new EntradaMovimiento();
            if (controlando)
            {
                entrada.Adelante = Win32.TeclaPresionada(Keys.W) || Win32.TeclaPresionada(Keys.Up);
                entrada.Atras = Win32.TeclaPresionada(Keys.S) || Win32.TeclaPresionada(Keys.Down);
                entrada.Izquierda = Win32.TeclaPresionada(Keys.A) || Win32.TeclaPresionada(Keys.Left);
                entrada.Derecha = Win32.TeclaPresionada(Keys.D) || Win32.TeclaPresionada(Keys.Right);
                entrada.Saltar = Win32.TeclaPresionada(Keys.Space);
                // Shift corre al caminar y baja al volar (Ctrl se reserva para Ctrl+S y Ctrl+I).
                bool shift = Win32.TeclaPresionada(Keys.ShiftKey);
                entrada.Bajar = shift && jugador.Volando;
                // Correr necesita comida: con 3 muslos o menos no se puede.
                entrada.Correr = shift && !jugador.Volando && vida.PuedeCorrer && entrada.Adelante;
            }
            Vec3 antes = jugador.Posicion;
            bool estabaEnSuelo = jugador.EnSuelo;
            jugador.Actualizar(dt, entrada, this);
            if (jugador.Posicion.Y < -10)
            {
                if (vida.Muerto) Reaparecer();
                else vida.Danar(100f, "al caer al vacío");
            }
            Vec3 movido = jugador.Posicion - antes;
            ActualizarJuego(dt, entrada, (float)Math.Sqrt(movido.X * movido.X + movido.Z * movido.Z), estabaEnSuelo && !jugador.EnSuelo && jugador.Velocidad.Y > 0);
            ActualizarRed(dt);
            ActualizarInterfaz(dt);

            objetivo = Rayo.Lanzar(jugador.Ojo, jugador.Direccion, Alcance, Seleccionable);

            if (capturado && botonIzquierdo)
            {
                repeticionIzquierdo -= dt;
                if (repeticionIzquierdo <= 0) { Romper(); repeticionIzquierdo = 0.22f; }
            }
            if (capturado && botonDerecho && !EsComidaActiva)
            {
                repeticionDerecho -= dt;
                if (repeticionDerecho <= 0) { Colocar(); repeticionDerecho = 0.22f; }
            }

            particulas.Actualizar(dt, this);
            render.Dia = ciclo.Luz;
            render.ReconstruirPendientes(8 + Math.Min(40, render.TotalChunks / 60));
            if (balanceo > 0) balanceo = Math.Max(0, balanceo - dt * 4f);
            if (tiempoAviso > 0) tiempoAviso -= dt;
            if (tiempoNombre > 0) tiempoNombre -= dt;
            tiempoNubes += dt;
        }

        // ------------------------------------------------------------------ consultas del mundo
        public bool EsSolido(int x, int y, int z)
        {
            if (y < 0) return true;
            if (x < 0 || z < 0 || x >= mundo.Ancho || z >= mundo.Largo) return true;
            if (y >= mundo.Alto) return false;
            int id = mundo.ObtenerBloque(x, y, z);
            return id != 0 && Atlas.Obtener(id).Solido;
        }

        public bool EsLiquido(int x, int y, int z)
        {
            int id = mundo.ObtenerBloque(x, y, z);
            if (id == 0) return false;
            ModoDibujo modo = Atlas.Obtener(id).Modo;
            return modo == ModoDibujo.Liquido || modo == ModoDibujo.Lava;
        }

        private bool EsLava(int x, int y, int z)
        {
            int id = mundo.ObtenerBloque(x, y, z);
            return id != 0 && Atlas.Obtener(id).Modo == ModoDibujo.Lava;
        }

        public float LimiteSuperior
        {
            get { return mundo.Alto + 24; }
        }

        private bool Seleccionable(int x, int y, int z)
        {
            int id = mundo.ObtenerBloque(x, y, z);
            if (id == 0) return false;
            ModoDibujo modo = Atlas.Obtener(id).Modo;
            return modo != ModoDibujo.Liquido && modo != ModoDibujo.Lava;
        }

        // ------------------------------------------------------------------ construcción
        private void Romper()
        {
            if (vida.Muerto) return;
            // Primero se golpea a un mob si está más cerca que el bloque apuntado.
            if (GolpearMob()) return;
            if (!objetivo.Hay) return;
            int x = objetivo.X, y = objetivo.Y, z = objetivo.Z;
            int id = mundo.ObtenerBloque(x, y, z);
            ResultadoConstruccion r = juego.Eliminar(x, y, z);
            Balancear();
            if (r == ResultadoConstruccion.Recolectado || r == ResultadoConstruccion.RetiradoSinSumar)
            {
                particulas.Romper(x, y, z, Atlas.Obtener(id).Lado, render.BrilloEn(x, y + 1, z));
                CambioLocal(x, y, z);
                if (r == ResultadoConstruccion.Recolectado) AlRecolectar(juego.UltimoRecolectado);
                BloqueRoto(id, x, y, z);
                NotificarEstado();
            }
            else
            {
                InformarResultado(r);
            }
        }

        /// <summary>Aplica en pantalla y en la red un bloque que cambió en este equipo.</summary>
        private void CambioLocal(int x, int y, int z)
        {
            render.BloqueCambiado(x, y, z);
            render.ReconstruirPendientes(int.MaxValue);
            if (red != null) red.EnviarBloque(x, y, z, mundo.ObtenerBloque(x, y, z));
        }

        private void Colocar()
        {
            if (!objetivo.Hay) return;
            int x = objetivo.X + objetivo.NX, y = objetivo.Y + objetivo.NY, z = objetivo.Z + objetivo.NZ;
            TipoBloque tipo = TipoActivo;
            if (tipo == null || vida.Muerto) return;
            if (!tipo.EsColocable)
            {
                if (!tipo.EsComida && tipo.Categoria != TipoBloque.Herramienta) Avisar("El " + tipo.Nombre.ToLowerInvariant() + " no se coloca: sirve para fabricar (E).", Color.FromArgb(255, 210, 120));
                return;
            }
            if (!mundo.EstaDentro(x, y, z))
            {
                Avisar("No se puede construir fuera de los límites del mundo.", Color.FromArgb(255, 210, 120));
                return;
            }
            if (Atlas.Obtener(tipo.IdTipoBloque).Solido && jugador.OcupaBloque(x, y, z))
            {
                return; // no se coloca un bloque encima del propio jugador
            }
            ResultadoConstruccion r = juego.Colocar(x, y, z, tipo);
            Balancear();
            if (r == ResultadoConstruccion.Colocado)
            {
                CambioLocal(x, y, z);
                if (tipo.Nombre == "Brote") RegistrarBrote(x, y, z);
                NotificarEstado();
            }
            else
            {
                InformarResultado(r);
            }
        }

        private void InformarResultado(ResultadoConstruccion r)
        {
            Color advertencia = Color.FromArgb(255, 200, 110);
            switch (r)
            {
                case ResultadoConstruccion.SinExistencias:
                    Avisar("No tienes bloques de " + TipoActivo.Nombre.ToLowerInvariant() + ". Rompe más con clic izquierdo.", advertencia);
                    SystemSounds.Beep.Play();
                    break;
                case ResultadoConstruccion.InventarioLleno:
                    Avisar("Inventario lleno: ya tienes " + Inventario.CapacidadMaxima + " unidades de ese tipo.", advertencia);
                    SystemSounds.Beep.Play();
                    break;
                case ResultadoConstruccion.Indestructible:
                    Avisar("La capa inferior del mundo no se puede romper.", advertencia);
                    break;
                case ResultadoConstruccion.NoColocable:
                    Avisar("Ese tipo de bloque no se puede colocar.", advertencia);
                    break;
                case ResultadoConstruccion.SinSoporte:
                    Avisar(TipoActivo != null && TipoActivo.Luz > 0 ? "La antorcha necesita un bloque sólido debajo." : "Las plantas se colocan sobre pasto o tierra.", advertencia);
                    break;
            }
        }

        private void NotificarEstado()
        {
            if (EstadoCambiado != null) EstadoCambiado(this, EventArgs.Empty);
        }

        public void Avisar(string mensaje, Color color, float segundos = 3.5f)
        {
            aviso = mensaje;
            colorAviso = color;
            tiempoAviso = segundos;
        }

        public void SeleccionarRanura(int indice)
        {
            ranuraActiva = ((indice % barra.Length) + barra.Length) % barra.Length;
            tiempoNombre = 2f;
            comiendo = 0;
            NotificarEstado();
        }

        // ------------------------------------------------------------------ mouse y teclado
        private void CapturarMouse()
        {
            if (capturado) return;
            capturado = true;
            Focus();
            if (!cursorOculto) { Cursor.Hide(); cursorOculto = true; }
            Cursor.Clip = RectangleToScreen(ClientRectangle);
            Cursor.Position = PointToScreen(new Point(ClientSize.Width / 2, ClientSize.Height / 2));
        }

        /// <summary>Devuelve el cursor al sistema (pausa), por ejemplo al abrir un diálogo.</summary>
        public void LiberarMouse()
        {
            capturado = false;
            botonIzquierdo = botonDerecho = false;
            Cursor.Clip = Rectangle.Empty;
            if (cursorOculto) { Cursor.Show(); cursorOculto = false; }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (!capturado)
            {
                if (ClicInterfaz(e.Location, e.Button)) return;
                if (e.Button == MouseButtons.Left) CapturarMouse();
                return;
            }
            if (chatAbierto) return;
            if (e.Button == MouseButtons.Left)
            {
                botonIzquierdo = true;
                Romper();
                repeticionIzquierdo = 0.3f;
            }
            else if (e.Button == MouseButtons.Right)
            {
                botonDerecho = true;
                Colocar();
                repeticionDerecho = 0.3f;
            }
            else if (e.Button == MouseButtons.Middle && objetivo.Hay)
            {
                PonerEnBarra(CatalogoBloques.Obtener(mundo.ObtenerBloque(objetivo.X, objetivo.Y, objetivo.Z)));
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Left) botonIzquierdo = false;
            if (e.Button == MouseButtons.Right) botonDerecho = false;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            SeleccionarRanura(ranuraActiva + (e.Delta > 0 ? -1 : 1));
        }

        protected override bool IsInputKey(Keys keyData)
        {
            return true;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (chatAbierto)
            {
                TeclaChat(e);
                e.Handled = true;
                e.SuppressKeyPress = e.KeyCode == Keys.Enter || e.KeyCode == Keys.Escape;
                return;
            }
            if (e.Control || e.Alt) return;
            if (inventarioAbierto)
            {
                if (e.KeyCode == Keys.E || e.KeyCode == Keys.Escape) CerrarInventario();
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }
            if (e.KeyCode >= Keys.D1 && e.KeyCode <= Keys.D9) SeleccionarRanura(e.KeyCode - Keys.D1);
            else if (e.KeyCode >= Keys.NumPad1 && e.KeyCode <= Keys.NumPad9) SeleccionarRanura(e.KeyCode - Keys.NumPad1);
            else if (e.KeyCode == Keys.Escape) LiberarMouse();
            else if (e.KeyCode == Keys.F) Volando = !Volando;
            else if (e.KeyCode == Keys.F5) AlternarCamara();
            else if (e.KeyCode == Keys.R)
            {
                Reaparecer();
                Avisar("De vuelta al punto de aparición.", Color.White);
            }
            else if (e.KeyCode == Keys.E) AbrirInventario();
            else if (e.KeyCode == Keys.T || e.KeyCode == Keys.Enter) AbrirChat();
            else return;
            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if (chatAbierto) CaracterChat(e.KeyChar);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            posicionMouse = e.Location;
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            LiberarMouse();
        }

        // ------------------------------------------------------------------ dibujo 3D
        private void Dibujar()
        {
            int ancho = ClientSize.Width, alto = ClientSize.Height;
            GL.glViewport(0, 0, ancho, alto);
            texto.NuevoCuadro();

            bool bajoAgua = jugador.CabezaEnAgua;
            Color niebla = bajoAgua ? Color.FromArgb(30, 60, 140) : ciclo.ColorNiebla;
            if (EsLava((int)Math.Floor(jugador.Ojo.X), (int)Math.Floor(jugador.Ojo.Y), (int)Math.Floor(jugador.Ojo.Z))) { niebla = Color.FromArgb(200, 70, 10); bajoAgua = true; }
            float distancia = bajoAgua ? 14f : cfg.DistanciaVision;
            GL.glColorMask(1, 1, 1, 1);
            GL.glClearColor(niebla.R / 255f, niebla.G / 255f, niebla.B / 255f, 1);
            GL.glClear(GL.COLOR_BUFFER_BIT | GL.DEPTH_BUFFER_BIT);
            // El alfa del framebuffer queda en 1: la ventana siempre se compone opaca.
            GL.glColorMask(1, 1, 1, 0);

            float lejos = Math.Max(256f, cfg.DistanciaVision * 2.5f);
            float[] proyeccion = Matriz.Perspectiva(cfg.CampoVision, (float)ancho / alto, 0.05f, lejos);
            Vec3 ojo = (terceraPersona ? CamaraTercera() : jugador.Ojo) + Sacudida();
            float[] vista = Matriz.Vista(ojo, jugador.Giro, jugador.Inclinacion);
            matrizVista = Matriz.Multiplicar(proyeccion, vista);
            frustum.Actualizar(matrizVista);
            GL.glMatrixMode(GL.PROJECTION);
            GL.glLoadMatrixf(proyeccion);
            GL.glMatrixMode(GL.MODELVIEW);
            GL.glLoadMatrixf(vista);

            GL.glEnable(GL.FOG);
            GL.glFogi(GL.FOG_MODE, GL.LINEAR);
            GL.glFogf(GL.FOG_START, bajoAgua ? 0f : distancia * 0.35f);
            GL.glFogf(GL.FOG_END, distancia);
            GL.glFogfv(GL.FOG_COLOR, new float[] { niebla.R / 255f, niebla.G / 255f, niebla.B / 255f, 1 });

            if (!bajoAgua)
            {
                DibujarCielo(ojo, lejos, niebla);
                DibujarAstros(ojo);
            }

            GL.glEnable(GL.TEXTURE_2D);
            GL.glEnable(GL.ALPHA_TEST);
            GL.glBindTexture(GL.TEXTURE_2D, Atlas.Textura);
            render.Dibujar(frustum, ojo, distancia, false);
            render.DibujarBordes(distancia, false);
            GL.glBindTexture(GL.TEXTURE_2D, Atlas.Textura);
            particulas.Dibujar(jugador.Giro, jugador.Inclinacion);
            GL.glDisable(GL.ALPHA_TEST);
            DibujarJugadoresRemotos();
            mobs.Dibujar(render, (float)reloj.Elapsed.TotalSeconds);
            GL.glBindTexture(GL.TEXTURE_2D, Atlas.Textura);

            DibujarSeleccion();
            DibujarNubes(ojo, lejos);

            // Agua: primero solo profundidad y luego color, para que no se vean sus caras internas.
            GL.glEnable(GL.BLEND);
            GL.glBindTexture(GL.TEXTURE_2D, Atlas.Textura);
            GL.glColorMask(0, 0, 0, 0);
            render.Dibujar(frustum, ojo, distancia, true);
            GL.glColorMask(1, 1, 1, 0);
            render.Dibujar(frustum, ojo, distancia, true);
            render.DibujarBordes(distancia, true);
            GL.glDisable(GL.BLEND);
            GL.glDisable(GL.FOG);

            if (!terceraPersona) DibujarBloqueEnMano(ancho, alto);
            DibujarHud(ancho, alto, bajoAgua);
        }

        /// <summary>
        /// Cielo: un disco sobre el jugador, azul en el centro y del color de la niebla en el borde,
        /// así el horizonte se funde con el terreno lejano.
        /// </summary>
        private void DibujarCielo(Vec3 ojo, float lejos, Color niebla)
        {
            float y = Math.Max(mundo.Alto + 40, ojo.Y + 30);
            float radio = lejos * 0.9f;
            const int segmentos = 24;
            GL.glDisable(GL.TEXTURE_2D);
            GL.glDisable(GL.FOG);
            GL.glDepthMask(0);
            GL.glBegin(GL.TRIANGLES);
            for (int i = 0; i < segmentos; i++)
            {
                double a0 = i * Math.PI * 2 / segmentos, a1 = (i + 1) * Math.PI * 2 / segmentos;
                Color cielo = ciclo.ColorCielo;
                GL.glColor3f(cielo.R / 255f, cielo.G / 255f, cielo.B / 255f);
                GL.glVertex3f(ojo.X, y, ojo.Z);
                // Orden horario visto desde arriba: la cara queda hacia abajo, visible desde el suelo.
                GL.glColor3f(niebla.R / 255f, niebla.G / 255f, niebla.B / 255f);
                GL.glVertex3f(ojo.X + (float)Math.Cos(a0) * radio, y, ojo.Z + (float)Math.Sin(a0) * radio);
                GL.glVertex3f(ojo.X + (float)Math.Cos(a1) * radio, y, ojo.Z + (float)Math.Sin(a1) * radio);
            }
            GL.glEnd();
            GL.glDepthMask(1);
            GL.glEnable(GL.FOG);
        }

        private void DibujarNubes(Vec3 ojo, float lejos)
        {
            float y = mundo.Alto + 14;
            float d = lejos * 1.5f;
            const float escala = 1f / (12f * 64f); // cada texel de la textura cubre 12 bloques
            float desplazamiento = tiempoNubes * 0.6f;
            GL.glEnable(GL.TEXTURE_2D);
            GL.glEnable(GL.BLEND);
            GL.glDisable(GL.CULL_FACE);
            GL.glBindTexture(GL.TEXTURE_2D, Atlas.TexturaNubes);
            float brillo = 0.25f + 0.75f * ciclo.Luz;
            GL.glColor4f(brillo, brillo, brillo * 1.02f, 0.85f);
            GL.glBegin(GL.QUADS);
            float x0 = ojo.X - d, x1 = ojo.X + d, z0 = ojo.Z - d, z1 = ojo.Z + d;
            GL.glTexCoord2f((x0 + desplazamiento) * escala, z0 * escala); GL.glVertex3f(x0, y, z0);
            GL.glTexCoord2f((x0 + desplazamiento) * escala, z1 * escala); GL.glVertex3f(x0, y, z1);
            GL.glTexCoord2f((x1 + desplazamiento) * escala, z1 * escala); GL.glVertex3f(x1, y, z1);
            GL.glTexCoord2f((x1 + desplazamiento) * escala, z0 * escala); GL.glVertex3f(x1, y, z0);
            GL.glEnd();
            GL.glEnable(GL.CULL_FACE);
            GL.glDisable(GL.BLEND);
        }

        private void DibujarSeleccion()
        {
            if (!objetivo.Hay) return;
            const float e = 0.003f;
            float x0 = objetivo.X - e, y0 = objetivo.Y - e, z0 = objetivo.Z - e;
            float x1 = objetivo.X + 1 + e, y1 = objetivo.Y + 1 + e, z1 = objetivo.Z + 1 + e;
            GL.glDisable(GL.TEXTURE_2D);
            GL.glEnable(GL.BLEND);
            GL.glLineWidth(2f);
            GL.glColor4f(0, 0, 0, 0.55f);
            GL.glBegin(GL.LINES);
            Linea(x0, y0, z0, x1, y0, z0); Linea(x1, y0, z0, x1, y0, z1); Linea(x1, y0, z1, x0, y0, z1); Linea(x0, y0, z1, x0, y0, z0);
            Linea(x0, y1, z0, x1, y1, z0); Linea(x1, y1, z0, x1, y1, z1); Linea(x1, y1, z1, x0, y1, z1); Linea(x0, y1, z1, x0, y1, z0);
            Linea(x0, y0, z0, x0, y1, z0); Linea(x1, y0, z0, x1, y1, z0); Linea(x1, y0, z1, x1, y1, z1); Linea(x0, y0, z1, x0, y1, z1);
            GL.glEnd();
            GL.glDisable(GL.BLEND);
            GL.glEnable(GL.TEXTURE_2D);
        }

        private static void Linea(float x0, float y0, float z0, float x1, float y1, float z1)
        {
            GL.glVertex3f(x0, y0, z0);
            GL.glVertex3f(x1, y1, z1);
        }

        /// <summary>El bloque activo en la mano, abajo a la derecha, con un vaivén al usarlo.</summary>
        private void DibujarBloqueEnMano(int ancho, int alto)
        {
            TipoBloque tipo = TipoActivo;
            if (tipo == null || juego.Inventario.ObtenerCantidad(tipo.IdTipoBloque) == 0) return;
            Apariencia a = Atlas.Obtener(tipo.IdTipoBloque);

            GL.glClear(GL.DEPTH_BUFFER_BIT);
            GL.glMatrixMode(GL.PROJECTION);
            GL.glLoadMatrixf(Matriz.Perspectiva(70, (float)ancho / alto, 0.05f, 10f));
            GL.glMatrixMode(GL.MODELVIEW);
            GL.glLoadIdentity();
            float s = (float)Math.Sin(balanceo * Math.PI);
            float caminar = (float)Math.Sin(reloj.Elapsed.TotalSeconds * 9) * Math.Min(1f, new Vec3(jugador.Velocidad.X, 0, jugador.Velocidad.Z).Longitud / 4f) * 0.02f;
            GL.glTranslatef(0.56f - s * 0.12f, -0.56f + caminar - s * 0.1f, -0.85f - s * 0.1f);
            GL.glRotatef(45f + s * 20f, 0, 1, 0);
            GL.glRotatef(-s * 25f, 1, 0, 0);
            GL.glScalef(0.36f, 0.36f, 0.36f);
            GL.glTranslatef(-0.5f, -0.5f, -0.5f);

            GL.glEnable(GL.TEXTURE_2D);
            GL.glEnable(GL.ALPHA_TEST);
            GL.glBindTexture(GL.TEXTURE_2D, Atlas.Textura);
            float u0, v0, u1, v1;
            float brilloMano = render.BrilloEn((int)Math.Floor(jugador.Ojo.X), (int)Math.Floor(jugador.Ojo.Y), (int)Math.Floor(jugador.Ojo.Z));
            if (a.Plano)
            {
                // Plantas, comida y materiales se sostienen como un dibujo plano.
                Atlas.Uv(a.Icono, out u0, out v0, out u1, out v1);
                GL.glDisable(GL.CULL_FACE);
                GL.glBegin(GL.QUADS);
                GL.glColor3f(brilloMano, brilloMano, brilloMano);
                GL.glTexCoord2f(u0, v1); GL.glVertex3f(0, 0, 0.5f);
                GL.glTexCoord2f(u1, v1); GL.glVertex3f(1, 0, 0.5f);
                GL.glTexCoord2f(u1, v0); GL.glVertex3f(1, 1, 0.5f);
                GL.glTexCoord2f(u0, v0); GL.glVertex3f(0, 1, 0.5f);
                GL.glEnd();
                GL.glEnable(GL.CULL_FACE);
                GL.glDisable(GL.ALPHA_TEST);
                return;
            }
            GL.glBegin(GL.QUADS);
            Atlas.Uv(a.Arriba, out u0, out v0, out u1, out v1);
            GL.glColor3f(brilloMano, brilloMano, brilloMano);
            GL.glTexCoord2f(u0, v0); GL.glVertex3f(0, 1, 0);
            GL.glTexCoord2f(u0, v1); GL.glVertex3f(0, 1, 1);
            GL.glTexCoord2f(u1, v1); GL.glVertex3f(1, 1, 1);
            GL.glTexCoord2f(u1, v0); GL.glVertex3f(1, 1, 0);
            Atlas.Uv(a.Lado, out u0, out v0, out u1, out v1);
            GL.glColor3f(0.8f * brilloMano, 0.8f * brilloMano, 0.8f * brilloMano);
            GL.glTexCoord2f(u0, v1); GL.glVertex3f(0, 0, 1);
            GL.glTexCoord2f(u1, v1); GL.glVertex3f(1, 0, 1);
            GL.glTexCoord2f(u1, v0); GL.glVertex3f(1, 1, 1);
            GL.glTexCoord2f(u0, v0); GL.glVertex3f(0, 1, 1);
            GL.glColor3f(0.6f * brilloMano, 0.6f * brilloMano, 0.6f * brilloMano);
            GL.glTexCoord2f(u0, v1); GL.glVertex3f(0, 0, 0);
            GL.glTexCoord2f(u1, v1); GL.glVertex3f(0, 0, 1);
            GL.glTexCoord2f(u1, v0); GL.glVertex3f(0, 1, 1);
            GL.glTexCoord2f(u0, v0); GL.glVertex3f(0, 1, 0);
            GL.glEnd();
            GL.glDisable(GL.ALPHA_TEST);
        }

        // ------------------------------------------------------------------ HUD
        private void DibujarHud(int ancho, int alto, bool bajoAgua)
        {
            GL.glMatrixMode(GL.PROJECTION);
            GL.glLoadIdentity();
            GL.glOrtho(0, ancho, alto, 0, -1, 1);
            GL.glMatrixMode(GL.MODELVIEW);
            GL.glLoadIdentity();
            GL.glDisable(GL.DEPTH_TEST);
            GL.glDisable(GL.CULL_FACE);
            GL.glEnable(GL.BLEND);

            float escala = Math.Max(1f, Math.Min(ancho / 900f, alto / 600f));
            if (bajoAgua) Rectangulo(0, 0, ancho, alto, jugador.CabezaEnAgua ? Color.FromArgb(90, 20, 50, 160) : Color.FromArgb(150, 200, 70, 10));
            if (vida.DesdeDano < 0.4f) Rectangulo(0, 0, ancho, alto, Color.FromArgb((int)(110 * (1 - vida.DesdeDano / 0.4f)), 200, 0, 0));
            DibujarNombresJugadores(ancho, alto, escala);

            if (capturado && !chatAbierto)
            {
                // Mira con mezcla invertida para que se vea sobre cualquier fondo.
                GL.glDisable(GL.TEXTURE_2D);
                GL.glBlendFunc(GL.ONE_MINUS_DST_COLOR, GL.ZERO);
                float cx = ancho / 2f, cy = alto / 2f, m = 10 * escala, g = Math.Max(2f, 2 * escala);
                GL.glColor4f(1, 1, 1, 1);
                GL.glBegin(GL.QUADS);
                Quad(cx - m, cy - g / 2, cx + m, cy + g / 2);
                Quad(cx - g / 2, cy - m, cx + g / 2, cy - g / 2);
                Quad(cx - g / 2, cy + g / 2, cx + g / 2, cy + m);
                GL.glEnd();
                GL.glBlendFunc(GL.SRC_ALPHA, GL.ONE_MINUS_SRC_ALPHA);
            }

            DibujarBarra(ancho, alto, escala);
            DibujarVidaYHambre(ancho, alto, escala);

            GL.glEnable(GL.TEXTURE_2D);
            float t = 15 * escala;
            texto.Dibujar("BlockForge · " + mundo.Nombre, 10, 8, t, Color.White, true);
            if (juego.HayCambios) texto.Dibujar("● Cambios sin guardar (Ctrl+S)", 10, 8 + t * 1.5f, t * 0.85f, Color.FromArgb(255, 220, 120));
            if (MostrarDepuracion)
            {
                float y = 8 + t * 3f;
                string[] lineas =
                {
                    fps + " FPS" + (sincronizacionVertical ? " (vsync)" : ""),
                    "Posición: " + TextoPosicion,
                    "Mirando hacia: " + PuntoCardinal(),
                    "Chunks dibujados: " + render.ChunksDibujados + " / " + render.TotalChunks,
                    "Mundo: " + mundo.Ancho + " × " + mundo.Largo + " × " + mundo.Alto + "  Semilla: " + mundo.Semilla,
                    "Hora: " + ciclo.Texto + "  Luz: " + render.BrilloEn((int)Math.Floor(jugador.Posicion.X), (int)Math.Floor(jugador.Posicion.Y + 1), (int)Math.Floor(jugador.Posicion.Z)).ToString("0.00") +
                    "  Mobs: " + mobs.Mobs.Count,
                    objetivo.Hay ? "Apuntando a: " + NombreBloque(mundo.ObtenerBloque(objetivo.X, objetivo.Y, objetivo.Z)) +
                                   " (" + objetivo.X + ", " + objetivo.Y + ", " + objetivo.Z + ")" : "Apuntando a: -",
                    "OpenGL: " + GL.Texto(GL.RENDERER)
                };
                foreach (string linea in lineas)
                {
                    texto.Dibujar(linea, 10, y, t * 0.8f, Color.White);
                    y += t * 1.25f;
                }
            }
            float yDerecha = 8;
            if (red != null)
            {
                string estado = red.Conectado
                    ? "● En línea · " + red.CantidadJugadores + (red.CantidadJugadores == 1 ? " jugador" : " jugadores") + " · Tab: lista"
                    : "● Sin conexión";
                texto.Dibujar(estado, ancho - 10, yDerecha, t * 0.85f, red.Conectado ? Color.FromArgb(140, 235, 140) : Color.FromArgb(255, 130, 110), true, 2);
                yDerecha += t * 1.4f;
            }
            texto.Dibujar((ciclo.EsDeNoche ? "☾ " : "☀ ") + ciclo.Texto, ancho - 10, yDerecha, t * 0.85f, Color.FromArgb(230, 230, 230), true, 2);
            yDerecha += t * 1.4f;
            if (jugador.Volando) texto.Dibujar("Vuelo", ancho - 10, yDerecha, t * 0.85f, Color.FromArgb(180, 220, 255), true, 2);

            float baseBarra = alto - 64 * escala;
            if (tiempoNombre > 0 && TipoActivo != null)
            {
                float alfa = Math.Min(1f, tiempoNombre);
                texto.Dibujar(TipoActivo.Nombre, ancho / 2f, baseBarra - t * 2.4f, t * 1.05f, Color.FromArgb((int)(alfa * 255), Color.White), true, 1);
            }
            bool menuEnPantalla = !capturado || inventarioAbierto;
            if (tiempoAviso > 0 && !string.IsNullOrEmpty(aviso) && !menuEnPantalla)
            {
                float alfa = Math.Min(1f, tiempoAviso);
                texto.Dibujar(aviso, ancho / 2f, baseBarra - t * 4f, t, Color.FromArgb((int)(alfa * 255), colorAviso), true, 1);
            }

            DibujarCapasInterfaz(ancho, alto, escala);

            GL.glDisable(GL.BLEND);
            GL.glEnable(GL.DEPTH_TEST);
            GL.glEnable(GL.CULL_FACE);
        }

        private void DibujarBarra(int ancho, int alto, float escala)
        {
            int n = barra.Length;
            float lado = 46 * escala, separacion = 4 * escala;
            float total = n * lado + (n - 1) * separacion;
            float x = (ancho - total) / 2f, y = alto - lado - 12 * escala;

            GL.glDisable(GL.TEXTURE_2D);
            Rectangulo(x - 6 * escala, y - 6 * escala, x + total + 6 * escala, y + lado + 6 * escala, Color.FromArgb(110, 0, 0, 0));
            for (int i = 0; i < n; i++)
            {
                float rx = x + i * (lado + separacion);
                Rectangulo(rx, y, rx + lado, y + lado, Color.FromArgb(120, 20, 20, 20));
                if (i == ranuraActiva)
                {
                    float g = 3 * escala;
                    Color blanco = Color.FromArgb(240, 255, 255, 255);
                    Rectangulo(rx - g, y - g, rx + lado + g, y, blanco);
                    Rectangulo(rx - g, y + lado, rx + lado + g, y + lado + g, blanco);
                    Rectangulo(rx - g, y, rx, y + lado, blanco);
                    Rectangulo(rx + lado, y, rx + lado + g, y + lado, blanco);
                }
            }

            GL.glEnable(GL.TEXTURE_2D);
            GL.glEnable(GL.ALPHA_TEST);
            GL.glBindTexture(GL.TEXTURE_2D, Atlas.Textura);
            for (int i = 0; i < n; i++)
            {
                if (barra[i] == null) continue;
                float rx = x + i * (lado + separacion);
                int cantidad = juego.Inventario.ObtenerCantidad(barra[i].IdTipoBloque);
                DibujarIcono(Atlas.Obtener(barra[i].IdTipoBloque), rx + lado / 2f, y + lado / 2f, lado * 0.36f, cantidad > 0 ? 1f : 0.35f);
            }
            GL.glDisable(GL.ALPHA_TEST);
            for (int i = 0; i < n; i++)
            {
                float rx = x + i * (lado + separacion);
                if (barra[i] != null)
                {
                    int cantidad = juego.Inventario.ObtenerCantidad(barra[i].IdTipoBloque);
                    texto.Dibujar(cantidad.ToString(), rx + lado - 3 * escala, y + lado - 19 * escala, 13 * escala,
                                  cantidad > 0 ? Color.White : Color.FromArgb(160, 160, 160), true, 2);
                }
                texto.Dibujar((i + 1).ToString(), rx + 4 * escala, y + 1 * escala, 10 * escala, Color.FromArgb(200, 220, 220, 220));
            }
            if (comiendo > 0)
            {
                // Barra de progreso al comer, sobre el espacio activo.
                float rx = x + ranuraActiva * (lado + separacion);
                Rectangulo(rx + 4, y - 9 * escala, rx + lado - 4, y - 5 * escala, Color.FromArgb(180, 0, 0, 0));
                Rectangulo(rx + 4, y - 9 * escala, rx + 4 + (lado - 8) * Math.Min(1f, comiendo / TiempoComer), y - 5 * escala, Color.FromArgb(230, 120, 220, 90));
            }
        }

        /// <summary>Cubo isométrico del bloque para la barra.</summary>
        private static void DibujarIcono(Apariencia a, float cx, float cy, float r, float luz)
        {
            float u0, v0, u1, v1;
            if (a.Plano)
            {
                Atlas.Uv(a.Icono, out u0, out v0, out u1, out v1);
                GL.glColor3f(luz, luz, luz);
                GL.glBegin(GL.QUADS);
                GL.glTexCoord2f(u0, v0); GL.glVertex2f(cx - r, cy - r);
                GL.glTexCoord2f(u0, v1); GL.glVertex2f(cx - r, cy + r);
                GL.glTexCoord2f(u1, v1); GL.glVertex2f(cx + r, cy + r);
                GL.glTexCoord2f(u1, v0); GL.glVertex2f(cx + r, cy - r);
                GL.glEnd();
                return;
            }
            GL.glBegin(GL.QUADS);
            Atlas.Uv(a.Arriba, out u0, out v0, out u1, out v1);
            GL.glColor3f(luz, luz, luz);
            GL.glTexCoord2f(u0, v0); GL.glVertex2f(cx, cy - r);
            GL.glTexCoord2f(u1, v0); GL.glVertex2f(cx + r, cy - r / 2);
            GL.glTexCoord2f(u1, v1); GL.glVertex2f(cx, cy);
            GL.glTexCoord2f(u0, v1); GL.glVertex2f(cx - r, cy - r / 2);
            Atlas.Uv(a.Lado, out u0, out v0, out u1, out v1);
            GL.glColor3f(luz * 0.8f, luz * 0.8f, luz * 0.8f);
            GL.glTexCoord2f(u0, v0); GL.glVertex2f(cx - r, cy - r / 2);
            GL.glTexCoord2f(u1, v0); GL.glVertex2f(cx, cy);
            GL.glTexCoord2f(u1, v1); GL.glVertex2f(cx, cy + r);
            GL.glTexCoord2f(u0, v1); GL.glVertex2f(cx - r, cy + r / 2);
            GL.glColor3f(luz * 0.6f, luz * 0.6f, luz * 0.6f);
            GL.glTexCoord2f(u0, v0); GL.glVertex2f(cx, cy);
            GL.glTexCoord2f(u1, v0); GL.glVertex2f(cx + r, cy - r / 2);
            GL.glTexCoord2f(u1, v1); GL.glVertex2f(cx + r, cy + r / 2);
            GL.glTexCoord2f(u0, v1); GL.glVertex2f(cx, cy + r);
            GL.glEnd();
        }

        /// <summary>Rectángulo de color sólido (deja la textura desactivada).</summary>
        private static void Rectangulo(float x0, float y0, float x1, float y1, Color c)
        {
            GL.glDisable(GL.TEXTURE_2D);
            GL.glColor4f(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
            GL.glBegin(GL.QUADS);
            Quad(x0, y0, x1, y1);
            GL.glEnd();
        }

        private static void Quad(float x0, float y0, float x1, float y1)
        {
            GL.glVertex2f(x0, y0);
            GL.glVertex2f(x0, y1);
            GL.glVertex2f(x1, y1);
            GL.glVertex2f(x1, y0);
        }

        private string PuntoCardinal()
        {
            double grados = (jugador.Giro * 180 / Math.PI + 360) % 360;
            string[] nombres = { "Norte (-Z)", "Noreste", "Este (+X)", "Sureste", "Sur (+Z)", "Suroeste", "Oeste (-X)", "Noroeste" };
            return nombres[(int)Math.Round(grados / 45) % 8];
        }

        private static string NombreBloque(int id)
        {
            if (id == 0) return "Aire";
            TipoBloque t = CatalogoBloques.Obtener(id);
            return t != null ? t.Nombre : "?";
        }
    }
}
