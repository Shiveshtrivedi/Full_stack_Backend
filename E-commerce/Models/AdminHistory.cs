using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace E_commerce.Models
{
    public class AdminHistory
    {
        [Key]
        public int HistoryId { get; set; }

        [Required]
        public string ActionType { get; set; } = string.Empty;

        public string Details { get; set; } = string.Empty;

        [Required]
        public DateTime ActionDate { get; set; } = DateTime.Now;

        [ForeignKey("User")]
        public int? UserId { get; set; }
        public User? User { get; set; }

        [ForeignKey("Product")]
        public int? ProductId { get; set; }
        public Product? Product { get; set; }

        public bool IsAdminAction { get; set; } = false;
        public bool DeleteFlag { get; set; } = false;


    }
}
