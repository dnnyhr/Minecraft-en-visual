using System.IO;
using System.Text;

namespace BlockForge.Red
{
    /// <summary>
    /// Mensajes del protocolo multijugador. Cada mensaje viaja por TCP como
    /// [longitud int32][tipo byte][datos]. El anfitrión es la autoridad: guarda el mundo
    /// y reenvía a todos los cambios de bloques, las posiciones y el chat.
    /// </summary>
    internal enum TipoMensaje : byte
    {
        /// <summary>Interno: la conexión se cerró.</summary>
        Desconexion = 0,
        /// <summary>Cliente → anfitrión: versión y nombre del jugador.</summary>
        Hola = 1,
        /// <summary>Anfitrión → cliente: id asignado y el mundo completo comprimido.</summary>
        Bienvenida = 2,
        /// <summary>Ambos: un bloque cambió (x, y, z, id).</summary>
        Bloque = 3,
        /// <summary>Ambos: posición y orientación de un jugador.</summary>
        Posicion = 4,
        /// <summary>Anfitrión → clientes: un jugador entró.</summary>
        Entra = 5,
        /// <summary>Anfitrión → clientes: un jugador salió.</summary>
        Sale = 6,
        /// <summary>Ambos: mensaje de chat.</summary>
        Chat = 7,
        /// <summary>Anfitrión → cliente: no se aceptó la conexión.</summary>
        Rechazo = 8,
        /// <summary>Anfitrión → clientes: hora del día y estado de todos los mobs (10 veces por segundo).</summary>
        Mobs = 9,
        /// <summary>Cliente → anfitrión: el jugador golpeó un mob (id, daño).</summary>
        AtacarMob = 10,
        /// <summary>Anfitrión → cliente: el jugador recibe daño (cantidad, origen).</summary>
        Dano = 11,
        /// <summary>Anfitrión → clientes: explotó un creeper (centro, radio), solo para efectos.</summary>
        Explosion = 12,
        /// <summary>Anfitrión → cliente: objetos que soltó un mob que mató el jugador.</summary>
        Recompensa = 13,
        /// <summary>Ambos: un jugador dio un golpe (para animar su brazo en los demás equipos).</summary>
        Golpe = 14,
        /// <summary>Cliente → anfitrión: el jugador golpeó a otro jugador (PvP): id del objetivo y daño.</summary>
        AtacarJugador = 15
    }

    internal class Mensaje
    {
        public TipoMensaje Tipo;
        public byte[] Datos;
        public Conexion Origen;

        public BinaryReader Lector()
        {
            return new BinaryReader(new MemoryStream(Datos ?? new byte[0]), Encoding.UTF8);
        }
    }

    internal static class Protocolo
    {
        public const int Version = 4;
        public const int PuertoPredeterminado = 25570;
        public const int PuertoDescubrimiento = 25571;
        public const int TamanoMaximo = 64 * 1024 * 1024;

        /// <summary>Arma los datos de un mensaje con un BinaryWriter.</summary>
        public static byte[] Armar(System.Action<BinaryWriter> escribir)
        {
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter w = new BinaryWriter(m, Encoding.UTF8))
                {
                    escribir(w);
                }
                return m.ToArray();
            }
        }
    }
}
