using Business_School.Models;

namespace Business_School.ViewModels.Dashboard
{
    public class DashboardStudentVM
   {
      // A. Bienvenida personalizada
   public string StudentName { get; set; } = string.Empty;

      // B. Mi progreso (Gamificación)
        public int TotalPoints { get; set; }
        public StudentLevel Level { get; set; }
        public List<StudentAchievementVM> Achievements { get; set; } = new();

        // C. Mis Clubs
        public List<StudentClubInfoVM> MyClubs { get; set; } = new();

    // D. Clubs Recomendados
        public List<RecommendedClubInfoVM> RecommendedClubs { get; set; } = new();

  // E. Próximos eventos
        public List<StudentEventVM> UpcomingEvents { get; set; } = new();

        // F. Mis eventos registrados
        public List<StudentEventVM> MyRegisteredEvents { get; set; } = new();
  }

    public class StudentAchievementVM
    {
        public int Id { get; set; }
  public string Name { get; set; } = string.Empty;
     public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = "bi-trophy"; // Bootstrap icon class
        public DateTime UnlockedAt { get; set; }
    }

    public class StudentClubInfoVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public int MemberCount { get; set; }
        public DateTime JoinedAt { get; set; }
    }

    public class RecommendedClubInfoVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
      public string Description { get; set; } = string.Empty;
     public string DepartmentName { get; set; } = string.Empty;
      public int MatchPercentage { get; set; }
        public string Reason { get; set; } = string.Empty;
   public int MemberCount { get; set; }
    }

    public class StudentEventVM
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public bool IsRegistered { get; set; }
        public int PointsReward { get; set; }
    }
}