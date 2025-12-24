using Business_School.Data;
using Business_School.Helpers;
using Business_School.Models;
using Business_School.ViewModels.Admin;
using Business_School.ViewModels.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Business_School.Controllers
{
    [Authorize(Roles = RoleHelper.Admin)]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly ApplicationDbContext _context;

        public AdminController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<int>> roleManager,
        ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<IActionResult> FixInvalidEvents()
        {
            var invalidEvents = await _context.Events
          .Include(e => e.Department)
           .Where(e => e.StartDate == DateTime.MinValue || e.StartDate.Year < 2000)
               .OrderBy(e => e.Title)
          .ToListAsync();

            ViewData["TotalInvalid"] = invalidEvents.Count;
            return View(invalidEvents);
        }

        public async Task<IActionResult> AdminDashboard()
        {
            // Obtener todos los usuarios con rol Student
            var students = await _userManager.GetUsersInRoleAsync(RoleHelper.Student);

            // Excluir los usuarios de prueba por email
            var filteredStudents = students
                .Where(s => !s.Email.EndsWith("@businessschool.com"))
                .Take(5)
                .ToList();

            var vm = new DashboardAdminVM
            {
                RecentStudents = filteredStudents,
                TotalStudents = filteredStudents.Count,
                TotalClubs = await _context.Clubs.CountAsync(),
                TotalDepartments = await _context.Departments.CountAsync()
            };

            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FixEventDate(int eventId, DateTime newStartDate)
        {
            var ev = await _context.Events.FindAsync(eventId);
            if (ev == null)
            {
                TempData["Error"] = "Evento no encontrado.";
                return RedirectToAction(nameof(FixInvalidEvents));
            }

            ev.StartDate = newStartDate;
            ev.EndDate = newStartDate.AddHours(2); // Duración por defecto: 2 horas
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Evento '{ev.Title}' actualizado correctamente. Nueva fecha: {newStartDate:dd/MM/yyyy HH:mm}";
            return RedirectToAction(nameof(FixInvalidEvents));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FixAllInvalidEvents()
        {
            var invalidEvents = await _context.Events
          .Where(e => e.StartDate == DateTime.MinValue || e.StartDate.Year < 2000)
     .ToListAsync();

            if (!invalidEvents.Any())
            {
                TempData["Warning"] = "No hay eventos con fechas inválidas para corregir.";
                return RedirectToAction(nameof(FixInvalidEvents));
            }

            var baseDate = DateTime.UtcNow.AddDays(30).Date.AddHours(10); // 30 días desde hoy a las 10:00
            int count = 0;

            foreach (var ev in invalidEvents)
            {
                // Espaciar los eventos: cada uno 1 día después del anterior
                ev.StartDate = baseDate.AddDays(count);
                ev.EndDate = ev.StartDate.AddHours(2);
                count++;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Se han corregido {count} eventos. Las fechas se han asignado a partir del {baseDate:dd/MM/yyyy}.";
            return RedirectToAction(nameof(FixInvalidEvents));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAllInvalidEvents()
        {
            var invalidEvents = await _context.Events
           .Include(e => e.EventClubs)
              .Include(e => e.EventAttendances)
              .Where(e => e.StartDate == DateTime.MinValue || e.StartDate.Year < 2000)
         .ToListAsync();

            if (!invalidEvents.Any())
            {
                TempData["Warning"] = "No hay eventos con fechas inválidas para eliminar.";
                return RedirectToAction(nameof(FixInvalidEvents));
            }

            int count = invalidEvents.Count;
            _context.Events.RemoveRange(invalidEvents);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Se han eliminado {count} eventos con fechas inválidas.";
            return RedirectToAction(nameof(FixInvalidEvents));
        }

        // ========================================
        // GESTIÓN DE USUARIOS Y ROLES
        // ========================================

        /// <summary>
        /// Vista principal para gestionar usuarios y roles
        /// </summary>
        public async Task<IActionResult> ManageUsers(string? returnUrl = null)
        {
            var users = await _context.Users
               .Include(u => u.Department)
                 .AsNoTracking()
                    .OrderBy(u => u.FullName)
                .ToListAsync();

            var userList = new List<UserListVM>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                userList.Add(new UserListVM
                {
                    Id = u.Id,
                    Email = u.Email ?? string.Empty,
                    FullName = u.FullName,
                    Roles = roles.ToList(),
                    DepartmentName = u.Department?.Name
                });
            }

            // Preparar datos para dropdowns
            ViewData["Departments"] = await _context.Departments
     .OrderBy(d => d.Name)
       .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name })
      .ToListAsync();

            ViewData["Clubs"] = await _context.Clubs
           .Include(c => c.Department)
         .OrderBy(c => c.Name)
      .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name + " (" + c.Department.Name + ")" })
                .ToListAsync();

            // Filtrar usuarios para los dropdowns:
            // - Excluir administradores (System Administrator)
            // - Solo mostrar usuarios con rol Student o sin roles (candidatos a promoción)
            var eligibleForManager = new List<SelectListItem>();
            var eligibleForClubLeader = new List<SelectListItem>();

            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);

                // Excluir administradores del sistema
                if (roles.Contains(RoleHelper.Admin))
                    continue;

                // Para Manager de Departamento: usuarios con rol Student, DepartmentManager, o sin roles
                // (excluir usuarios genéricos sin email válido o con nombres genéricos)
                if (!string.IsNullOrEmpty(u.Email) && u.Email.Contains("@"))
                {
                    // Candidatos para DepartmentManager
                    if (roles.Contains(RoleHelper.Student) || roles.Contains(RoleHelper.DepartmentManager) || !roles.Any())
                    {
                        eligibleForManager.Add(new SelectListItem
                        {
                            Value = u.Id.ToString(),
                            Text = $"{u.FullName} ({u.Email})" + (roles.Any() ? $" [{string.Join(", ", roles)}]" : " [Sin rol]")
                        });
                    }

                    // Candidatos para ClubLeader: estudiantes o ya líderes de club
                    if (roles.Contains(RoleHelper.Student) || roles.Contains(RoleHelper.ClubLeader) || !roles.Any())
                    {
                        eligibleForClubLeader.Add(new SelectListItem
                        {
                            Value = u.Id.ToString(),
                            Text = $"{u.FullName} ({u.Email})" + (roles.Any() ? $" [{string.Join(", ", roles)}]" : " [Sin rol]")
                        });
                    }
                }
            }

            ViewData["EligibleForManager"] = eligibleForManager.OrderBy(x => x.Text).ToList();
            ViewData["EligibleForClubLeader"] = eligibleForClubLeader.OrderBy(x => x.Text).ToList();
            ViewData["AllRoles"] = new[] { RoleHelper.Admin, RoleHelper.DepartmentManager, RoleHelper.ClubLeader, RoleHelper.Student };
            ViewData["ReturnUrl"] = returnUrl;

            return View(userList);
        }

        /// <summary>
        /// Actualiza los roles de un usuario (AJAX-friendly)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserRoles(int userId, List<string> roles, string? returnUrl = null)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                TempData["Error"] = "Usuario no encontrado.";
                return RedirectToAction(nameof(ManageUsers), new { returnUrl });
            }

            var allRoles = new[] { RoleHelper.Admin, RoleHelper.DepartmentManager, RoleHelper.ClubLeader, RoleHelper.Student };

            // Asegurar que los roles existen
            foreach (var r in allRoles)
            {
                if (!await _roleManager.RoleExistsAsync(r))
                    await _roleManager.CreateAsync(new IdentityRole<int>(r));
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var toRemove = currentRoles.Where(r => !roles.Contains(r)).ToList();
            var toAdd = roles.Where(r => !currentRoles.Contains(r)).ToList();

            if (toRemove.Any())
                await _userManager.RemoveFromRolesAsync(user, toRemove);
            if (toAdd.Any())
                await _userManager.AddToRolesAsync(user, toAdd);

            TempData["Success"] = $"Roles de '{user.FullName}' actualizados correctamente.";
            return RedirectToAction(nameof(ManageUsers), new { returnUrl });
        }

        /// <summary>
        /// Asigna un usuario como manager de un departamento
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetDepartmentManager(int userId, int departmentId, string? returnUrl = null)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            var department = await _context.Departments.FindAsync(departmentId);

            if (user == null || department == null)
            {
                TempData["Error"] = "Usuario o departamento no encontrado.";
                return RedirectToAction(nameof(ManageUsers), new { returnUrl });
            }

            department.ManagerUserId = user.Id;
            await _context.SaveChangesAsync();

            // Asegurar que tiene el rol
            if (!await _userManager.IsInRoleAsync(user, RoleHelper.DepartmentManager))
                await _userManager.AddToRoleAsync(user, RoleHelper.DepartmentManager);

            TempData["Success"] = $"'{user.FullName}' es ahora el manager del departamento '{department.Name}'.";
            return RedirectToAction(nameof(ManageUsers), new { returnUrl });
        }

        /// <summary>
        /// Asigna un usuario como líder de un club
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetClubLeader(int userId, int clubId, string? returnUrl = null)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            var club = await _context.Clubs.Include(c => c.Department).FirstOrDefaultAsync(c => c.Id == clubId);

            if (user == null || club == null)
            {
                TempData["Error"] = "Usuario o club no encontrado.";
                return RedirectToAction(nameof(ManageUsers), new { returnUrl });
            }

            club.LeaderId = user.Id;
            await _context.SaveChangesAsync();

            // Asegurar que tiene el rol
            if (!await _userManager.IsInRoleAsync(user, RoleHelper.ClubLeader))
                await _userManager.AddToRoleAsync(user, RoleHelper.ClubLeader);

            TempData["Success"] = $"'{user.FullName}' es ahora el líder del club '{club.Name}'.";
            return RedirectToAction(nameof(ManageUsers), new { returnUrl });
        }

        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.AsNoTracking().ToListAsync();
            var list = new List<UserListVM>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                list.Add(new UserListVM
                {
                    Id = u.Id,
                    Email = u.Email ?? string.Empty,
                    FullName = u.FullName,
                    Roles = roles.ToList()
                });
            }
            return View(list);
        }

        public async Task<IActionResult> AssignRole(string userId)
        {
            if (!int.TryParse(userId, out var id)) return BadRequest();
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();
            var current = await _userManager.GetRolesAsync(user);
            var vm = new AssignRoleVM
            {
                UserId = user.Id,
                Email = user.Email!,
                SelectedRoles = current.ToList()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(string userId, List<string> roles)
        {
            if (!int.TryParse(userId, out var id)) return BadRequest();
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();

            var allRoles = new[] { RoleHelper.Admin, RoleHelper.DepartmentManager, RoleHelper.ClubLeader, RoleHelper.Student };
            // Ensure roles exist
            foreach (var r in allRoles)
                if (!await _roleManager.RoleExistsAsync(r))
                    await _roleManager.CreateAsync(new IdentityRole<int>(r));

            var current = await _userManager.GetRolesAsync(user);
            var toRemove = current.Where(r => !roles.Contains(r));
            var toAdd = roles.Where(r => !current.Contains(r));

            if (toRemove.Any()) await _userManager.RemoveFromRolesAsync(user, toRemove);
            if (toAdd.Any()) await _userManager.AddToRolesAsync(user, toAdd);

            TempData["Success"] = "Roles actualizados";
            return RedirectToAction(nameof(Users));
        }


        public async Task<IActionResult> AssignDepartmentManager()
        {
            var vm = new AssignDepartmentManagerVM
            {
                Departments = await _context.Departments.OrderBy(d => d.Name)
            .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name }).ToListAsync(),
                Users = await _context.Users.OrderBy(u => u.FullName)
            .Select(u => new SelectListItem { Value = u.Id.ToString(), Text = u.FullName + " (" + u.Email + ")" }).ToListAsync()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignDepartmentManager(string userId, int departmentId)
        {
            if (!int.TryParse(userId, out var id)) return BadRequest();
            var dep = await _context.Departments.FindAsync(departmentId);
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (dep == null || user == null) return NotFound();

            dep.ManagerUserId = user.Id;
            await _context.SaveChangesAsync();

            // Ensure role
            if (!await _userManager.IsInRoleAsync(user, RoleHelper.DepartmentManager))
                await _userManager.AddToRoleAsync(user, RoleHelper.DepartmentManager);

            TempData["Success"] = "Department Manager asignado";
            return RedirectToAction(nameof(Users));
        }

        public async Task<IActionResult> AssignClubLeader()
        {
            var vm = new AssignClubLeaderVM
            {
                Clubs = await _context.Clubs.OrderBy(c => c.Name)
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToListAsync(),
                Users = await _context.Users.OrderBy(u => u.FullName)
            .Select(u => new SelectListItem { Value = u.Id.ToString(), Text = u.FullName + " (" + u.Email + ")" }).ToListAsync()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignClubLeader(string userId, int clubId)
        {
            if (!int.TryParse(userId, out var id)) return BadRequest();
            var club = await _context.Clubs.FindAsync(clubId);
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (club == null || user == null) return NotFound();

            club.LeaderId = user.Id;
            await _context.SaveChangesAsync();

            if (!await _userManager.IsInRoleAsync(user, RoleHelper.ClubLeader))
                await _userManager.AddToRoleAsync(user, RoleHelper.ClubLeader);

            TempData["Success"] = "Club Leader asignado";
            return RedirectToAction(nameof(Users));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            if (!int.TryParse(userId, out var id)) return BadRequest();
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                TempData["Error"] = string.Join(", ", result.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Users));
            }

            TempData["Success"] = "Usuario eliminado";
            return RedirectToAction(nameof(Users));
        }
    }
}
