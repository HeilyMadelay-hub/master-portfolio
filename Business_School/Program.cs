using Business_School.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
 options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
 .AddRoles<IdentityRole>()
 .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

var app = builder.Build();


// We now create the initial roles because we have used .AddRoles<IdentityRole>().
// This method adds role support to the Identity system by registering the role services
// and allowing the app to create, manage, and assign roles to users.

//A scope is a controlled lifetime for services, ensuring they live as long as needed and are disposed properly.-Container

using (var scope = app.Services.CreateScope()) {

 // EN:1- Creates a temporary scope outside an HTTP request and gets the services available in that scope with ServiceProvider
 // ES:1- Crea un scope temporal fuera de una solicitud HTTP y obtiene los servicios disponibles en ese scope with ServiceProvider

 var services = scope.ServiceProvider;

 //2. We create the roles
 var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
 var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

 string[] roles = { "Admin", "DepartmentManager", "ClubLeader", "Student","User" };
 foreach (var role in roles)
 {
 if (!await roleManager.RoleExistsAsync(role))
 {
 await roleManager.CreateAsync(new IdentityRole(role));
 }
 }

 //3. admin
 var adminEmail = "admin@businessschool.com";
 var adminUser = await userManager.FindByEmailAsync(adminEmail);
 if (adminUser == null)
 {
 adminUser = new IdentityUser
 {
 UserName = adminEmail,
 Email = adminEmail,
 EmailConfirmed = true 
 };

 var result = await userManager.CreateAsync(adminUser, "Admin123!");
 if (result.Succeeded)
 {
 await userManager.AddToRoleAsync(adminUser, "Admin");
 }
 else
 {
 Console.WriteLine("Failed to create admin user: " + string.Join("; ", result.Errors.Select(e => e.Description)));
 }
 }

 // We assign User as rol by default to any user 
 var users = userManager.Users.ToList();
 foreach (var user in users)
 {
 var role = await userManager.GetRolesAsync(user);
 if (!role.Any())
 {
 await userManager.AddToRoleAsync(user, "User");
 }
 }

 //We create two example users (replaced previous DepartmentManager and ClubLeader examples)
 var hrManagerEmail = "manager.hr@businessschool.com";
 var hrManager = await userManager.FindByEmailAsync(hrManagerEmail);
 if (hrManager == null)
 {
 hrManager = new IdentityUser { UserName = hrManagerEmail, Email = hrManagerEmail, EmailConfirmed = true };
 var result = await userManager.CreateAsync(hrManager, "Manager456!");
 if (result.Succeeded)
 {
 await userManager.AddToRoleAsync(hrManager, "DepartmentManager");
 }
 else
 {
 Console.WriteLine("Failed to create HR manager: " + string.Join("; ", result.Errors.Select(e => e.Description)));
 }
 }

 var sportsLeaderEmail = "leader.sports@businessschool.com";
 var sportsLeader = await userManager.FindByEmailAsync(sportsLeaderEmail);
 if (sportsLeader == null)
 {
 sportsLeader = new IdentityUser { UserName = sportsLeaderEmail, Email = sportsLeaderEmail, EmailConfirmed = true };
 var result = await userManager.CreateAsync(sportsLeader, "Leader456!");
 if (result.Succeeded)
 {
 await userManager.AddToRoleAsync(sportsLeader, "ClubLeader");
 }
 else
 {
 Console.WriteLine("Failed to create sports leader: " + string.Join("; ", result.Errors.Select(e => e.Description)));
 }
 }

 // Crear usuario estudiante
 var studentEmail = "student@businessschool.com";
 var studentUser = await userManager.FindByEmailAsync(studentEmail);
 if (studentUser == null)
 {
 studentUser = new IdentityUser { UserName = studentEmail, Email = studentEmail, EmailConfirmed = true };
 var result = await userManager.CreateAsync(studentUser, "Student123!");
 if (result.Succeeded)
 {
 await userManager.AddToRoleAsync(studentUser, "Student");
 }
 else
 {
 Console.WriteLine("Failed to create student user: " + string.Join("; ", result.Errors.Select(e => e.Description)));
 }
 }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
 app.UseMigrationsEndPoint();
}
else
{
 app.UseExceptionHandler("/Home/Error");
 // The default HSTS value is30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
 app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
 name: "default",
 pattern: "{controller=Account}/{action=Login}/{id?}");
app.MapRazorPages();

app.Run();
