using Business_School.Data;
using Business_School.Helpers;
using Business_School.Models;
using Business_School.Models.JoinTables; // add for StudentClub
using Business_School.ViewModels.Student;
using Business_School.ViewModels.Students;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore; // Required for EF operations
using System.Security.Claims;

namespace Business_School.Controllers
{
    [Authorize(Roles = RoleHelper.Admin + "," + RoleHelper.Student + "," + RoleHelper.DepartmentManager)]
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager; // fix generic to int

        public StudentsController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<int>> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }



        /*
         
        Why we pass the user manager?

        Creating users (CreateAsync)
        Adding roles (AddToRoleAsync)
        Finding users (FindByEmailAsync)
        Password validation
        Password hashing
        Claims, external logins, etc.
        can only be performed correctly using **UserManager**.

        The DbContext DOES NOT manage:
        - Password Hashing

        - Security Tokens

        - Claims

        - Identity Rules

        - Roles assigned by Identity

        Therefore, when using Identity, you **must never** save users directly using DbContext.
        
         */

        [Authorize(Roles = RoleHelper.Admin)]
        public async Task<IActionResult> Index(string? returnUrl)//We dont pass nothing because this method will show a list there is not a preselected student
        {
            var excludedEmails = new[] { "user01@businessschool.com", "user02@businessschool.com", "user03@businessschool.com", "admin@businessschool.com" }; // added admin to exclusion list

            var students = await _db.Users
            .Include(s => s.ClubMemberships)
            .ThenInclude(sc => sc.Club)
            .AsNoTracking()
            .Where(u => !excludedEmails.Contains(u.Email))
            .ToListAsync();

            // Map to ViewModel
            var studentVMs = students.Select(s => new StudentListVM
            {
                Id = s.Id,
                FullName = $"{s.FullName}",
                Email = s.Email,
                Points = s.Points,
                Clubs = s.ClubMemberships.Select(sc => sc.Club.Name).ToList()

            }).ToList();

            ViewBag.ReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? Url.Action("Admin", "Dashboard") : returnUrl;
            return View(studentVMs);
        }

        [Authorize(Roles = RoleHelper.Admin + "," + RoleHelper.DepartmentManager + "," + RoleHelper.Student)]
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            //Show student details (clubs, events, gamification progress)

            // Check1: Ensure an ID was provided in the request
            if (id == null)
                return NotFound();

            var student = await _db.Users
            .Include(s => s.ClubMemberships)
                .ThenInclude(sc => sc.Club)
            .Include(s => s.EventAttendances)
                .ThenInclude(ea => ea.Event)
            .Include(s => s.Achievements)
                .ThenInclude(sa => sa.Achievement)
            .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
                return NotFound();

            var studentVM = new StudentDetailsVM
            {
                Id = student.Id,
                FullName = student.FullName,
                Email = student.Email,
                Points = student.Points,
                Level = student.Level,
                Clubs = student.ClubMemberships.Select(sc => sc.Club.Name).ToList(),
                Events = student.EventAttendances.Select(ea => new EventVM
                {
                    Id = ea.EventId,
                    Name = ea.Event.Title,
                    Date = ea.Event.StartDate,
                    Attended = ea.HasAttended
                }).ToList(),
                Achievements = student.Achievements.Select(a => new AchievementVM
                {
                    Title = a.Achievement.Name,
                    Description = a.Achievement.Description,
                }).ToList(),

                // Asignar el ID del departamento del primer club (o null si no pertenece a ninguno)
                DepartmentId = student.ClubMemberships
                         .Select(sc => sc.Club.DepartmentId)
                         .FirstOrDefault()
            };


            //Pass the prepared View
            return View(studentVM);
        }

        [Authorize(Roles = RoleHelper.Admin)]
        public IActionResult Create(string? returnUrl)
        {
            var vm = new StudentFormVM
            {
                ReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? Url.Action("Admin", "Dashboard") : returnUrl
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleHelper.Admin)]
        public async Task<IActionResult> Create(StudentFormVM vm, string? returnUrl)
        {
            if (!ModelState.IsValid)
            {
                vm.ReturnUrl = string.IsNullOrWhiteSpace(vm.ReturnUrl) ? (string.IsNullOrWhiteSpace(returnUrl) ? Url.Action("Admin", "Dashboard") : returnUrl) : vm.ReturnUrl;
                return View(vm);
            }

            //1. Create the entity
            var user = new ApplicationUser
            {
                FullName = vm.FullName,
                Email = vm.Email,
                UserName = vm.Email,
                Points = vm.Points,
            };

            //2. Create the user Identity with password
            var result = await _userManager.CreateAsync(user, vm.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                vm.ReturnUrl = string.IsNullOrWhiteSpace(vm.ReturnUrl) ? (string.IsNullOrWhiteSpace(returnUrl) ? Url.Action("Admin", "Dashboard") : returnUrl) : vm.ReturnUrl;
                return View(vm);
            }

            //3. Asign him the role
            await _userManager.AddToRoleAsync(user, RoleHelper.Student);

            TempData["Success"] = "Estudiante creado correctamente.";

            var safeReturn = vm.ReturnUrl ?? returnUrl;//Si vm.ReturnUrl tiene un valor (no es null), úsalo; si es null, usa returnUrl.De esta manera, siempre tienes una URL válida a la que redirigir después del login o acción.Es una forma segura de asignar un valor por defecto si el primero es null.
            if (!string.IsNullOrWhiteSpace(safeReturn) && Url.IsLocalUrl(safeReturn))
            {
                return Redirect(safeReturn);
            }
            return RedirectToAction("Admin", "Dashboard");
        }

        [Authorize(Roles = RoleHelper.Admin)]
        public async Task<IActionResult> Edit(int? id, string? returnUrl)
        {
            // Validate id
            if (id == null)
                return NotFound();

            var user = await _db.Users
                .Include(u => u.ClubMemberships)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                return NotFound();

            var vm = new StudentFormVM
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email!,
                Points = user.Points,
                SelectedClubIds = user.ClubMemberships.Select(sc => sc.ClubId).ToList(),
                Clubs = await _db.Clubs.OrderBy(c => c.Name).Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToListAsync(),
                ReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? Url.Action("Admin", "Dashboard") : returnUrl
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleHelper.Admin)]
        public async Task<IActionResult> Edit(int id, StudentFormVM vm, string? returnUrl)
        {
            if (id != vm.Id)
                return BadRequest();

            ModelState.Remove(nameof(StudentFormVM.Password));
            ModelState.Remove(nameof(StudentFormVM.ConfirmPassword));
            if (!ModelState.IsValid)
            {
                vm.Clubs = await _db.Clubs.OrderBy(c => c.Name).Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToListAsync();
                vm.ReturnUrl = string.IsNullOrWhiteSpace(vm.ReturnUrl) ? (string.IsNullOrWhiteSpace(returnUrl) ? Url.Action("Admin", "Dashboard") : returnUrl) : vm.ReturnUrl;
                return View(vm);
            }

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return NotFound();

            if (!string.Equals(user.Email, vm.Email, StringComparison.OrdinalIgnoreCase))
            {
                user.UserName = vm.Email;
                user.Email = vm.Email;
                var emailResult = await _userManager.UpdateAsync(user);
                if (!emailResult.Succeeded)
                {
                    foreach (var error in emailResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);
                    vm.Clubs = await _db.Clubs.OrderBy(c => c.Name).Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToListAsync();
                    vm.ReturnUrl = string.IsNullOrWhiteSpace(vm.ReturnUrl) ? (string.IsNullOrWhiteSpace(returnUrl) ? Url.Action("Admin", "Dashboard") : returnUrl) : vm.ReturnUrl;
                    return View(vm);
                }
            }

            user.FullName = vm.FullName;
            user.Points = vm.Points;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                vm.Clubs = await _db.Clubs.OrderBy(c => c.Name).Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToListAsync();
                vm.ReturnUrl = string.IsNullOrWhiteSpace(vm.ReturnUrl) ? (string.IsNullOrWhiteSpace(returnUrl) ? Url.Action("Admin", "Dashboard") : returnUrl) : vm.ReturnUrl;
                return View(vm);
            }

            // Update club memberships (many-to-many)
            var memberships = await _db.StudentClubs.Where(sc => sc.UserStudentId == user.Id).ToListAsync();
            var toRemove = memberships.Where(m => !vm.SelectedClubIds.Contains(m.ClubId)).ToList();
            var currentClubIds = memberships.Select(m => m.ClubId).ToHashSet();
            var toAdd = vm.SelectedClubIds.Where(cid => !currentClubIds.Contains(cid)).ToList();

            if (toRemove.Any()) _db.StudentClubs.RemoveRange(toRemove);
            foreach (var cid in toAdd)
            {
                _db.StudentClubs.Add(new StudentClub { UserStudentId = user.Id, ClubId = cid, JoinedAt = DateTime.UtcNow });
            }
            await _db.SaveChangesAsync();

            TempData["Success"] = "Estudiante actualizado correctamente.";
            var safeReturn = vm.ReturnUrl ?? returnUrl;
            if (!string.IsNullOrWhiteSpace(safeReturn) && Url.IsLocalUrl(safeReturn))
            {
                return Redirect(safeReturn);
            }
            return RedirectToAction("Admin", "Dashboard");
        }

        // GET: Student Edit propio
        [Authorize(Roles = RoleHelper.Student)]
        public async Task<IActionResult> EditSelf(string? returnUrl)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var vm = new StudentFormVM
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email!,
                SelectedClubIds = user.ClubMemberships.Select(sc => sc.ClubId).ToList(),
                Clubs = await _db.Clubs.OrderBy(c => c.Name)
                          .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                          .ToListAsync(),
                ReturnUrl = string.IsNullOrWhiteSpace(returnUrl)
                                ? Url.Action("Student", "Dashboard")
                                : returnUrl
            };
            return View(vm); // reutilizamos la misma vista
        }

        // POST: Student Edit propio
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleHelper.Student)]
        public async Task<IActionResult> EditSelf(StudentFormVM vm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.Id != vm.Id) return BadRequest();

            ModelState.Remove(nameof(StudentFormVM.Password));
            ModelState.Remove(nameof(StudentFormVM.ConfirmPassword));

            if (!ModelState.IsValid)
            {
                vm.Clubs = await _db.Clubs.OrderBy(c => c.Name)
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                    .ToListAsync();
                return View(vm);
            }

            // Actualizar datos
            user.FullName = vm.FullName;
            if (!string.Equals(user.Email, vm.Email, StringComparison.OrdinalIgnoreCase))
            {
                user.Email = vm.Email;
                user.UserName = vm.Email;
            }
            await _userManager.UpdateAsync(user);

            // Actualizar clubs
            var memberships = await _db.StudentClubs.Where(sc => sc.UserStudentId == user.Id).ToListAsync();
            var toRemove = memberships.Where(m => !vm.SelectedClubIds.Contains(m.ClubId)).ToList();
            var currentClubIds = memberships.Select(m => m.ClubId).ToHashSet();
            var toAdd = vm.SelectedClubIds.Where(cid => !currentClubIds.Contains(cid)).ToList();

            if (toRemove.Any()) _db.StudentClubs.RemoveRange(toRemove);
            foreach (var cid in toAdd) _db.StudentClubs.Add(new StudentClub { UserStudentId = user.Id, ClubId = cid, JoinedAt = DateTime.UtcNow });
            await _db.SaveChangesAsync();

            TempData["Success"] = "Perfil actualizado correctamente.";
            return Redirect(Url.IsLocalUrl(vm.ReturnUrl) ? vm.ReturnUrl : Url.Action("Student", "Dashboard"));
        }


        [Authorize(Roles = RoleHelper.Admin)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var user = await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();

            // Reuse a lightweight VM for confirmation view
            var vm = new StudentListVM
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email!,
                Points = user.Points,
                Clubs = new List<string>() // optional: could load clubs, but not necessary for delete confirmation
            };

            return View(vm);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleHelper.Admin)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return NotFound();

            // Removing via UserManager ensures Identity cleanup (logins, claims, roles)
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                // In case of failure, show confirmation view again
                var vm = new StudentListVM
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email!,
                    Points = user.Points,
                    Clubs = new List<string>()
                };
                return View("Delete", vm);
            }

            TempData["Success"] = "Estudiante eliminado correctamente.";
            return RedirectToAction("Index", "Students");
        }

        // --- STUDENT FUNCTIONS ---
        [Authorize(Roles = RoleHelper.Student)]
        public async Task<IActionResult> Profile()
        {
            // Get current logged-in user id
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var idInt = int.Parse(userId);

            var user = await _db.Users
                .Include(u => u.ClubMemberships)
                .ThenInclude(sc => sc.Club)
                .Include(u => u.EventAttendances)
                .ThenInclude(ea => ea.Event)
                .Include(u => u.Achievements)
                .ThenInclude(sa => sa.Achievement)
                .FirstOrDefaultAsync(u => u.Id == idInt);

            if (user == null)
                return NotFound();

            var vm = new StudentProfileVM
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email!,
                Points = user.Points,
                Level = user.Level.ToString(),
                Clubs = user.ClubMemberships.Select(sc => sc.Club.Name).ToList(),
                UpcomingEvents = user.EventAttendances.Select(ea => new StudentEventVM
                {
                    Id = ea.EventId,
                    Name = ea.Event.Title,
                    Date = ea.Event.StartDate.ToString("yyyy-MM-dd HH:mm"),
                    Attended = ea.HasAttended
                }).ToList(),
                Achievements = user.Achievements.Select(a => new StudentAchievementVM
                {
                    Title = a.Achievement.Name,
                    Description = a.Achievement.Description,
                    DateEarned = "" // not tracked currently
                }).ToList()
            };

            return View(vm);
        }
    }
}
