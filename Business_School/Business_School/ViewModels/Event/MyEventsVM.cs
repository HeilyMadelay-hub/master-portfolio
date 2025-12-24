namespace Business_School.ViewModels.Event
{
    public class MyEventsVM
    {
        public List<EventAttendaceVM> UpcomingEvents { get; set; } = new();
        public List<EventAttendaceVM> PastEvents { get; set; } = new();
    }
}
