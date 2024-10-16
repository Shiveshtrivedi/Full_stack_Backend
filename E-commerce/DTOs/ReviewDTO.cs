using System.ComponentModel.DataAnnotations;

namespace E_commerce.DTOs
{
    public class ReviewDTO
    {
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; } = string.Empty;
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }
        [Required]
        public string Comment { get; set; } = string.Empty;
        public DateTime ReviewDate { get; set; }
    }
}
