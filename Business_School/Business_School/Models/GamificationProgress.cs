namespace Business_School.Models
{
    public class GamificationProgress
    {

        public int Points { get; set; }
        public StudentLevel Level { get; set; } = StudentLevel.Principiante;
        public List<string> Achievements { get; set; } = new();
    }
}
