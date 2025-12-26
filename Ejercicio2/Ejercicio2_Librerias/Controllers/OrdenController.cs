using Ejercicio2.Services;
using Ejercicio2_Librerias.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Ejercicio2.Controllers
{
    [ApiController]
    [Route("api/v1/ordenes")] // Plural y versionado
    public class OrdenController : ControllerBase
    {
        private readonly OrdenService _ordenService;

        public OrdenController(OrdenService ordenService)
        {
            _ordenService = ordenService;
        }

        // GET: api/v1/ordenes
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<OrdenDto>>> GetAll()
        {
            try
            {
                var ordenes = await _ordenService.ObtenerTodasLasOrdenesAsync();
                return Ok(ordenes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error interno del servidor", detalle = ex.Message });
            }
        }

        // GET: api/v1/ordenes/5
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrdenDto>> GetById(int id)
        {
            try
            {
                var orden = await _ordenService.ObtenerOrdenAsync(id);

                if (orden == null)
                    return NotFound(new { error = $"Orden con ID {id} no encontrada" });

                return Ok(orden);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error interno del servidor", detalle = ex.Message });
            }
        }

        // POST: api/v1/ordenes
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<OrdenDto>> Create([FromBody] CreateOrdenDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var orden = await _ordenService.CrearOrdenAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = orden.OrdenId }, orden);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error interno del servidor", detalle = ex.Message });
            }
        }

        // DELETE: api/v1/ordenes/5
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _ordenService.CancelarOrdenAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error interno del servidor", detalle = ex.Message });
            }
        }

        // GET: api/v1/ordenes/por-fechas?fechaInicio=2025-01-01&fechaFin=2025-12-31
        [HttpGet("por-fechas")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<OrdenDto>>> GetByDateRange(
            [FromQuery] DateTime fechaInicio,
            [FromQuery] DateTime fechaFin)
        {
            try
            {
                var ordenes = await _ordenService.ObtenerOrdenesPorFechasAsync(fechaInicio, fechaFin);
                return Ok(ordenes);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error interno del servidor", detalle = ex.Message });
            }
        }
    }
}