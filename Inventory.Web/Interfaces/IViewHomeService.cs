using Inventory.Web.ViewModels.Home;
using System.Threading.Tasks;

namespace Inventory.Web.Interfaces
{
    public interface IViewHomeService
    {
        Task<DashboardVM> GetDashboardMetricsAsync();
    }
}