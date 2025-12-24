using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Business_School.ViewModels.Club
{
    public class ClubEditVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "Nombre no puede ser mas de 100 caracteres")]
        [Display(Name = "Nombre del Club")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Descripcion por encima de los 500 caracteres permitidos")]
        [Display(Name = "Descripcion")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Por favor selecciona un departamento")]
        [Display(Name = "Departamento")]
        public int DepartmentId { get; set; }

        // Nuevo: ID del líder seleccionado
        [Display(Name = "Líder del Club")]
        public int? LeaderId { get; set; }

        // Para el dropdown de líderes
        public SelectList? Leaders { get; set; }

        // Para el dropdown de departamentos
        public SelectList? Departments { get; set; }

        // Editable ahora
        [Display(Name = "Cantidad de Miembros (actual)")]
        public int MemberCount { get; set; }

        // Nueva: capacidad máxima configurable
        [Display(Name = "Capacidad máxima")]
        [Range(1, int.MaxValue, ErrorMessage = "Capacidad debe ser mayor que 0")]
        public int? Capacity { get; set; }
    }
}
