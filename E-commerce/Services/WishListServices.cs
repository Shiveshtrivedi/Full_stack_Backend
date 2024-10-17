using AutoMapper;
using E_commerce.Context;
using E_commerce.DTOs;
using E_commerce.Models;
using E_commerce.Utils;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Services
{
    public class WishListServices : IWishListServices
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public WishListServices(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<List<WishlistReadDto>> GetUserWishlist(int userId)
        {
            try
            {
                var wishListItems = await _context.WishLists
                    .Where(w => w.UserId == userId)
                    .Include(w => w.Product)
                    .ToListAsync();

                return _mapper.Map<List<WishlistReadDto>>(wishListItems);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving the user's wishlist.", ex);
            }
        }

        public async Task<WishlistReadDto> AddProductToWishList(int userId, int productId)
        {
            try
            {
                var existingProduct = await _context.Products.FindAsync(productId);
                if (existingProduct == null)
                {
                    throw new Exception("Product not found.");
                }

                var existingItemInWishList = await _context.WishLists
                    .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

                if (existingItemInWishList != null)
                {
                    throw new Exception("Product already exists in the wishlist.");
                }

                var wishListItem = new WishList
                {
                    UserId = userId,
                    ProductId = productId
                };

                _context.WishLists.Add(wishListItem);
                await _context.SaveChangesAsync();

                return _mapper.Map<WishlistReadDto>(wishListItem);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while adding the product to the wishlist.", ex);
            }
        }

        public async Task<bool> DeleteProductFromWishList(int userId, int productId)
        {
            try
            {
                var wishList = await _context.WishLists
                    .FirstOrDefaultAsync(w => w.ProductId == productId && w.UserId == userId);

                if (wishList == null)
                {
                    throw new Exception("Wishlist item not found.");
                }

                _context.WishLists.Remove(wishList);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while deleting the product from the wishlist.", ex);
            }
        }
    }
}
