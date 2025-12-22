using Business_School.Data;
using Business_School.Helpers;
using Business_School.Models;
using Business_School.Models.JoinTables;
using Business_School.Services.Recommendation;
using Business_School.ViewModels.Event;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Business_School.Controllers
{
    [Authorize]
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IGamificationService _gamificationService; // ? injected
        private readonly UserManager<ApplicationUser> _userManager;

        public EventsController(ApplicationDbContext db, IGamificationService gamificationService, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _gamificationService = gamificationService;
            _userManager = userManager;
        }

        // ajax endpoint ,because return json , doesn't return a view and doesnt reload the page 

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> GetNextEvents()
        {
            var now = DateTime.UtcNow; // Cambiar a UtcNow para consistencia
            var nextEvents = await _db.Events
                .Where(e => e.StartDate >= now)
                .OrderBy(e => e.StartDate)
                .Select(e => new
                {
                    e.Id,
                    e.Title,
                    StartDate = e.StartDate.ToString("dd/MM/yyyy HH:mm")
                })
                .ToListAsync();

            return Json(new
            {
                count = nextEvents.Count,
                events = nextEvents
            });
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

       
        public async Task<IActionResult> Index()
        {
            var events = await _db.Events
            .Include(e => e.Department)
            .Include(e => e.EventClubs).ThenInclude(ec => ec.Club)
            .Include(e => e.EventAttendances)
            .OrderBy(e => e.StartDate)
            .Select(e => new EventListVM
            {
                Id = e.Id,
                Title = e.Title,
                StartDate = e.StartDate,
                DepartmentName = e.Department != null ? e.Department.Name : "Sin departamento",
                Clubs = e.EventClubs.Select(ec => ec.Club.Name).ToList(),
                RegisteredCount = e.EventAttendances.Count
            }).ToListAsync();

            return View(events);
        }

        public async Task<IActionResult> Details(int? id, string? returnUrl = null)
        {
            if (id == null) return NotFound();
            var userId = int.Parse(_userManager.GetUserId(User));
            var ev = await _db.Events
            .Include(e => e.Department)
            .Include(e => e.EventClubs).ThenInclude(ec => ec.Club)
            .Include(e => e.EventAttendances)
            .FirstOrDefaultAsync(e => e.Id == id);
            if (ev == null) return NotFound();

            var normalizedReturn = NormalizeReturnUrl(returnUrl);

            var vm = new EventDetailsVM
            {
                Id = ev.Id,
                Title = ev.Title,
                Description = ev.Description ?? string.Empty,
                StartDate = ev.StartDate,
                DepartmentName = ev.Department?.Name ?? "Sin departamento",
                Clubs = ev.EventClubs.Select(ec => ec.Club.Name).ToList(),
                ClubIds = ev.EventClubs.Select(ec => ec.ClubId).ToList(), // <-- IDs
                MaxCapacity = ev.Capacity ?? 0,
                RegisteredCount = ev.EventAttendances.Count,
                IsUserRegistered = ev.EventAttendances.Any(a => a.UserStudentId == userId),
                ReturnUrl = normalizedReturn
            };

            return View(vm);
        }

        /*
         * When we create an event from the department  dashboard 
         * and you selected edit an event the department selector should not be visible.
         */

        [Authorize(Roles = RoleHelper.Admin + "," + RoleHelper.DepartmentManager + "," + RoleHelper.ClubLeader)]
        public async Task<IActionResult> Create(string? returnUrl = null)
        {
            var vm = new EventFormVM
            {
                Departments = await _db.Departments.
                OrderBy(d => d.Name).
                Select(d => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = d.Id.ToString(), Text = d.Name }).
                ToListAsync(),

                Clubs = await _db.Clubs.OrderBy(c => c.Name).
                Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem 
                { Value = c.Id.ToString(), Text = c.Name }).ToListAsync()
            };


            //if the role is departmentmanager , add the department to the vm  so it will appear automatically in the form.

            if (User.IsInRole(RoleHelper.DepartmentManager))
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                var departmentId = await _db.Departments
                    .Where(d => d.ManagerUserId == userId)
                    .Select(d => d.Id)
                    .FirstOrDefaultAsync();

                vm.DepartmentId = departmentId;
            }



            ViewData["ReturnUrl"] = NormalizeReturnUrl(returnUrl);


            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleHelper.Admin + "," + RoleHelper.DepartmentManager + "," + RoleHelper.ClubLeader)]
        public async Task<IActionResult> Create(EventFormVM model, string? returnUrl = null)
        {
            // If the user is a DepartmentManager, force the DepartmentId to their own department in this way can not create events outside of this appartment 

            if (User.IsInRole(RoleHelper.DepartmentManager))
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

                model.DepartmentId = await _db.Departments
                    .Where(d => d.ManagerUserId == userId)
                    .Select(d => d.Id)
                    .FirstOrDefaultAsync();
            }


            if (!ModelState.IsValid)
            {
                model.Departments = await _db.Departments.OrderBy(d => d.Name).Select(d => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = d.Id.ToString(), Text = d.Name }).ToListAsync();
                model.Clubs = await _db.Clubs.OrderBy(c => c.Name).Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToListAsync();
                ViewData["ReturnUrl"] = NormalizeReturnUrl(returnUrl);
                return View(model);
            }
            var ev = new Event
            {
                Title = model.Title,
                Description = model.Description,
                StartDate = model.StartDate,
                EndDate = model.StartDate.AddHours(2), // default duration
                DepartmentId = model.DepartmentId,
                Capacity = model.MaxCapacity,
                OrganizerId = int.Parse(_userManager.GetUserId(User))
            };
            _db.Events.Add(ev);
            await _db.SaveChangesAsync();
            // Clubs relations
            foreach (var clubId in model.SelectedClubIds.Distinct())
            {
                _db.EventClubs.Add(new EventClub { EventId = ev.Id, ClubId = clubId });
            }
            await _db.SaveChangesAsync();

            var normalizedReturn = NormalizeReturnUrl(returnUrl);
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

        [Authorize(Roles = RoleHelper.Admin + "," + RoleHelper.DepartmentManager + "," + RoleHelper.ClubLeader)]
        public async Task<IActionResult> Edit(int? id, string? returnUrl = null)
        {
            if (id == null) return NotFound();

            //Here you would only load the events of the related department 

            var ev = await _db.Events.Include(e => e.EventClubs).FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null) return NotFound();

            var vm = new EventFormVM
            {
                Id = ev.Id,
                Title = ev.Title,
                Description = ev.Description,
                StartDate = ev.StartDate,
                DepartmentId = ev.DepartmentId ?? 0,
                MaxCapacity = ev.Capacity,
                SelectedClubIds = ev.EventClubs.Select(ec => ec.ClubId).ToList(),
                Departments = await _db.Departments.OrderBy(d => d.Name).Select(d => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = d.Id.ToString(), Text = d.Name }).ToListAsync(),
                Clubs = await _db.Clubs.OrderBy(c => c.Name).Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToListAsync()
            };


            ViewData["ReturnUrl"] = NormalizeReturnUrl(returnUrl);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleHelper.Admin + "," + RoleHelper.DepartmentManager + "," + RoleHelper.ClubLeader)]
        public async Task<IActionResult> Edit(int id, EventFormVM model, string? returnUrl = null)
        {
            if (id != model.Id) return BadRequest();
            var ev = await _db.Events.Include(e => e.EventClubs).FirstOrDefaultAsync(e => e.Id == id);
            if (ev == null) return NotFound();
            if (!ModelState.IsValid)
            {
                model.Departments = await _db.Departments.OrderBy(d => d.Name).Select(d => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = d.Id.ToString(), Text = d.Name }).ToListAsync();
                model.Clubs = await _db.Clubs.OrderBy(c => c.Name).Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToListAsync();
                ViewData["ReturnUrl"] = NormalizeReturnUrl(returnUrl);
                return View(model);
            }
            ev.Title = model.Title;
            ev.Description = model.Description;
            ev.StartDate = model.StartDate;
            ev.EndDate = model.StartDate.AddHours(2);
            ev.DepartmentId = model.DepartmentId;
            ev.Capacity = model.MaxCapacity;
            // Update clubs
            _db.EventClubs.RemoveRange(ev.EventClubs);
            foreach (var clubId in model.SelectedClubIds.Distinct())
            {
                _db.EventClubs.Add(new EventClub { EventId = ev.Id, ClubId = clubId });
            }
            await _db.SaveChangesAsync();

            var normalizedReturn = NormalizeReturnUrl(returnUrl);
            if (!string.IsNullOrEmpty(normalizedReturn))
                return LocalRedirect(normalizedReturn);

            // Fallback por rol
            if (User.IsInRole(RoleHelper.DepartmentManager))
            {
                return RedirectToAction("DepartmentManager", "Dashboard");
            }

            if (User.IsInRole(RoleHelper.ClubLeader))
            {
                return RedirectToAction("ClubLeader", "Dashboard");
            }

            // Admin u otros
            return RedirectToAction("Index");
        }

        [Authorize(Roles = RoleHelper.Admin + "," + RoleHelper.DepartmentManager + "," + RoleHelper.ClubLeader)]
        public async Task<IActionResult> Delete(int? id, string? returnUrl = null)
        {
            if (id == null) return NotFound();
            var ev = await _db.Events.Include(e => e.Department).FirstOrDefaultAsync(e => e.Id == id);
            if (ev == null) return NotFound();

            ViewData["ReturnUrl"] = NormalizeReturnUrl(returnUrl) ?? Url.Action("Index", "Clubs")!;
            return View(ev);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string? returnUrl = null)
        {
            var eventToDelete = await _db.Events
            .Include(e => e.EventClubs)
            .Include(e => e.EventAttendances)
            .FirstOrDefaultAsync(e => e.Id == id);

            if (eventToDelete == null)
            {
                TempData["Error"] = "Evento no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            _db.Events.Remove(eventToDelete);
            await _db.SaveChangesAsync();

            TempData["DeleteSuccess"] = $"Evento '{eventToDelete.Title}' eliminado correctamente.";

            var normalizedReturn = NormalizeReturnUrl(returnUrl);
            if (!string.IsNullOrEmpty(normalizedReturn))
            {
                return LocalRedirect(normalizedReturn);
            }

            return RedirectToAction("Index", "Clubs");
        }

        // ========================================
        // STUDENT ACTIONS
        // ========================================
        [HttpPost]
        [Authorize(Roles = RoleHelper.Student)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(int eventId, string? returnUrl = null)
        {
            var ev = await _db.Events.Include(e => e.EventAttendances).FirstOrDefaultAsync(e => e.Id == eventId);
            if (ev == null) return NotFound();
            var userId = int.Parse(_userManager.GetUserId(User));
            if (ev.Capacity.HasValue && ev.EventAttendances.Count >= ev.Capacity.Value)
            {
                TempData["Error"] = "El evento ha alcanzado su aforo máximo.";
                var norm = NormalizeReturnUrl(returnUrl);
                if (!string.IsNullOrEmpty(norm))
                    return LocalRedirect(norm);
                return RedirectToAction(nameof(Details), new { id = eventId });
            }
            if (ev.EventAttendances.Any(a => a.UserStudentId == userId))
            {
                TempData["Error"] = "Ya estás registrado en este evento.";
                var norm = NormalizeReturnUrl(returnUrl);
                if (!string.IsNullOrEmpty(norm))
                    return LocalRedirect(norm);
                return RedirectToAction(nameof(Details), new { id = eventId });
            }
            _db.EventAttendances.Add(new EventAttendance { EventId = eventId, UserStudentId = userId });
            await _db.SaveChangesAsync();
            TempData["Success"] = "Registro completado.";
            {
                var norm = NormalizeReturnUrl(returnUrl);
                if (!string.IsNullOrEmpty(norm))
                    return LocalRedirect(norm);
            }
            return RedirectToAction(nameof(Details), new { id = eventId });
        }

        [HttpPost]
        [Authorize(Roles = RoleHelper.Student)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAttendance(int eventId, string? returnUrl = null)
        {
            var ev = await _db.Events.Include(e => e.EventAttendances).FirstOrDefaultAsync(e => e.Id == eventId);
            if (ev == null) return NotFound();
            var userId = int.Parse(_userManager.GetUserId(User));
            var attendance = ev.EventAttendances.FirstOrDefault(a => a.UserStudentId == userId);
            if (attendance == null)
            {
                TempData["Error"] = "Debes registrarte antes de marcar asistencia.";
                var norm = NormalizeReturnUrl(returnUrl);
                if (!string.IsNullOrEmpty(norm))
                    return LocalRedirect(norm);
                return RedirectToAction(nameof(Details), new { id = eventId });
            }
            if (attendance.HasAttended)
            {
                TempData["Error"] = "Ya has marcado asistencia.";
                var norm = NormalizeReturnUrl(returnUrl);
                if (!string.IsNullOrEmpty(norm))
                    return LocalRedirect(norm);
                return RedirectToAction(nameof(Details), new { id = eventId });
            }
            attendance.HasAttended = true;
            attendance.AttendedAt = DateTime.UtcNow;
            var student = await _userManager.GetUserAsync(User);
            await _gamificationService.AwardPointsForAttendingEventAsync(student, ev);
            attendance.PointsAwarded = ev.DefaultPointsReward;
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Asistencia registrada. Has ganado {ev.DefaultPointsReward} puntos.";
            var normalizedReturnMark = NormalizeReturnUrl(returnUrl);
            if (!string.IsNullOrEmpty(normalizedReturnMark))
                return LocalRedirect(normalizedReturnMark);
            return RedirectToAction(nameof(MyEvents));
        }

        [Authorize(Roles = RoleHelper.Student)]
        public async Task<IActionResult> MyEvents()
        {
            var userId = int.Parse(_userManager.GetUserId(User));
            var now = DateTime.UtcNow;
            var attendances = await _db.EventAttendances
            .Include(ea => ea.Event)
            .Where(ea => ea.UserStudentId == userId)
            .OrderBy(ea => ea.Event.StartDate)
            .ToListAsync();
            var vm = new MyEventsVM
            {
                UpcomingEvents = attendances.Where(a => a.Event.StartDate >= now).Select(a => new EventAttendaceVM
                {
                    EventId = a.EventId,
                    Title = a.Event.Title,
                    StartDate = a.Event.StartDate,
                    HasAttended = a.HasAttended,
                    AttendedAt = a.AttendedAt,
                    PointsAwarded = a.PointsAwarded
                }).ToList(),
                PastEvents = attendances.Where(a => a.Event.StartDate < now).Select(a => new EventAttendaceVM
                {
                    EventId = a.EventId,
                    Title = a.Event.Title,
                    StartDate = a.Event.StartDate,
                    HasAttended = a.HasAttended,
                    AttendedAt = a.AttendedAt,
                    PointsAwarded = a.PointsAwarded
                }).ToList()
            };
            return View(vm);
        }

        public async Task<IActionResult> Upcoming()
        {
            var now = DateTime.UtcNow;
            var events = await _db.Events
            .Include(e => e.Department)
            .Where(e => e.StartDate >= now)
            .OrderBy(e => e.StartDate)
            .Select(e => new EventListVM
            {
                Id = e.Id,
                Title = e.Title,
                StartDate = e.StartDate,
                DepartmentName = e.Department != null ? e.Department.Name : "Sin departamento",
                Clubs = e.EventClubs.Select(ec => ec.Club.Name).ToList(),
                RegisteredCount = e.EventAttendances.Count
            }).ToListAsync();
            return View(events);
        }
    }
}
