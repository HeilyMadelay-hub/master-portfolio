namespace Business_School.ViewModels.Dashboard
{
    // Recommended club - Dashboard Controller
    public class RecommendedClubVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string DepartmentName { get; set; }
        public int MatchPercentage { get; set; } // Porcentaje de coincidencia con el estudiante
        public string Reason { get; set; }       // Explicación de la recomendación
    }

}
