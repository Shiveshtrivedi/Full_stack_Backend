namespace E_commerce.DTOs
{
    public class InventoryDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int StockAvailable { get; set; }
        public int StockSold { get; set; }
    }

    public class UpdateStockDTO
    {
        public List<ProductSaleDTO> Products { get; set; } = new List<ProductSaleDTO>();
    }

    public class ProductSaleDTO
    {
        public int ProductId { get; set; }
        public int QuantitySold { get; set; }
    }

    public class AdminUpdateStockDTO
    {
        public int ProductId { get; set; }
        public int AdditionalStock { get; set; }
    }
}
