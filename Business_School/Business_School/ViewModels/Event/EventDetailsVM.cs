namespace Business_School.ViewModels.Event
{
    public class EventDetailsVM
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public string DepartmentName { get; set; } = "Sin departamento";
        public List<string> Clubs { get; set; } = new();
        public List<int> ClubIds { get; set; } = new();        
        public int? PrimaryClubId => ClubIds.Count == 1 ? ClubIds[0] : null;
        public int MaxCapacity { get; set; }
        public int RegisteredCount { get; set; }
        public bool IsUserRegistered { get; set; }
        public string? ReturnUrl { get; set; }                   
    }

}