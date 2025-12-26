using Ejercicio2.Data;
using Ejercicio2.Models;
using Ejercicio2_Librerias.DTO;
using Microsoft.EntityFrameworkCore;

namespace Ejercicio2.Services
{
    public class ProductoService
    {
        private readonly AppDbContext _context;

        public ProductoService(AppDbContext context)
        {
            _context = context;
        }

        // Obtener todos los productos con proyección a DTO
        public async Task<List<ProductoDto>> ObtenerTodosAsync()
        {
            return await _context.Productos
                .AsNoTracking()
                .Select(p => new ProductoDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    CantidadDisponible = p.CantidadDisponible,
                    Precio = p.Precio,
                    Disponible = p.CantidadDisponible > 0
                })
                .ToListAsync();
        }

        // Obtener producto por ID
        public async Task<ProductoDto?> ObtenerPorIdAsync(int id)
        {
            return await _context.Productos
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new ProductoDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    CantidadDisponible = p.CantidadDisponible,
                    Precio = p.Precio,
                    Disponible = p.CantidadDisponible > 0
                })
                .FirstOrDefaultAsync();
        }

        // Crear producto
        public async Task<ProductoDto> CrearAsync(CreateProductoDto dto)
        {
            // Validar que no exista un producto con el mismo nombre
            var existe = await _context.Productos
                .AnyAsync(p => p.Nombre.ToLower() == dto.Nombre.ToLower());

            if (existe)
                throw new InvalidOperationException($"Ya existe un producto con el nombre '{dto.Nombre}'");

            var producto = new Producto
            {
                Nombre = dto.Nombre,
                CantidadDisponible = dto.CantidadDisponible,
                Precio = dto.Precio
            };

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            return new ProductoDto
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                CantidadDisponible = producto.CantidadDisponible,
                Precio = producto.Precio,
                Disponible = producto.CantidadDisponible > 0
            };
        }

        // Actualizar producto
        public async Task<ProductoDto> ActualizarAsync(int id, UpdateProductoDto dto)
        {
            var producto = await _context.Productos.FindAsync(id);

            if (producto == null)
                throw new KeyNotFoundException($"Producto con ID {id} no encontrado");

            // Validar nombre duplicado si se está actualizando
            if (!string.IsNullOrWhiteSpace(dto.Nombre) && dto.Nombre != producto.Nombre)
            {
                var existe = await _context.Productos
                    .AnyAsync(p => p.Nombre.ToLower() == dto.Nombre.ToLower() && p.Id != id);

                if (existe)
                    throw new InvalidOperationException($"Ya existe otro producto con el nombre '{dto.Nombre}'");

                producto.Nombre = dto.Nombre;
            }

            if (dto.CantidadDisponible.HasValue)
                producto.CantidadDisponible = dto.CantidadDisponible.Value;

            if (dto.Precio.HasValue)
                producto.Precio = dto.Precio.Value;

            await _context.SaveChangesAsync();

            return new ProductoDto
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                CantidadDisponible = producto.CantidadDisponible,
                Precio = producto.Precio,
                Disponible = producto.CantidadDisponible > 0
            };
        }

        // Eliminar producto
        public async Task EliminarAsync(int id)
        {
            var producto = await _context.Productos
                .Include(p => p.OrdenesDetalle)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (producto == null)
                throw new KeyNotFoundException($"Producto con ID {id} no encontrado");

            // Validar que no tenga órdenes asociadas
            if (producto.OrdenesDetalle.Any())
                throw new InvalidOperationException("No se puede eliminar el producto porque tiene órdenes asociadas");

            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();
        }

        // Verificar disponibilidad
        public async Task<bool> EsDisponibleAsync(int productoId)
        {
            var producto = await _context.Productos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == productoId);

            if (producto == null)
                throw new KeyNotFoundException($"Producto con ID {productoId} no encontrado");

            return producto.CantidadDisponible > 0;
        }

        // Verificar stock suficiente
        public async Task<bool> TieneStockSuficienteAsync(int productoId, int cantidad)
        {
            var producto = await _context.Productos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == productoId);

            if (producto == null)
                throw new KeyNotFoundException($"Producto con ID {productoId} no encontrado");

            return producto.CantidadDisponible >= cantidad;
        }
    }
}