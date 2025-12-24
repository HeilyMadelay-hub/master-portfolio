using System.ComponentModel.DataAnnotations;

namespace Business_School.ViewModels.Student
{
    // for Index (admin)
    public class StudentListVM
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

        // Technical Note:
        // In the Student ViewModels, List<string> is used for Clubs for convenience,
        // as we only need to display basic information (club name) without loading
        // the entire Club entity and its relationships, thus avoiding overloading the view.
        // If more club information is required, using a ClubVM sub-ViewModel is recommended.

        public List<string> Clubs { get; set; } = new List<string>();

    }
}
