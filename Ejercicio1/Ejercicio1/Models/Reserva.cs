using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;
using System.Runtime.ConstrainedExecution;

namespace Ejercicio1.Models
{
    public class Reserva
    {
        public int ReservaId { get; set; } // No se pone Key porque EF lo entiende por convencion


        //la relacion con dispositvo

        [Required]//Porque no puede existir una reserva sin dispositivo
        public int DispositivoId { get; set; }
        public Dispositivo? Dispositivo { get; set; }

        //clase usuario en vez de string para definir la relacion mas completa
        //con un id nos aseguramos los datos y tambien required porque alguien habra que tenido hacer una reserva entonces para llevar un registro
        [Required]
        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }
        /*
         
        El ? pertenece a los Nullable Reference Types (NRT) de C# e indica que la propiedad de
        navegación puede ser null en memoria, aunque la relación exista en la base de datos, evitando 
        que MVC la valide implícitamente como [Required] cuando no se envía en el POST y así ModelState.
        IsValid no falle. Con el ? la propiedad “puede ser null en este contexto”, pero UsuarioId sigue 
        siendo obligatorio, y esto no significa que la reserva pueda no tener usuario ni que la relación
        sea opcional en la base de datos, ya que la relación real la controla el UsuarioId. Marcarlas 
        como nullable permite evitar errores de validación en MVC sin afectar la integridad de la 
        relación en EF Core.
         
         
         
         */

        //required ambos porque la reserva necesita unas fechas para indicar cuando vence el formato sera controlado en la vista
        [Required]
        public DateTime FechaInicio { get; set; }
       
        [Required]
        public DateTime FechaFin { get; set; }


    }
}
