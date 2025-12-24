namespace Business_School.ViewModels.Admin
{
    public class UserListVM
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public string? DepartmentName { get; set; }
        public string? ManagedDepartmentName { get; set; }
        public string? LeadingClubName { get; set; }
    }
}
