using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Ejercicio2.Models
{
    public class Producto
    {
        //No ponemos Key porque por convencion entiende que es Id de Producto
        public int Id { get; set; }

        //obligatorio porque necesitas un producto para ordenar,pero en la base de datos el nombre no puede ser muy largo entonces se limita con maxlength y regularexpresion porque un producto no puede tener solo numeros
        [Required(ErrorMessage = "El nombre del producto es obligatorio")]
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "El nombre solo puede contener letras y espacios")]
        public string Nombre { get; set; }

        //obligatorio por cantidad y para que pueda calcular bien los restantes disponibles el numero debe ser mayor a 0 
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "La cantidad disponible debe ser mayor o igual a cero")]
        public int CantidadDisponible { get; set; }

        //añadimos este atributo para luego calcular el total decimal porque tiene mas precision para operaciones que float y double son binarios
        [Required]//otra vez porque todo tiene un precio
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal Precio { get; set; }

        //Esta propiedad es calculada y no se guarda en la base de datos podriamos ponerla aqui pero queda mas limpio en el servicio como en orden hemos explicado
        //public bool Disponible => CantidadDisponible > 0;

        // Relación con órdenes (uno a muchos) con ICollection porque asi tenemos flexibilidad a la hora de la estructura de datos
        public ICollection<OrdenDetalle> OrdenesDetalle { get; set; } = new List<OrdenDetalle>();
    }
}
