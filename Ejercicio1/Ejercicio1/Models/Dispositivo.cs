using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ejercicio1.Models
{
    public class Dispositivo
    {

        public int DispositivoId { get; set; }

        //required y maxlength y validar que lo que meta,meta letras 
        [Required]
        [MaxLength(100)]
        [RegularExpression(@"^[A-Za-z]+( [A-Za-z]+)*$",
                    ErrorMessage = "El nombre solo puede contener letras y un único espacio entre palabras")]
        public string Nombre { get; set; }

        //notMapped porque el atributo tiene que ser calculado en base a las reservas activas y no se guarda en la base de datos porque estar consultando 
        //cada rato es lento
        [NotMapped]
        public bool Disponible =>
        Reservas?.Any(r => r.FechaInicio <= DateTime.Now &&
                           r.FechaFin >= DateTime.Now) != true;

        //la relacion tiene que ser un dispositivo varias reservas.No varias reservas con varios dispositvos por principios de requisitos y lo inicializamos 
        //en list para evitar el nullexception
        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();//Las listas no necesitan ? ,porque si lo pones podria ser null y para acceder a ella tendrias que comprobar que no es null inicianizandola no te tienes que preocupar de null.Siempre habra un objeto aunque este vacio  porque apunta en memoria a un objeto aunque este vacio por eso no hace falta el ?
    }
}
