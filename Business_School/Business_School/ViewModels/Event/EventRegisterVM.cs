namespace Business_School.ViewModels.Event
{
    public class EventRegisterVM
    {
        public int EventId { get; set; }
        public string EventTitle { get; set; }
        public DateTime StartDate { get; set; }

        public int CurrentRegistrations { get; set; }
        public int? MaxCapacity { get; set; }
    }
    //Only shows info of the event before register in it the validations happens in the method register apart from that
    //is a informative screen before confirm the assitance
}
