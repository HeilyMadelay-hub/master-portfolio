using Business_School.Data;
using Business_School.Models;
using Business_School.Services;
using Business_School.Services.Recommendation;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
 options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
{
 options.SignIn.RequireConfirmedAccount = false;
})
 .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = ".BusinessSchool.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;

    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;

    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";

    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});


builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IGamificationService, GamificationService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope()) {
 var services = scope.ServiceProvider;
 var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
 var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
 var db = services.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();//Ejecuta todas las migraciones de Entity Framework correctamente. con EnsureCreatedAsync cae en ciclos infinitos
    await DataSeeder.SeedAsync(userManager, roleManager, db);
}

if (app.Environment.IsDevelopment())
{
 app.UseMigrationsEndPoint();
}
else
{
 app.UseExceptionHandler("/Home/Error");
 app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Usa Home/Index como raíz y HomeController redirige a Account/Login
app.MapControllerRoute(
 name: "default",
 pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
// Aplicar migraciones y crear la base de datos automáticamente
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // Esto crea la base de datos si no existe y aplica las migraciones
        context.Database.Migrate();

        Console.WriteLine("✅ Base de datos creada y migraciones aplicadas correctamente");

        // Si tienes un DataSeeder, llámalo aquí
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
        await DataSeeder.SeedAsync(userManager, roleManager, context);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error al crear la base de datos: {ex.Message}");
        throw;
    }
}

app.Run();
app.Run();
