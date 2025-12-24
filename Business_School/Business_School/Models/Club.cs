using Business_School.Models.JoinTables;
using System.Collections.Generic;

namespace Business_School.Models
{
    public class Club
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        // FK
        public int DepartmentId { get; set; }

        public Department? Department { get; set; }

        // Leader is optional and can be assigned later
        public int? LeaderId { get; set; }
        public ApplicationUser? Leader { get; set; }

        // Capacity (maximum allowed members)
        public int? Capacity { get; set; }

        // Many-to-many with Student
        public ICollection<StudentClub> StudentClubs { get; set; } = new List<StudentClub>();

        public ICollection<EventClub> EventClubs { get; set; } = new List<EventClub>();

    }

}