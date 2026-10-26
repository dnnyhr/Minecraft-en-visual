using System;

namespace BlockForge.Models
{
    /// <summary>Opciones generales de la aplicación.</summary>
    public class Configuracion
    {
        public const int DistanciaMinima = 32, DistanciaMaxima = 256;
        public const int CampoMinimo = 50, CampoMaximo = 110;
        public const int SensibilidadMinima = 1, SensibilidadMaxima = 10;

        public int IdConfiguracion { get; set; }
        /// <summary>Distancia de visión en bloques (donde termina la niebla).</summary>
        public int DistanciaVision { get; set; }
        /// <summary>Campo de visión vertical en grados.</summary>
        public int CampoVision { get; set; }
        /// <summary>Sensibilidad del mouse (1 a 10).</summary>
        public int Sensibilidad { get; set; }
        public int AnchoPredeterminado { get; set; }
        public int LargoPredeterminado { get; set; }
        public int AltoPredeterminado { get; set; }
        public DateTime FechaModificacion { get; set; }
        public string UsuarioModifica { get; set; }

        public static Configuracion PorDefecto()
        {
            return new Configuracion
            {
                DistanciaVision = 128,
                CampoVision = 70,
                Sensibilidad = 5,
                AnchoPredeterminado = 128,
                LargoPredeterminado = 128,
                AltoPredeterminado = 64,
                FechaModificacion = DateTime.Now
            };
        }
    }
}
