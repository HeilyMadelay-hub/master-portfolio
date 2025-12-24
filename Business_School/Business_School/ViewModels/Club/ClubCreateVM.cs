using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Business_School.ViewModels.Club
{
    public class ClubCreateVM
    {
        [Required(ErrorMessage = "El nombre del club es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar 100 caracteres.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(500, ErrorMessage = "La descripción no puede superar 500 caracteres.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un departamento.")]
        [Display(Name = "Departamento")]
        public int? DepartmentId { get; set; } // nullable para que ModelState no falle automáticamente

        // Dropdown de departamentos: no se debe validar ni enlazar desde el POST
        [ValidateNever]
        public IEnumerable<SelectListItem>? Departments { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
