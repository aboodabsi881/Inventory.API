using Inventory.Core.Entities.Products;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Core.Entities.Carts
{
    public class Cart
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; } 
    }
}
