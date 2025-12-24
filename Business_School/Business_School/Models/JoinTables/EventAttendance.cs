namespace Business_School.Models.JoinTables
{
    public class EventAttendance
    {
        public int EventId { get; set; }
        public Event Event { get; set; } = null!;

        public int UserStudentId { get; set; }
        public ApplicationUser UserStudent { get; set; } = null!;

        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
        public bool HasAttended { get; set; } = false;
        public DateTime? AttendedAt { get; set; }
        public int PointsAwarded { get; set; } = 0;
    }
}
