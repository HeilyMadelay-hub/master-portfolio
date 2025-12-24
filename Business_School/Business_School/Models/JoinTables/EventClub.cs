namespace Business_School.Models.JoinTables
{
    public class EventClub
    {
        public int EventId { get; set; }
        public Event Event { get; set; } = null!;

        public int ClubId { get; set; }
        public Club Club { get; set; } = null!;
    }
}
