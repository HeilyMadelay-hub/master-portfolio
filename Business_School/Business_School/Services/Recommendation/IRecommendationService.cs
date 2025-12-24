using Business_School.Models;

namespace Business_School.Services.Recommendation
{
    // The controller asks to the interface for something it needs
    // and the interface defines what methods are available.
    // That's why they are always together in th same folder 
    // This interface declares what methods will implement
    public interface IRecommendationService
    {
        Task<List<Club>> RecommendClubsAsync(ApplicationUser student, int maxResults = 5);
    }
}