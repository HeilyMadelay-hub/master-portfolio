using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Business_School.Models
{
    public class Department
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del departamento es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar 100 caracteres.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [Phone(ErrorMessage = "Número de teléfono no válido.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "Email no válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La ubicación de la oficina es obligatoria.")]
        [StringLength(100, ErrorMessage = "La ubicación no puede superar 100 caracteres.")]
        public string OfficeLocation { get; set; } = string.Empty;

        public int? ManagerUserId { get; set; }
        public ApplicationUser? ManagerUser { get; set; }

        public ICollection<ApplicationUser> Students { get; set; } = new List<ApplicationUser>();
        public ICollection<Club> Clubs { get; set; } = new List<Club>();
        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}

