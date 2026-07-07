namespace LabExp.Models.AdminModels
{
    public class AdminDashboardViewModel
    {
        public int TotalScientists { get; set; }

        public int TotalSubjects { get; set; }

        public int TotalTests { get; set; }

        public int TotalSubstances { get; set; }

        public int TotalStatuses { get; set; }

        public int TotalSeverities { get; set; }

        public int TotalClearances { get; set; }

        public List<RecentTestViewModel> RecentTests { get; set; } = new();
    }
}
