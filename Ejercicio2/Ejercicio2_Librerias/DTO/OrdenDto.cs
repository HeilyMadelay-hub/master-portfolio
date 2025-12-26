using System.ComponentModel.DataAnnotations;

namespace Ejercicio2_Librerias.DTOs
{
    public class OrdenDto
{
    public int OrdenId { get; set; }
    public DateTime Fecha { get; set; }
    public List<OrdenDetalleDto> Detalles { get; set; }
    public decimal Total { get; set; } // Calculado
}

public class OrdenDetalleDto
{
    public int OrdenDetalleId { get; set; }
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; }
    public decimal PrecioUnitario { get; set; }
    public int Cantidad { get; set; }
    public decimal Subtotal { get; set; } // Calculado
}

public class CreateOrdenDto
{
    [Required(ErrorMessage = "Debe incluir al menos un detalle")]
    [MinLength(1, ErrorMessage = "Debe incluir al menos un producto")]
    public List<CreateOrdenDetalleDto> Detalles { get; set; }
}

public class CreateOrdenDetalleDto
{
    [Required(ErrorMessage = "El ID del producto es obligatorio")]
    [Range(1, int.MaxValue, ErrorMessage = "El ID del producto debe ser mayor a 0")]
    public int ProductoId { get; set; }

    [Required(ErrorMessage = "La cantidad es obligatoria")]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
    public int Cantidad { get; set; }
}
}