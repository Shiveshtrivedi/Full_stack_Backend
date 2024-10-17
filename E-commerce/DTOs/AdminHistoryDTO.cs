namespace E_commerce.DTOs
{
    public class HistoryDTO
    {
        public int HistoryId { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public DateTime ActionDate { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductImage { get; set; }
        public bool IsAdminAction { get; set; }
        public decimal? Price { get; set; }
    }
    public class CreateHistoryDTO
    {
        public string ActionType { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public DateTime ActionDate { get; set; } = DateTime.Now;
        public int? UserId { get; set; }
        public int? ProductId { get; set; }
        public bool IsAdminAction { get; set; } = false;
    }

}
