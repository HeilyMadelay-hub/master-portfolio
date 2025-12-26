using Ejercicio1.Data;
using Ejercicio1.DTOs;
using Ejercicio1.Models;
using Ejercicio1.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

//El API Controller no replica toda la lógica del controlador
//MVC porque su responsabilidad es exponer endpoints REST y no
//gestionar vistas, formularios ni navegación web. Por eso solo
//implementa métodos CRUD que devuelven JSON y códigos HTTP.
//La lógica de negocio se encapsula en servicios para evitar duplicación, 
//facilitar el mantenimiento y permitir que tanto el controlador MVC como el 
//controlador API reutilicen las mismas reglas del dominio.

namespace Ejercicio1.Controllers.Api
{
    // ApiController: Atributo que habilita comportamientos automáticos para APIs REST
    // como validación automática del ModelState, inferencia de [FromBody], etc.
    [ApiController]

    // Route: Define la ruta base para todos los endpoints de este controlador
    // api/v1/reservas - El versionado (v1) permite mantener múltiples versiones de la API
    [Route("api/v1/reservas")]

    // ControllerBase: Clase base para controladores API (sin soporte para vistas)
    // Es más ligera que Controller porque no incluye funcionalidad de MVC con Razor
    public class ReservasApiController : ControllerBase
    {
        // Inyección de dependencia del servicio de reservas
        // El servicio contiene toda la lógica de negocio reutilizable
        private readonly ReservaService _reservaService;

        // Constructor: ASP.NET Core inyecta automáticamente las dependencias
        public ReservasApiController(ReservaService reservaService)
        {
            _reservaService = reservaService;
        }

        // GET: api/v1/reservas
        // HttpGet: Indica que este método responde a peticiones HTTP GET
        [HttpGet]

        // ActionResult<T>: Permite devolver tanto el tipo T como resultados HTTP (Ok, NotFound, etc.)
        // IEnumerable<ReservaDto>: Devuelve una colección de DTOs en lugar de entidades
        public async Task<ActionResult<IEnumerable<ReservaDto>>> GetReservas()
        {
            // Obtiene todas las reservas con sus relaciones (Usuario y Dispositivo) del servicio
            var reservas = await _reservaService.ObtenerTodasLasReservasConRelaciones();

            // PROBLEMA: Esto devuelve entidades con ciclos de referencia
            // SOLUCIÓN: Proyectar a DTOs para evitar el error de serialización JSON
            var reservasDto = reservas.Select(r => new ReservaDto
            {
                ReservaId = r.ReservaId,
                DispositivoId = r.DispositivoId,
                DispositivoNombre = r.Dispositivo?.Nombre ?? "Sin dispositivo",
                UsuarioId = r.UsuarioId,
                UsuarioNombre = r.Usuario?.NombreCompleto ?? "Sin usuario",
                FechaInicio = r.FechaInicio,
                FechaFin = r.FechaFin
            }).ToList();

            // Ok(data): Devuelve HTTP 200 con el contenido en formato JSON
            return Ok(reservasDto);
        }

        // POST: api/v1/reservas
        // HttpPost: Indica que este método responde a peticiones HTTP POST (crear recursos)
        [HttpPost]

        // IActionResult: Tipo de retorno genérico que permite devolver cualquier resultado HTTP
        // [FromBody] está implícito por [ApiController], indica que el parámetro viene del cuerpo JSON
        public async Task<IActionResult> CreateReserva([FromBody] Reserva reserva)
        {
            // ModelState.IsValid: Verifica las validaciones de anotaciones [Required], etc.
            // [ApiController] hace esto automáticamente, pero es buena práctica dejarlo explícito
            if (!ModelState.IsValid)
                // BadRequest(ModelState): Devuelve HTTP 400 con los errores de validación
                return BadRequest(ModelState);

            try
            {
                // Delega la creación al servicio donde está toda la lógica de negocio
                // (validaciones de fechas, disponibilidad, límites de usuario, etc.)
                await _reservaService.CrearReserva(reserva);

                // MEJORA: Usa CreatedAtAction en lugar de Ok para seguir estándares REST
                // CreatedAtAction devuelve HTTP 201 con la ubicación del recurso creado
                return CreatedAtAction(
                    nameof(GetReservas), // Nombre de la acción para obtener el recurso
                    new { id = reserva.ReservaId }, // Parámetros de ruta
                    new ReservaDto // Devuelve el DTO de la reserva creada
                    {
                        ReservaId = reserva.ReservaId,
                        DispositivoId = reserva.DispositivoId,
                        DispositivoNombre = reserva.Dispositivo?.Nombre ?? "Sin dispositivo",
                        UsuarioId = reserva.UsuarioId,
                        UsuarioNombre = reserva.Usuario?.NombreCompleto ?? "Sin usuario",
                        FechaInicio = reserva.FechaInicio,
                        FechaFin = reserva.FechaFin
                    }
                );
            }
            catch (Exception ex)
            {
                // Captura excepciones lanzadas por el servicio (ej: "Dispositivo no disponible")
                // BadRequest: Devuelve HTTP 400 con el mensaje de error
                // MEJORA: Devolver un objeto estructurado en lugar de string
                return BadRequest(new { error = ex.Message });
            }
        }

        // DELETE: api/v1/reservas/5
        // HttpDelete: Indica que este método responde a peticiones HTTP DELETE (eliminar recursos)
        [HttpDelete("{id:int}")] // {id:int} - Parámetro de ruta con restricción de tipo entero

        // IActionResult: Permite devolver resultados sin contenido (NoContent)
        public async Task<IActionResult> DeleteReserva(int id)
        {
            try
            {
                // Delega la cancelación al servicio que valida si la reserva puede cancelarse
                // (ej: no se pueden cancelar reservas que ya comenzaron)
                await _reservaService.CancelarReserva(id);

                // NoContent(): Devuelve HTTP 204 (operación exitosa sin contenido de respuesta)
                // Es el estándar REST para DELETE exitoso
                return NoContent();
            }
            catch (Exception ex)
            {
                // Si la reserva no existe o no puede cancelarse, devuelve HTTP 400
                // MEJORA: Diferenciar entre NotFound (404) y BadRequest (400)
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
