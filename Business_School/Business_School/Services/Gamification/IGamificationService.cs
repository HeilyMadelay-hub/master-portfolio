using Business_School.Models;

namespace Business_School.Services.Recommendation
{
  
    public interface IGamificationService
    {

        Task AddPointsForJoiningClubAsync(ApplicationUser student, int points = 50);

        StudentLevel CalculateLevel(int totalPoints);


        Task<GamificationProgress> GetProgressAsync(ApplicationUser student, bool isClubLeader);

        Task AwardPointsForAttendingEventAsync(ApplicationUser student, Event @event, int? overridePoints = null);
    }
}