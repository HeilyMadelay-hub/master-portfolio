
using Business_School.Models.JoinTables;

namespace Business_School.Models
{
    public class Achievement
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public ICollection<StudentAchievement> StudentAchievements { get; set; } = new List<StudentAchievement>();
    }

}
