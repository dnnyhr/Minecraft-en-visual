namespace BlockForge.Models
{
    /// <summary>Persona que accede al sistema con un rol (Jugador o Administrador).</summary>
    public class Usuario
    {
        public const string RolJugador = "Jugador";
        public const string RolAdministrador = "Administrador";

        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; }
        public string Rol { get; set; }
        public bool Activo { get; set; }

        /// <summary>Aspecto del personaje (null si usa el predeterminado).</summary>
        public Skin Skin { get; set; }

        public bool EsAdministrador
        {
            get { return Rol == RolAdministrador; }
        }
    }

    /// <summary>Skin con el formato de Minecraft: PNG de 64×64 (o 64×32 en el formato antiguo).</summary>
    public class Skin
    {
        public byte[] Png { get; set; }
        /// <summary>Modelo "Alex": brazos de 3 píxeles de ancho en lugar de 4.</summary>
        public bool Delgada { get; set; }
        /// <summary>De dónde salió: el nombre de la cuenta de Minecraft o "archivo".</summary>
        public string Origen { get; set; }
    }
}
