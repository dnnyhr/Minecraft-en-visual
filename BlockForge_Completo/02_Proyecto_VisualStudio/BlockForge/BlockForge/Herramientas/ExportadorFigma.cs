using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using BlockForge.Controls;
using BlockForge.Forms;
using BlockForge.Helpers;
using BlockForge.Models;
using BlockForge.Services;

namespace BlockForge.Herramientas
{
    /// <summary>
    /// Exporta las pantallas REALES de BlockForge para el plugin de Figma (carpeta 06_Figma).
    /// Se ejecuta con:  BlockForge.exe --exportar-figma [carpeta]
    ///
    /// Abre cada formulario de verdad (con datos de demostración, sin tocar la base de datos), le toma
    /// una captura y recorre sus controles para escribir "pantallas.json": la posición, el tamaño, los
    /// colores, las fuentes y los textos exactos que usa Windows Forms. Lo que el programa dibuja a mano
    /// (paisaje, íconos de bloques, vista 3D) se guarda como imagen. El plugin reconstruye con eso cada
    /// pantalla en Figma como capas editables y deja la captura real como referencia.
    /// </summary>
    internal static class ExportadorFigma
    {
        private static string carpeta;
        private static Bitmap captura;      // captura de la pantalla que se está exportando
        private static Form formulario;
        private static int contadorImagenes;
        private static string prefijo;
        private static readonly StringBuilder registro = new StringBuilder();

        public static bool Solicitado(string[] args)
        {
            return Array.IndexOf(args, "--exportar-figma") >= 0;
        }

        public static void Ejecutar(string[] args)
        {
            int i = Array.IndexOf(args, "--exportar-figma");
            carpeta = i + 1 < args.Length ? args[i + 1] : Path.Combine(AppContext.BaseDirectory, "ExportFigma");
            Directory.CreateDirectory(carpeta);
            Directory.CreateDirectory(Path.Combine(carpeta, "img"));
            try
            {
                PrepararDemostracion();
                List<object> pantallas = new List<object>();

                Capturar(pantallas, "01_FrmAcceso", "Inicio de sesión", new FrmAcceso(), f =>
                {
                    Poner(f, "txtUsuario", "admin");
                    Poner(f, "txtContrasena", "Admin2026");
                });
                Capturar(pantallas, "02_FrmMenuPrincipal", "Menú principal", new FrmMenuPrincipal(), null);
                Capturar(pantallas, "03_FrmNuevoMundo", "Nuevo mundo", new FrmNuevoMundo(), f => Poner(f, "txtNombre", "Valle de Ruby"));
                Capturar(pantallas, "04_FrmCargarMundo", "Cargar mundo", new FrmCargarMundo(), null);

                Inventario inventario;
                Mundo mundo = MundoDemostracion(out inventario);
                Capturar(pantallas, "05_FrmMundo_Juego", "Área de juego 3D", new FrmMundo(mundo, inventario), f => Vista(f).PrepararCaptura("juego"), true);
                Capturar(pantallas, "06_FrmMundo_Pausa", "Área de juego: menú de pausa", new FrmMundo(mundo, inventario), f => Vista(f).PrepararCaptura("pausa"), true);
                Capturar(pantallas, "07_FrmMundo_Inventario", "Área de juego: inventario y fabricación", new FrmMundo(mundo, inventario), f => Vista(f).PrepararCaptura("inventario"), true);

                Capturar(pantallas, "08_FrmInventario", "Inventario", new FrmInventario(mundo.Nombre, inventario), null);
                Capturar(pantallas, "09_FrmGestionPartidas", "Gestión de partidas", new FrmGestionPartidas(), null);
                Capturar(pantallas, "10_FrmConfiguracion", "Configuración", new FrmConfiguracion(), null);
                Capturar(pantallas, "11_FrmCuenta", "Mi cuenta", new FrmCuenta(), null);
                Capturar(pantallas, "12_FrmSkin", "Mi skin", new FrmSkin(), null);
                Capturar(pantallas, "13_FrmOpcionesPartida", "Abrir partida", new FrmOpcionesPartida(), null);
                Capturar(pantallas, "14_FrmUnirse", "Unirse a partida", new FrmUnirse(), null);

                Dictionary<string, object> raiz = new Dictionary<string, object>
                {
                    { "aplicacion", "BlockForge" },
                    { "version", Application.ProductVersion },
                    { "formato", 1 },
                    { "generado", DateTime.Now.ToString("yyyy-MM-dd HH:mm") },
                    { "sistema", Environment.OSVersion.VersionString },
                    { "entorno", Entorno() },
                    { "pantallas", pantallas }
                };
                JsonSerializerOptions opciones = new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
                File.WriteAllText(Path.Combine(carpeta, "pantallas.json"), JsonSerializer.Serialize(raiz, opciones), new UTF8Encoding(false));
                registro.AppendLine("OK: " + pantallas.Count + " pantallas en " + carpeta);
            }
            catch (Exception ex)
            {
                registro.AppendLine("ERROR: " + ex);
            }
            File.WriteAllText(Path.Combine(carpeta, "exportacion.log"), registro.ToString());
        }

        [System.Runtime.InteropServices.DllImport("ntdll.dll", EntryPoint = "wine_get_version", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern IntPtr VersionDeWine();

        /// <summary>Deja constancia de dónde se generó la exportación: Windows real o Wine (capa de compatibilidad en Linux).</summary>
        private static string Entorno()
        {
            try
            {
                return "Wine " + System.Runtime.InteropServices.Marshal.PtrToStringAnsi(VersionDeWine()) + " (BlockForge.exe ejecutado en Linux)";
            }
            catch (Exception)
            {
                return "Windows";
            }
        }

        // ------------------------------------------------------------------ datos de demostración
        private static void PrepararDemostracion()
        {
            Mensajes.Silencioso = true;
            CatalogoBloques.Establecer(CatalogoDeLosScripts());
            Sesion.UsuarioActual = new Usuario { IdUsuario = 1, NombreUsuario = "admin", Rol = Usuario.RolAdministrador, Activo = true };
            Configuracion cfg = Configuracion.PorDefecto();
            cfg.FechaModificacion = new DateTime(2026, 11, 2, 18, 30, 0);
            cfg.UsuarioModifica = "admin";
            Sesion.Configuracion = cfg;
            MundoService.DatosDemostracion = new List<ResumenMundo>
            {
                new ResumenMundo { IdMundo = 1, Nombre = "Valle de Ruby", Usuario = "ruby", Ancho = 128, Alto = 64, Largo = 128, TotalBloques = 541210, UltimoGuardado = new DateTime(2026, 11, 8, 21, 14, 0) },
                new ResumenMundo { IdMundo = 2, Nombre = "Castillo de Madai", Usuario = "madai", Ancho = 256, Alto = 128, Largo = 256, TotalBloques = 4310877, UltimoGuardado = new DateTime(2026, 11, 7, 17, 2, 0) },
                new ResumenMundo { IdMundo = 3, Nombre = "Isla de Danilo", Usuario = "danilo", Ancho = 128, Alto = 64, Largo = 128, TotalBloques = 538964, UltimoGuardado = new DateTime(2026, 11, 5, 10, 48, 0) },
                new ResumenMundo { IdMundo = 4, Nombre = "Mundo de pruebas", Usuario = "admin", Ancho = 64, Alto = 64, Largo = 64, TotalBloques = 134512, UltimoGuardado = new DateTime(2026, 10, 30, 9, 5, 0) }
            };
        }

        /// <summary>El catálogo de bloques tal como lo crean los scripts SQL incluidos (mismo orden = mismos Id).</summary>
        private static List<TipoBloque> CatalogoDeLosScripts()
        {
            List<TipoBloque> lista = new List<TipoBloque>();
            Assembly ensamblado = typeof(ExportadorFigma).Assembly;
            string[] recursos = ensamblado.GetManifestResourceNames();
            Array.Sort(recursos, StringComparer.Ordinal);
            Regex tupla = new Regex(@"\('([^']+)',\s*'(#[0-9A-Fa-f]{6})'(?:,\s*(\d),\s*(\d)(?:,\s*'(\w+)',\s*(\d+),\s*(\d+))?)?\)");
            foreach (string recurso in recursos)
            {
                if (!recurso.EndsWith(".sql", StringComparison.OrdinalIgnoreCase)) continue;
                string sql;
                using (StreamReader lector = new StreamReader(ensamblado.GetManifestResourceStream(recurso))) sql = lector.ReadToEnd();
                int desde = 0;
                while ((desde = sql.IndexOf("INSERT INTO TipoBloque", desde, StringComparison.OrdinalIgnoreCase)) >= 0)
                {
                    int hasta = sql.IndexOf(';', desde);
                    if (hasta < 0) hasta = sql.Length;
                    foreach (Match m in tupla.Matches(sql.Substring(desde, hasta - desde)))
                    {
                        string nombre = m.Groups[1].Value;
                        if (lista.Exists(t => t.Nombre == nombre)) continue;
                        lista.Add(new TipoBloque
                        {
                            IdTipoBloque = lista.Count + 1,
                            Nombre = nombre,
                            ColorHex = m.Groups[2].Value,
                            EsSolido = !m.Groups[3].Success || m.Groups[3].Value == "1",
                            EsRecolectable = !m.Groups[4].Success || m.Groups[4].Value == "1",
                            Categoria = m.Groups[5].Success ? m.Groups[5].Value : TipoBloque.Bloque,
                            Alimento = m.Groups[6].Success ? int.Parse(m.Groups[6].Value) : 0,
                            Luz = m.Groups[7].Success ? int.Parse(m.Groups[7].Value) : 0
                        });
                    }
                    desde = hasta;
                }
                foreach (Match m in Regex.Matches(sql, @"UPDATE TipoBloque SET Categoria = '(\w+)' WHERE Nombre = '([^']+)'"))
                {
                    TipoBloque tipo = lista.Find(t => t.Nombre == m.Groups[2].Value);
                    if (tipo != null) tipo.Categoria = m.Groups[1].Value;
                }
            }
            registro.AppendLine("Catálogo: " + lista.Count + " tipos.");
            return lista;
        }

        private static Mundo MundoDemostracion(out Inventario inventario)
        {
            Mundo mundo = new Mundo(128, 64, 128) { IdMundo = 1, IdUsuario = 1, Nombre = "Valle de Ruby", Semilla = 20261108, ConLagos = true, FechaCreacion = DateTime.Now };
            GeneradorTerreno.Generar(mundo);
            inventario = new Inventario();
            if (Construcciones.ConstruirCasa(mundo, new Random(mundo.Semilla))) inventario = Construcciones.MaterialesIniciales();
            foreach (string nombre in new[] { "Pasto", "Tierra", "Piedra", "Arena" })
            {
                inventario.EstablecerCantidad(CatalogoBloques.PorNombre(nombre).IdTipoBloque, 48);
            }
            mundo.HoraDia = 0.2f;
            return mundo;
        }

        private static VistaMundo3D Vista(Form f)
        {
            return (VistaMundo3D)f.Controls.Find("vistaMundo", true)[0];
        }

        private static void Poner(Form f, string nombre, string texto)
        {
            Control[] c = f.Controls.Find(nombre, true);
            if (c.Length > 0) c[0].Text = texto;
        }

        private static void Esperar(int milisegundos)
        {
            DateTime fin = DateTime.Now.AddMilliseconds(milisegundos);
            while (DateTime.Now < fin)
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(10);
            }
        }

        // ------------------------------------------------------------------ captura de una pantalla
        private static void Capturar(List<object> pantallas, string id, string titulo, Form f, Action<Form> preparar, bool juego = false)
        {
            try
            {
                formulario = f;
                prefijo = id;
                contadorImagenes = 0;
                f.StartPosition = FormStartPosition.Manual;
                f.Location = new Point(40, 40);
                f.TopMost = true;
                f.Show();
                f.Activate();
                Esperar(400);
                if (preparar != null) preparar(f);
                // Sin foco en ningún control: así no aparecen el cursor de texto ni selecciones en la captura.
                f.ActiveControl = null;
                if (juego)
                {
                    // La vista 3D avanza cuadro a cuadro: se dibujan varios para que todos los chunks tengan malla.
                    // Se espera además a que se desvanezcan los avisos de bienvenida del chat (10 segundos).
                    VistaMundo3D vista = Vista(f);
                    DateTime fin = DateTime.Now.AddSeconds(11.5);
                    while (DateTime.Now < fin)
                    {
                        vista.Refresh();
                        Application.DoEvents();
                        System.Threading.Thread.Sleep(8);
                    }
                    if (vista.ErrorInicio != null) registro.AppendLine(id + ": OpenGL no disponible: " + vista.ErrorInicio);
                }
                Esperar(500);
                f.Refresh();
                Esperar(200);

                Size cliente = f.ClientSize;
                captura = new Bitmap(cliente.Width, cliente.Height, PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(captura))
                {
                    g.CopyFromScreen(f.PointToScreen(Point.Empty), Point.Empty, cliente);
                }
                captura.Save(Path.Combine(carpeta, id + ".png"), ImageFormat.Png);

                List<object> nodos = new List<object>();
                nodos.Add(Rect("Fondo", 0, 0, cliente.Width, cliente.Height, f.BackColor));
                foreach (Control hijo in EnOrden(f)) nodos.Add(Nodo(hijo));
                nodos.RemoveAll(n => n == null);

                pantallas.Add(new Dictionary<string, object>
                {
                    { "id", id },
                    { "formulario", f.GetType().Name },
                    { "nombre", titulo },
                    { "titulo", f.Text },
                    { "ancho", cliente.Width },
                    { "alto", cliente.Height },
                    { "escala", f.DeviceDpi / 96f },
                    { "redimensionable", f.FormBorderStyle == FormBorderStyle.Sizable },
                    { "minimizar", f.MinimizeBox },
                    { "maximizar", f.MaximizeBox },
                    { "captura", id + ".png" },
                    { "nodos", nodos }
                });
                registro.AppendLine(id + ": " + cliente.Width + "x" + cliente.Height);
            }
            catch (Exception ex)
            {
                registro.AppendLine(id + ": ERROR " + ex);
            }
            finally
            {
                try { f.Close(); f.Dispose(); } catch (Exception) { }
                Esperar(200);
            }
        }

        /// <summary>Los hijos del fondo hacia el frente (Windows Forms guarda primero el que está más al frente).</summary>
        private static List<Control> EnOrden(Control padre)
        {
            List<Control> lista = new List<Control>();
            for (int i = padre.Controls.Count - 1; i >= 0; i--)
            {
                if (padre.Controls[i].Visible && padre.Controls[i].Width > 0 && padre.Controls[i].Height > 0) lista.Add(padre.Controls[i]);
            }
            return lista;
        }

        // ------------------------------------------------------------------ primitivas del archivo
        private static string Hex(Color c)
        {
            return string.Format("#{0:X2}{1:X2}{2:X2}", c.R, c.G, c.B);
        }

        private static Dictionary<string, object> Base(string tipo, string nombre, float x, float y, float w, float h)
        {
            return new Dictionary<string, object> { { "tipo", tipo }, { "nombre", nombre }, { "x", x }, { "y", y }, { "w", w }, { "h", h } };
        }

        private static Dictionary<string, object> Rect(string nombre, float x, float y, float w, float h, Color relleno)
        {
            Dictionary<string, object> n = Base("rect", nombre, x, y, w, h);
            if (relleno.A > 0)
            {
                n["relleno"] = Hex(relleno);
                n["opacidad"] = relleno.A / 255f;
            }
            return n;
        }

        private static Dictionary<string, object> Borde(Dictionary<string, object> n, Color color, float grosor)
        {
            n["borde"] = Hex(color);
            n["bordeOpacidad"] = color.A / 255f;
            n["grosor"] = grosor;
            return n;
        }

        private static Dictionary<string, object> Grupo(string nombre, float x, float y, float w, float h, List<object> hijos, bool recortar = false)
        {
            Dictionary<string, object> n = Base("grupo", nombre, x, y, w, h);
            hijos.RemoveAll(o => o == null);
            n["hijos"] = hijos;
            if (recortar) n["recortar"] = true;
            return n;
        }

        private static Dictionary<string, object> Imagen(string nombre, float x, float y, float w, float h, Bitmap bmp)
        {
            string archivo = "img/" + prefijo + "_" + (++contadorImagenes).ToString("00") + "_" + Regex.Replace(nombre, "[^A-Za-z0-9]", "") + ".png";
            bmp.Save(Path.Combine(carpeta, archivo.Replace('/', Path.DirectorySeparatorChar)), ImageFormat.Png);
            Dictionary<string, object> n = Base("imagen", nombre, x, y, w, h);
            n["archivo"] = archivo;
            return n;
        }

        /// <summary>Recorte de la captura real (para lo que dibuja Windows: casillas, deslizadores, flechas...).</summary>
        private static Dictionary<string, object> Recorte(string nombre, Rectangle zona)
        {
            zona.Intersect(new Rectangle(Point.Empty, captura.Size));
            if (zona.Width <= 0 || zona.Height <= 0) return null;
            using (Bitmap parte = captura.Clone(zona, PixelFormat.Format32bppArgb))
            {
                return Imagen(nombre, zona.X, zona.Y, zona.Width, zona.Height, parte);
            }
        }

        private static Color Pixel(int x, int y)
        {
            x = Math.Max(0, Math.Min(captura.Width - 1, x));
            y = Math.Max(0, Math.Min(captura.Height - 1, y));
            return captura.GetPixel(x, y);
        }

        private static Dictionary<string, object> Fuente(Font f)
        {
            string familia = f.Name, estilo = "Regular";
            foreach (string peso in new[] { "Semibold", "Semilight", "Light", "Black" })
            {
                if (familia.EndsWith(" " + peso, StringComparison.OrdinalIgnoreCase))
                {
                    familia = familia.Substring(0, familia.Length - peso.Length - 1);
                    estilo = peso;
                }
            }
            if (f.Bold) estilo = "Bold";
            if (f.Italic) estilo = estilo == "Regular" ? "Italic" : estilo + " Italic";
            return new Dictionary<string, object>
            {
                { "familia", familia },
                { "estilo", estilo },
                { "puntos", f.SizeInPoints },
                { "px", f.SizeInPoints * formulario.DeviceDpi / 72f }
            };
        }

        /// <summary>
        /// Texto ya resuelto: "x,y" es la esquina superior izquierda del bloque de líneas, "w" el ancho donde se
        /// alinea y "lineas" los renglones tal como los parte Windows (el plugin no vuelve a calcular saltos).
        /// </summary>
        private static Dictionary<string, object> Texto(string nombre, string texto, Font fuente, Color color, RectangleF zona, ContentAlignment alineacion, bool partir, float altoLinea = 0)
        {
            if (string.IsNullOrEmpty(texto)) return null;
            int alto = altoLinea > 0 ? (int)Math.Round(altoLinea) : AltoLinea(fuente);
            List<string> lineas = PartirLineas(texto, fuente, partir ? zona.Width : float.MaxValue);
            // Con varios renglones, Windows no dibuja el último si no cabe completo en el control.
            if (partir && lineas.Count > 1)
            {
                int caben = Math.Max(1, (int)Math.Floor(zona.Height / alto));
                if (lineas.Count > caben) lineas.RemoveRange(caben, lineas.Count - caben);
            }
            float altoBloque = lineas.Count * alto;
            string h = "izquierda", v = "arriba";
            switch (alineacion)
            {
                case ContentAlignment.TopCenter: case ContentAlignment.MiddleCenter: case ContentAlignment.BottomCenter: h = "centro"; break;
                case ContentAlignment.TopRight: case ContentAlignment.MiddleRight: case ContentAlignment.BottomRight: h = "derecha"; break;
            }
            switch (alineacion)
            {
                case ContentAlignment.MiddleLeft: case ContentAlignment.MiddleCenter: case ContentAlignment.MiddleRight: v = "medio"; break;
                case ContentAlignment.BottomLeft: case ContentAlignment.BottomCenter: case ContentAlignment.BottomRight: v = "abajo"; break;
            }
            float y = zona.Y;
            if (v == "medio") y = zona.Y + (float)Math.Floor((zona.Height - altoBloque) / 2f);
            else if (v == "abajo") y = zona.Bottom - altoBloque;
            Dictionary<string, object> n = Base("texto", nombre, zona.X, y, zona.Width, altoBloque);
            n["texto"] = string.Join("\n", lineas);
            n["fuente"] = Fuente(fuente);
            n["color"] = Hex(color);
            n["opacidad"] = color.A / 255f;
            n["alinear"] = h;
            n["altoLinea"] = alto;
            // Ancho real de cada renglón en Windows: sirve para ajustar el espaciado si Figma usa otra fuente.
            List<int> anchos = new List<int>();
            foreach (string linea in lineas)
            {
                anchos.Add(linea.Length == 0 ? 0 : TextRenderer.MeasureText(linea, fuente, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix).Width);
            }
            n["anchos"] = anchos;
            return n;
        }

        /// <summary>Parte el texto en renglones midiendo con el mismo motor de texto (GDI) que usan los controles.</summary>
        private static List<string> PartirLineas(string texto, Font fuente, float ancho)
        {
            List<string> lineas = new List<string>();
            const TextFormatFlags medir = TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix;
            foreach (string parrafo in texto.Replace("\r\n", "\n").Split('\n'))
            {
                if (ancho == float.MaxValue || TextRenderer.MeasureText(parrafo, fuente, Size.Empty, medir).Width <= ancho)
                {
                    lineas.Add(parrafo);
                    continue;
                }
                string actual = string.Empty;
                foreach (string palabra in parrafo.Split(' '))
                {
                    string candidata = actual.Length == 0 ? palabra : actual + " " + palabra;
                    if (actual.Length > 0 && TextRenderer.MeasureText(candidata, fuente, Size.Empty, medir).Width > ancho)
                    {
                        lineas.Add(actual);
                        actual = palabra;
                    }
                    else actual = candidata;
                }
                lineas.Add(actual);
            }
            return lineas;
        }

        /// <summary>Margen horizontal que el texto de los controles deja a cada lado (un sexto del alto de la fuente).</summary>
        private static int Margen(Font fuente)
        {
            return (int)Math.Ceiling(AltoLinea(fuente) / 6f);
        }

        /// <summary>Alto de un renglón con el motor de texto de los controles (GDI): es el que usa TextRenderer.</summary>
        private static int AltoLinea(Font fuente)
        {
            return TextRenderer.MeasureText("Ag", fuente, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.SingleLine).Height;
        }

        private static Rectangle Zona(Control c)
        {
            // Bounds incluye el borde del control (PointToScreen(0,0) daría el interior, sin el borde).
            Point origen = c.Parent != null ? c.Parent.PointToScreen(c.Location) : c.PointToScreen(Point.Empty);
            return new Rectangle(formulario.PointToClient(origen), c.Size);
        }

        private static Color FondoReal(Control c)
        {
            while (c != null && c.BackColor.A == 0) c = c.Parent;
            return c != null ? c.BackColor : SystemColors.Control;
        }

        // ------------------------------------------------------------------ controles
        private static object Nodo(Control c)
        {
            Rectangle r = Zona(c);
            List<object> hijos = new List<object>();
            string nombre = string.IsNullOrEmpty(c.Name) ? c.GetType().Name : c.Name;
            // Windows Forms recorta todo lo que un control dibuja a sus propios límites.
            bool recortar = true;

            if (c is VistaMundo3D)
            {
                hijos.Add(Recorte("Vista 3D (OpenGL)", r));
            }
            else if (c is TarjetaMenu)
            {
                Tarjeta((TarjetaMenu)c, r, hijos);
            }
            else if (c is Label)
            {
                Label l = (Label)c;
                if (l.BackColor.A > 0 && l.BackColor != FondoReal(l.Parent)) hijos.Add(Rect("Fondo", r.X, r.Y, r.Width, r.Height, l.BackColor));
                int m = Margen(l.Font);
                RectangleF zona = new RectangleF(r.X + m, r.Y, r.Width - 2 * m, r.Height);
                hijos.Add(Texto("Texto", l.Text, l.Font, l.Enabled ? l.ForeColor : SystemColors.GrayText, zona, l.TextAlign, !l.AutoSize));
            }
            else if (c is Button)
            {
                Boton((Button)c, r, hijos);
            }
            else if (c is CheckBox || c is RadioButton)
            {
                ButtonBase b = (ButtonBase)c;
                int glifo = (int)Math.Round(13 * c.DeviceDpi / 96f);
                hijos.Add(Recorte(c is CheckBox ? "Casilla" : "Opción", new Rectangle(r.X, r.Y, glifo + 2, r.Height)));
                RectangleF zona = new RectangleF(r.X + glifo + 2 + Margen(b.Font), r.Y, r.Width - glifo - 2 - 2 * Margen(b.Font), r.Height);
                hijos.Add(Texto("Texto", b.Text, b.Font, b.Enabled ? b.ForeColor : SystemColors.GrayText, zona, ContentAlignment.MiddleLeft, true));
            }
            else if (c is TextBox)
            {
                CajaTexto((TextBox)c, r, hijos);
            }
            else if (c is NumericUpDown)
            {
                Dictionary<string, object> caja = Rect("Caja", r.X, r.Y, r.Width, r.Height, c.BackColor);
                hijos.Add(Borde(caja, Pixel(r.X, r.Y + r.Height / 2), 1));
                foreach (Control hijo in EnOrden(c)) hijos.Add(Nodo(hijo));
            }
            else if (c is DataGridView)
            {
                Tabla((DataGridView)c, r, hijos);
            }
            else if (c is ToolStrip)
            {
                Barra((ToolStrip)c, r, hijos);
            }
            else if (c is GroupBox)
            {
                Caja((GroupBox)c, r, hijos);
            }
            else if (c is Panel)
            {
                Panel p = (Panel)c;
                if (c.Name == "pnlLogo" && formulario is FrmAcceso) Cabecera(p, r, hijos, 30, 24, 60, 24f, 98, 14, 9.75f, 103, 62, false);
                else if (c.Name == "pnlEncabezado" && formulario is FrmMenuPrincipal) Cabecera(p, r, hijos, 26, 30, 72, 28f, 104, 18, 10.5f, 110, 76, true);
                else
                {
                    if (p.BackColor.A > 0) hijos.Add(Rect("Fondo", r.X, r.Y, r.Width, r.Height, p.BackColor));
                    if (TienePintadoPropio(p)) hijos.Add(Recorte("Dibujo", r));
                    if (p.BorderStyle != BorderStyle.None) hijos.Add(Borde(Rect("Borde", r.X, r.Y, r.Width, r.Height, Color.Transparent), Pixel(r.X, r.Y + r.Height / 2), 1));
                }
                foreach (Control hijo in EnOrden(c)) hijos.Add(Nodo(hijo));
            }
            else
            {
                // Cualquier otro control (deslizadores, flechas de los campos numéricos...): sus píxeles reales.
                hijos.Add(Recorte(c.GetType().Name, r));
            }
            return Grupo(nombre, r.X, r.Y, r.Width, r.Height, hijos, recortar);
        }

        private static bool TienePintadoPropio(Control c)
        {
            // Los paneles con evento Paint (por ejemplo la vista previa de la skin) se guardan como imagen.
            PropertyInfo eventos = typeof(System.ComponentModel.Component).GetProperty("Events", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo clave = typeof(Control).GetField("s_paintEvent", BindingFlags.NonPublic | BindingFlags.Static);
            if (eventos == null || clave == null) return true;
            System.ComponentModel.EventHandlerList lista = (System.ComponentModel.EventHandlerList)eventos.GetValue(c);
            return lista[clave.GetValue(null)] != null;
        }

        private static void Boton(Button b, Rectangle r, List<object> hijos)
        {
            if (b.FlatStyle != FlatStyle.Flat)
            {
                hijos.Add(Recorte("Botón", r));
                return;
            }
            Dictionary<string, object> fondo = Rect("Fondo", r.X, r.Y, r.Width, r.Height, b.BackColor);
            int grosor = b.FlatAppearance.BorderSize;
            // El botón predeterminado del formulario (Enter) se dibuja con el borde un píxel más grueso.
            if (grosor > 0 && ReferenceEquals(formulario.AcceptButton, b) && b.Enabled) grosor++;
            Color borde = b.FlatAppearance.BorderColor.IsEmpty ? b.ForeColor : b.FlatAppearance.BorderColor;
            if (grosor > 0) Borde(fondo, borde, grosor);
            hijos.Add(fondo);

            Rectangle interior = Rectangle.FromLTRB(r.Left + b.Padding.Left, r.Top + b.Padding.Top, r.Right - b.Padding.Right, r.Bottom - b.Padding.Bottom);
            if (b.Image != null)
            {
                int margen = 4 + b.FlatAppearance.BorderSize;
                int x = interior.Left + margen, y = interior.Top + (interior.Height - b.Image.Height) / 2;
                using (Bitmap copia = new Bitmap(b.Image)) hijos.Add(Imagen("Imagen", x, y, b.Image.Width, b.Image.Height, copia));
            }
            int m = Margen(b.Font) + Math.Max(grosor, b.FlatAppearance.BorderSize) + 1;
            RectangleF zona = new RectangleF(interior.X + m, interior.Y, interior.Width - 2 * m, interior.Height);
            hijos.Add(Texto("Texto", b.Text.Replace("&", ""), b.Font, b.Enabled ? b.ForeColor : SystemColors.GrayText, zona, b.TextAlign, true));
        }

        private static void CajaTexto(TextBox t, Rectangle r, List<object> hijos)
        {
            Dictionary<string, object> caja = Rect("Caja", r.X, r.Y, r.Width, r.Height, t.BackColor);
            if (t.BorderStyle != BorderStyle.None)
            {
                Borde(caja, Pixel(r.X, r.Y + r.Height / 2), 1);
                Color abajo = Pixel(r.X + r.Width / 2, r.Bottom - 1);
                hijos.Add(caja);
                if (abajo != Pixel(r.X, r.Y + r.Height / 2)) hijos.Add(Rect("Línea inferior", r.X + 1, r.Bottom - 1, r.Width - 2, 1, abajo));
            }
            else hijos.Add(caja);
            string texto = t.Text;
            if (texto.Length == 0) return;
            if (t.UseSystemPasswordChar || t.PasswordChar != '\0') texto = new string('●', texto.Length);
            int borde = t.BorderStyle == BorderStyle.Fixed3D ? 2 : t.BorderStyle == BorderStyle.FixedSingle ? 1 : 0;
            ContentAlignment alineacion = t.TextAlign == HorizontalAlignment.Right ? ContentAlignment.TopRight
                : t.TextAlign == HorizontalAlignment.Center ? ContentAlignment.TopCenter : ContentAlignment.TopLeft;
            RectangleF zona = new RectangleF(r.X + borde + 2, r.Y + borde + 1, r.Width - 2 * borde - 4, r.Height - 2 * borde);
            hijos.Add(Texto("Texto", texto, t.Font, t.Enabled ? t.ForeColor : SystemColors.GrayText, zona, alineacion, false));
        }

        private static void Caja(GroupBox caja, Rectangle r, List<object> hijos)
        {
            int mitad = AltoLinea(caja.Font) / 2;
            Color linea = Pixel(r.X, r.Y + r.Height / 2);
            hijos.Add(Borde(Rect("Marco", r.X, r.Y + mitad, r.Width, r.Height - mitad, Color.Transparent), linea, 1));
            if (caja.Text.Length > 0)
            {
                Size medida = TextRenderer.MeasureText(caja.Text, caja.Font, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
                hijos.Add(Rect("Fondo del título", r.X + 7, r.Y, medida.Width + 4, AltoLinea(caja.Font), FondoReal(caja)));
                hijos.Add(Texto("Título", caja.Text, caja.Font, caja.ForeColor, new RectangleF(r.X + 9, r.Y, medida.Width + 20, AltoLinea(caja.Font)), ContentAlignment.TopLeft, false));
            }
            foreach (Control hijo in EnOrden(caja)) hijos.Add(Nodo(hijo));
        }

        private static void Tabla(DataGridView t, Rectangle r, List<object> hijos)
        {
            hijos.Add(Rect("Fondo", r.X, r.Y, r.Width, r.Height, t.BackgroundColor));
            int borde = t.BorderStyle == BorderStyle.None ? 0 : 1;
            for (int fila = -1; fila < t.Rows.Count; fila++)
            {
                if (fila >= 0 && !t.Rows[fila].Displayed) continue;
                List<object> celdas = new List<object>();
                Rectangle zonaFila = Rectangle.Empty;
                foreach (DataGridViewColumn columna in t.Columns)
                {
                    if (!columna.Visible) continue;
                    Rectangle z = t.GetCellDisplayRectangle(columna.Index, fila, false);
                    if (z.Width <= 0 || z.Height <= 0) continue;
                    z.Offset(r.X, r.Y);
                    zonaFila = zonaFila.IsEmpty ? z : Rectangle.Union(zonaFila, z);
                    DataGridViewCell celda = fila < 0 ? (DataGridViewCell)columna.HeaderCell : t.Rows[fila].Cells[columna.Index];
                    DataGridViewCellStyle estilo = celda.InheritedStyle;
                    bool seleccionada = fila >= 0 && celda.Selected;
                    // El color real de la celda se lee de la captura (respeta filas alternas, selección y CellFormatting).
                    Color fondo = Pixel(z.Right - 3, z.Y + 3);
                    celdas.Add(Rect("Celda", z.X, z.Y, z.Width, z.Height, fondo));
                    string texto = Convert.ToString(celda.FormattedValue);
                    ContentAlignment alineacion = ContentAlignment.MiddleLeft;
                    string a = estilo.Alignment.ToString();
                    if (a.EndsWith("Right")) alineacion = ContentAlignment.MiddleRight;
                    else if (a.EndsWith("Center")) alineacion = ContentAlignment.MiddleCenter;
                    int m = Margen(estilo.Font);
                    RectangleF zona = new RectangleF(z.X + m, z.Y + 1, z.Width - 2 * m - 1, z.Height - 2);
                    Color colorTexto = seleccionada ? estilo.SelectionForeColor : estilo.ForeColor;
                    if (colorTexto.A == 0) colorTexto = t.DefaultCellStyle.ForeColor.A > 0 ? t.DefaultCellStyle.ForeColor : t.ForeColor;
                    celdas.Add(Texto("Texto", texto, estilo.Font, colorTexto, zona, alineacion, false));
                }
                if (zonaFila.IsEmpty) continue;
                if (fila >= 0 && t.CellBorderStyle != DataGridViewCellBorderStyle.None)
                {
                    celdas.Add(Rect("Línea", zonaFila.X, zonaFila.Bottom - 1, zonaFila.Width, 1, t.GridColor));
                }
                hijos.Add(Grupo(fila < 0 ? "Encabezado" : "Fila " + (fila + 1), zonaFila.X, zonaFila.Y, zonaFila.Width, zonaFila.Height, celdas));
            }
            if (borde > 0) hijos.Add(Borde(Rect("Borde", r.X, r.Y, r.Width, r.Height, Color.Transparent), Pixel(r.X, r.Y + r.Height / 2), 1));
        }

        private static void Barra(ToolStrip barra, Rectangle r, List<object> hijos)
        {
            hijos.Add(Rect("Fondo", r.X, r.Y, r.Width, r.Height, Pixel(r.Right - 4, r.Y + r.Height / 2)));
            foreach (ToolStripItem item in barra.Items)
            {
                if (!item.Visible || item.Bounds.Width <= 0) continue;
                Rectangle z = item.Bounds;
                z.Offset(r.X, r.Y);
                if (item is ToolStripSeparator || string.IsNullOrEmpty(item.Text))
                {
                    hijos.Add(Recorte(item.Name, z));
                    continue;
                }
                List<object> partes = new List<object>();
                ToolStripStatusLabel etiqueta = item as ToolStripStatusLabel;
                if (etiqueta != null && etiqueta.BorderSides != ToolStripStatusLabelBorderSides.None)
                {
                    if ((etiqueta.BorderSides & ToolStripStatusLabelBorderSides.Left) != 0) partes.Add(Rect("Separador", z.X, z.Y, 1, z.Height, Pixel(z.X, z.Y + z.Height / 2)));
                    if ((etiqueta.BorderSides & ToolStripStatusLabelBorderSides.Right) != 0) partes.Add(Rect("Separador", z.Right - 1, z.Y, 1, z.Height, Pixel(z.Right - 1, z.Y + z.Height / 2)));
                }
                Font fuente = item.Font;
                ContentAlignment alineacion = item is ToolStripMenuItem ? ContentAlignment.MiddleCenter : item.TextAlign;
                int m = Margen(fuente);
                partes.Add(Texto("Texto", item.Text.Replace("&", ""), fuente, item.Enabled ? item.ForeColor : SystemColors.GrayText,
                                 new RectangleF(z.X + m, z.Y, z.Width - 2 * m, z.Height), alineacion, false));
                hijos.Add(Grupo(item.Name, z.X, z.Y, z.Width, z.Height, partes));
            }
        }

        // ------------------------------------------------------------------ lo que BlockForge dibuja a mano
        /// <summary>
        /// Cabecera con el paisaje de bloques, el logotipo y el título. Las medidas son las mismas que usan
        /// pnlLogo_Paint (FrmAcceso) y pnlEncabezado_Paint (FrmMenuPrincipal).
        /// </summary>
        private static void Cabecera(Panel p, Rectangle r, List<object> hijos, int xLogo, int yLogo, int ladoLogo, float puntosTitulo,
                                     int xTitulo, int yTitulo, float puntosSub, int xSub, int ySub, bool zonaUsuario)
        {
            float k = Dibujo.Escala(p);
            using (Bitmap paisaje = new Bitmap(r.Width, r.Height, PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(paisaje)) Dibujo.Paisaje(g, new Rectangle(0, 0, r.Width, r.Height), k);
                hijos.Add(Imagen("Paisaje de bloques", r.X, r.Y, r.Width, r.Height, paisaje));
            }
            if (zonaUsuario)
            {
                Rectangle zona = Rectangle.Empty;
                foreach (string nombre in new[] { "lblUsuario", "btnCerrarSesion", "btnSkin", "btnCuenta" })
                {
                    Control c = p.Controls[nombre];
                    if (c != null) zona = zona.IsEmpty ? c.Bounds : Rectangle.Union(zona, c.Bounds);
                }
                zona.Inflate((int)(8 * k), (int)(6 * k));
                hijos.Add(Rect("Zona del usuario", r.X + zona.X, r.Y + zona.Y, zona.Width, zona.Height, Color.FromArgb(120, 20, 22, 26)));
            }
            int lado = (int)(ladoLogo * k);
            using (Bitmap logo = new Bitmap(lado, lado, PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(logo)) Dibujo.Logotipo(g, new Rectangle(0, 0, lado, lado));
                hijos.Add(Imagen("Logotipo", r.X + (int)(xLogo * k), r.Y + (int)(yLogo * k), lado, lado, logo));
            }
            using (Font titulo = new Font("Segoe UI", puntosTitulo, FontStyle.Bold))
            using (Font subtitulo = new Font("Segoe UI Semibold", puntosSub))
            using (Graphics g = p.CreateGraphics())
            {
                hijos.Add(TextoGdiPlus(g, "Título", "BlockForge", titulo, Color.White, r.X + xTitulo * k, r.Y + yTitulo * k));
                hijos.Add(TextoGdiPlus(g, "Subtítulo", "Construye, explora y juega con tus amigos", subtitulo, Color.FromArgb(235, 240, 250), r.X + xSub * k, r.Y + ySub * k));
            }
        }

        /// <summary>Texto dibujado con Graphics.DrawString y sombra (Dibujo.TextoConSombra).</summary>
        private static Dictionary<string, object> TextoGdiPlus(Graphics g, string nombre, string texto, Font fuente, Color color, float x, float y)
        {
            SizeF medida = g.MeasureString(texto, fuente);
            float px = fuente.SizeInPoints * formulario.DeviceDpi / 72f;
            // DrawString deja a la izquierda un sexto del tamaño de la fuente antes del primer carácter.
            Dictionary<string, object> n = Texto(nombre, texto, fuente, color, new RectangleF(x + px / 6f, y, medida.Width, medida.Height), ContentAlignment.TopLeft, false, fuente.GetHeight(g));
            n["sombra"] = new Dictionary<string, object> { { "dx", 2 }, { "dy", 2 }, { "color", "#000000" }, { "opacidad", 150 / 255f } };
            return n;
        }

        /// <summary>Tarjeta del menú principal: mismas medidas y colores que TarjetaMenu.OnPaint.</summary>
        private static void Tarjeta(TarjetaMenu t, Rectangle r, List<object> hijos)
        {
            float k = t.DeviceDpi / 96f;
            Dictionary<string, object> fondo = Rect("Fondo", r.X, r.Y, r.Width, r.Height, t.Enabled ? Color.FromArgb(43, 45, 50) : Color.FromArgb(40, 42, 46));
            fondo["radio"] = 8;
            hijos.Add(Borde(fondo, Color.FromArgb(62, 65, 72), 1));
            Dictionary<string, object> franja = Rect("Franja", r.X, r.Y, 11, r.Height, t.Enabled ? t.Acento : Color.Gray);
            franja["radio"] = 8;
            hijos.Add(Grupo("Franja de color", r.X, r.Y, 5, r.Height, new List<object> { franja }, true));

            int icono = Math.Min((int)(48 * k), t.Height - (int)(24 * k));
            int margen = (int)(20 * k);
            using (Bitmap bmp = new Bitmap(icono, icono, PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(bmp)) Dibujo.IconoBloque(g, new Rectangle(0, 0, icono, icono), t.Bloque);
                hijos.Add(Imagen("Ícono " + t.Bloque, r.X + margen, r.Y + (t.Height - icono) / 2, icono, icono, bmp));
            }
            int xTexto = margen + icono + (int)(16 * k);
            using (Font titulo = new Font("Segoe UI Semibold", 12F))
            {
                int altoTitulo = TextRenderer.MeasureText("Ag", titulo).Height;
                int yTitulo = (int)(10 * k);
                int ancho = t.Width - xTexto - (int)(12 * k);
                int m = Margen(titulo);
                hijos.Add(Texto("Título", t.Text, titulo, t.Enabled ? Color.White : Color.FromArgb(130, 130, 130),
                                new RectangleF(r.X + xTexto + m, r.Y + yTitulo, ancho - 2 * m, altoTitulo), ContentAlignment.TopLeft, false, altoTitulo));
                int yDescripcion = yTitulo + altoTitulo;
                m = Margen(t.Font);
                hijos.Add(Texto("Descripción", t.Descripcion, t.Font, Color.FromArgb(178, 182, 190),
                                new RectangleF(r.X + xTexto + m, r.Y + yDescripcion, ancho - 2 * m, t.Height - yDescripcion - (int)(4 * k)), ContentAlignment.TopLeft, true));
            }
        }
    }
}
