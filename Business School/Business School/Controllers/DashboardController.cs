using Business_School.Data;
using Business_School.Helpers;
using Business_School.Models;
using Business_School.Services;
using Business_School.Services.Recommendation;
using Business_School.ViewModels.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Business_School.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IGamificationService _gamificationService;
        private readonly IRecommendationService _recommendationService;

        public DashboardController(ApplicationDbContext db, IGamificationService gamificationService, IRecommendationService recommendationService)
        {
            _db = db;
            _gamificationService = gamificationService;
            _recommendationService = recommendationService;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole(RoleHelper.Admin)) return RedirectToAction(nameof(Admin));
            if (User.IsInRole(RoleHelper.DepartmentManager)) return RedirectToAction(nameof(DepartmentManager));
            if (User.IsInRole(RoleHelper.ClubLeader)) return RedirectToAction(nameof(ClubLeader));
            if (User.IsInRole(RoleHelper.Student)) return RedirectToAction(nameof(Student));
            return View();
        }

        [Authorize(Roles = RoleHelper.Admin)]
        public async Task<IActionResult> Admin()
        {
            // Obtener IDs de usuarios que son Admin para excluirlos del conteo de estudiantes
            var adminRole = await _db.Roles.FirstOrDefaultAsync(r => r.Name == RoleHelper.Admin);
            var adminUserIds = adminRole != null 
                ? await _db.UserRoles.Where(ur => ur.RoleId == adminRole.Id).Select(ur => ur.UserId).ToListAsync()
                : new List<int>();

            var vm = new DashboardAdminVM
            {
                // Contar solo estudiantes reales (excluyendo admins y usuarios genéricos)
                TotalStudents = await _db.Users
                    .Where(u => !adminUserIds.Contains(u.Id))
                    .Where(u => u.DepartmentId != null) // Solo usuarios con departamento
                    .Where(u => !u.FullName.Contains("Usuario General")) // Excluir usuarios de prueba
                    .CountAsync(),
                TotalClubs = await _db.Clubs.CountAsync(),
                TotalDepartments = await _db.Departments.CountAsync(),
                NextEvents = await _db.Events
                    .Where(e => e.StartDate >= DateTime.UtcNow)
                    .OrderBy(e => e.StartDate)
                    .Take(5)
                    .ToListAsync()
            };

            vm.StudentsByDepartment = await _db.Departments
                .Select(d => new { d.Name, Count = d.Students.Count })
                .ToDictionaryAsync(x => x.Name, x => x.Count);

            // Últimos estudiantes: solo estudiantes reales
            // Excluir: Admins, usuarios sin departamento, usuarios con nombre "Usuario General"
            vm.RecentStudents = await _db.Users
                .Include(u => u.Department)
                .Where(u => !adminUserIds.Contains(u.Id)) // Excluir admins
                .Where(u => u.DepartmentId != null) // Solo con departamento asignado
                .Where(u => !u.FullName.Contains("Usuario General")) // Excluir usuarios genéricos
                .Where(u => !u.FullName.Contains("System Administrator")) // Excluir admin del sistema
                .OrderByDescending(u => u.Id)
                .Take(5)
                .ToListAsync();

            return View(vm);
        }

        [Authorize(Roles = RoleHelper.DepartmentManager)]
        public async Task<IActionResult> DepartmentManager()
        {
            // Obtener userId
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            // Traer el departamento con todo lo necesario (clubs y estudiantes)
            var department = await _db.Departments
                .Include(d => d.Clubs)
                    .ThenInclude(c => c.Leader)
                .Include(d => d.Clubs)
                    .ThenInclude(c => c.StudentClubs)
                .Include(d => d.Students)
                    .ThenInclude(s => s.ClubMemberships)
                        .ThenInclude(cm => cm.Club)
                .FirstOrDefaultAsync(d => d.ManagerUserId == userId);

            if (department == null)
                return View(new DashboardDepartmentManagerVM());

            var now = DateTime.UtcNow;

            // Traer eventos del departamento con asistentes
            var events = await _db.Events
                .Where(e => e.DepartmentId == department.Id)
                .Include(e => e.EventAttendances)
                .OrderBy(e => e.StartDate)
                .ToListAsync();

            // Mapear al ViewModel
            var vm = new DashboardDepartmentManagerVM
            {
                // A. Datos del Departamento
                DepartmentId = department.Id,
                DepartmentName = department.Name,
                DepartmentEmail = department.Email,
                DepartmentPhone = department.PhoneNumber,
                DepartmentOfficeLocation = department.OfficeLocation,

                // B. KPIs
                TotalClubs = department.Clubs.Count,
                TotalStudents = department.Students.Count,
                TotalEvents = events.Count,

                // C. Lista de Clubs
                Clubs = department.Clubs.Select(c => new DepartmentClubVM
                {
                    Id = c.Id,
                    Name = c.Name,
                    LeaderName = c.Leader?.FullName ?? "Sin líder",
                    StudentCount = c.StudentClubs.Count
                }).ToList(),

                // D. Lista de Estudiantes
                Students = department.Students.Select(s => new DepartmentStudentVM
                {
                    Id = s.Id,
                    FullName = s.FullName,
                    Email = s.Email ?? string.Empty,
                    Clubs = s.ClubMemberships.Select(cm => cm.Club?.Name ?? "")
                                              .Where(n => !string.IsNullOrEmpty(n))
                                              .ToList(),
                    Points = s.Points,
                    Level = s.Level
                }).ToList(),

                // E. Eventos del Departamento
                Events = events.Select(e => new DepartmentEventVM
                {
                    Id = e.Id,
                    Title = e.Title,
                    StartDate = e.StartDate,
                    Capacity = e.Capacity,
                    RegisteredCount = e.EventAttendances.Count,
                    Status = GetEventStatus(e, now)
                }).ToList()
            };

            // F. Gráficas - Inscripciones por mes (últimos 6 meses)
            var sixMonthsAgo = now.AddMonths(-6);
            var registrations = await _db.EventAttendances
                .Include(ea => ea.Event)
                .Where(ea => ea.Event.DepartmentId == department.Id && ea.RegisteredAt >= sixMonthsAgo)
                .ToListAsync();

            vm.EventRegistrationsByMonth = registrations
                .GroupBy(r => r.RegisteredAt.ToString("MMM yyyy"))
                .OrderBy(g => DateTime.Parse("01 " + g.Key))
                .ToDictionary(g => g.Key, g => g.Count());

            // Top 10 estudiantes por puntos
            vm.TopStudentsByPoints = department.Students
                .OrderByDescending(s => s.Points)
                .Take(10)
                .ToDictionary(s => s.FullName, s => s.Points);

            return View(vm);
        }

        [Authorize(Roles = RoleHelper.ClubLeader)]
        public async Task<IActionResult> ClubLeader()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var club = await _db.Clubs
                .Include(c => c.Department)
                .Include(c => c.StudentClubs).ThenInclude(sc => sc.UserStudent)
                .Include(c => c.EventClubs).ThenInclude(ec => ec.Event).ThenInclude(e => e.EventAttendances)
                .FirstOrDefaultAsync(c => c.LeaderId == userId);

            if (club == null)
            {
                return View(new DashboardClubLeaderVM());
            }

            var now = DateTime.UtcNow;

            var vm = new DashboardClubLeaderVM
            {
                // A. Información del Club
                ClubId = club.Id,
                ClubName = club.Name,
                ClubDescription = club.Description,
                DepartmentName = club.Department?.Name ?? "Sin departamento",
                DepartmentId = club.DepartmentId,

                // B. KPIs
                TotalStudents = club.StudentClubs.Count,
                TotalEvents = club.EventClubs.Count,
                TotalPointsAwarded = club.StudentClubs.Sum(sc => sc.PointsFromThisClub),

                // C. Estudiantes del Club
                Students = club.StudentClubs.Select(sc => new ClubStudentVM
                {
                    Id = sc.UserStudent.Id,
                    FullName = sc.UserStudent.FullName,
                    Email = sc.UserStudent.Email ?? string.Empty,
                    Points = sc.UserStudent.Points,
                    Level = sc.UserStudent.Level,
                    JoinedAt = sc.JoinedAt
                }).OrderByDescending(s => s.Points).ToList(),

                // D. Próximos Eventos del Club
                Events = club.EventClubs
                    .Where(ec => ec.Event != null)
                    .Select(ec => new ClubEventVM
                    {
                        Id = ec.Event.Id,
                        Title = ec.Event.Title,
                        StartDate = ec.Event.StartDate,
                        Capacity = ec.Event.Capacity,
                        RegisteredCount = ec.Event.EventAttendances.Count,
                        Status = GetEventStatus(ec.Event, now)
                    })
                    .OrderBy(e => e.StartDate)
                    .ToList()
            };

            // E. Gráfica - Actividad mensual (inscripciones a eventos del club por mes)
            var sixMonthsAgo = now.AddMonths(-6);
            var clubEventIds = club.EventClubs.Select(ec => ec.EventId).ToList();
            var attendances = await _db.EventAttendances
                .Where(ea => clubEventIds.Contains(ea.EventId) && ea.RegisteredAt >= sixMonthsAgo)
                .ToListAsync();

            vm.MonthlyActivity = attendances
                .GroupBy(a => a.RegisteredAt.ToString("MMM yyyy"))
                .OrderBy(g => DateTime.Parse("01 " + g.Key))
                .ToDictionary(g => g.Key, g => g.Count());

            return View(vm);
        }

        [Authorize(Roles = RoleHelper.Student)]
        public async Task<IActionResult> Student()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var user = await _db.Users
                .Include(u => u.ClubMemberships).ThenInclude(sc => sc.Club).ThenInclude(c => c.Department)
                .Include(u => u.ClubMemberships).ThenInclude(sc => sc.Club).ThenInclude(c => c.StudentClubs)
                .Include(u => u.EventAttendances).ThenInclude(ea => ea.Event).ThenInclude(e => e.Department)
                .Include(u => u.Achievements).ThenInclude(sa => sa.Achievement)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound();

            var now = DateTime.UtcNow;
            var registeredEventIds = user.EventAttendances.Select(ea => ea.EventId).ToHashSet();

            // Get recommended clubs
            var recommendedClubs = await _recommendationService.RecommendClubsAsync(user);

            // Get upcoming events
            var upcomingEvents = await _db.Events
                .Include(e => e.Department)
                .Where(e => e.StartDate >= now)
                .OrderBy(e => e.StartDate)
                .Take(10)
                .ToListAsync();

            var vm = new DashboardStudentVM
            {
                // A. Bienvenida personalizada
                StudentName = user.FullName,

                // B. Mi progreso
                TotalPoints = user.Points,
                Level = user.Level,
                Achievements = user.Achievements.Select(a => new StudentAchievementVM
                {
                    Id = a.Achievement?.Id ?? 0,
                    Name = a.Achievement?.Name ?? "Logro",
                    Description = a.Achievement?.Description ?? "",
                    Icon = GetAchievementIcon(a.Achievement?.Name ?? ""),
                    UnlockedAt = a.EarnedAt
                }).ToList(),

                // C. Mis Clubs
                MyClubs = user.ClubMemberships
                    .Where(cm => cm.Club != null)
                    .Select(cm => new StudentClubInfoVM
                    {
                        Id = cm.Club.Id,
                        Name = cm.Club.Name,
                        DepartmentName = cm.Club.Department?.Name ?? "Sin departamento",
                        MemberCount = cm.Club.StudentClubs.Count,
                        JoinedAt = cm.JoinedAt
                    }).ToList(),

                // D. Clubs Recomendados
                RecommendedClubs = recommendedClubs.Select((c, index) => new RecommendedClubInfoVM
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    DepartmentName = c.Department?.Name ?? "Sin departamento",
                    MatchPercentage = CalculateMatchPercentage(user, c, index),
                    Reason = GetRecommendationReason(user, c),
                    MemberCount = c.StudentClubs.Count
                }).ToList(),

                // E. Próximos eventos
                UpcomingEvents = upcomingEvents.Select(e => new StudentEventVM
                {
                    Id = e.Id,
                    Title = e.Title,
                    StartDate = e.StartDate,
                    DepartmentName = e.Department?.Name ?? "General",
                    IsRegistered = registeredEventIds.Contains(e.Id),
                    PointsReward = e.DefaultPointsReward
                }).ToList(),

                // F. Mis eventos registrados
                MyRegisteredEvents = user.EventAttendances
                    .Where(ea => ea.Event != null && ea.Event.StartDate >= now)
                    .Select(ea => new StudentEventVM
                    {
                        Id = ea.Event.Id,
                        Title = ea.Event.Title,
                        StartDate = ea.Event.StartDate,
                        DepartmentName = ea.Event.Department?.Name ?? "General",
                        IsRegistered = true,
                        PointsReward = ea.Event.DefaultPointsReward
                    })
                    .OrderBy(e => e.StartDate)
                    .ToList()
            };

            return View(vm);
        }

        public async Task<IActionResult> RecommendedClubs()
        {
            return View();
        }

        // Helper methods
        private static string GetEventStatus(Event e, DateTime now)
        {
            if (e.EndDate < now) return "Cerrado";
            if (e.Capacity.HasValue && e.EventAttendances.Count >= e.Capacity.Value) return "Lleno";
            if (e.StartDate > now) return "Abierto";
            return "En curso";
        }

        private static string GetAchievementIcon(string achievementName)
        {
            return achievementName.ToLower() switch
            {
                var n when n.Contains("club") => "bi-people-fill",
                var n when n.Contains("event") => "bi-calendar-event",
                var n when n.Contains("primer") || n.Contains("first") => "bi-star-fill",
                var n when n.Contains("expert") || n.Contains("experto") => "bi-gem",
                var n when n.Contains("leader") || n.Contains("líder") => "bi-person-badge",
                _ => "bi-trophy-fill"
            };
        }

        private static int CalculateMatchPercentage(ApplicationUser user, Club club, int index)
        {
            // Base percentage decreases with position
            var basePercentage = 95 - (index * 10);
                
            // Bonus if same department
            if (user.DepartmentId == club.DepartmentId)
                basePercentage += 5;

            return Math.Max(50, Math.Min(99, basePercentage));
        }

        private static string GetRecommendationReason(ApplicationUser user, Club club)
        {
            if (user.DepartmentId == club.DepartmentId)
                return "Pertenece a tu departamento";
            
            return "Basado en tus intereses";
        }
    }
}
