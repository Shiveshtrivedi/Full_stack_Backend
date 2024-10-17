using System.ComponentModel.DataAnnotations.Schema;

namespace E_commerce.DTOs
{
    public class OrderDTO
    {
        public int OrderId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public List<OrderDetailDTO>? OrderDetails { get; set; }
        public string RazorpayOrderId { get; set; }
        public string TransctionId { get; set; }
        public string UserName { get; set; }
    }

    public class OrderDetailDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

    }

    public class CreateOrderDTO
    {
        public int UserId { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? TransctionId { get; set; }
        public List<OrderItemDTO>? Items { get; set; }
    }

    public class OrderItemDTO
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

    public class DetailedOrderDTO : OrderDTO
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public ShippingAddressDTO? ShippingAddress { get; set; }
    }

    public class OrderUpdateDTO
    {
        public string? Status { get; set; }
        public string? PaymentMethod { get; set; }
        public string? TransctionId { get; set; }
    }
}
