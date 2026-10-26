using System;

namespace BlockForge.Models
{
    /// <summary>Registro de un guardado de un mundo.</summary>
    public class Partida
    {
        public int IdPartida { get; set; }
        public int IdMundo { get; set; }
        public DateTime FechaGuardado { get; set; }
        public int TotalBloques { get; set; }
    }

    /// <summary>Fila que se muestra en las tablas de mundos y de gestión de partidas.</summary>
    public class ResumenMundo
    {
        public int IdMundo { get; set; }
        public string Nombre { get; set; }
        public string Usuario { get; set; }
        public int Ancho { get; set; }
        public int Alto { get; set; }
        public int Largo { get; set; }
        public int TotalBloques { get; set; }
        public DateTime? UltimoGuardado { get; set; }

        public string Dimensiones
        {
            get { return Ancho + " × " + Largo + " × " + Alto; }
        }

        public string UltimoGuardadoTexto
        {
            get { return UltimoGuardado.HasValue ? UltimoGuardado.Value.ToString("dd/MM/yyyy HH:mm") : "Sin guardar"; }
        }
    }
}
