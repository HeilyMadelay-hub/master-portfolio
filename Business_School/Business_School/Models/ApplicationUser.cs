using Business_School.Models.JoinTables;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Business_School.Models
{
    // ApplicationUser inherits from IdentityUser<int> to extend the built-in ASP.NET Identity user.
    // - By specifying <int>, we use an integer as the primary key instead of the default string (GUID).
    // - This allows us to add custom properties specific to our application, such as:
    //     - FirstName, LastName
    //     - DepartmentId (linking the user to a Department if they are a student)
    //     - Points and Level (student progress tracking)
    //     - Navigation properties for Clubs, Events, and Achievements
    // - Using a single ApplicationUser class lets us represent all roles (Student, ClubLeader, DepartmentManager, Admin)
    //   without duplicating user tables, keeping the Identity system consistent.

    public class ApplicationUser : IdentityUser<int>
    {
        //Common data
        public string FullName { get; set; } = string.Empty;

        // only if it is a student
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }

        public int Points { get; set; } = 0;



        [NotMapped]//This is not going to the database only for the view
        public GamificationProgress Gamification { get; set; }

        public StudentLevel Level { get; set; } = StudentLevel.Principiante;

        public ICollection<StudentClub> ClubMemberships { get; set; } = new List<StudentClub>();
        public ICollection<EventAttendance> EventAttendances { get; set; } = new List<EventAttendance>();
        public ICollection<StudentAchievement> Achievements { get; set; } = new List<StudentAchievement>();
       
    }

}
