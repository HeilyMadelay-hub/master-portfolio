namespace Ejercicio1.DTOs
{
    public class DispositivoDto
    {
        public int DispositivoId { get; set; }
        public string Nombre { get; set; }
        public List<ReservaDto> Reservas { get; set; }
    }

    public class ReservaDto
    {
        public int ReservaId { get; set; }
        public int DispositivoId { get; set; }
        public string DispositivoNombre { get; set; }
        public int UsuarioId { get; set; }
        public string UsuarioNombre { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Estado => FechaFin >= DateTime.Now ? "Activa" : "Finalizada";
    }

    public class DispositivoSimpleDto
    {
        public int DispositivoId { get; set; }
        public string Nombre { get; set; }
        public int NumeroReservas { get; set; }
    }
}