namespace Business_School.ViewModels.Event
{
    public class EventAttendaceVM
    {
        // Para MyEvents y marcar asistencia - Events Controller
        public int EventId { get; set; }
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public bool HasAttended { get; set; }
        public DateTime? AttendedAt { get; set; }
        public int PointsAwarded { get; set; }
    }
}
