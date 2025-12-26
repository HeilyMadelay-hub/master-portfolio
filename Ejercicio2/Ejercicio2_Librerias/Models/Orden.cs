using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;


namespace Ejercicio2.Models
{
    public class Orden
    {
        //se crea la clase porque representa un conjunto de acciones entre quien la creo y lo que genera
        public int OrdenId { get; set; }

        [Required]//obligatorio porque la orden se pide en un momento y hay que capturarlo
        public DateTime Fecha { get; set; } = DateTime.Now;

        public ICollection<OrdenDetalle> Detalles { get; set; } = new List<OrdenDetalle>();//uno - n porque una orden puede haber muchos detalles
    }
}
