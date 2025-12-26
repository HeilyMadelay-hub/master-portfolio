using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Ejercicio2.Models
{
    public class OrdenDetalle
    {
        //representa los productos que forman parte de la orden cada linea de la orden es una clase con su producto y cantidas

        public int OrdenDetalleId { get; set; }

        //obligatorio por la relacion porque necesitas una orden para un producto
        [Required]
        public int ProductoId { get; set; }
        public Producto Producto { get; set; }


        //pero todos esos detalles pertenecen a una orden por eso aqui va el uno
        public int OrdenId { get; set; }
        public Orden Orden { get; set; }


        //obligatorio y con un rango max porque necesitas ordenar una cantidad por logica pero revisar la cantidad que ordenas para que no te gastes el producto esto se hace en servicio porque es logica de negocio
        [Required, Range(1, int.MaxValue)]
        public int Cantidad { get; set; }

    
    }

}
