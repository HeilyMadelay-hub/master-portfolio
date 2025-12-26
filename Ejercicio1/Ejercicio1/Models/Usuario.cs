using System.ComponentModel.DataAnnotations;

namespace Ejercicio1.Models
{
    public class Usuario
    {
        public int UsuarioId { get; set; }

        //validacion de nombre completo y que lo que meta sea letras por eso regularExpression
        [Required]
        [MaxLength(50)]
        [RegularExpression(@"^[A-Za-z]+( [A-Za-z]+)*$",
            ErrorMessage = "El nombre solo puede contener letras y un unico espacio si tienes dos nombres")]
        public string NombreCompleto { get; set; }

        //No es List es ICollection porque List luego si queremos cambiar la interfaz es mas rigida que ICollection que acepta todo tipo de estructuras
        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

        //Limitar el numero de reservas o metodos como poder reservar o
        //eliminar van en el servicio o repositorio (opcional) no en la entidad para respetar el principio de responsabilidad
    }
}
