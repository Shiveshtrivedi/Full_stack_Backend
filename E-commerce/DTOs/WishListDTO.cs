using E_commerce.Models;

namespace E_commerce.DTOs
{
    public class WishlistCreateDto
    {
        public int UserId { get; set; }
        public int ProductId { get; set; }
    }
    public class WishlistReadDto
    {
        public int WishlistId { get; set; }
        public int UserId { get; set; }

        public Product Product { get; set; }
    }
    public class WishlistRemoveDto
    {
        public int UserId { get; set; }
        public int ProductId { get; set; }
    }



}
