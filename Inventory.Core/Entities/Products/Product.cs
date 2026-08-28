using Inventory.Core.Entities.Carts;
using Inventory.Core.Entities.Categories;
using Inventory.Core.Entities.Favorites;
using Inventory.Core.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Core.Entities.Products
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public PowerType? PowerType { get; set; }
        public string Img { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public List<Favorite> Favorites { get; set; } = new List<Favorite>();
        public List<Cart> Cart { get; set; } = new List<Cart>();
        public int Quantity { get; set; } = 0;
    }
}
