using Business_School.Data;
using Business_School.Models;
using Business_School.Services.Recommendation;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Business_School.Services
{
    public class GamificationService : IGamificationService
    {
        private readonly ApplicationDbContext _context;

        public GamificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddPointsForJoiningClubAsync(ApplicationUser student, int points = 50)
        {
            student.Points += points;
            await _context.SaveChangesAsync();
        }

        public StudentLevel CalculateLevel(int totalPoints)
        {
            if (totalPoints < 100) return StudentLevel.Principiante;
            if (totalPoints < 250) return StudentLevel.Intermedio;
            if (totalPoints < 500) return StudentLevel.Avanzado;
            return StudentLevel.Experto;
        }

        public async Task<GamificationProgress> GetProgressAsync(ApplicationUser student, bool isClubLeader)
        {
            var user = await _context.Users
                .Include(u => u.ClubMemberships)
                 .ThenInclude(sc => sc.Club)
        .Include(u => u.EventAttendances)
            .ThenInclude(ep => ep.Event)
                .FirstOrDefaultAsync(u => u.Id == student.Id);


            if (user == null) return null;

            var progress = new GamificationProgress
            {
                Points = user?.Points ?? 0,
                Level = CalculateLevel(user?.Points ?? 0),
                Achievements = new List<string>()
            };



            // Achievements by clubs
            if (user.ClubMemberships?.Count >= 1) progress.Achievements.Add("Primer Club");             
            if (user.ClubMemberships?.Count >= 3) progress.Achievements.Add("Miembro Activo");         // 3 clubs
            if (user.ClubMemberships?.Count >= 5) progress.Achievements.Add("ClubMaster");             // 5 clubs

            // Achievements by events
            int eventosAsistidos = user.EventAttendances?.Count(ep => ep.HasAttended) ?? 0;
            if (eventosAsistidos >= 1) progress.Achievements.Add("Primer Evento");                     
            if (eventosAsistidos >= 5) progress.Achievements.Add("Asistente Frecuente");              // 5 eventos
            if (eventosAsistidos >= 10) progress.Achievements.Add("EventoManíaco");                   // 10 eventos

            //  specials achievments
            if (isClubLeader) progress.Achievements.Add("Líder Emergente");                       // is ClubLeader
            if (progress.Level == StudentLevel.Experto) progress.Achievements.Add("Experto en Gamificación"); // maximum level

            return progress;
        }

        // ? New method to award points for attending events
        public async Task AwardPointsForAttendingEventAsync(ApplicationUser student, Event @event, int? overridePoints = null)
        {
            int points = overridePoints ?? @event.DefaultPointsReward;
            student.Points += points;
            // Update level after awarding points
            student.Level = CalculateLevel(student.Points);
            await _context.SaveChangesAsync();
        }

      
    }
}
