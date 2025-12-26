using Ejercicio1.Data;
using Ejercicio1.Models;
using Ejercicio1.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.DependencyResolver;

namespace Ejercicio1.Controllers
{
    
    public class DispositivosController : Controller
    {

        private readonly AppDbContext _context;
        //No es un error pero si añades el readonly evitarias que pueda ser asignado en otra parte que no sea el constructor

        private readonly DispositivoService _dispositivoService;

        public DispositivosController(AppDbContext context, DispositivoService dispositivoService)
        {
            _context = context;
            _dispositivoService = dispositivoService;
        }

        /*
        En los controladores (tanto Controller como ControllerBase) existen errores comunes que no deben cometerse porque afectan directamente 
        al correcto funcionamiento de la aplicación.

        Ya que Controller es para cuando haces una aplicación MVC con vistas
        Y controllerbase para APIREST solo datos no vistas por eso se usa junto con ApiController
        
        
        Entre los más habituales se encuentran: no usar correctamente async/await, no definir las 
        rutas de los métodos, no incluir las relaciones necesarias mediante Include y ThenInclude, utilizar Single en lugar de métodos seguros como FirstOrDefault,
        no validar el ModelState en acciones POST, no manejar excepciones, no comprobar la concurrencia cuando varios usuarios acceden simultáneamente a los mismos
        recursos, no validar la integridad de los datos al eliminar registros, realizar cálculos incorrectos, olvidar definir rutas específicas por acción y no utilizar 
        [ValidateAntiForgeryToken] en operaciones sensibles como POST, PUT o DELETE.

        Para evitar mezclar responsabilidades y permitir que la aplicación funcione correctamente tanto como API como MVC, se crean dos controladores separados. 
        El controlador de API, versionado (por ejemplo api/v1), expone los datos en formato JSON y devuelve respuestas estándar como Ok, NotFound o BadRequest, 
        lo que permite su visualización y prueba mediante Swagger. Por otro lado, el controlador MVC se encarga exclusivamente de la renderización de vistas Razor
        para el usuario final. Ambos controladores pueden compartir la misma lógica y datos, pero cada uno está diseñado para un propósito específico, siguiendo buenas prácticas 
        de arquitectura y separación de responsabilidades.Como por ejemplo [ValidateAntiForgeryToken] esto lo vas a ver en mis controladores porque lo uso para proteger contra 
        los ataques en aplicaciones web con fomularios se pone en el post put y delete
     
        Falta los metodos get evidentemente de los post porque si no se abre el formulario
         
         */
        [HttpGet]
        public async Task<IActionResult> Index()
        {

            var dispositivos = await _context.Dispositivos
                            .Include(d => d.Reservas)
                            .ThenInclude(r => r.Usuario)
                            .ToListAsync();

            return View(dispositivos);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var dispositivo = await _context.Dispositivos
                .Include(d => d.Reservas)
                .ThenInclude(r => r.Usuario)
                .FirstOrDefaultAsync(d => d.DispositivoId == id);

            if (dispositivo == null)
                return NotFound();

            return View(dispositivo);
        }

        // GET: mostrar formulario
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Dispositivo d)
        {
            //Para evitar duplicados
            if (await _context.Dispositivos.AnyAsync(x => x.Nombre == d.Nombre))//variables diferentes porque si ponemos las dos variables d estan comparando lo mismo
            {
                ModelState.AddModelError("Nombre", "Ya existe un dispositivo con ese nombre."); // AddModelError conseguimos que aparezca en la vista
            }

            if (!ModelState.IsValid)
                return View(d);

            try
            {
                _context.Dispositivos.Add(d);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al crear el dispositivo: " + ex.Message);
                return View(d);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var dispositivo = await _context.Dispositivos
                .FirstOrDefaultAsync(d => d.DispositivoId == id);

            if (dispositivo == null)
                return NotFound();

            return View(dispositivo); 
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Dispositivo d)
        {
            if (id != d.DispositivoId)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(d);

            try
            {
                _context.Entry(d).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Dispositivos.AnyAsync(x => x.DispositivoId == id))
                    return NotFound();
                else
                    throw;
            }
        }

        // GET: Dispositivos/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var dispositivo = await _context.Dispositivos
                .Include(d => d.Reservas)
                .FirstOrDefaultAsync(d => d.DispositivoId == id);

            if (dispositivo == null)
                return NotFound();

            return View(dispositivo);
        }


        [HttpPost("{id:int}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dispositivo = await _context.Dispositivos
                .Include(d => d.Reservas)
                .FirstOrDefaultAsync(d => d.DispositivoId == id);

            if (dispositivo == null)
                return NotFound();

            if (dispositivo.Reservas.Any(r => r.FechaFin >= DateTime.Now))
            {
                TempData["Error"] = "No se puede eliminar el dispositivo porque tiene reservas activas.";
                return RedirectToAction(nameof(Index));
            }

            _context.Dispositivos.Remove(dispositivo);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Dispositivo eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Ver si un dispositivo está disponible en un rango de fechas
        [HttpGet("{id:int}/disponible")]
        public async Task<IActionResult> DispositivoDisponible(int id, DateTime inicio, DateTime fin)
        {
            bool disponible = await _dispositivoService.DispositivoDisponible(id, inicio, fin);
            return Json(new { DispositivoId = id, Disponible = disponible });
        }

        // GET: Obtener estadísticas de reservas por dispositivo
        [HttpGet("estadisticas")]
        public async Task<IActionResult> Estadisticas()
        {
            var estadisticas = await _dispositivoService.ObtenerEstadisticasPorDispositivo();
            return Json(estadisticas);
        }
    }
}
