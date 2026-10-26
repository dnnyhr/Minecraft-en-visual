using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BlockForge.Data;
using BlockForge.Helpers;
using BlockForge.Models;

namespace BlockForge.Services
{
    /// <summary>Error de una regla de negocio que debe mostrarse al usuario.</summary>
    public class ExcepcionValidacion : Exception
    {
        public ExcepcionValidacion(string mensaje) : base(mensaje) { }
    }

    /// <summary>Creación, carga, guardado, restablecimiento y eliminación de mundos.</summary>
    public class MundoService
    {
        private readonly MundoRepository mundos = new MundoRepository();
        private readonly BloqueRepository bloques = new BloqueRepository();
        private readonly InventarioRepository inventarios = new InventarioRepository();
        private readonly PartidaRepository partidas = new PartidaRepository();

        /// <summary>RF-03 y RF-04: valida, genera el terreno y lo guarda en una transacción.</summary>
        public Mundo Crear(string nombre, int ancho, int largo, int alto, bool conLagos)
        {
            string mensaje;
            nombre = (nombre ?? string.Empty).Trim();
            if (!Validaciones.NombreMundoValido(nombre, out mensaje)) throw new ExcepcionValidacion(mensaje);
            if (!Validaciones.DimensionesValidas(ancho, largo, alto))
            {
                throw new ExcepcionValidacion("Las dimensiones están fuera de los rangos permitidos.");
            }
            if (mundos.ExisteNombre(Sesion.UsuarioActual.IdUsuario, nombre))
            {
                throw new ExcepcionValidacion("Ya tiene un mundo con ese nombre.");
            }

            Mundo mundo = new Mundo(ancho, alto, largo)
            {
                IdUsuario = Sesion.UsuarioActual.IdUsuario,
                Nombre = nombre,
                Semilla = new Random().Next(1, int.MaxValue),
                ConLagos = conLagos,
                FechaCreacion = DateTime.Now
            };
            GeneradorTerreno.Generar(mundo);

            using (SqlConnection conexion = Conexion.Abrir())
            using (SqlTransaction transaccion = conexion.BeginTransaction())
            {
                try
                {
                    mundo.IdMundo = mundos.Insertar(conexion, transaccion, mundo);
                    bloques.Reemplazar(conexion, transaccion, mundo);
                    inventarios.Guardar(conexion, transaccion, mundo.IdMundo, new Inventario(), CatalogoBloques.Tipos);
                    partidas.Registrar(conexion, transaccion, mundo.IdMundo, mundo.ContarBloques());
                    transaccion.Commit();
                }
                catch
                {
                    transaccion.Rollback();
                    throw;
                }
            }
            return mundo;
        }

        /// <summary>
        /// RF-05: carga los bloques y el inventario del último guardado. Si el mundo no tiene
        /// bloques (por ejemplo, un mundo 2D migrado de la versión 1), se genera con su semilla.
        /// </summary>
        public Mundo Abrir(int idMundo, out Inventario inventario)
        {
            Mundo mundo = mundos.ObtenerPorId(idMundo);
            if (mundo == null) throw new ExcepcionValidacion("El mundo seleccionado ya no existe.");
            if (!bloques.CargarEnMundo(mundo))
            {
                GeneradorTerreno.Generar(mundo);
                mundo.JugadorX = mundo.JugadorY = mundo.JugadorZ = null;
            }
            // Mundos de versiones anteriores: se les agrega hierro y diamante bajo tierra (siempre igual, según la semilla).
            else if (!GeneradorTerreno.TieneMinerales(mundo)) GeneradorTerreno.AgregarMinerales(mundo);
            inventario = inventarios.Obtener(idMundo);
            return mundo;
        }

        /// <summary>RF-10 y RN-12: guarda todo o nada dentro de una transacción.</summary>
        public void Guardar(Mundo mundo, Inventario inventario)
        {
            using (SqlConnection conexion = Conexion.Abrir())
            using (SqlTransaction transaccion = conexion.BeginTransaction())
            {
                try
                {
                    bloques.Reemplazar(conexion, transaccion, mundo);
                    inventarios.Guardar(conexion, transaccion, mundo.IdMundo, inventario, CatalogoBloques.Tipos);
                    partidas.Registrar(conexion, transaccion, mundo.IdMundo, mundo.ContarBloques());
                    transaccion.Commit();
                }
                catch
                {
                    transaccion.Rollback();
                    throw;
                }
            }
        }

        /// <summary>RF-14: vuelve a generar el terreno con la misma semilla y deja el inventario en cero.</summary>
        public void Restablecer(int idMundo)
        {
            VerificarAdministrador();
            Mundo mundo = mundos.ObtenerPorId(idMundo);
            if (mundo == null) throw new ExcepcionValidacion("La partida seleccionada ya no existe.");
            GeneradorTerreno.Generar(mundo);
            mundo.JugadorX = mundo.JugadorY = mundo.JugadorZ = null;
            Guardar(mundo, new Inventario());
        }

        /// <summary>RF-13: elimina el mundo; sus datos dependientes se borran en cascada.</summary>
        public void Eliminar(int idMundo)
        {
            VerificarAdministrador();
            mundos.Eliminar(idMundo);
        }

        public List<ResumenMundo> ListarMisMundos()
        {
            return mundos.ListarPorUsuario(Sesion.UsuarioActual.IdUsuario);
        }

        public List<ResumenMundo> ListarTodos(string filtro)
        {
            VerificarAdministrador();
            return mundos.ListarTodos(filtro);
        }

        /// <summary>RNF-05: las funciones administrativas también se validan en los servicios.</summary>
        private static void VerificarAdministrador()
        {
            if (!Sesion.EsAdministrador)
            {
                throw new ExcepcionValidacion("Esta acción solo está disponible para el Administrador.");
            }
        }
    }
}
