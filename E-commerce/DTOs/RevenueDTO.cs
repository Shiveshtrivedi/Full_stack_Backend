namespace E_commerce.DTOs
{
    public class RevenueDTO
    {
        public DateTime Date { get; set; } = DateTime.Now;
        public decimal TotalRevenue { get; set; }
    }

    public class RevenueByDateDTO
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
