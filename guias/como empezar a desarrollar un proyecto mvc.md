# Guía de Desarrollo Proyecto MVC con C# .NET

## 📋 Orden de Desarrollo

```
1. Modelos/Entidades
   ↓
2. DbContext
   ↓
3. Program.cs (configuración inicial)
   ↓
4. Servicios (opcional pero recomendado)
   ↓
5. Controladores MVC
   ↓
6. Vistas (Scaffolding o manual)
   ↓
7. Migraciones
```

---

## 1️⃣ Modelos (Entidades)

Define las clases que representan tu dominio y se mapean a tablas de la base de datos.

```csharp
public class Dispositivo
{
    public int DispositivoId { get; set; }
    
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(100, ErrorMessage = "Máximo 100 caracteres")]
    public string Nombre { get; set; }
    
    // Relación 1:N
    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    
    // Propiedad calculada
    [NotMapped]
    public bool Disponible => !Reservas.Any(r => 
        r.FechaInicio <= DateTime.Now && r.FechaFin >= DateTime.Now);
}

public class Reserva
{
    public int ReservaId { get; set; }
    
    [Required]
    public int DispositivoId { get; set; }
    public Dispositivo? Dispositivo { get; set; }
    
    [Required]
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    
    [Required]
    public DateTime FechaInicio { get; set; }
    
    [Required]
    public DateTime FechaFin { get; set; }
}

public class Usuario
{
    public int UsuarioId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string NombreCompleto { get; set; }
    
    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
```

**Validaciones comunes:**
- `[Required]` - Campo obligatorio
- `[MaxLength]` - Longitud máxima
- `[Range]` - Rango de valores
- `[RegularExpression]` - Validación de formato

**Propiedades de navegación:**
- Usar `?` para propiedades nullable en contextos MVC: `public Dispositivo? Dispositivo { get; set; }`
- Evita errores de validación cuando no se envían en POST

---

## 2️⃣ DbContext

Configura el contexto de Entity Framework Core.

```csharp
public class AppDbContext : DbContext
{
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Dispositivo> Dispositivos { get; set; }
    public DbSet<Reserva> Reservas { get; set; }
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configuración de Dispositivo
        modelBuilder.Entity<Dispositivo>(entity =>
        {
            entity.HasKey(d => d.DispositivoId);
            entity.Property(d => d.Nombre).IsRequired().HasMaxLength(100);
            
            entity.HasMany(d => d.Reservas)
                  .WithOne(r => r.Dispositivo)
                  .HasForeignKey(r => r.DispositivoId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        
        // Configuración de Reserva
        modelBuilder.Entity<Reserva>(entity =>
        {
            entity.HasKey(r => r.ReservaId);
            entity.Property(r => r.FechaInicio).IsRequired();
            entity.Property(r => r.FechaFin).IsRequired();
        });
        
        // Configuración de Usuario
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(u => u.UsuarioId);
            entity.Property(u => u.NombreCompleto).IsRequired().HasMaxLength(100);
            
            entity.HasMany(u => u.Reservas)
                  .WithOne(r => r.Usuario)
                  .HasForeignKey(r => r.UsuarioId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        
        // Datos semilla (opcional)
        modelBuilder.Entity<Usuario>().HasData(
            new Usuario { UsuarioId = 1, NombreCompleto = "Juan García" },
            new Usuario { UsuarioId = 2, NombreCompleto = "María López" }
        );
        
        modelBuilder.Entity<Dispositivo>().HasData(
            new Dispositivo { DispositivoId = 1, Nombre = "Laptop HP" },
            new Dispositivo { DispositivoId = 2, Nombre = "Monitor Samsung" }
        );
    }
}
```

---

## 3️⃣ Program.cs

Configura servicios, middleware y rutas MVC.

```csharp
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// MVC con vistas
builder.Services.AddControllersWithViews();

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Servicios de negocio
builder.Services.AddScoped<ReservaService>();
builder.Services.AddScoped<DispositivoService>();

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Ruta por defecto
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dispositivos}/{action=Index}/{id?}");

app.Run();
```

**appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ReservasDB;Trusted_Connection=true;TrustServerCertificate=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

---

## 4️⃣ Servicios (Business Logic)

Centraliza la lógica de negocio y validaciones complejas.

```csharp
public class ReservaService
{
    private readonly AppDbContext _context;
    
    public ReservaService(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task CrearReserva(Reserva reserva)
    {
        // Validación 1: Fechas coherentes
        if (reserva.FechaInicio >= reserva.FechaFin)
            throw new InvalidOperationException("La fecha de inicio debe ser anterior a la fecha de fin");
        
        // Validación 2: No crear reservas en el pasado
        if (reserva.FechaInicio < DateTime.Now)
            throw new InvalidOperationException("No se pueden crear reservas en el pasado");
        
        // Validación 3: Disponibilidad del dispositivo
        bool disponible = !await _context.Reservas.AnyAsync(r =>
            r.DispositivoId == reserva.DispositivoId &&
            r.FechaInicio < reserva.FechaFin &&
            r.FechaFin > reserva.FechaInicio);
        
        if (!disponible)
            throw new InvalidOperationException("El dispositivo ya está reservado en esas fechas");
        
        // Validación 4: Límite de reservas simultáneas por usuario
        int reservasActivas = await _context.Reservas.CountAsync(r =>
            r.UsuarioId == reserva.UsuarioId && r.FechaFin >= DateTime.Now);
        
        if (reservasActivas >= 3)
            throw new InvalidOperationException("El usuario ha alcanzado el límite de reservas simultáneas");
        
        // Usar transacción para asegurar consistencia
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            _context.Reservas.Add(reserva);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    
    public async Task<List<Reserva>> ObtenerTodasConRelaciones()
    {
        return await _context.Reservas
            .Include(r => r.Usuario)
            .Include(r => r.Dispositivo)
            .OrderByDescending(r => r.FechaInicio)
            .ToListAsync();
    }
    
    public async Task CancelarReserva(int reservaId)
    {
        var reserva = await _context.Reservas.FindAsync(reservaId);
        
        if (reserva == null)
            throw new KeyNotFoundException("Reserva no encontrada");
        
        if (reserva.FechaInicio < DateTime.Now)
            throw new InvalidOperationException("No se pueden cancelar reservas que ya han comenzado");
        
        _context.Reservas.Remove(reserva);
        await _context.SaveChangesAsync();
    }
}
```

---

## 5️⃣ Controladores MVC

Gestionan las peticiones HTTP y devuelven vistas.

```csharp
public class DispositivosController : Controller
{
    private readonly AppDbContext _context;
    private readonly DispositivoService _dispositivoService;
    
    public DispositivosController(AppDbContext context, DispositivoService dispositivoService)
    {
        _context = context;
        _dispositivoService = dispositivoService;
    }
    
    // GET: Dispositivos
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var dispositivos = await _context.Dispositivos
            .Include(d => d.Reservas)
            .ThenInclude(r => r.Usuario)
            .ToListAsync();
        
        return View(dispositivos);
    }
    
    // GET: Dispositivos/Details/5
    [HttpGet]
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
    
    // GET: Dispositivos/Create
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }
    
    // POST: Dispositivos/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Dispositivo dispositivo)
    {
        // Validar duplicados
        if (await _context.Dispositivos.AnyAsync(d => d.Nombre == dispositivo.Nombre))
        {
            ModelState.AddModelError("Nombre", "Ya existe un dispositivo con ese nombre");
        }
        
        if (!ModelState.IsValid)
            return View(dispositivo);
        
        try
        {
            _context.Dispositivos.Add(dispositivo);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Dispositivo creado correctamente";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Error al crear el dispositivo: {ex.Message}");
            return View(dispositivo);
        }
    }
    
    // GET: Dispositivos/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var dispositivo = await _context.Dispositivos.FindAsync(id);
        
        if (dispositivo == null)
            return NotFound();
        
        return View(dispositivo);
    }
    
    // POST: Dispositivos/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Dispositivo dispositivo)
    {
        if (id != dispositivo.DispositivoId)
            return BadRequest();
        
        if (!ModelState.IsValid)
            return View(dispositivo);
        
        try
        {
            _context.Entry(dispositivo).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Dispositivo actualizado correctamente";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Dispositivos.AnyAsync(d => d.DispositivoId == id))
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
    
    // POST: Dispositivos/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var dispositivo = await _context.Dispositivos
            .Include(d => d.Reservas)
            .FirstOrDefaultAsync(d => d.DispositivoId == id);
        
        if (dispositivo == null)
            return NotFound();
        
        // Validar que no tenga reservas activas
        if (dispositivo.Reservas.Any(r => r.FechaFin >= DateTime.Now))
        {
            TempData["Error"] = "No se puede eliminar el dispositivo porque tiene reservas activas";
            return RedirectToAction(nameof(Index));
        }
        
        _context.Dispositivos.Remove(dispositivo);
        await _context.SaveChangesAsync();
        
        TempData["Success"] = "Dispositivo eliminado correctamente";
        return RedirectToAction(nameof(Index));
    }
}
```

---

## 6️⃣ Vistas

### Layout (_Layout.cshtml)
```html
<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - Sistema de Reservas</title>
    <link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.min.css" />
    <link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
</head>
<body>
    <header>
        <nav class="navbar navbar-expand-sm navbar-dark bg-dark">
            <div class="container-fluid">
                <a class="navbar-brand" asp-controller="Dispositivos" asp-action="Index">Sistema Reservas</a>
                <div class="navbar-collapse">
                    <ul class="navbar-nav me-auto">
                        <li class="nav-item">
                            <a class="nav-link" asp-controller="Dispositivos" asp-action="Index">Dispositivos</a>
                        </li>
                        <li class="nav-item">
                            <a class="nav-link" asp-controller="Reservas" asp-action="Index">Reservas</a>
                        </li>
                    </ul>
                </div>
            </div>
        </nav>
    </header>
    
    <div class="container mt-4">
        @if (TempData["Success"] != null)
        {
            <div class="alert alert-success alert-dismissible fade show">
                @TempData["Success"]
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        }
        @if (TempData["Error"] != null)
        {
            <div class="alert alert-danger alert-dismissible fade show">
                @TempData["Error"]
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        }
        
        <main role="main" class="pb-3">
            @RenderBody()
        </main>
    </div>
    
    <script src="~/lib/jquery/dist/jquery.min.js"></script>
    <script src="~/lib/bootstrap/dist/js/bootstrap.bundle.min.js"></script>
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
```

### Index.cshtml (Lista)
```html
@model IEnumerable<Dispositivo>
@{
    ViewData["Title"] = "Dispositivos";
}

<h1>@ViewData["Title"]</h1>

<p>
    <a asp-action="Create" class="btn btn-primary">Crear Nuevo</a>
</p>

<table class="table table-striped">
    <thead>
        <tr>
            <th>Nombre</th>
            <th>Estado</th>
            <th>Acciones</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var item in Model)
        {
            <tr>
                <td>@item.Nombre</td>
                <td>
                    @if (item.Disponible)
                    {
                        <span class="badge bg-success">Disponible</span>
                    }
                    else
                    {
                        <span class="badge bg-danger">Ocupado</span>
                    }
                </td>
                <td>
                    <a asp-action="Edit" asp-route-id="@item.DispositivoId" class="btn btn-sm btn-warning">Editar</a>
                    <a asp-action="Details" asp-route-id="@item.DispositivoId" class="btn btn-sm btn-info">Detalles</a>
                    <a asp-action="Delete" asp-route-id="@item.DispositivoId" class="btn btn-sm btn-danger">Eliminar</a>
                </td>
            </tr>
        }
    </tbody>
</table>
```

### Create.cshtml (Formulario)
```html
@model Dispositivo
@{
    ViewData["Title"] = "Crear Dispositivo";
}

<h1>@ViewData["Title"]</h1>

<form asp-action="Create" method="post">
    <div asp-validation-summary="ModelOnly" class="text-danger"></div>
    
    <div class="mb-3">
        <label asp-for="Nombre" class="form-label"></label>
        <input asp-for="Nombre" class="form-control" />
        <span asp-validation-for="Nombre" class="text-danger"></span>
    </div>
    
    <div class="mb-3">
        <button type="submit" class="btn btn-primary">Crear</button>
        <a asp-action="Index" class="btn btn-secondary">Cancelar</a>
    </div>
</form>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
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

## ✅ Checklist de Buenas Prácticas MVC

### Modelos
- ✅ Validaciones con atributos
- ✅ Propiedades de navegación con `?` (nullable)
- ✅ Propiedades calculadas con `[NotMapped]`

### Controladores
- ✅ Usar `Include` y `ThenInclude` para cargar relaciones
- ✅ `[ValidateAntiForgeryToken]` en POST/PUT/DELETE
- ✅ Validar `ModelState.IsValid`
- ✅ Usar `TempData` para mensajes entre redirecciones
- ✅ `FirstOrDefaultAsync` en lugar de `Single`
- ✅ Verificar existencia antes de eliminar/editar

### Servicios
- ✅ Lógica de negocio compleja
- ✅ Validaciones antes de guardar
- ✅ Transacciones cuando sea necesario
- ✅ Métodos async con `await`

### Vistas
- ✅ Usar Tag Helpers (`asp-action`, `asp-controller`, `asp-for`)
- ✅ Validaciones del lado del cliente con `_ValidationScriptsPartial`
- ✅ Mostrar mensajes con `TempData`
- ✅ Layout consistente

### DbContext
- ✅ Configuración Fluent API en `OnModelCreating`
- ✅ No usar `Database.EnsureCreated()` (usar migraciones)
- ✅ Definir comportamiento en cascada con `OnDelete`

---

## 🎯 Flujo de una Request MVC

```
[Cliente navegador]
     ↓
[Controller] - procesa request
     ↓
[Service] - lógica de negocio (opcional)
     ↓
[DbContext] - acceso a datos
     ↓
[Controller] - devuelve modelo
     ↓
[Vista Razor] - renderiza HTML
     ↓
[Cliente navegador]
```

---

## 📌 Diferencias clave: MVC vs API

| Aspecto | MVC | API |
|---------|-----|-----|
| **Clase base** | `Controller` | `ControllerBase` |
| **Devuelve** | Vistas (HTML) | JSON/XML |
| **Usa DTOs** | No (usa entidades) | Sí (siempre) |
| **Include** | Sí (para vistas) | No (proyectar con Select) |
| **ValidateAntiForgeryToken** | Sí | No |
| **TempData** | Sí | No |
| **RedirectToAction** | Sí | No |

---

## 📋 Resumen de Orden de Creación

1. **Modelos** → Define entidades con validaciones y relaciones
2. **DbContext** → Configura EF Core y relaciones
3. **Program.cs** → Registra servicios y configura rutas
4. **Servicios** → Lógica de negocio (opcional pero recomendado)
5. **Controladores** → Gestionan requests y devuelven vistas
6. **Vistas** → Interfaz de usuario con Razor
7. **Migraciones** → Crea y actualiza la base de datos