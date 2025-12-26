using Ejercicio1.Data;
using Ejercicio1.Models;
using Microsoft.EntityFrameworkCore;



namespace Ejercicio1.Service
{
    public class ReservaService
    {
        private readonly AppDbContext _context;

        public ReservaService(AppDbContext context)
        {
            _context = context;
        }

        
        public async Task CrearReserva(Reserva r)//va aqui y no en el controlador porque es logica de negocio compleja
        {
            if (r == null)
                throw new ArgumentNullException(nameof(r), "La reserva no puede ser null.");

            // 1. Validar que tenga un dispositivo 
            if (r.DispositivoId <= 0)
                throw new InvalidOperationException("La reserva debe estar asociada a un dispositivo.");

            //-----------------------------------------

            //Con esto evitamos que nos salte la excepcion de no se puede crear reservas en el pasado
            //Usamos DateTime.Now en el servidor incluye segundos y milisegundos no datetime-local porque aunque seleccione la hora actual puede ser menor que DateTime.Now y el sistema puede
            //pensar que es pasado

            // Redondeamos FechaInicio y FechaFin para evitar problemas de segundos/milisegundos
            r.FechaInicio = r.FechaInicio.AddSeconds(-r.FechaInicio.Second).AddMilliseconds(-r.FechaInicio.Millisecond);
            r.FechaFin = r.FechaFin.AddSeconds(-r.FechaFin.Second).AddMilliseconds(-r.FechaFin.Millisecond);

            // 2. Validar que las fechas sean coherentes
            if (r.FechaInicio >= r.FechaFin)
                throw new InvalidOperationException("La fecha de inicio debe ser anterior a la fecha de fin.");
            
            //  Validar que no sea en el pasado
            if (r.FechaInicio < DateTime.Now.AddSeconds(-5)) // pequeño margen de seguridad
                throw new InvalidOperationException("No se pueden crear reservas en el pasado.");


            //-----------------------------------------



            // 3. Validar disponibilidad del dispositivo 
            bool disponible = !await _context.Reservas.AnyAsync(res =>
                res.DispositivoId == r.DispositivoId &&
                res.FechaInicio < r.FechaFin &&
                res.FechaFin > r.FechaInicio);

            if (!disponible)
                throw new InvalidOperationException("El dispositivo ya está reservado en esas fechas.");

            // 4. Controlar el número de reservas simultáneas del usuario 
            int reservasActivas = await _context.Reservas.CountAsync(res =>
                res.UsuarioId == r.UsuarioId && res.FechaFin >= DateTime.Now);

            if (reservasActivas >= 3)
                throw new InvalidOperationException("El usuario ha alcanzado el límite de reservas simultáneas.");

            // 5. Usar transacción para asegurar consistencia 
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Reservas.Add(r);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

       //Las consultas para el controlador para mostrar info al usuario
        public async Task<List<Reserva>> ObtenerTodasLasReservasConRelaciones()
        {
            return await _context.Reservas
                .Include(r => r.Usuario)         // Incluye datos del usuario
                .Include(r => r.Dispositivo)     // Incluye datos del dispositivo
                .OrderByDescending(r => r.FechaInicio)
                .ToListAsync();
        }

        public async Task<List<Reserva>> ObtenerReservasActivasDeUsuario(int usuarioId)
        {
            return await _context.Reservas
                .Include(r => r.Dispositivo)
                .Where(r => r.UsuarioId == usuarioId && r.FechaFin >= DateTime.Now)
                .OrderBy(r => r.FechaInicio)
                .ToListAsync();
        }

    
        public async Task<List<Reserva>> FiltrarReservas(
            int? usuarioId = null,
            int? dispositivoId = null,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null)
        {
            var query = _context.Reservas
                .Include(r => r.Usuario)
                .Include(r => r.Dispositivo)
                .AsQueryable();

           
            if (usuarioId.HasValue)
                query = query.Where(r => r.UsuarioId == usuarioId.Value);

            if (dispositivoId.HasValue)
                query = query.Where(r => r.DispositivoId == dispositivoId.Value);

            if (fechaDesde.HasValue)
                query = query.Where(r => r.FechaInicio >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(r => r.FechaFin <= fechaHasta.Value);

            return await query
                .OrderByDescending(r => r.FechaInicio)
                .ToListAsync();
        }

       


        public async Task CancelarReserva(int reservaId)
        {
            var reserva = await _context.Reservas.FindAsync(reservaId);

            if (reserva == null)
                throw new InvalidOperationException("Reserva no encontrada.");

            if (reserva.FechaInicio < DateTime.Now)
                throw new InvalidOperationException("No se pueden cancelar reservas que ya han comenzado.");

            _context.Reservas.Remove(reserva);
            await _context.SaveChangesAsync();
        }

        //Este va aqui porque es historial de dispositivos para tenerlo hay que tener reservas hechas entonces por eso va aqui
        public async Task<List<Reserva>> ObtenerHistorialDeDispositivo(int dispositivoId)
        {
            return await _context.Reservas
                .Include(r => r.Usuario)
                .Where(r => r.DispositivoId == dispositivoId)
                .OrderByDescending(r => r.FechaInicio)
                .ToListAsync();
        }
    }
}