using System;
using System.Collections.Generic;
using System.Text;

namespace AvaloniaApplication19.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public double Price { get; set; }
        public string Description { get; set; }
    }
}
