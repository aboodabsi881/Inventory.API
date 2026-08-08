namespace Inventory.Web.ViewModels.Home
{
    public class DashboardVM
    {
        public int TotalProducts { get; set; }
        public int LowStockCount { get; set; }
        public int TotalCategories { get; set; }
        public int TotalUsers { get; set; }
        public List<RecentProductVM> RecentProducts { get; set; } = new();
    }
}
