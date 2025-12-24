// Business_School.Models.ViewModels/RegisterViewModel.cs
using Business_School.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Business_School.Models.ViewModels
{
    public class RegisterVM
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50, MinimumLength = 2)]
        [Display(Name = "Nombre")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son obligatorios")]
        [StringLength(100, MinimumLength = 2)]
        [Display(Name = "Apellidos")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email obligatorio")]
        [EmailAddress(ErrorMessage = "Email invalido")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, MinimumLength = 8,
            ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debes seleccionar tu nivel académico")]
        [Display(Name = "Nivel académico")]
        public StudentLevel Level { get; set; }

        [Required(ErrorMessage = "Debes pertenecer a un departamento")]
        [Display(Name = "Departamento")]
        public int DepartmentId { get; set; }

        //department drop down 
        public SelectList? DepartmentList { get; set; }

     
        [MustBeTrue(ErrorMessage = "Debes aceptar los términos y condiciones")]
        [Display(Name = "Acepto los términos y condiciones y la política de privacidad")]
        public bool TermsAccepted { get; set; }

        [Display(Name = "Mantener sesión iniciada")]
        public bool RememberMe { get; set; } = true;

        // redirection post register
        public string? ReturnUrl { get; set; }
    }
}

