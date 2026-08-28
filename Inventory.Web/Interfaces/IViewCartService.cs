using Inventory.Web.ViewModels.Carts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventory.Web.Interfaces
{
    public interface IViewCartService
    {
        Task<List<CartVM>> GetCartItemsAsync();
        Task<(bool Success, CartVM? Item, bool Removed, int Quantity, decimal GrandTotal, string? ErrorMessage)> AddOrUpdateItemAsync(int productId, string? actionType, int change);
        Task<(bool Success, decimal GrandTotal, string? ErrorMessage)> RemoveItemAsync(int id);
        Task<decimal> GetSafeCartTotalAsync();
    }
}