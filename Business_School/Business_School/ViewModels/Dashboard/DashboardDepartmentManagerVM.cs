using Business_School.Models;

namespace Business_School.ViewModels.Dashboard
{
    public class DashboardDepartmentManagerVM
    {
        // A. Datos del Departamento
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string DepartmentEmail { get; set; } = string.Empty;
        public string DepartmentPhone { get; set; } = string.Empty;
        public string DepartmentOfficeLocation { get; set; } = string.Empty;

        // B. KPIs del Departamento
        public int TotalClubs { get; set; }
        public int TotalStudents { get; set; }
        public int TotalEvents { get; set; }

        // C. Lista de Clubs del Departamento
        public List<DepartmentClubVM> Clubs { get; set; } = new();

        // D. Lista de Estudiantes del Departamento
        public List<DepartmentStudentVM> Students { get; set; } = new();

        // E. Eventos del Departamento
        public List<DepartmentEventVM> Events { get; set; } = new();

        // F. Gráficas - Inscripciones a eventos por mes
        public Dictionary<string, int> EventRegistrationsByMonth { get; set; } = new();

        // Gráfica - Actividad de estudiantes (puntos acumulados por estudiante top 10)
        public Dictionary<string, int> TopStudentsByPoints { get; set; } = new();
    }

    public class DepartmentClubVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string LeaderName { get; set; } = string.Empty;
        public int StudentCount { get; set; }
    }

    public class DepartmentStudentVM
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Clubs { get; set; } = new();
        public int Points { get; set; }
        public StudentLevel Level { get; set; }
    }

    public class DepartmentEventVM
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public int? Capacity { get; set; }
        public int RegisteredCount { get; set; }
        public string Status { get; set; } = string.Empty; // Abierto, Cerrado, Lleno

        public string DepartmentName { get; set; } = string.Empty;
        public List<string> ClubNames { get; set; } = new();
    }
}
