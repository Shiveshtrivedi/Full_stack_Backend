namespace E_commerce.Models
{
    public class WishList
    {
        public int WishListId { get; set; }
        public DateTime DateAdded { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}
