using Business_School.Data;
using Business_School.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Business_School.Services.Recommendation
{
    //The service implements what the interface has promised 

    public class RecommendationService : IRecommendationService
    {
        private readonly ApplicationDbContext _context;

        public RecommendationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Club>> RecommendClubsAsync(ApplicationUser student, int maxResults = 5)
        {
            var studentClubIds = student.ClubMemberships.Select(sc => sc.ClubId).ToList();

            // Clubs from the same department
            var recommended = await _context.Clubs
                .Include(c => c.Department)
                .Include(c => c.StudentClubs)
                .Where(c => !studentClubIds.Contains(c.Id))
                .OrderByDescending(c => c.StudentClubs.Count)
                .Take(maxResults)
                .ToListAsync();

            return recommended;
        }
    }
}