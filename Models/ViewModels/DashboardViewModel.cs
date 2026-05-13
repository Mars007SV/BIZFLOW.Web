using System.Collections.Generic;

namespace BIZFLOW.Web.Models.ViewModels
{
    public class DashboardViewModel
    {
        public DashboardStatistics Statistics { get; set; } = new DashboardStatistics();
        public List<RecentActivity> RecentActivities { get; set; } = new List<RecentActivity>();
    }

    public class DashboardStatistics
    {
        public int TotalProducts { get; set; }
        public int ProductsInDeficit { get; set; }
        public int RecentOperationsCount { get; set; }
        public int TotalCategories { get; set; }
    }

    public class RecentActivity
    {
        public string ProductName { get; set; } = string.Empty;
        public string OperationType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime Date { get; set; }
        public string? UserName { get; set; }
    }
}
