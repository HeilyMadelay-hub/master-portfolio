namespace Business_School.ViewModels.Dashboard
{
    public class DashboardAdminVM
    {
        public int TotalStudents { get; set; }
        public int TotalClubs { get; set; }
        public int TotalDepartments { get; set; }
        public Dictionary<string, int> StudentsByDepartment { get; set; } = new();
        public List<Business_School.Models.Event> NextEvents { get; set; } = new();
        public List<Business_School.Models.ApplicationUser> RecentStudents { get; set; } = new();
    }
}
