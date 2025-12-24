namespace Business_School.Models.JoinTables
{
    public class StudentClub
    {
        public int UserStudentId { get; set; }
        public ApplicationUser UserStudent { get; set; } = null!;

        public int ClubId { get; set; }
        public Club Club { get; set; } = null!;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public bool IsLeader { get; set; } = false;     // para rol ClubLeader
        public int PointsFromThisClub { get; set; } = 0;
    }
}
