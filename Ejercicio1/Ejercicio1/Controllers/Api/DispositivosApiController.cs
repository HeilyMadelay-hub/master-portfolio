using Ejercicio1.Data;
using Ejercicio1.DTOs;
using Ejercicio1.Models;
using Ejercicio1.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ejercicio1.Controllers.Api
{

    //el versionado de la api
    [ApiController]
    [Route("api/v1/dispositivos")]
    public class DispositivosApiController : ControllerBase
    {

        private readonly AppDbContext _context;
        //No es un error pero si añades el readonly evitarias que pueda ser asignado en otra parte que no sea el constructor

        private readonly DispositivoService _dispositivoService;

        public DispositivosApiController(AppDbContext context, DispositivoService dispositivoService)
        {
            _context = context;
            _dispositivoService = dispositivoService;
        }

        // GET api/v1/dispositivos
        [HttpGet]
        // Define un método asíncrono que devuelve una ActionResult que contiene una colección de Dispositivo
        // IEnumerable permite flexibilidad porque cualquier colección (List, Array, etc.) puede ser devuelta
        public async Task<ActionResult<IEnumerable<Dispositivo>>> GetDispositivos()
        {
            // Inicia una consulta asíncrona a la base de datos para obtener todos los dispositivos
            var dispositivos = await _context.Dispositivos

                // Include: Carga EAGER (anticipada) de la propiedad de navegación Reservas
                // Trae todas las reservas asociadas a cada dispositivo en la misma consulta SQL
                .Include(d => d.Reservas)

                // ThenInclude: Carga en cascada - después de cargar Reservas, carga también Usuario de cada Reserva
                // Permite acceder a r.Usuario.NombreCompleto sin hacer consultas adicionales
                .ThenInclude(r => r.Usuario)

                // Select: PROYECCIÓN de datos - transforma las entidades de EF a DTOs
                // Esto rompe el ciclo de referencias circulares porque creas objetos nuevos sin navegación inversa
                .Select(d => new DispositivoDto
                {
                    // Mapea la propiedad DispositivoId del dispositivo al DTO
                    DispositivoId = d.DispositivoId,

                    // Mapea la propiedad Nombre del dispositivo al DTO
                    Nombre = d.Nombre,

                    // Transforma la colección de Reservas (entidades) a una colección de ReservaDto
                    Reservas = d.Reservas.Select(r => new ReservaDto
                    {
                        // Mapea el ID de la reserva
                        ReservaId = r.ReservaId,

                        // Mapea el ID del dispositivo asociado
                        DispositivoId = r.DispositivoId,

                        // En lugar de incluir el objeto Dispositivo completo (que causaría ciclo),
                        // solo incluye el nombre del dispositivo como string
                        DispositivoNombre = d.Nombre,

                        // Mapea el ID del usuario que hizo la reserva
                        UsuarioId = r.UsuarioId,

                        // En lugar de incluir el objeto Usuario completo (que causaría ciclo),
                        // solo incluye el nombre del usuario como string
                        // Accede a r.Usuario gracias al ThenInclude anterior
                        UsuarioNombre = r.Usuario.NombreCompleto,

                        // Mapea la fecha de inicio de la reserva
                        FechaInicio = r.FechaInicio,

                        // Mapea la fecha de fin de la reserva
                        FechaFin = r.FechaFin

                    }).ToList() // Convierte la proyección de reservas a una List<ReservaDto>
                })

            // ToListAsync: Ejecuta la consulta de forma asíncrona y materializa los resultados en memoria como List
            // El await espera a que la base de datos devuelva todos los datos
            .ToListAsync();

            // Ok(dispositivos): Devuelve un HTTP 200 con los dispositivos en formato JSON
            // ASP.NET Core serializa automáticamente los DTOs a JSON
            return Ok(dispositivos);
        }
        //Para resumir de esta manera con DTOS evitamos ciclos y controlamos que datos exponemos ,con el select en lugar de include proyecta directamente a dto 

        // GET api/v1/dispositivos/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Dispositivo>> GetDispositivo(int id)
        {
            var dispositivo = await _context.Dispositivos
            .Include(d => d.Reservas)
            .ThenInclude(r => r.Usuario)
            .Where(d => d.DispositivoId == id)
            .Select(d => new DispositivoDto
            {
                DispositivoId = d.DispositivoId,
                Nombre = d.Nombre,
                Reservas = d.Reservas.Select(r => new ReservaDto
                {
                    ReservaId = r.ReservaId,
                    DispositivoId = r.DispositivoId,
                    DispositivoNombre = d.Nombre,
                    UsuarioId = r.UsuarioId,
                    UsuarioNombre = r.Usuario.NombreCompleto,
                    FechaInicio = r.FechaInicio,
                    FechaFin = r.FechaFin
                }).ToList()
            })
            .FirstOrDefaultAsync();

            if (dispositivo == null)
                return NotFound();

            return Ok(dispositivo);
        }

        // DELETE api/v1/dispositivos/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteDispositivo(int id)
        {
            var dispositivo = await _context.Dispositivos
                .Include(d => d.Reservas)
                .FirstOrDefaultAsync(d => d.DispositivoId == id);

            if (dispositivo == null)
                return NotFound();

            if (dispositivo.Reservas.Any(r => r.FechaFin >= DateTime.Now))
                return BadRequest("El dispositivo tiene reservas activas.");

            _context.Dispositivos.Remove(dispositivo);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // GET api/v1/dispositivos/5/disponible?inicio=2025-12-16T10:00&fin=2025-12-16T12:00

        [HttpGet("{id:int}/disponible")]
        public async Task<IActionResult> DispositivoDisponible(int id, [FromQuery] DateTime inicio, [FromQuery] DateTime fin)
        {
            bool disponible = await _dispositivoService.DispositivoDisponible(id, inicio, fin);
            return Ok(new { DispositivoId = id, Disponible = disponible, Inicio = inicio, Fin = fin });
        }

        // GET api/v1/dispositivos/estadisticas
        [HttpGet("estadisticas")]
        public async Task<IActionResult> ObtenerEstadisticas()
        {
            var estadisticas = await _dispositivoService.ObtenerEstadisticasPorDispositivo();
            return Ok(estadisticas);
        }
    }

}


