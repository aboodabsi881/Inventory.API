using Inventory.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventory.Core.Interfaces
{
    public interface ICartService
    {
        Task<IReadOnlyList<CartResponseDto>> GetCartAsync();
        Task<CartResponseDto?> AddOrUpdateItemAsync(int productId, int change);
        Task<bool> RemoveItemAsync(int cartId);
        Task<decimal> GetCartTotalAsync();
    }
}