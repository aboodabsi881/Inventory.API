using Inventory.Web.Interfaces;
using Inventory.Web.ViewModels.Carts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Inventory.Web.Services
{
    public class ViewCartService : IViewCartService
    {
        private readonly HttpClient _client;
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public ViewCartService(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient("InventoryAPI");
        }

        public async Task<List<CartVM>> GetCartItemsAsync()
        {
            try
            {
                return await _client.GetFromJsonAsync<List<CartVM>>("Carts", JsonOptions) ?? new List<CartVM>();
            }
            catch
            {
                return new List<CartVM>();
            }
        }

        public async Task<(bool Success, CartVM? Item, bool Removed, int Quantity, decimal GrandTotal, string? ErrorMessage)> AddOrUpdateItemAsync(int productId, string? actionType, int change)
        {
            int delta = (actionType == "decrement" || change < 0) ? -1 : 1;

            var response = await _client.PostAsync($"Carts/items?productId={productId}&change={delta}", null);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                string errorMsg = "API Error";
                try
                {
                    using var doc = JsonDocument.Parse(responseContent);
                    if (doc.RootElement.TryGetProperty("message", out var msgProp))
                        errorMsg = msgProp.GetString() ?? responseContent;
                }
                catch
                {
                    errorMsg = string.IsNullOrWhiteSpace(responseContent) ? response.ReasonPhrase ?? "Error" : responseContent;
                }

                return (false, null, false, 0, 0, errorMsg);
            }

            CartVM? updatedItem = null;
            bool isRemoved = false;

            try
            {
                if (!string.IsNullOrWhiteSpace(responseContent))
                {
                    using var doc = JsonDocument.Parse(responseContent);
                    if (doc.RootElement.TryGetProperty("removed", out var remProp) && remProp.GetBoolean())
                    {
                        isRemoved = true;
                    }
                    else
                    {
                        updatedItem = JsonSerializer.Deserialize<CartVM>(responseContent, JsonOptions);
                    }
                }
            }
            catch
            {
                updatedItem = null;
            }

            isRemoved = isRemoved || updatedItem == null || updatedItem.Quantity <= 0;
            decimal grandTotal = await GetSafeCartTotalAsync();
            int finalQty = isRemoved ? 0 : updatedItem!.Quantity;

            return (true, isRemoved ? null : updatedItem, isRemoved, finalQty, grandTotal, null);
        }

        public async Task<(bool Success, decimal GrandTotal, string? ErrorMessage)> RemoveItemAsync(int id)
        {
            var response = await _client.DeleteAsync($"Carts/{id}");
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return (false, 0, content);
            }

            decimal grandTotal = await GetSafeCartTotalAsync();
            return (true, grandTotal, null);
        }

        public async Task<decimal> GetSafeCartTotalAsync()
        {
            try
            {
                var response = await _client.GetAsync("Carts/total");
                if (!response.IsSuccessStatusCode) return 0;

                var content = await response.Content.ReadAsStringAsync();
                if (decimal.TryParse(content, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal simpleTotal))
                {
                    return simpleTotal;
                }

                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;
                if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("total", out var totalProp))
                {
                    return totalProp.GetDecimal();
                }
            }
            catch
            {
            }

            return 0;
        }
    }
}