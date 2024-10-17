using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_commerce.Models
{
    public enum OrderStatus
    {
        Pending,
        Delivered,
        Cancelled,
        Shipped
    }
    public class Order
    {
        public int OrderId { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User? User { get; set; } = null;

        public DateTime OrderDate { get; set; } = DateTime.Now;

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

        public int? ShippingAddressID { get; set; }
        public ShippingAddress? ShippingAddress { get; set; }
        public string? PaymentMethod { get; set; } = string.Empty;

        public string RazorpayOrderId { get; set; }
        public string? TransctionId
        {
            get; set;
        }

    }

    public class OrderDetail
    {
        public int OrderDetailId { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        [Required]
        public int Quantity { get; set; }
        [Required]
        public decimal Price { get; set; }

    }
}
