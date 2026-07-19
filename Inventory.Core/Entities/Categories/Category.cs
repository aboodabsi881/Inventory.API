using Inventory.Core.Entities.Products;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Core.Entities.Categories
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Img { get; set; }
        public List<Product> Products { get; set; } = [];
    }
}
