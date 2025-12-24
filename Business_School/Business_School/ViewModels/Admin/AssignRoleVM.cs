using Microsoft.AspNetCore.Mvc.Rendering;

namespace Business_School.ViewModels.Admin
{
 public class AssignRoleVM
 {
 public int UserId { get; set; }
 public string Email { get; set; }
 public List<string> AvailableRoles { get; set; } = new() { "Admin", "DepartmentManager", "ClubLeader", "Student" };
 public List<string> SelectedRoles { get; set; } = new();
 }
}
