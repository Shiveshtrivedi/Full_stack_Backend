using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_commerce.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        [Required]
        public string ProductName { get; set; } = string.Empty;
        [Required]
        public string ProductDescription { get; set; } = string.Empty;
        [Required]
        public string Image { get; set; } = string.Empty;
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required]
        public int Stock { get; set; }

        [Required]
        public string Category { get; set; } = string.Empty;
        public double? Rating { get; set; } = 0.0;

        public int? UserId { get; set; }
        public User? User { get; set; }
        public bool DeleteFlag { get; set; } = false;
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }
    }
}
