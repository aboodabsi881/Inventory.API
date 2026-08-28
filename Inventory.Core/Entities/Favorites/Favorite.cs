using Inventory.Core.Entities.Products;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Core.Entities.Favorites
{
    public class Favorite
    {
        public int Id { get; set; }
        public bool IsFavorite { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int UserId { get; set; }
    }
}
