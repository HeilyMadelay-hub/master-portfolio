using System.ComponentModel.DataAnnotations;

namespace Ejercicio2_Librerias.DTO
{

    public class ProductoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int CantidadDisponible { get; set; }
        public decimal Precio { get; set; }
        public bool Disponible { get; set; } // Propiedad calculada
    }

    public class CreateProductoDto
    {
        [Required(ErrorMessage = "El nombre del producto es obligatorio")]
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El nombre solo puede contener letras y espacios")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "La cantidad disponible es obligatoria")]
        [Range(0, int.MaxValue, ErrorMessage = "La cantidad disponible debe ser mayor o igual a cero")]
        public int CantidadDisponible { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal Precio { get; set; }
    }

    public class UpdateProductoDto
    {
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El nombre solo puede contener letras y espacios")]
        public string? Nombre { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "La cantidad disponible debe ser mayor o igual a cero")]
        public int? CantidadDisponible { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal? Precio { get; set; }
    }
} 