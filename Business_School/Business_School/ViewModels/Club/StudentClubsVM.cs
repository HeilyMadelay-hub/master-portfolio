using System.ComponentModel.DataAnnotations;

namespace Business_School.ViewModels.Club
{
    //For MyClubs

    public class StudentClubVM
    {
        [Required]
        public int ClubId { get; set; }

        [Required(ErrorMessage = "El nombre del club es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre del club no puede superar los 100 caracteres.")]
        public string ClubName { get; set; }

        [StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "El departamento es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre del departamento no puede superar los 100 caracteres.")]
        public string DepartmentName { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de unión")]
        public DateTime JoinedAt { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Los puntos deben ser un número positivo.")]
        [Display(Name = "Puntos de este club")]
        public int PointsFromThisClub { get; set; }
    }
}
