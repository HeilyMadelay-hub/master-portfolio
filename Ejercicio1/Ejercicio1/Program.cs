using Microsoft.EntityFrameworkCore;
using Ejercicio1.Data;
using Ejercicio1.Service;

var builder = WebApplication.CreateBuilder(args);

//mvc con vistas + api
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();//swagger para documentar api

//dbcontext con la cadena de conexion
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//servicios de negocios
builder.Services.AddScoped<ReservaService>();
builder.Services.AddScoped<DispositivoService>();

//cors y la politica para controlar si puedes hacer peticiones a otro dominio dependiendo de que es lo que quieras hacer y quien eres
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

//middleware es para filtrar las solicitudes
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
// Aplicar CORS antes de Authorization porque sino el navegador rechaza las peticiones
app.UseCors("AllowAll");
app.UseAuthorization();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sistema Reservas v1");
    });
}


// **Rutas API**
app.MapControllers(); // Esto mapeara todos los controladores con [ApiController] porque no es lo mismo controladores y vistas que exponer los controladores en endpoints de api

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dispositivos}/{action=Index}/{id?}");

app.Run();
