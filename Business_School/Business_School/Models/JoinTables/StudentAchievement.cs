namespace Business_School.Models.JoinTables
{
    public class StudentAchievement
    {
        public int UserStudentId { get; set; }
        public ApplicationUser UserStudent { get; set; } = null!;

        public int AchievementId { get; set; }
        public Achievement Achievement { get; set; } = null!;

        public DateTime EarnedAt { get; set; }
    }

}
