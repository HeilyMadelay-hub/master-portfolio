# Guía de Desarrollo Web API con C# .NET

## 📋 Orden de Desarrollo

```
1. Modelos/Entidades
   ↓
2. DTOs
   ↓
3. DbContext
   ↓
4. Servicios
   ↓
5. Controladores
   ↓
6. Program.cs
   ↓
7. Migraciones
```

---

## 1️⃣ Modelos (Entidades)

Define las clases que representan tu dominio y se mapean a tablas de la base de datos.

```csharp
public class Producto
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; }
    
    [Required]
    [Range(0, int.MaxValue)]
    public int CantidadDisponible { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Precio { get; set; }
    
    // Relaciones
    public ICollection<OrdenDetalle> OrdenesDetalle { get; set; } = new List<OrdenDetalle>();
}
```

**Validaciones comunes:**
- `[Required]` - Campo obligatorio
- `[MaxLength]` - Longitud máxima
- `[Range]` - Rango de valores
- `[RegularExpression]` - Validación de formato

**Propiedades calculadas:**
```csharp
[NotMapped]
public bool Disponible => CantidadDisponible > 0;
```

---

## 2️⃣ DTOs (Data Transfer Objects)

Define contratos de entrada y salida de la API. **Siempre se crean después de los modelos.**

### DTOs de Lectura (GET)
```csharp
public class ProductoDto
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public decimal Precio { get; set; }
    public bool Disponible { get; set; }
}
```

### DTOs de Creación (POST)
```csharp
public class CreateProductoDto
{
    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Precio { get; set; }
}
```

### DTOs de Actualización (PUT/PATCH)
```csharp
public class UpdateProductoDto
{
    [MaxLength(100)]
    public string? Nombre { get; set; }
    
    [Range(0.01, double.MaxValue)]
    public decimal? Precio { get; set; }
}
```

---

## 3️⃣ DbContext

Configura el contexto de Entity Framework Core.

```csharp
public class AppDbContext : DbContext
{
    public DbSet<Producto> Productos { get; set; }
    public DbSet<Orden> Ordenes { get; set; }
    public DbSet<OrdenDetalle> OrdenesDetalle { get; set; }
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configuración con Fluent API
        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Precio).HasPrecision(18, 2);
            
            entity.HasMany(p => p.OrdenesDetalle)
                  .WithOne(od => od.Producto)
                  .HasForeignKey(od => od.ProductoId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        
        // Datos semilla (opcional)
        modelBuilder.Entity<Producto>().HasData(
            new Producto { Id = 1, Nombre = "Laptop", CantidadDisponible = 10, Precio = 1500M }
        );
    }
}
```

---

## 4️⃣ Servicios (Business Logic)

Centraliza la lógica de negocio, validaciones y acceso a datos.

```csharp
public class ProductoService
{
    private readonly AppDbContext _context;
    
    public ProductoService(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<ProductoDto>> ObtenerTodosAsync()
    {
        return await _context.Productos
            .AsNoTracking()
            .Select(p => new ProductoDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Precio = p.Precio,
                Disponible = p.CantidadDisponible > 0
            })
            .ToListAsync();
    }
    
    public async Task<ProductoDto> CrearAsync(CreateProductoDto dto)
    {
        // Validaciones de negocio
        var existe = await _context.Productos
            .AnyAsync(p => p.Nombre == dto.Nombre);
        
        if (existe)
            throw new InvalidOperationException("El producto ya existe");
        
        var producto = new Producto
        {
            Nombre = dto.Nombre,
            Precio = dto.Precio
        };
        
        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();
        
        return new ProductoDto
        {
            Id = producto.Id,
            Nombre = producto.Nombre,
            Precio = producto.Precio
        };
    }
}
```

**Buenas prácticas:**
- Usar `AsNoTracking()` en consultas de solo lectura
- Proyectar a DTOs con `Select()` en lugar de `Include()`
- Usar transacciones para operaciones complejas
- Validar reglas de negocio antes de guardar

---

## 5️⃣ Controladores

Reciben requests HTTP y devuelven responses. **No contienen lógica de negocio.**

```csharp
[ApiController]
[Route("api/v1/productos")]
public class ProductoController : ControllerBase
{
    private readonly ProductoService _productoService;
    
    public ProductoController(ProductoService productoService)
    {
        _productoService = productoService;
    }
    
    // GET: api/v1/productos
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductoDto>>> GetAll()
    {
        try
        {
            var productos = await _productoService.ObtenerTodosAsync();
            return Ok(productos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error interno", detalle = ex.Message });
        }
    }
    
    // POST: api/v1/productos
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductoDto>> Create([FromBody] CreateProductoDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        try
        {
            var producto = await _productoService.CrearAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = producto.Id }, producto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
```

**Códigos HTTP:**
- `200 OK` - Operación exitosa (GET)
- `201 Created` - Recurso creado (POST)
- `204 NoContent` - Operación exitosa sin contenido (DELETE)
- `400 BadRequest` - Error de validación
- `404 NotFound` - Recurso no encontrado
- `500 InternalServerError` - Error del servidor

---

## 6️⃣ Program.cs

Configura servicios, middleware y rutas.

```csharp
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controladores con configuración JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = 
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Servicios
builder.Services.AddScoped<ProductoService>();
builder.Services.AddScoped<OrdenService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
```

---

## 7️⃣ Migraciones

```bash
# Crear migración
dotnet ef migrations add InitialCreate

# Aplicar migración
dotnet ef database update

# Eliminar última migración
dotnet ef migrations remove
```

---

## ✅ Checklist de Buenas Prácticas

### Modelos
- ✅ Validaciones con atributos
- ✅ Relaciones configuradas correctamente
- ✅ Propiedades calculadas con `[NotMapped]`

### DTOs
- ✅ Separar Create/Update/Response DTOs
- ✅ Validaciones en DTOs de entrada
- ✅ No exponer relaciones circulares

### Servicios
- ✅ Lógica de negocio y validaciones
- ✅ Usar `AsNoTracking()` en consultas read-only
- ✅ Proyectar a DTOs con `Select()`
- ✅ Usar transacciones cuando sea necesario
- ✅ Métodos async con `await`

### Controladores
- ✅ Rutas RESTful en plural (`/api/v1/productos`)
- ✅ Códigos HTTP correctos
- ✅ Validar `ModelState`
- ✅ Manejo de excepciones
- ✅ Métodos async

### DbContext
- ✅ Configuración Fluent API en `OnModelCreating`
- ✅ No usar `Database.EnsureCreated()` (usar migraciones)

### Asincronía
- ✅ Usar `async/await` siempre
- ✅ `SaveChangesAsync()` en lugar de `SaveChanges()`
- ✅ `ToListAsync()`, `FirstOrDefaultAsync()`, etc.

---

## 🎯 Flujo de una Request

```
[Cliente HTTP]
     ↓
[DTO] - valida datos
     ↓
[Controller] - recibe request
     ↓
[Service] - lógica de negocio
     ↓
[DbContext] - acceso a datos
     ↓
[Response DTO] - devuelve datos
     ↓
[Cliente HTTP]
```

---

## 📌 Reglas de Oro

1. **DTOs siempre después de modelos**
2. **Servicios contienen lógica de negocio**
3. **Controladores solo reciben y devuelven datos**
4. **Proyectar a DTOs, no usar Include en APIs**
5. **AsNoTracking en consultas read-only**
6. **Transacciones para operaciones complejas**
7. **Async/await siempre**