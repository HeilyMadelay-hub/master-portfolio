using Microsoft.AspNetCore.Mvc.Rendering;

namespace Business_School.ViewModels.Admin
{
 public class AssignClubLeaderVM
 {
 public int ClubId { get; set; }
 public IEnumerable<SelectListItem> Clubs { get; set; } = new List<SelectListItem>();
 public int UserId { get; set; }
 public IEnumerable<SelectListItem> Users { get; set; } = new List<SelectListItem>();
 }
}
