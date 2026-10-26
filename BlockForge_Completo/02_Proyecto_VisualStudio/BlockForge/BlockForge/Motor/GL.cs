using System;
using System.Runtime.InteropServices;

namespace BlockForge.Motor
{
    /// <summary>
    /// Enlace mínimo con OpenGL 1.1 (opengl32.dll), WGL y GDI. Es la misma ruta de dibujo
    /// "fixed function" que usa el backend GL1 de ClassiCube, y no requiere paquetes NuGet:
    /// opengl32.dll viene con Windows y el controlador de video aporta la implementación.
    /// </summary>
    internal static class GL
    {
        private const string Lib = "opengl32.dll";

        public const int COLOR_BUFFER_BIT = 0x4000, DEPTH_BUFFER_BIT = 0x0100;
        public const int POINTS = 0x0000, LINES = 0x0001, TRIANGLES = 0x0004, QUADS = 0x0007;
        public const int DEPTH_TEST = 0x0B71, CULL_FACE = 0x0B44, TEXTURE_2D = 0x0DE1, BLEND = 0x0BE2;
        public const int ALPHA_TEST = 0x0BC0, FOG = 0x0B60, LINE_SMOOTH = 0x0B20, POLYGON_OFFSET_FILL = 0x8037;
        public const int ZERO = 0, ONE = 1, SRC_ALPHA = 0x0302, ONE_MINUS_SRC_ALPHA = 0x0303, ONE_MINUS_DST_COLOR = 0x0307;
        public const int LEQUAL = 0x0203, GREATER = 0x0204;
        public const int BACK = 0x0405;
        public const int PROJECTION = 0x1701, MODELVIEW = 0x1700;
        public const int TEXTURE_MAG_FILTER = 0x2800, TEXTURE_MIN_FILTER = 0x2801, TEXTURE_WRAP_S = 0x2802, TEXTURE_WRAP_T = 0x2803;
        public const int NEAREST = 0x2600, LINEAR = 0x2601, REPEAT = 0x2901, CLAMP = 0x2900;
        public const int RGBA = 0x1908, BGRA_EXT = 0x80E1, UNSIGNED_BYTE = 0x1401, FLOAT = 0x1406;
        public const int VERTEX_ARRAY = 0x8074, COLOR_ARRAY = 0x8076, TEXTURE_COORD_ARRAY = 0x8078;
        public const int COMPILE = 0x1300;
        public const int FOG_MODE = 0x0B65, FOG_START = 0x0B63, FOG_END = 0x0B64, FOG_COLOR = 0x0B66, FOG_DENSITY = 0x0B62, EXP = 0x0800;
        public const int UNPACK_ALIGNMENT = 0x0CF5;
        public const int VENDOR = 0x1F00, RENDERER = 0x1F01, VERSION = 0x1F02;

        [DllImport(Lib)] public static extern void glClear(int mascara);
        [DllImport(Lib)] public static extern void glClearColor(float r, float g, float b, float a);
        [DllImport(Lib)] public static extern void glEnable(int capacidad);
        [DllImport(Lib)] public static extern void glDisable(int capacidad);
        [DllImport(Lib)] public static extern void glViewport(int x, int y, int ancho, int alto);
        [DllImport(Lib)] public static extern void glMatrixMode(int modo);
        [DllImport(Lib)] public static extern void glLoadIdentity();
        [DllImport(Lib)] public static extern void glLoadMatrixf(float[] m);
        [DllImport(Lib)] public static extern void glOrtho(double izq, double der, double abajo, double arriba, double cerca, double lejos);
        [DllImport(Lib)] public static extern void glPushMatrix();
        [DllImport(Lib)] public static extern void glPopMatrix();
        [DllImport(Lib)] public static extern void glTranslatef(float x, float y, float z);
        [DllImport(Lib)] public static extern void glRotatef(float angulo, float x, float y, float z);
        [DllImport(Lib)] public static extern void glScalef(float x, float y, float z);
        [DllImport(Lib)] public static extern void glBlendFunc(int origen, int destino);
        [DllImport(Lib)] public static extern void glAlphaFunc(int funcion, float referencia);
        [DllImport(Lib)] public static extern void glDepthMask(byte activo);
        [DllImport(Lib)] public static extern void glDepthFunc(int funcion);
        [DllImport(Lib)] public static extern void glColorMask(byte r, byte g, byte b, byte a);
        [DllImport(Lib)] public static extern void glCullFace(int modo);
        [DllImport(Lib)] public static extern void glPolygonOffset(float factor, float unidades);
        [DllImport(Lib)] public static extern void glLineWidth(float ancho);
        [DllImport(Lib)] public static extern void glPointSize(float tamano);
        [DllImport(Lib)] public static extern void glHint(int objetivo, int modo);
        [DllImport(Lib)] public static extern void glPixelStorei(int nombre, int valor);

        [DllImport(Lib)] public static extern void glGenTextures(int n, uint[] texturas);
        [DllImport(Lib)] public static extern void glDeleteTextures(int n, uint[] texturas);
        [DllImport(Lib)] public static extern void glBindTexture(int objetivo, uint textura);
        [DllImport(Lib)] public static extern void glTexParameteri(int objetivo, int nombre, int valor);
        [DllImport(Lib)] public static extern void glTexImage2D(int objetivo, int nivel, int formatoInterno, int ancho, int alto,
                                                               int borde, int formato, int tipo, IntPtr pixeles);

        [DllImport(Lib)] public static extern uint glGenLists(int rango);
        [DllImport(Lib)] public static extern void glDeleteLists(uint lista, int rango);
        [DllImport(Lib)] public static extern void glNewList(uint lista, int modo);
        [DllImport(Lib)] public static extern void glEndList();
        [DllImport(Lib)] public static extern void glCallList(uint lista);

        [DllImport(Lib)] public static extern void glBegin(int modo);
        [DllImport(Lib)] public static extern void glEnd();
        [DllImport(Lib)] public static extern void glVertex2f(float x, float y);
        [DllImport(Lib)] public static extern void glVertex3f(float x, float y, float z);
        [DllImport(Lib)] public static extern void glTexCoord2f(float u, float v);
        [DllImport(Lib)] public static extern void glColor4f(float r, float g, float b, float a);
        [DllImport(Lib)] public static extern void glColor3f(float r, float g, float b);

        [DllImport(Lib)] public static extern void glEnableClientState(int arreglo);
        [DllImport(Lib)] public static extern void glDisableClientState(int arreglo);
        [DllImport(Lib)] public static extern void glVertexPointer(int tamano, int tipo, int paso, IntPtr datos);
        [DllImport(Lib)] public static extern void glTexCoordPointer(int tamano, int tipo, int paso, IntPtr datos);
        [DllImport(Lib)] public static extern void glColorPointer(int tamano, int tipo, int paso, IntPtr datos);
        [DllImport(Lib)] public static extern void glDrawArrays(int modo, int primero, int cantidad);

        [DllImport(Lib)] public static extern void glFogi(int nombre, int valor);
        [DllImport(Lib)] public static extern void glFogf(int nombre, float valor);
        [DllImport(Lib)] public static extern void glFogfv(int nombre, float[] valores);
        [DllImport(Lib)] public static extern IntPtr glGetString(int nombre);

        [DllImport(Lib)] public static extern IntPtr wglCreateContext(IntPtr hdc);
        [DllImport(Lib)] public static extern bool wglMakeCurrent(IntPtr hdc, IntPtr contexto);
        [DllImport(Lib)] public static extern bool wglDeleteContext(IntPtr contexto);
        [DllImport(Lib, CharSet = CharSet.Ansi)] public static extern IntPtr wglGetProcAddress(string nombre);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate bool SwapIntervalEXT(int intervalo);

        public static string Texto(int nombre)
        {
            IntPtr p = glGetString(nombre);
            return p == IntPtr.Zero ? "?" : Marshal.PtrToStringAnsi(p);
        }
    }

    /// <summary>Funciones de Windows necesarias para crear el contexto OpenGL.</summary>
    internal static class Win32
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct PIXELFORMATDESCRIPTOR
        {
            public ushort nSize, nVersion;
            public uint dwFlags;
            public byte iPixelType, cColorBits, cRedBits, cRedShift, cGreenBits, cGreenShift, cBlueBits, cBlueShift;
            public byte cAlphaBits, cAlphaShift, cAccumBits, cAccumRedBits, cAccumGreenBits, cAccumBlueBits, cAccumAlphaBits;
            public byte cDepthBits, cStencilBits, cAuxBuffers, iLayerType, bReserved;
            public uint dwLayerMask, dwVisibleMask, dwDamageMask;
        }

        public const uint PFD_DRAW_TO_WINDOW = 0x04, PFD_SUPPORT_OPENGL = 0x20, PFD_DOUBLEBUFFER = 0x01;
        public const int CS_OWNDC = 0x20;

        [DllImport("gdi32.dll")] public static extern int ChoosePixelFormat(IntPtr hdc, ref PIXELFORMATDESCRIPTOR pfd);
        [DllImport("gdi32.dll")] public static extern bool SetPixelFormat(IntPtr hdc, int formato, ref PIXELFORMATDESCRIPTOR pfd);
        [DllImport("gdi32.dll")] public static extern bool SwapBuffers(IntPtr hdc);
        [DllImport("user32.dll")] public static extern IntPtr GetDC(IntPtr hwnd);
        [DllImport("user32.dll")] public static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);
        [DllImport("user32.dll")] public static extern short GetAsyncKeyState(int tecla);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)] public static extern IntPtr LoadLibrary(string archivo);

        [StructLayout(LayoutKind.Sequential)]
        public struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam, lParam;
            public uint time;
            public int x, y;
        }

        [DllImport("user32.dll")] public static extern bool PeekMessage(out MSG msg, IntPtr hwnd, uint min, uint max, uint quitar);

        // Raw Input: movimiento del mouse sin aceleración ni recentrado (como en los juegos).
        [StructLayout(LayoutKind.Sequential)]
        public struct RAWINPUTDEVICE
        {
            public ushort usUsagePage, usUsage;
            public uint dwFlags;
            public IntPtr hwndTarget;
        }

        public const int WM_INPUT = 0x00FF;
        public const uint RID_INPUT = 0x10000003;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RegisterRawInputDevices(RAWINPUTDEVICE[] dispositivos, uint cantidad, uint tamano);
        [DllImport("user32.dll")]
        public static extern uint GetRawInputData(IntPtr hRawInput, uint comando, IntPtr datos, ref uint tamano, uint tamanoCabecera);

        /// <summary>Lee el desplazamiento relativo de un WM_INPUT de mouse. Devuelve false si no es un movimiento relativo.</summary>
        public static bool LeerMovimientoRaw(IntPtr lParam, out int dx, out int dy)
        {
            dx = dy = 0;
            int cabecera = 8 + 2 * IntPtr.Size;
            uint tamano = 0;
            GetRawInputData(lParam, RID_INPUT, IntPtr.Zero, ref tamano, (uint)cabecera);
            if (tamano == 0 || tamano > 512) return false;
            IntPtr buffer = Marshal.AllocHGlobal((int)tamano);
            try
            {
                if (GetRawInputData(lParam, RID_INPUT, buffer, ref tamano, (uint)cabecera) != tamano) return false;
                if (Marshal.ReadInt32(buffer, 0) != 0) return false; // RIM_TYPEMOUSE
                ushort banderas = (ushort)Marshal.ReadInt16(buffer, cabecera);
                if ((banderas & 1) != 0) return false; // MOUSE_MOVE_ABSOLUTE (escritorio remoto, tabletas)
                dx = Marshal.ReadInt32(buffer, cabecera + 12);
                dy = Marshal.ReadInt32(buffer, cabecera + 16);
                return true;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        public static bool TeclaPresionada(System.Windows.Forms.Keys tecla)
        {
            return (GetAsyncKeyState((int)tecla) & 0x8000) != 0;
        }
    }
}
