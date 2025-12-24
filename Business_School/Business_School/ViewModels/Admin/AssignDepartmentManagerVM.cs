using Microsoft.AspNetCore.Mvc.Rendering;

namespace Business_School.ViewModels.Admin
{
 public class AssignDepartmentManagerVM
 {
 public int DepartmentId { get; set; }
 public IEnumerable<SelectListItem> Departments { get; set; } = new List<SelectListItem>();
 public int UserId { get; set; }
 public IEnumerable<SelectListItem> Users { get; set; } = new List<SelectListItem>();
 }
}
