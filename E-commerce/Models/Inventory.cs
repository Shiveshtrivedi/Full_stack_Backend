using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_commerce.Models
{
    public class Inventory
    {
        public int InventoryId { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock available must be a non-negative number.")]
        public int StockAvailable { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Stock sold must be a non-negative number.")]
        public int StockSold { get; set; }
    }
}
