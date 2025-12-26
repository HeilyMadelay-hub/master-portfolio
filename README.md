\# 🎓 Advanced Backend Development Exam

\## Master in Full Stack Multicloud Development 2025-2026



!\[.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge\&logo=dotnet)

!\[C#](https://img.shields.io/badge/C%23-12.0-239120?style=for-the-badge\&logo=csharp)

!\[Entity Framework](https://img.shields.io/badge/Entity\_Framework-8.0-512BD4?style=for-the-badge)

!\[SQL Server](https://img.shields.io/badge/SQL\_Server-LocalDB-CC2927?style=for-the-badge\&logo=microsoft-sql-server)



---



\## 📋 Table of Contents

\- \[Overview](#overview)

\- \[Branch Structure](#branch-structure)

\- \[Exercise 1: MVC Laboratory Device Reservation System](#exercise-1-mvc-laboratory-device-reservation-system)

\- \[Exercise 2: Web API Inventory \& Orders Management System](#exercise-2-web-api-inventory--orders-management-system)

\- \[Installation \& Setup](#installation--setup)

\- \[Development Guides](#development-guides)

\- \[Screenshots](#screenshots)

\- \[Key Features Implemented](#key-features-implemented)

\- \[Errors Found \& Solutions](#errors-found--solutions)

\- \[Technologies Used](#technologies-used)



---



\## 🎯 Overview



This branch contains two practical exam projects focused on \*\*advanced backend development\*\* using \*\*ASP.NET Core\*\*. Both projects were initially provided with intentional errors and bad practices that needed to be identified, corrected, and improved following enterprise-level standards.



\### Exam Objectives:

\- ✅ Identify and fix data model issues

\- ✅ Implement proper Entity Framework Core configurations

\- ✅ Apply business logic validation and service layer patterns

\- ✅ Correct DbContext setup and dependency injection

\- ✅ Implement advanced LINQ queries with projections

\- ✅ Follow REST API best practices

\- ✅ Ensure proper async/await patterns

\- ✅ Document all errors found and solutions applied



---



\## 📂 Branch Structure



```

backend-advanced-module3-exam/

│

├── Ejercicio1/                          # Exercise 1: MVC Project

│   ├── Controllers/

│   │   ├── DispositivosController.cs

│   │   ├── ReservasController.cs

│   │   └── UsuariosController.cs

│   ├── Models/

│   │   ├── Dispositivo.cs

│   │   ├── Reserva.cs

│   │   └── Usuario.cs

│   ├── Data/

│   │   └── AppDbContext.cs

│   ├── Services/

│   │   ├── ReservaService.cs

│   │   └── DispositivoService.cs

│   ├── Views/

│   │   ├── Dispositivos/

│   │   ├── Reservas/

│   │   └── Shared/

│   ├── Migrations/

│   ├── wwwroot/

│   ├── appsettings.json

│   └── Program.cs

│

├── Ejercicio2/                          # Exercise 2: Web API Project

│   ├── Controllers/

│   │   ├── ProductosController.cs

│   │   └── OrdenesController.cs

│   ├── Models/

│   │   ├── Producto.cs

│   │   ├── Orden.cs

│   │   └── DetalleOrden.cs

│   ├── DTOs/

│   │   ├── ProductoDto.cs

│   │   ├── OrdenCreateDto.cs

│   │   └── OrdenDetalleDto.cs

│   ├── Data/

│   │   └── InventoryDbContext.cs

│   ├── Services/

│   │   ├── ProductoService.cs

│   │   └── OrdenService.cs

│   ├── Migrations/

│   ├── appsettings.json

│   └── Program.cs

│

├── guias/                                # Development Guides

│   ├── como empezar a desarrollar un proyecto mvc.txt

│   └── como empezar a desarrollar web api.txt

│

├── img/

│   ├── Ejercicio1.png                   # MVC Application Screenshot

│   └── Ejercicio2.png                   # Web API Swagger Screenshot

│

└── README.md                             # This file

```



---



\## 🏥 Exercise 1: MVC Laboratory Device Reservation System



\### Description

A web application built with \*\*ASP.NET Core MVC\*\* to manage laboratory device reservations. The system allows users to reserve devices while enforcing business rules and validations.



!\[Exercise 1 - MVC Application](./img/Ejercicio1.png)



\### Key Requirements Addressed:



\#### 1. ✅ Data Model Corrections

\- Fixed entity relationships (One-to-Many, Many-to-Many)

\- Added data annotations and validations (`\[Required]`, `\[MaxLength]`, etc.)

\- Corrected DbSet and table names for consistency



\*\*Example:\*\*

```csharp

public class Dispositivo

{

&nbsp;   public int DispositivoId { get; set; }

&nbsp;   

&nbsp;   \[Required(ErrorMessage = "El nombre es obligatorio")]

&nbsp;   \[MaxLength(100, ErrorMessage = "Máximo 100 caracteres")]

&nbsp;   public string Nombre { get; set; }

&nbsp;   

&nbsp;   // Relación 1:N corregida

&nbsp;   public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

}

```



\#### 2. ✅ Calculated Property: `Disponibilidad` (Availability)

```csharp

\[NotMapped]

public bool Disponible => !Reservas.Any(r => 

&nbsp;   r.FechaInicio <= DateTime.Now \&\& r.FechaFin >= DateTime.Now);

```

\- Dynamic calculation without database storage

\- Indicates if a device is currently available

\- Uses `\[NotMapped]` to prevent EF Core from creating a column



\#### 3. ✅ Business Logic Service

Implemented `ReservaService` with comprehensive validations:



```csharp

public class ReservaService

{

&nbsp;   private readonly AppDbContext \_context;

&nbsp;   

&nbsp;   public async Task CrearReserva(Reserva reserva)

&nbsp;   {

&nbsp;       // Validación 1: Fechas coherentes

&nbsp;       if (reserva.FechaInicio >= reserva.FechaFin)

&nbsp;           throw new InvalidOperationException("La fecha de inicio debe ser anterior a la fecha de fin");

&nbsp;       

&nbsp;       // Validación 2: Disponibilidad del dispositivo

&nbsp;       bool disponible = !await \_context.Reservas.AnyAsync(r =>

&nbsp;           r.DispositivoId == reserva.DispositivoId \&\&

&nbsp;           r.FechaInicio < reserva.FechaFin \&\&

&nbsp;           r.FechaFin > reserva.FechaInicio);

&nbsp;       

&nbsp;       if (!disponible)

&nbsp;           throw new InvalidOperationException("El dispositivo ya está reservado");

&nbsp;       

&nbsp;       // Validación 3: Límite de reservas simultáneas

&nbsp;       int reservasActivas = await \_context.Reservas.CountAsync(r =>

&nbsp;           r.UsuarioId == reserva.UsuarioId \&\& r.FechaFin >= DateTime.Now);

&nbsp;       

&nbsp;       if (reservasActivas >= 3)

&nbsp;           throw new InvalidOperationException("Límite de reservas alcanzado");

&nbsp;       

&nbsp;       // Usar transacción para consistencia

&nbsp;       using var transaction = await \_context.Database.BeginTransactionAsync();

&nbsp;       try

&nbsp;       {

&nbsp;           \_context.Reservas.Add(reserva);

&nbsp;           await \_context.SaveChangesAsync();

&nbsp;           await transaction.CommitAsync();

&nbsp;       }

&nbsp;       catch

&nbsp;       {

&nbsp;           await transaction.RollbackAsync();

&nbsp;           throw;

&nbsp;       }

&nbsp;   }

}

```



\#### 4. ✅ DbContext \& Program.cs Configuration

\- Fixed connection string configuration in `appsettings.json`

\- Removed `Database.EnsureCreated()` to enable migrations

\- Registered services correctly in DI container



\*\*Before (❌):\*\*

```csharp

// Bloqueaba las migraciones

\_context.Database.EnsureCreated();

```



\*\*After (✅):\*\*

```csharp

// Program.cs

builder.Services.AddDbContext<AppDbContext>(options =>

&nbsp;   options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));



builder.Services.AddScoped<ReservaService>();

builder.Services.AddScoped<DispositivoService>();

```



\#### 5. ✅ Advanced Queries

```csharp

// Consulta con Include para cargar relaciones

var reservas = await \_context.Reservas

&nbsp;   .Include(r => r.Usuario)

&nbsp;   .Include(r => r.Dispositivo)

&nbsp;   .Where(r => r.FechaInicio >= DateTime.Now)

&nbsp;   .OrderByDescending(r => r.FechaInicio)

&nbsp;   .ToListAsync();



// Consulta con filtrado dinámico

public async Task<List<Dispositivo>> BuscarDispositivos(string nombre, bool? disponible)

{

&nbsp;   var query = \_context.Dispositivos

&nbsp;       .Include(d => d.Reservas)

&nbsp;       .AsQueryable();

&nbsp;   

&nbsp;   if (!string.IsNullOrWhiteSpace(nombre))

&nbsp;       query = query.Where(d => d.Nombre.Contains(nombre));

&nbsp;   

&nbsp;   if (disponible.HasValue \&\& disponible.Value)

&nbsp;       query = query.Where(d => !d.Reservas.Any(r => 

&nbsp;           r.FechaInicio <= DateTime.Now \&\& r.FechaFin >= DateTime.Now));

&nbsp;   

&nbsp;   return await query.ToListAsync();

}

```



---



\## 📦 Exercise 2: Web API Inventory \& Orders Management System



\### Description

A \*\*RESTful Web API\*\* built with \*\*ASP.NET Core\*\* to manage warehouse inventory and purchase orders. The project required identifying and fixing architectural, design, and EF Core issues.



!\[Exercise 2 - Web API Swagger](./img/Ejercicio2.png)



\### Audit Checklist Completed:



\#### 1. ✅ Data Models (Entities / EF Core)

\*\*Reviewed and Fixed:\*\*

\- ✅ Validation rules (`\[Required]`, `\[MaxLength]`, `\[Range]`)

\- ✅ Properties that shouldn't be stored (`\[NotMapped]`)

\- ✅ Invalid or inconsistent field values

\- ✅ Entity relationships and foreign keys

\- ✅ OnModelCreating configurations

\- ✅ Calculated properties implementation



\*\*Example:\*\*

```csharp

public class Producto

{

&nbsp;   public int ProductoId { get; set; }

&nbsp;   

&nbsp;   \[Required]

&nbsp;   \[MaxLength(200)]

&nbsp;   public string Nombre { get; set; }

&nbsp;   

&nbsp;   \[Range(0, double.MaxValue, ErrorMessage = "El precio debe ser positivo")]

&nbsp;   public decimal Precio { get; set; }

&nbsp;   

&nbsp;   \[Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]

&nbsp;   public int Stock { get; set; }

&nbsp;   

&nbsp;   // Propiedad calculada - NO se almacena en BD

&nbsp;   \[NotMapped]

&nbsp;   public bool DisponibleParaVenta => Stock > 0;

}

```



\#### 2. ✅ DbContext \& EF Core Configuration

\*\*Fixed:\*\*

\- ✅ DbSet registrations (consistent names and types)

\- ✅ Program.cs configuration (context registration, connection strings)

\- ✅ Database provider setup

\- ✅ Migration-blocking code removed (`Database.EnsureCreated()`)

\- ✅ Incorrect table names corrected



\#### 3. ✅ Business Logic Services

\*\*Implemented:\*\*

```csharp

public class OrdenService

{

&nbsp;   private readonly InventoryDbContext \_context;

&nbsp;   

&nbsp;   public async Task<Orden> CrearOrden(OrdenCreateDto dto)

&nbsp;   {

&nbsp;       // Validación de stock ANTES de crear la orden

&nbsp;       foreach (var detalle in dto.Detalles)

&nbsp;       {

&nbsp;           var producto = await \_context.Productos.FindAsync(detalle.ProductoId);

&nbsp;           

&nbsp;           if (producto == null)

&nbsp;               throw new InvalidOperationException($"Producto {detalle.ProductoId} no existe");

&nbsp;           

&nbsp;           if (producto.Stock < detalle.Cantidad)

&nbsp;               throw new InvalidOperationException(

&nbsp;                   $"Stock insuficiente para {producto.Nombre}. Disponible: {producto.Stock}, Solicitado: {detalle.Cantidad}");

&nbsp;       }

&nbsp;       

&nbsp;       // Usar transacción para garantizar consistencia

&nbsp;       using var transaction = await \_context.Database.BeginTransactionAsync();

&nbsp;       try

&nbsp;       {

&nbsp;           var orden = new Orden

&nbsp;           {

&nbsp;               FechaCreacion = DateTime.Now,

&nbsp;               Estado = "Pendiente"

&nbsp;           };

&nbsp;           

&nbsp;           \_context.Ordenes.Add(orden);

&nbsp;           await \_context.SaveChangesAsync();

&nbsp;           

&nbsp;           // Actualizar stock de productos

&nbsp;           foreach (var detalle in dto.Detalles)

&nbsp;           {

&nbsp;               var producto = await \_context.Productos.FindAsync(detalle.ProductoId);

&nbsp;               producto.Stock -= detalle.Cantidad;

&nbsp;               

&nbsp;               var detalleOrden = new DetalleOrden

&nbsp;               {

&nbsp;                   OrdenId = orden.OrdenId,

&nbsp;                   ProductoId = detalle.ProductoId,

&nbsp;                   Cantidad = detalle.Cantidad,

&nbsp;                   PrecioUnitario = producto.Precio

&nbsp;               };

&nbsp;               

&nbsp;               \_context.DetallesOrden.Add(detalleOrden);

&nbsp;           }

&nbsp;           

&nbsp;           await \_context.SaveChangesAsync();

&nbsp;           await transaction.CommitAsync();

&nbsp;           

&nbsp;           return orden;

&nbsp;       }

&nbsp;       catch

&nbsp;       {

&nbsp;           await transaction.RollbackAsync();

&nbsp;           throw;

&nbsp;       }

&nbsp;   }

}

```



\#### 4. ✅ Web API Controllers

\*\*Improved:\*\*



\*\*Before (❌):\*\*

```csharp

\[HttpGet]

public List<Producto> GetProductos()  // Sync, sin status codes

{

&nbsp;   return \_context.Productos.ToList();

}



\[HttpPost]

public void CreateProducto(Producto producto)  // Sin validación, sin respuesta

{

&nbsp;   \_context.Productos.Add(producto);

&nbsp;   \_context.SaveChanges();

}

```



\*\*After (✅):\*\*

```csharp

\[HttpGet]

public async Task<ActionResult<IEnumerable<ProductoDto>>> GetProductos()

{

&nbsp;   var productos = await \_context.Productos

&nbsp;       .AsNoTracking()

&nbsp;       .Select(p => new ProductoDto

&nbsp;       {

&nbsp;           ProductoId = p.ProductoId,

&nbsp;           Nombre = p.Nombre,

&nbsp;           Precio = p.Precio,

&nbsp;           Stock = p.Stock

&nbsp;       })

&nbsp;       .ToListAsync();

&nbsp;   

&nbsp;   return Ok(productos);  // 200 OK

}



\[HttpPost]

public async Task<ActionResult<ProductoDto>> CreateProducto(ProductoCreateDto dto)

{

&nbsp;   if (!ModelState.IsValid)

&nbsp;       return BadRequest(ModelState);  // 400 Bad Request

&nbsp;   

&nbsp;   var producto = new Producto

&nbsp;   {

&nbsp;       Nombre = dto.Nombre,

&nbsp;       Precio = dto.Precio,

&nbsp;       Stock = dto.Stock

&nbsp;   };

&nbsp;   

&nbsp;   \_context.Productos.Add(producto);

&nbsp;   await \_context.SaveChangesAsync();

&nbsp;   

&nbsp;   var productoDto = new ProductoDto

&nbsp;   {

&nbsp;       ProductoId = producto.ProductoId,

&nbsp;       Nombre = producto.Nombre,

&nbsp;       Precio = producto.Precio,

&nbsp;       Stock = producto.Stock

&nbsp;   };

&nbsp;   

&nbsp;   return CreatedAtAction(nameof(GetProducto), new { id = producto.ProductoId }, productoDto);  // 201 Created

}



\[HttpDelete("{id}")]

public async Task<IActionResult> DeleteProducto(int id)

{

&nbsp;   var producto = await \_context.Productos.FindAsync(id);

&nbsp;   

&nbsp;   if (producto == null)

&nbsp;       return NotFound();  // 404 Not Found

&nbsp;   

&nbsp;   \_context.Productos.Remove(producto);

&nbsp;   await \_context.SaveChangesAsync();

&nbsp;   

&nbsp;   return NoContent();  // 204 No Content

}

```



\#### 5. ✅ REST Endpoints

\*\*Standardized:\*\*

\- ✅ Proper REST naming conventions

\- ✅ Plural routes (`/api/productos`, not `/api/producto`)

\- ✅ API versioning (`/api/v1/...`)

\- ✅ Appropriate response codes



\*\*Endpoint Structure:\*\*

```

GET    /api/v1/productos           → 200 OK (lista)

GET    /api/v1/productos/{id}      → 200 OK / 404 Not Found

POST   /api/v1/productos           → 201 Created / 400 Bad Request

PUT    /api/v1/productos/{id}      → 204 No Content / 404 Not Found

DELETE /api/v1/productos/{id}      → 204 No Content / 404 Not Found



GET    /api/v1/ordenes             → 200 OK

POST   /api/v1/ordenes             → 201 Created / 400 Bad Request

```



\#### 6. ✅ Asynchrony

\*\*Fixed all sync operations:\*\*



\*\*Before (❌):\*\*

```csharp

public List<Producto> GetProductos()

{

&nbsp;   return \_context.Productos.ToList();  // Bloquea el thread

}



public void UpdateStock(int id, int cantidad)

{

&nbsp;   var producto = \_context.Productos.Find(id);  // Sync

&nbsp;   producto.Stock -= cantidad;

&nbsp;   \_context.SaveChanges();  // Sync

}

```



\*\*After (✅):\*\*

```csharp

public async Task<List<Producto>> GetProductosAsync()

{

&nbsp;   return await \_context.Productos.ToListAsync();  // Async

}



public async Task UpdateStockAsync(int id, int cantidad)

{

&nbsp;   var producto = await \_context.Productos.FindAsync(id);  // Async

&nbsp;   producto.Stock -= cantidad;

&nbsp;   await \_context.SaveChangesAsync();  // Async

}

```



\#### 7. ✅ Database Queries Optimization



\*\*Optimized with DTOs and AsNoTracking:\*\*

```csharp

// ❌ Antes: Trae toda la entidad, con tracking innecesario

public async Task<List<Producto>> GetProductos()

{

&nbsp;   return await \_context.Productos.ToListAsync();

}



// ✅ Después: Proyección a DTO, sin tracking

public async Task<List<ProductoDto>> GetProductos()

{

&nbsp;   return await \_context.Productos

&nbsp;       .AsNoTracking()  // No tracking para read-only

&nbsp;       .Select(p => new ProductoDto

&nbsp;       {

&nbsp;           ProductoId = p.ProductoId,

&nbsp;           Nombre = p.Nombre,

&nbsp;           Precio = p.Precio,

&nbsp;           Stock = p.Stock

&nbsp;       })

&nbsp;       .ToListAsync();

}



// Consulta con Include para relaciones

public async Task<OrdenDetalleDto> GetOrdenConDetalles(int id)

{

&nbsp;   return await \_context.Ordenes

&nbsp;       .AsNoTracking()

&nbsp;       .Include(o => o.Detalles)

&nbsp;           .ThenInclude(d => d.Producto)

&nbsp;       .Where(o => o.OrdenId == id)

&nbsp;       .Select(o => new OrdenDetalleDto

&nbsp;       {

&nbsp;           OrdenId = o.OrdenId,

&nbsp;           FechaCreacion = o.FechaCreacion,

&nbsp;           Total = o.Detalles.Sum(d => d.Cantidad \* d.PrecioUnitario),

&nbsp;           Detalles = o.Detalles.Select(d => new DetalleDto

&nbsp;           {

&nbsp;               ProductoNombre = d.Producto.Nombre,

&nbsp;               Cantidad = d.Cantidad,

&nbsp;               PrecioUnitario = d.PrecioUnitario

&nbsp;           }).ToList()

&nbsp;       })

&nbsp;       .FirstOrDefaultAsync();

}

```



\#### 8. ✅ Exception Handling



\*\*Before (❌):\*\*

```csharp

\[HttpGet("{id}")]

public async Task<ActionResult<Producto>> GetProducto(int id)

{

&nbsp;   // Single() lanza excepción si no existe

&nbsp;   var producto = await \_context.Productos.SingleAsync(p => p.ProductoId == id);

&nbsp;   return producto;

}



\[HttpDelete("{id}")]

public async Task<IActionResult> DeleteProducto(int id)

{

&nbsp;   // No verifica existencia antes de eliminar

&nbsp;   var producto = new Producto { ProductoId = id };

&nbsp;   \_context.Productos.Remove(producto);

&nbsp;   await \_context.SaveChangesAsync();

&nbsp;   return NoContent();

}

```



\*\*After (✅):\*\*

```csharp

\[HttpGet("{id}")]

public async Task<ActionResult<ProductoDto>> GetProducto(int id)

{

&nbsp;   var producto = await \_context.Productos

&nbsp;       .AsNoTracking()

&nbsp;       .FirstOrDefaultAsync(p => p.ProductoId == id);

&nbsp;   

&nbsp;   if (producto == null)

&nbsp;       return NotFound();  // 404 si no existe

&nbsp;   

&nbsp;   var dto = new ProductoDto

&nbsp;   {

&nbsp;       ProductoId = producto.ProductoId,

&nbsp;       Nombre = producto.Nombre,

&nbsp;       Precio = producto.Precio,

&nbsp;       Stock = producto.Stock

&nbsp;   };

&nbsp;   

&nbsp;   return Ok(dto);

}



\[HttpDelete("{id}")]

public async Task<IActionResult> DeleteProducto(int id)

{

&nbsp;   var producto = await \_context.Productos.FindAsync(id);

&nbsp;   

&nbsp;   if (producto == null)

&nbsp;       return NotFound();  // Verifica existencia primero

&nbsp;   

&nbsp;   try

&nbsp;   {

&nbsp;       \_context.Productos.Remove(producto);

&nbsp;       await \_context.SaveChangesAsync();

&nbsp;       return NoContent();  // 204 No Content

&nbsp;   }

&nbsp;   catch (DbUpdateException ex)

&nbsp;   {

&nbsp;       // Manejo de errores de BD (ej. constraint violation)

&nbsp;       return BadRequest(new { error = "No se puede eliminar el producto porque tiene órdenes asociadas" });

&nbsp;   }

}

```



---



\## ⚙️ Installation \& Setup



\### Prerequisites

\- \[.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

\- \[SQL Server LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb) or SQL Server

\- \[Visual Studio 2022](https://visualstudio.microsoft.com/) or \[VS Code](https://code.visualstudio.com/)

\- \[SQL Server Management Studio (SSMS)](https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms) (optional)



\### Setup Steps



\#### 1. Clone the Repository and Switch to Branch

```bash

git clone https://github.com/yourusername/your-repo-name.git

cd your-repo-name

git checkout backend-advanced-module3-exam

```



\#### 2. Exercise 1 - MVC Reservation System

```bash

cd Ejercicio1



\# Restore dependencies

dotnet restore



\# Update connection string in appsettings.json if needed

\# "DefaultConnection": "Server=(localdb)\\\\mssqllocaldb;Database=ReservasDB;Trusted\_Connection=true;TrustServerCertificate=true"



\# Create and apply migrations

dotnet ef migrations add InitialCreate

dotnet ef database update



\# Run the application

dotnet run



\# Access in browser: https://localhost:5001

```



\*\*Common Issues:\*\*

\- \*\*Error\*\*: "Cannot open database ReservasDB"

&nbsp; - \*\*Solution\*\*: Run `dotnet ef database update` to create the database

&nbsp; 

\- \*\*Error\*\*: "dotnet ef command not found"

&nbsp; - \*\*Solution\*\*: Install EF Core tools: `dotnet tool install --global dotnet-ef`



\#### 3. Exercise 2 - Web API Inventory System

```bash

cd ../Ejercicio2



\# Restore dependencies

dotnet restore



\# Update connection string in appsettings.json if needed

\# "DefaultConnection": "Server=(localdb)\\\\mssqllocaldb;Database=InventoryDB;Trusted\_Connection=true;TrustServerCertificate=true"



\# Create and apply migrations

dotnet ef migrations add InitialCreate

dotnet ef database update



\# Run the application

dotnet run



\# Access Swagger UI: https://localhost:5001/swagger

```



---



\## 📚 Development Guides



This repository includes comprehensive step-by-step guides for developing ASP.NET Core applications:



\### 📖 Available Guides



\#### 1. \*\*MVC Development Guide\*\*

\*\*Location\*\*: `guias/como empezar a desarrollar un proyecto mvc.txt`



\*\*Topics covered:\*\*

\- Setting up an ASP.NET Core MVC project from scratch

\- Project structure and best practices

\- Implementing Models, Views, and Controllers

\- Entity Framework Core integration

\- Database migrations workflow

\- Dependency injection setup

\- Validation and error handling

\- Service layer implementation

\- Bootstrap UI integration



\*\*Perfect for\*\*: Building web applications with server-side rendering like Exercise 1 (Laboratory Device Reservation System)



\#### 2. \*\*Web API Development Guide\*\*

\*\*Location\*\*: `guias/como empezar a desarrollar web api.txt`



\*\*Topics covered:\*\*

\- Creating a RESTful Web API with ASP.NET Core

\- API project structure and organization

\- Implementing RESTful endpoints

\- DTO pattern for data transfer

\- HTTP status codes and response handling

\- Swagger/OpenAPI documentation

\- Authentication and authorization basics

\- Query optimization and performance

\- Testing API endpoints



\*\*Perfect for\*\*: Building REST APIs like Exercise 2 (Inventory \& Orders Management System)



\### 🎯 How to Use These Guides



These guides served as the foundation for developing both exam exercises and contain:

\- ✅ Complete setup instructions

\- ✅ Code examples and templates

\- ✅ Best practices and conventions

\- ✅ Common pitfalls to avoid

\- ✅ Troubleshooting tips



\*\*Recommended Workflow:\*\*

1\. Read the appropriate guide before starting a new project

2\. Follow the structure outlined in the guide

3\. Reference the exam exercises for practical implementations

4\. Apply the patterns and practices demonstrated



---



\## 📸 Screenshots



\### Exercise 1 - MVC Application

!\[Laboratory Device Reservation System](./img/Ejercicio1.png)



\*\*Features shown:\*\*

\- Device list with availability status

\- CRUD operations for devices

\- Reservation management interface

\- Bootstrap-styled responsive UI

\- TempData success/error messages



\### Exercise 2 - Web API

!\[Inventory \& Orders Management API](./img/Ejercicio2.png)



\*\*Features shown:\*\*

\- Swagger UI documentation

\- RESTful endpoints for products and orders

\- DTO-based request/response models

\- HTTP status codes demonstration

\- API versioning structure



---



\## ✨ Key Features Implemented



\### Exercise 1 (MVC)

\- ✅ \*\*Device Management\*\*: Full CRUD with validation

\- ✅ \*\*Reservation System\*\*: Business rule enforcement

\- ✅ \*\*Dynamic Availability\*\*: Calculated property `\[NotMapped]`

\- ✅ \*\*Service Layer\*\*: Separation of business logic

\- ✅ \*\*Advanced Queries\*\*: `Include`, `ThenInclude`, dynamic filtering

\- ✅ \*\*Transaction Management\*\*: Data consistency

\- ✅ \*\*User-Friendly UI\*\*: Bootstrap 5, TempData messages

\- ✅ \*\*Proper Async/Await\*\*: Throughout the application



\### Exercise 2 (Web API)

\- ✅ \*\*RESTful Design\*\*: Proper HTTP verbs and status codes

\- ✅ \*\*DTO Pattern\*\*: Separation of concerns

\- ✅ \*\*Stock Validation\*\*: Before creating orders

\- ✅ \*\*Swagger Documentation\*\*: OpenAPI specification

\- ✅ \*\*Async Operations\*\*: All database calls are async

\- ✅ \*\*Query Optimization\*\*: `AsNoTracking`, projections

\- ✅ \*\*Exception Handling\*\*: Proper error responses

\- ✅ \*\*Transaction Management\*\*: For complex operations



---



\## 🐛 Errors Found \& Solutions



\### Summary of Corrections



Both exercises contained intentional errors that were identified and corrected following enterprise-level standards:



| Category | Exercise 1 Issues | Exercise 2 Issues | Solutions Applied |

|----------|------------------|-------------------|-------------------|

| \*\*Data Models\*\* | Missing `\[Required]`, nullable issues | Missing `\[NotMapped]`, wrong validations | Added annotations, fixed nullability |

| \*\*DbContext\*\* | `EnsureCreated()` blocking migrations | Incorrect DbSet names | Removed blocking code, fixed names |

| \*\*Services\*\* | No business logic validation | Missing stock validation | Implemented service layer |

| \*\*Controllers\*\* | Sync methods, no `Include` | Wrong HTTP codes, sync calls | Made async, proper status codes |

| \*\*Queries\*\* | Missing eager loading | No `AsNoTracking`, no DTOs | Optimized with `Include` and projections |

| \*\*REST API\*\* | N/A | Inconsistent routes | Standardized REST conventions |

| \*\*Exceptions\*\* | No null checks | `Single()` throwing errors | Added `FirstOrDefault()` + null checks |

| \*\*Transactions\*\* | None | Missing for orders | Implemented transaction scope |



\### Key Improvements Made



\#### Data Models \& Validation

\- Added proper data annotations (`\[Required]`, `\[MaxLength]`, `\[Range]`)

\- Implemented `\[NotMapped]` for calculated properties

\- Fixed entity relationships and foreign keys

\- Corrected nullable reference types



\#### Entity Framework Core

\- Removed `Database.EnsureCreated()` to enable proper migrations

\- Fixed DbContext configuration in `Program.cs`

\- Implemented eager loading with `Include` and `ThenInclude`

\- Added `AsNoTracking()` for read-only queries

\- Optimized queries with DTO projections



\#### Business Logic

\- Implemented service layer for business validations

\- Added transaction management for complex operations

\- Implemented stock validation before order creation

\- Added reservation conflict detection

\- Enforced business rules (max reservations, date validation)



\#### REST API Best Practices

\- Standardized endpoint naming (plural resources)

\- Implemented proper HTTP status codes

\- Added comprehensive Swagger documentation

\- Implemented DTO pattern for clean API contracts

\- Added proper exception handling with meaningful responses



\#### Async/Await Pattern

\- Converted all synchronous database operations to async

\- Implemented proper async/await throughout the application

\- Used `ConfigureAwait(false)` where appropriate



---



\## 🛠️ Technologies Used



| Technology | Version | Purpose |

|------------|---------|---------|

| \*\*.NET\*\* | 8.0 | Framework |

| \*\*C#\*\* | 12.0 | Programming Language |

| \*\*ASP.NET Core MVC\*\* | 8.0 | Web Framework (Exercise 1) |

| \*\*ASP.NET Core Web API\*\* | 8.0 | REST API (Exercise 2) |

| \*\*Entity Framework Core\*\* | 8.0 | ORM |

| \*\*SQL Server LocalDB\*\* | 2019+ | Database |

| \*\*Bootstrap\*\* | 5.3 | UI Framework (MVC) |

| \*\*Swashbuckle\*\* | 6.5+ | Swagger/OpenAPI (API) |



---



\## 📚 Learning Outcomes



Through this exam, the following skills were demonstrated:



\### 1. ✅ Entity Framework Core Mastery

\- Complex relationship configuration (`HasMany`, `WithOne`, `OnDelete`)

\- Migration management and troubleshooting

\- Query optimization (`Include`, `AsNoTracking`, projections)

\- Transaction handling for data consistency



\### 2. ✅ ASP.NET Core MVC

\- CRUD operations with proper validation

\- Service layer pattern for business logic

\- View rendering with Razor syntax

\- TempData for cross-request messaging



\### 3. ✅ ASP.NET Core Web API

\- RESTful API design principles

\- Proper HTTP status codes (200, 201, 204, 400, 404)

\- DTO pattern for data transfer

\- Swagger/OpenAPI documentation



\### 4. ✅ Best Practices

\- Async/await patterns throughout

\- Dependency injection configuration

\- Separation of concerns (Controllers → Services → Data

