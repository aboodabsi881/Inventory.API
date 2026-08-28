using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Inventory.Web.ViewModels.Home
{
    public class DashboardVM
    {
        public int TotalProducts { get; set; }
        public int TotalStockUnits { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }
        public int TotalCategories { get; set; }
        public int TotalUsers { get; set; }

        // Chart Data
        public List<string> CategoryLabels { get; set; } = new();
        public List<int> CategoryProductCounts { get; set; } = new();
        public List<string> TopProductLabels { get; set; } = new();
        public List<int> TopProductQuantities { get; set; } = new();

        public List<RecentProductVM> RecentProducts { get; set; } = new();
    }

}