using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Business_School.ViewModels.Event
{
    public class EventFormVM
    {
        public int Id { get; set; }

        [Required, StringLength(120)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public int DepartmentId { get; set; }
        public IEnumerable<SelectListItem> Departments { get; set; } = new List<SelectListItem>();

        public List<int> SelectedClubIds { get; set; } = new();
        public IEnumerable<SelectListItem> Clubs { get; set; } = new List<SelectListItem>();

        public int? MaxCapacity { get; set; }
    }

}
