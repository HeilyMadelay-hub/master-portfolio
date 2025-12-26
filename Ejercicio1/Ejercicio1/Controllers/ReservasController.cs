using Ejercicio1.Data;
using Ejercicio1.Models;
using Ejercicio1.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ejercicio1.Controllers
{

    public class ReservasController : Controller
    {

        //No se crea usuario controller porque el foco principal son reserva y dispositivos se puede acceder a los datos de Usuario desde 
        //ICollection<Reserva> cuando carguemos el usuario con include ya que cada reserva tiene un usuario id

        // falta inyectar el servicio
        private readonly AppDbContext _context;
        private readonly ReservaService _reservaService;

        public ReservasController(AppDbContext context, ReservaService reservaService)
        {
            _context = context;
            _reservaService = reservaService;
        }


        // GET: Reservas
        public async Task<IActionResult> Index()
        {
            var reservas = await _reservaService.ObtenerTodasLasReservasConRelaciones();


            return View(reservas);
        }


        // GET: Reservas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var reserva = await _context.Reservas
                .Include(r => r.Dispositivo)
                .Include(r => r.Usuario)
                .FirstOrDefaultAsync(r => r.ReservaId == id);

            if (reserva == null) return NotFound();

            return View(reserva);
        }

        //tengo que modificar esto asi porque como no le paso nada a la vista no me la localiza porque razor no sabe que usar
   
        public async Task<IActionResult> Create(int? dispositivoId)
        {

            if (dispositivoId.HasValue)
            {
                // Verificar si hay reservas activas
                var tieneReservaActiva = await _context.Reservas
                    .AnyAsync(r => r.DispositivoId == dispositivoId.Value && r.FechaFin >= DateTime.Now);

                if (tieneReservaActiva)
                {
                    TempData["Error"] = "Este dispositivo ya tiene una reserva activa.";
                    return RedirectToAction("Index", "Dispositivos");
                }
            }

            var reserva = new Reserva
            {
                DispositivoId = dispositivoId ?? 0,
                //No se inicializan las fechas porque si no no se pasan las de la BD si las inicializamos la cogemos la fecha de hoy y eso esta mal porque causa fechas diferentes
            };

            ViewData["DispositivoId"] = new SelectList(_context.Dispositivos, "DispositivoId", "Nombre", dispositivoId);
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "UsuarioId", "NombreCompleto");

        
            return View(reserva);
        }



        // POST: Reservas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("DispositivoId,UsuarioId,FechaInicio,FechaFin")] Reserva reserva)
        {
            // Validación mínima antes del servicio
            if (reserva.DispositivoId <= 0)
                ModelState.AddModelError("DispositivoId", "Debe seleccionar un dispositivo.");

            if (reserva.UsuarioId <= 0)
                ModelState.AddModelError("UsuarioId", "Debe seleccionar un usuario.");


            if (!ModelState.IsValid)
            {
                CargarCombos(reserva.DispositivoId);
                return View(reserva);
            }

            try
            {
                await _reservaService.CrearReserva(reserva);
                return RedirectToAction("Index", "Dispositivos");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                CargarCombos(reserva.DispositivoId); 
                return View(reserva);
            }
        }

        // GET: Reservas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var reserva = await _context.Reservas
                .Include(r => r.Dispositivo)
                .Include(r => r.Usuario)
                .FirstOrDefaultAsync(r => r.ReservaId == id);

            if (reserva == null) return NotFound();

            return View(reserva);
        }

        // POST: Reservas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _reservaService.CancelarReserva(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        private void CargarCombos(int? dispositivoId = null)
        {
            ViewData["DispositivoId"] = new SelectList(_context.Dispositivos, "DispositivoId", "Nombre", dispositivoId);
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "UsuarioId", "NombreCompleto");
        }

    }
}

