using System.ComponentModel.DataAnnotations;

namespace E_commerce.DTOs
{
    public class SalesDTO
    {
        public int SaleId { get; set; }
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime SaleDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int TotalProductsSold { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal TotalProfit { get; set; }

    }
    public class CreateSaleDTO
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }
    }

    public class SalesComparisonDTO
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class SalesComparisonResultDTO
    {
        public decimal TotalSalesThisPeriod { get; set; }
        public decimal TotalSalesPreviousPeriod { get; set; }
        public decimal RevenueThisPeriod { get; set; }
        public decimal RevenuePreviousPeriod { get; set; }
    }

    public class SalesComparisonRequestDTO
    {
        public SalesComparisonDTO CurrentPeriod { get; set; }
        public SalesComparisonDTO PreviousPeriod { get; set; }
    }


}
