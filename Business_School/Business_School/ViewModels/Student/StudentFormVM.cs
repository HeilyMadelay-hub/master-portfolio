using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Business_School.ViewModels.Students
{
    public class StudentFormVM//Create/Edit ->Admin

        //pon urls en lso viewmodels para retornar seguro a la pagina y en la vista lo pasas como un hidden
    {
        public int Id { get; set; } 

        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "La contraseña y la confirmación no coinciden")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Los puntos no pueden ser negativos")]
        public int Points { get; set; } = 0;

        // Clubs selection for Edit
        public List<int> SelectedClubIds { get; set; } = new();
        public IEnumerable<SelectListItem> Clubs { get; set; } = new List<SelectListItem>();

        // Where to go after save/cancel
        public string? ReturnUrl { get; set; }
    }
}
