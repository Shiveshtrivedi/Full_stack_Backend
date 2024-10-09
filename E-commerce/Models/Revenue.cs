namespace E_commerce.Models
{
    public class Revenue
    {
        public int RevenueId { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        public decimal TotalRevenue { get; set; }

        public int TotalSales { get; set; }
    }
}
