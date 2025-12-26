using Ejercicio1.Data;
using Ejercicio1.DTOs;
using Microsoft.EntityFrameworkCore;


namespace Ejercicio1.Service
{
    public class DispositivoService
    {
        private readonly AppDbContext _context;
        //creamos un servicio para dispositivos por esta propiedad
        //public bool Disponible  porque es una caracteristica de la clase dispositivo
        //no reserva entonces no estaria limpio meter la funcion ahi

        public DispositivoService(AppDbContext context)
        {
            _context = context;
        }


        public async Task<bool> DispositivoDisponible(int dispositivoId, DateTime fechaInicio, DateTime fechaFin)
        {
            return !await _context.Reservas.AnyAsync(r =>
                r.DispositivoId == dispositivoId &&
                r.FechaInicio < fechaFin &&
                r.FechaFin > fechaInicio);
        }

        //Creamos un DTO para separar el modelo de base de datos de modelos de transferencia de datos
        public async Task<List<DispositivoEstadisticaDto>> ObtenerEstadisticasPorDispositivo()
        {
            return await _context.Reservas
                .GroupBy(r => r.Dispositivo.Nombre)
                .Select(g => new DispositivoEstadisticaDto
                {
                    Dispositivo = g.Key,
                    TotalReservas = g.Count(),
                    DuracionPromedioDias = g.Average(r => EF.Functions.DateDiffDay(r.FechaInicio, r.FechaFin))
                })
                .OrderByDescending(x => x.TotalReservas)
                .ToListAsync();
        }


    }
}
