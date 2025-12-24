using Business_School.Models;

namespace Business_School.ViewModels.Dashboard
{
    public class DashboardClubLeaderVM
    {
    // A. Información del Club
        public int ClubId { get; set; }
   public string ClubName { get; set; } = string.Empty;
      public string ClubDescription { get; set; } = string.Empty;
     public string DepartmentName { get; set; } = string.Empty;
    public int DepartmentId { get; set; }

  // B. KPIs del Club
        public int TotalStudents { get; set; }
      public int TotalEvents { get; set; }
      public int TotalPointsAwarded { get; set; }

        // C. Estudiantes del Club
     public List<ClubStudentVM> Students { get; set; } = new();

  // D. Próximos Eventos del Club
        public List<ClubEventVM> Events { get; set; } = new();

        // E. Gráfica - Actividad mensual del Club
  public Dictionary<string, int> MonthlyActivity { get; set; } = new();
  }

    public class ClubStudentVM
    {
public int Id { get; set; }
      public string FullName { get; set; } = string.Empty;
      public string Email { get; set; } = string.Empty;
        public int Points { get; set; }
        public StudentLevel Level { get; set; }
        public DateTime JoinedAt { get; set; }
  }

    public class ClubEventVM
    {
        public int Id { get; set; }
      public string Title { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public string Status { get; set; } = string.Empty; // Abierto, Cerrado, Lleno
        public int? Capacity { get; set; }
        public int RegisteredCount { get; set; }
    }
}