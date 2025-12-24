using Business_School.Models;
using System.ComponentModel.DataAnnotations;
// For Details (admin)

namespace Business_School.ViewModels.Students
{
    public class StudentDetailsVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre completo no puede superar los 100 caracteres.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Debe ser un correo electrónico válido.")]
        public string Email { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Los puntos no pueden ser negativos.")]
        public int Points { get; set; }

        public int? DepartmentId { get; set; } // department's id of the first  club


        [StringLength(50)]
        public StudentLevel Level { get; set; }

        // List of club names (read-only)
        public List<string> Clubs { get; set; } = new List<string>();

        // Sub-ViewModel for events
        public List<EventVM> Events { get; set; } = new List<EventVM>();

        // Sub-ViewModel for achievements
        public List<AchievementVM> Achievements { get; set; } = new List<AchievementVM>();
    }

    public class EventVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del evento es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre del evento no puede superar los 100 caracteres.")]
        public string Name { get; set; }

        public DateTime Date { get; set; }

        public bool Attended { get; set; }
    }

    public class AchievementVM
    {
        [Required(ErrorMessage = "El título del logro es obligatorio.")]
        [StringLength(100, ErrorMessage = "El título del logro no puede superar los 100 caracteres.")]
        public string Title { get; set; }

        [StringLength(300, ErrorMessage = "La descripción del logro no puede superar los 300 caracteres.")]
        public string Description { get; set; }

        public DateTime DateEarned { get; set; }
    }
}
