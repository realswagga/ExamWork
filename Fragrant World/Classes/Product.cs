using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fragrant_World.Classes
{
    public class Product
    {
        public string Article { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Photo { get; set; }
        public string Manufacturer { get; set; }
        public double Cost { get; set; }
        public int DiscountAmount { get; set; }
        public int QuantityInStock { get; set; }
        public string Status { get; set; }
    }
}
