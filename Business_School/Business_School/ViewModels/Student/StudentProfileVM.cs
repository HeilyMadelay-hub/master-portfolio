using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Business_School.ViewModels.Students
{
    public class StudentProfileVM
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage = "La contraseña y la confirmación no coinciden")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        public int Points { get; set; }

        public string Level { get; set; }

        public List<string> Clubs { get; set; } = new List<string>();

        public List<StudentEventVM> UpcomingEvents { get; set; } = new List<StudentEventVM>();

        public List<StudentAchievementVM> Achievements { get; set; } = new List<StudentAchievementVM>();
    }

    // We reutilise this sub-ViewModels
    public class StudentEventVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Date { get; set; }
        public bool Attended { get; set; }
    }

    public class StudentAchievementVM
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string DateEarned { get; set; }
    }
}
