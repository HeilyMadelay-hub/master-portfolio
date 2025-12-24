using Business_School.Models;
using System.ComponentModel.DataAnnotations;

namespace Business_School.ViewModels
{
    public class DepartmentFormViewModel
    {
       
        public Department Department { get; set; } = new();

        public List<ApplicationUser> Managers { get; set; } = new();

        public string? ReturnUrl { get; set; }
    }
}
