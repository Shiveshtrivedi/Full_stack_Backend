using AutoMapper;
using E_commerce.Context;
using E_commerce.DTOs;
using E_commerce.Models;
using E_commerce.Utils;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Services
{
    public class WishListServices:IWishListServices
    {
        private readonly DataContext _context;
        private readonly Mapper _mapper;
        public WishListServices(DataContext context,IMapper mapper)
        {
            _context = context;
            _mapper = (Mapper?)mapper;
        }

        public async Task<List<WishlistReadDto>> GetUserWishlist(int userId)
        {
            var wishListItem = await _context.WishLists.Where(w => w.UserId == userId)
                                                        .Include(w => w.Product)
                                                        .ToListAsync();

            return _mapper.Map<List<WishlistReadDto>>(wishListItem);

        }


        public async Task <WishlistReadDto> AddProductToWishList(int userId,int productId)
        {
            var existingProduct = await _context.Products.FindAsync(productId);
            if(existingProduct == null)
            {
                throw new Exception("Product Not Found");
            }
            var existingItemInWishList = await _context.WishLists.FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

            if(existingItemInWishList!=null)
            {
                throw new Exception("already exist in wishlist");
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

        public async Task<bool> DeleteProductFromWishList(int userId,int productId)
        {
            var wishList = await _context.WishLists.FirstOrDefaultAsync(w=>w.ProductId==productId && w.UserId==userId);
            if (wishList == null)
                throw new Exception("Wishlist item not found");

            _context.WishLists.Remove(wishList);
            await _context.SaveChangesAsync();

            return true;
        }

    }
}


