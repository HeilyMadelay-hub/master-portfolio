using Ejercicio2.Data;
using Ejercicio2.Models;
using Ejercicio2_Librerias.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Ejercicio2.Services
{
    public class OrdenService
    {
        private readonly AppDbContext _context;

        public OrdenService(AppDbContext context)
        {
            _context = context;
        }

        // Crear orden con múltiples productos (según el DTO)
        public async Task<OrdenDto> CrearOrdenAsync(CreateOrdenDto dto)
        {
            // Validar que haya al menos un detalle
            if (dto.Detalles == null || !dto.Detalles.Any())
                throw new InvalidOperationException("La orden debe contener al menos un producto");

            // Validar que no haya productos duplicados en la orden
            var productosIds = dto.Detalles.Select(d => d.ProductoId).ToList();
            if (productosIds.Count != productosIds.Distinct().Count())
                throw new InvalidOperationException("No puede haber productos duplicados en la misma orden");

            // Usar transacción para asegurar consistencia
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validar existencia y stock de todos los productos
                var productos = await _context.Productos
                    .Where(p => productosIds.Contains(p.Id))
                    .ToListAsync();

                // Verificar que todos los productos existen
                if (productos.Count != productosIds.Count)
                {
                    var productosEncontrados = productos.Select(p => p.Id).ToList();
                    var productosFaltantes = productosIds.Except(productosEncontrados).ToList();
                    throw new KeyNotFoundException($"Productos no encontrados: {string.Join(", ", productosFaltantes)}");
                }

                // Validar stock suficiente para cada producto
                var erroresStock = new List<string>();
                foreach (var detalleDto in dto.Detalles)
                {
                    var producto = productos.First(p => p.Id == detalleDto.ProductoId);
                    if (producto.CantidadDisponible < detalleDto.Cantidad)
                    {
                        erroresStock.Add($"'{producto.Nombre}': stock disponible {producto.CantidadDisponible}, solicitado {detalleDto.Cantidad}");
                    }
                }

                if (erroresStock.Any())
                    throw new InvalidOperationException($"Stock insuficiente para los siguientes productos: {string.Join("; ", erroresStock)}");

                // Crear la orden
                var orden = new Orden
                {
                    Fecha = DateTime.Now,
                    Detalles = new List<OrdenDetalle>()
                };

                decimal totalOrden = 0;

                // Crear los detalles y actualizar stock
                foreach (var detalleDto in dto.Detalles)
                {
                    var producto = productos.First(p => p.Id == detalleDto.ProductoId);

                    var detalle = new OrdenDetalle
                    {
                        ProductoId = detalleDto.ProductoId,
                        Cantidad = detalleDto.Cantidad,
                        Orden = orden
                    };

                    // Actualizar stock del producto
                    producto.CantidadDisponible -= detalleDto.Cantidad;

                    // Calcular subtotal
                    totalOrden += producto.Precio * detalleDto.Cantidad;

                    orden.Detalles.Add(detalle);
                }

                // Agregar orden al contexto (los detalles se agregan automáticamente por la relación)
                _context.Ordenes.Add(orden);

                // Guardar cambios
                await _context.SaveChangesAsync();

                // Commit de la transacción
                await transaction.CommitAsync();

                // Mapear a DTO para devolver
                var ordenDto = new OrdenDto
                {
                    OrdenId = orden.OrdenId,
                    Fecha = orden.Fecha,
                    Total = totalOrden,
                    Detalles = orden.Detalles.Select(d =>
                    {
                        var producto = productos.First(p => p.Id == d.ProductoId);
                        return new OrdenDetalleDto
                        {
                            OrdenDetalleId = d.OrdenDetalleId,
                            ProductoId = d.ProductoId,
                            ProductoNombre = producto.Nombre,
                            PrecioUnitario = producto.Precio,
                            Cantidad = d.Cantidad,
                            Subtotal = producto.Precio * d.Cantidad
                        };
                    }).ToList()
                };

                return ordenDto;
            }
            catch
            {
                // Rollback en caso de error
                await transaction.RollbackAsync();
                throw;
            }
        }

        // Obtener orden por ID con proyección a DTO
        public async Task<OrdenDto?> ObtenerOrdenAsync(int ordenId)
        {
            var ordenDto = await _context.Ordenes
                .AsNoTracking()
                .Where(o => o.OrdenId == ordenId)
                .Select(o => new OrdenDto
                {
                    OrdenId = o.OrdenId,
                    Fecha = o.Fecha,
                    Detalles = o.Detalles.Select(d => new OrdenDetalleDto
                    {
                        OrdenDetalleId = d.OrdenDetalleId,
                        ProductoId = d.ProductoId,
                        ProductoNombre = d.Producto.Nombre,
                        PrecioUnitario = d.Producto.Precio,
                        Cantidad = d.Cantidad,
                        Subtotal = d.Producto.Precio * d.Cantidad
                    }).ToList(),
                    Total = o.Detalles.Sum(d => d.Producto.Precio * d.Cantidad)
                })
                .FirstOrDefaultAsync();

            return ordenDto;
        }

        // Obtener todas las órdenes con proyección a DTO
        public async Task<List<OrdenDto>> ObtenerTodasLasOrdenesAsync()
        {
            return await _context.Ordenes
                .AsNoTracking()
                .Select(o => new OrdenDto
                {
                    OrdenId = o.OrdenId,
                    Fecha = o.Fecha,
                    Detalles = o.Detalles.Select(d => new OrdenDetalleDto
                    {
                        OrdenDetalleId = d.OrdenDetalleId,
                        ProductoId = d.ProductoId,
                        ProductoNombre = d.Producto.Nombre,
                        PrecioUnitario = d.Producto.Precio,
                        Cantidad = d.Cantidad,
                        Subtotal = d.Producto.Precio * d.Cantidad
                    }).ToList(),
                    Total = o.Detalles.Sum(d => d.Producto.Precio * d.Cantidad)
                })
                .OrderByDescending(o => o.Fecha)
                .ToListAsync();
        }

        // Cancelar orden (eliminar)
        public async Task CancelarOrdenAsync(int ordenId)
        {
            // Usar transacción para asegurar consistencia
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var orden = await _context.Ordenes
                    .Include(o => o.Detalles)
                    .ThenInclude(d => d.Producto)
                    .FirstOrDefaultAsync(o => o.OrdenId == ordenId);

                if (orden == null)
                    throw new KeyNotFoundException($"Orden con ID {ordenId} no encontrada");

                // Validar que la orden no sea muy antigua (opcional - regla de negocio)
                if (orden.Fecha < DateTime.Now.AddDays(-7))
                    throw new InvalidOperationException("No se pueden cancelar órdenes con más de 7 días de antigüedad");

                // Devolver el stock a los productos
                foreach (var detalle in orden.Detalles)
                {
                    detalle.Producto.CantidadDisponible += detalle.Cantidad;
                }

                // Eliminar la orden (los detalles se eliminan en cascada)
                _context.Ordenes.Remove(orden);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // Obtener órdenes por rango de fechas
        public async Task<List<OrdenDto>> ObtenerOrdenesPorFechasAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            if (fechaInicio > fechaFin)
                throw new InvalidOperationException("La fecha de inicio debe ser anterior a la fecha de fin");

            return await _context.Ordenes
                .AsNoTracking()
                .Where(o => o.Fecha >= fechaInicio && o.Fecha <= fechaFin)
                .Select(o => new OrdenDto
                {
                    OrdenId = o.OrdenId,
                    Fecha = o.Fecha,
                    Detalles = o.Detalles.Select(d => new OrdenDetalleDto
                    {
                        OrdenDetalleId = d.OrdenDetalleId,
                        ProductoId = d.ProductoId,
                        ProductoNombre = d.Producto.Nombre,
                        PrecioUnitario = d.Producto.Precio,
                        Cantidad = d.Cantidad,
                        Subtotal = d.Producto.Precio * d.Cantidad
                    }).ToList(),
                    Total = o.Detalles.Sum(d => d.Producto.Precio * d.Cantidad)
                })
                .OrderByDescending(o => o.Fecha)
                .ToListAsync();
        }
    }
}