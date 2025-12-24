namespace Business_School.ViewModels.Event
{
    public class EventListVM
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime StartDate { get; set; }

        public string DepartmentName { get; set; }
        public List<string> Clubs { get; set; } = new();
        public int RegisteredCount { get; set; } // number of students registered
    }

    //We dont put DataAnnotations because these only applys to ViewModels Form -> Create,Edit

}
