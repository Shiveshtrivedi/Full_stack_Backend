namespace E_commerce.DTOs
{
    public class CartDTO
    {
        public int CartId { get; set; }

        public int UserId { get; set; }

        public List<CartItemDTO> Items { get; set; } = new List<CartItemDTO>();
        public decimal? TotalPrice { get; set; }
        public int Quantity { get; set; }
    }

    public class CartItemDTO
    {
        public int CartItemId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
    }

    public class AddCartItemDTO
    {
        public int Quantity { get; set; }
        public int ProductId { get; set; }
    }

    public class UpdateCartItemDTO
    {
        public int Quantity { get; set; }
        public int ProductId { get; set; }
    }


}
