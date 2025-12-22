using Business_School.Data;
using Business_School.Models;
using Business_School.Models.JoinTables;
using Business_School.Services.Recommendation;
using Business_School.ViewModels.Club;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using Business_School.Helpers; // add helpers for role constants

namespace Business_School.Controllers
{

    /*

    Aprendizaje clave:

    [Required] siempre debe ir en la propiedad que recibe datos del formulario, no en la lista de opciones (SelectList).

    Usa int? para dropdowns obligatorios.

    Para mostrar errores, lo más seguro es iterar directamente sobre ModelState, no usar tipos anónimos.


     */
    [Authorize(Roles = RoleHelper.Admin + "," + RoleHelper.DepartmentManager + "," + RoleHelper.ClubLeader)]
    public class ClubsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IGamificationService _gamificationService;

        public ClubsController(ApplicationDbContext db, IGamificationService gamificationService)
        {
            _db = db;
            _gamificationService = gamificationService;
        }

        private string? NormalizeReturnUrl(string? returnUrl)
        {
            if (string.IsNullOrWhiteSpace(returnUrl)) return null;

            // Accept local relative URLs
            if (Url.IsLocalUrl(returnUrl)) return returnUrl;

            // If absolute but same origin, convert to relative path+query
            if (Uri.TryCreate(returnUrl, UriKind.Absolute, out var absolute))
            {
                var req = HttpContext.Request;
                var sameHost = string.Equals(absolute.Scheme, req.Scheme, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(absolute.Host, req.Host.Host, StringComparison.OrdinalIgnoreCase)
                    && (absolute.Port == req.Host.Port || !absolute.IsDefaultPort && req.Host.Port == null);
                if (sameHost)
                {
                    var relative = absolute.PathAndQuery;
                    if (Url.IsLocalUrl(relative)) return relative;
                }
            }
            // Otherwise, reject
            return null;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var clubs = await _db.Clubs
            .Include(cl => cl.Leader)
            .Include(cl => cl.Department)
            .Include(cl => cl.StudentClubs)
            .AsNoTracking()
            .ToListAsync();
            return View(clubs);
        }


        [HttpGet]
        public async Task<IActionResult> Create(int? departmentId, string? returnUrl = null)
        {
            var departmentsList = await _db.Departments.OrderBy(d => d.Name).ToListAsync();

            var vm = new ClubCreateVM
            {
                Departments = new SelectList(departmentsList, "Id", "Name"),
                DepartmentId = departmentId,
                ReturnUrl = NormalizeReturnUrl(returnUrl)
            };

            // Si el usuario es DepartmentManager, forzamos su propio departamento
            // para que no pueda crear club que no sean de su departamento
            if (User.IsInRole(RoleHelper.DepartmentManager))
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                var managerDeptId = await _db.Departments
                    .Where(d => d.ManagerUserId == userId)
                    .Select(d => d.Id)
                    .FirstOrDefaultAsync();

                vm.DepartmentId = managerDeptId;
            }

            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleHelper.Admin + "," + RoleHelper.DepartmentManager + "," + RoleHelper.ClubLeader)]
        public async Task<IActionResult> Create(ClubCreateVM vm)
        {


            if (User.IsInRole(RoleHelper.DepartmentManager))
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
//this is what is going to have the form
                vm.DepartmentId = await _db.Departments
                    .Where(d => d.ManagerUserId == userId)
                    .Select(d => d.Id)
                    .FirstOrDefaultAsync();
            }


            // Validaciones manuales adicionales
            if (!vm.DepartmentId.HasValue || vm.DepartmentId <= 0)
            {
                ModelState.AddModelError(nameof(vm.DepartmentId), "Seleccione un departamento válido.");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .Select(x => new { x.Key, x.Value.Errors })
                .ToList();

                ViewData["ModelErrors"] = errors;

                await PopulateDepartmentsSelectList(vm);
                return View(vm);
            }

            // Verificar existencia del departamento
            var existsDept = await _db.Departments.AnyAsync(d => d.Id == vm.DepartmentId.Value);
            if (!existsDept)
            {
                ModelState.AddModelError(nameof(vm.DepartmentId), "Departamento no válido.");
                await PopulateDepartmentsSelectList(vm);
                return View(vm);
            }

            // Evitar duplicados por nombre (case-insensitive)
            var normalizedName = vm.Name.Trim().ToLower();
            var duplicate = await _db.Clubs.AnyAsync(c => c.DepartmentId == vm.DepartmentId.Value && c.Name.ToLower() == normalizedName);
            if (duplicate)
            {
                ModelState.AddModelError(nameof(vm.Name), "Ya existe un club con ese nombre en este departamento.");
                await PopulateDepartmentsSelectList(vm);
                return View(vm);
            }

            var club = new Club
            {
                Name = vm.Name.Trim(),
                Description = vm.Description.Trim(),
                DepartmentId = vm.DepartmentId.Value
            };

            try
            {
                _db.Clubs.Add(club);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error al guardar el club: " + ex.Message);
                await PopulateDepartmentsSelectList(vm);
                return View(vm);
            }

            TempData["Success"] = "Club creado correctamente.";

            var normalizedReturn = NormalizeReturnUrl(vm.ReturnUrl);
            if (!string.IsNullOrEmpty(normalizedReturn))
                return LocalRedirect(normalizedReturn);

            if (User.IsInRole(RoleHelper.DepartmentManager))
                return RedirectToAction("DepartmentManager", "Dashboard");

            if (User.IsInRole(RoleHelper.ClubLeader))
            {
                return RedirectToAction("ClubLeader", "Dashboard");
            }

            return RedirectToAction(nameof(Index));
        }

        // Método privado para rellenar el dropdown de departamentos
        private async Task PopulateDepartmentsSelectList(ClubCreateVM vm)
        {
            vm.Departments = new SelectList(await _db.Departments.OrderBy(d => d.Name).ToListAsync(), "Id", "Name", vm.DepartmentId);
        }

        // Método privado para obtener la SelectList de líderes excluyendo emails y Admins
        private async Task<SelectList> BuildLeadersSelectAsync(int? selectedLeaderId)
        {
            var excludedEmails = new[] { "user01@businessschool.com", "user02@businessschool.com", "user03@businessschool.com" };
            var adminRole = await _db.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            var adminUserIds = new List<int>();
            if (adminRole != null)
            {
                adminUserIds = await _db.UserRoles
                .Where(ur => ur.RoleId == adminRole.Id)
                .Select(ur => ur.UserId)
                .ToListAsync();
            }

            var users = await _db.Users
                .Where(u => !excludedEmails.Contains(u.UserName.ToLower()) && !adminUserIds.Contains(u.Id))
                .OrderBy(u => u.Email)
                .ToListAsync();

            return new SelectList(users, "Id", "Email", selectedLeaderId);
        }





        [HttpGet]
        [Authorize(Roles = RoleHelper.Admin + "," + RoleHelper.DepartmentManager + "," + RoleHelper.ClubLeader)]
        public async Task<IActionResult> Edit(int? id, string? returnUrl = null)
        {
            if (id == null) return NotFound();
            var club = await _db.Clubs
            .Include(c => c.Leader)
            .Include(c => c.StudentClubs).ThenInclude(sc => sc.UserStudent)
            .Include(c => c.EventClubs).ThenInclude(ec => ec.Event)
            .FirstOrDefaultAsync(c => c.Id == id);
            if (club == null) return NotFound();

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = int.Parse(userIdStr);

            if (User.IsInRole(RoleHelper.ClubLeader) && club.LeaderId != userId)
            {
                TempData["Error"] = "Solo puedes editar tu propio club";
                return RedirectToAction(nameof(Index));
            }

            var leadersSelect = await BuildLeadersSelectAsync(club.LeaderId);

            var vm = new ClubEditVM
            {
                Id = club.Id,
                Name = club.Name,
                Description = club.Description,
                DepartmentId = club.DepartmentId,
                LeaderId = club.LeaderId,
                Leaders = leadersSelect,
                MemberCount = club.StudentClubs.Count,
                Capacity = club.Capacity,
                Departments = new SelectList(await _db.Departments.OrderBy(d => d.Name).ToListAsync(), "Id", "Name", club.DepartmentId)
            };
            ViewData["ReturnUrl"] = returnUrl;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleHelper.Admin + "," + RoleHelper.DepartmentManager + "," + RoleHelper.ClubLeader)]
        public async Task<IActionResult> Edit(int id, ClubEditVM model, string? returnUrl = null)
        {
            if (id != model.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                // Recargar dropdowns con el mismo filtro que el GET
                model.Departments = new SelectList(await _db.Departments.OrderBy(d => d.Name).ToListAsync(), "Id", "Name", model.DepartmentId);
                model.Leaders = await BuildLeadersSelectAsync(model.LeaderId);
                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
            }

            var club = await _db.Clubs.Include(c => c.StudentClubs).FirstOrDefaultAsync(c => c.Id == id);
            if (club == null) return NotFound();

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = int.Parse(userIdStr);
            if (User.IsInRole(RoleHelper.ClubLeader) && club.LeaderId != userId)
            {
                TempData["Error"] = "Solo puedes editar tu propio club";
                return RedirectToAction(nameof(Index));
            }

            club.Name = model.Name.Trim();
            club.Description = model.Description?.Trim() ?? string.Empty;
            club.DepartmentId = model.DepartmentId;
            club.LeaderId = model.LeaderId;
            // Update capacity (max seats)
            club.Capacity = model.Capacity;
            // Do not manipulate StudentClubs here; members are managed via JoinClub/LeaveClub respecting capacity

            await _db.SaveChangesAsync();

            TempData["Success"] = "¡Club actualizado!";


            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var club = await _db.Clubs
            .Include(c => c.Department)
            .Include(c => c.Leader)
            .Include(c => c.StudentClubs).ThenInclude(sc => sc.UserStudent) // load users
            .Include(c => c.EventClubs).ThenInclude(ec => ec.Event) // optional: show events
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

            if (club == null)
                return NotFound();

            // Remove placeholder memberships (those without a user)
            club.StudentClubs = club.StudentClubs.Where(sc => sc.UserStudent != null).ToList();

            return View(club);


        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {

            var club = await _db.Clubs
            .Include(c => c.Department)
            .Include(c => c.Leader)
            .Include(c => c.StudentClubs)
                .ThenInclude(sc => sc.UserStudent)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

            if (club == null)
                return NotFound();

            _db.Clubs.Remove(club);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Club '{club.Name}' borrado exactamente";
            return RedirectToAction("Index");


        }

        //url patron
        [HttpGet]
        public async Task<IActionResult> Details(int? id, string? returnUrl = null)
        {
            if (id == null)
                return NotFound();

            var club = await _db.Clubs
            .Include(c => c.Department)
            .Include(c => c.Leader)
            .Include(c => c.StudentClubs)
            .ThenInclude(sc => sc.UserStudent)
            .Include(c => c.EventClubs)
            .ThenInclude(ec => ec.Event)
            .FirstOrDefaultAsync(c => c.Id == id);


            if (club == null)
                return NotFound();

            // Check if current user is a member
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("Student"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Busca al usuario actual
                var currentUser = await _db.Users
                .FirstOrDefaultAsync(u => u.Id.ToString() == userId);

                if (currentUser != null)
                {
                    // Verifica si el usuario ya es miembro del club
                    ViewData["IsMember"] = club.StudentClubs
                    .Any(sc => sc.UserStudentId == currentUser.Id);
                }
            }
            string? normalizedReturn = null;

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                normalizedReturn = returnUrl;
            }

            ViewData["ReturnUrl"] = normalizedReturn;

            return View(club);



        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> JoinClub(int? clubId)
        {
            if (clubId == null) return BadRequest();

            // Parse user id (IdentityUser<int>)
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
                return Forbid();

            // Verify club exists
            var club = await _db.Clubs
                .Include(c => c.StudentClubs)
                .FirstOrDefaultAsync(c => c.Id == clubId.Value);
            if (club == null)
            {
                TempData["Error"] = "Club no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            // Enforce capacity if set
            if (club.Capacity.HasValue && club.StudentClubs.Count >= club.Capacity.Value)
            {
                TempData["Error"] = "El club ha alcanzado su capacidad máxima.";
                return RedirectToAction(nameof(Details), new { id = clubId.Value });
            }

            // Load user and memberships
            var user = await _db.Users
            .Include(u => u.ClubMemberships)
            .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                TempData["Error"] = "Perfil no existe.";
                return RedirectToAction(nameof(Index));
            }

            // Already a member?
            var alreadyMember = user.ClubMemberships.Any(sc => sc.ClubId == clubId.Value);
            if (alreadyMember)
            {
                TempData["Warning"] = "Ya eres miembro de este club.";
                return RedirectToAction(nameof(Details), new { id = clubId.Value });
            }

            // Create relation
            var studentClub = new StudentClub
            {
                UserStudentId = user.Id,
                ClubId = clubId.Value,
                JoinedAt = DateTime.UtcNow,

            };

            _db.StudentClubs.Add(studentClub);
            await _db.SaveChangesAsync();

            // Award points to user (optional gamification)
            int pointsEarned = 50;
            await _gamificationService.AddPointsForJoiningClubAsync(user, pointsEarned);

            TempData["Success"] = $"Te has unido a '{club.Name}' y has ganado {pointsEarned} puntos!";

            return RedirectToAction(nameof(Details), new { id = clubId.Value });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> LeaveClub(int? clubId)
        {
            if (clubId == null) return BadRequest();
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Forbid();

            var user = await _db.Users.Include(u => u.ClubMemberships).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                TempData["Error"] = "Perfil del estudiante no encontrado";
                return RedirectToAction(nameof(Index));
            }
            var membership = user.ClubMemberships.FirstOrDefault(sc => sc.ClubId == clubId.Value);
            if (membership == null)
            {
                TempData["Warning"] = "No eres miembro de este club.";
                return RedirectToAction(nameof(Details), new { id = clubId.Value });
            }
            var pointsToRemove = membership.PointsFromThisClub;
            user.Points = Math.Max(0, user.Points - pointsToRemove);
            _db.StudentClubs.Remove(membership);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Has dejado este club.";
            return RedirectToAction(nameof(MyClubs));
        }

        [HttpGet]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> MyClubs()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Forbid();
            var memberships = await _db.StudentClubs
            .Include(sc => sc.Club).ThenInclude(c => c.Department)
            .Where(sc => sc.UserStudentId == userId)
            .ToListAsync();
            var membershipsVM = memberships.Select(sc => new StudentClubVM
            {
                ClubId = sc.ClubId,
                ClubName = sc.Club.Name,
                Description = sc.Club.Description,
                DepartmentName = sc.Club.Department.Name,
                JoinedAt = sc.JoinedAt,
                PointsFromThisClub = sc.PointsFromThisClub
            }).ToList();
            return View(membershipsVM);
        }

        [HttpGet]
        [Authorize(Roles = "ClubLeader")]
        public async Task<IActionResult> MyClub()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Forbid();
            var club = await _db.Clubs
            .Include(c => c.StudentClubs).ThenInclude(sc => sc.UserStudent)
            .FirstOrDefaultAsync(c => c.LeaderId == userId);
            if (club == null)
            {
                TempData["Error"] = "No eres líder de ningún club";
                return RedirectToAction(nameof(Index));
            }
            return View(club);
        }

        [HttpGet]
        [Authorize(Roles = "ClubLeader")]
        public async Task<IActionResult> Students()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Forbid();
            var club = await _db.Clubs
            .Include(c => c.StudentClubs).ThenInclude(sc => sc.UserStudent)
            .FirstOrDefaultAsync(c => c.LeaderId == userId);
            if (club == null)
            {
                TempData["Error"] = "No eres líder de ningún club.";
                return RedirectToAction(nameof(Index));
            }
            var students = club.StudentClubs.Select(sc => sc.UserStudent).ToList();
            return View(students);
        }
    }
}