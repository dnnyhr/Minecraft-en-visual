using System.Windows.Forms;

namespace BlockForge.Helpers
{
    /// <summary>Mensajes del sistema con un formato uniforme (tabla de mensajes del documento).</summary>
    public static class Mensajes
    {
        private const string Titulo = "BlockForge";

        /// <summary>Sin ventanas de mensaje: lo usa la exportación de pantallas a Figma (--exportar-figma).</summary>
        public static bool Silencioso { get; set; }

        public static void Informacion(string texto)
        {
            if (Silencioso) return;
            MessageBox.Show(texto, Titulo, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void Advertencia(string texto)
        {
            if (Silencioso) return;
            MessageBox.Show(texto, Titulo, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void Error(string texto)
        {
            if (Silencioso) return;
            MessageBox.Show(texto, Titulo, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static bool Confirmar(string texto, string titulo)
        {
            if (Silencioso) return false;
            return MessageBox.Show(texto, titulo, MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2) == DialogResult.Yes;
        }

        public static DialogResult PreguntarGuardar(string texto)
        {
            if (Silencioso) return DialogResult.No;
            return MessageBox.Show(texto, Titulo, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
        }
    }
}
