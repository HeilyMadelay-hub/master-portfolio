using Ejercicio2.Data;
using Ejercicio2.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controladores con configuración JSON para evitar ciclos de referencia.Aunque uses DTOs, esta configuración es una capa extra de seguridad por si accidentalmente devuelvemos entidades con relaciones circulares.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Ignorar ciclos de referencia en JSON
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        // Ignorar propiedades nulas en la respuesta
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// Swagger para documentar la API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Configuración mejorada de Swagger
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Sistema de Inventario y Órdenes API",
        Version = "v1",
        Description = "API para gestionar productos y órdenes de compra"
    });
});

// Conexión a base de datos
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registrar servicios 
builder.Services.AddScoped<ProductoService>();
builder.Services.AddScoped<OrdenService>();

// CORS - Configuración adecuada
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Middleware en orden correcto
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sistema Inventario API v1");
    });
}

app.UseHttpsRedirection();

// CORS antes de Authorization
app.UseCors("AllowAll");

app.UseAuthorization();

// Mapear controladores API
app.MapControllers();

app.Run();