using System.Collections.Generic;
using System.Threading.Tasks;
using Inventory.Core.DTOs;

namespace Inventory.Core.Interfaces
{
    public interface ICartService
    {
        Task<CartResponseDto?> AddOrUpdateItemAsync(int productId, int change);
        Task<bool> RemoveItemAsync(int cartId);
        Task<decimal> GetCartTotalAsync();
        Task<IReadOnlyList<CartResponseDto>> GetCartAsync();
    }
}