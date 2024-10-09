using AutoMapper;
using E_commerce.Context;
using E_commerce.DTOs;
using E_commerce.Models;
using E_commerce.Utils;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Services
{
    public class CartServices:ICartServices
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        
        public CartServices(DataContext context,IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task <IEnumerable<CartDTO>> GetAllProductsFromCartAsync(int userId)
        {
            var cart = await _context.Carts
                                 .Include(c => c.Items)
                                 .ThenInclude(ci => ci.Product)
                                 .Where(c => c.UserId == userId)
                                 .FirstOrDefaultAsync();

            if(cart == null)
            {
                return null;
            }

            var cartDto = _mapper.Map<CartDTO>(cart);
            cartDto.TotalPrice = cart.Items.Sum(ci => ci.Quantity * ci.Product.Price); 
            foreach (var item in cartDto.Items)
            {
                var product = cart.Items.FirstOrDefault(ci => ci.ProductId == item.ProductId)?.Product;
                if (product != null)
                {
                    item.ImageUrl = product.Image;
                    item.ProductName = product.ProductName;
                    item.Price = product.Price;
                }
            }
            cartDto.TotalPrice = cart.Items.Sum(ci => ci.Quantity * ci.Product.Price);
            cartDto.Quantity = cart.Items.Sum(ci => ci.Quantity);

            return new List<CartDTO> { cartDto};

        }
        public async Task<bool> AddProductInCartAsync(int userId,AddCartItemDTO addCartItemDto)
        {
            var cart = await _context.Carts
                                           .Include(c => c.Items)
                                           .FirstOrDefaultAsync(c=>c.UserId == userId);

            Console.WriteLine($"cart values {cart}");
            if(cart == null) 
            {
                cart = new Cart {UserId = userId,Items = new List<CartItem>() };
                _context.Carts.Add(cart);   
            
            }

            var existingItem = cart.Items.FirstOrDefault(ci=>ci.ProductId==addCartItemDto.ProductId);
            Console.WriteLine($"existing item {existingItem}");
            if (existingItem != null)
            {
                existingItem.Quantity += addCartItemDto.Quantity;
            }
            else
            {
                var cartItem = new CartItem
                {
                    ProductId = addCartItemDto.ProductId,
                    Quantity = addCartItemDto.Quantity,
                    CartId = cart.CartId
                };
                Console.WriteLine($"cart item {cartItem}");
                cart.Items.Add(cartItem);
            }
            return await _context.SaveChangesAsync()>0;
            
        }

        public async Task<List<CartDTO>> UpdateCartByUserAsync(int userId, List<UpdateCartItemDTO> updateCartItemsDto)
        {
            var cart = await _context.Carts
                                     .Include(c => c.Items)
                                     .ThenInclude(ci => ci.Product)
                                     .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                throw new Exception("Cart not found");

            foreach (var updateCartItemDto in updateCartItemsDto)
            {
                var cartItem = cart.Items.FirstOrDefault(item => item.ProductId == updateCartItemDto.ProductId);

                if (cartItem == null)
                    throw new Exception($"Cart item with ProductId {updateCartItemDto.ProductId} not found");

                if (updateCartItemDto.Quantity > 0)
                {
                    cartItem.Quantity = updateCartItemDto.Quantity;
                }
                else
                {
                    _context.CartItems.Remove(cartItem);
                }
            }

            await _context.SaveChangesAsync();

            // Create a CartDTO to return
            var cartDto = new CartDTO
            {
                CartId = cart.CartId,
                UserId = cart.UserId,
                TotalPrice = cart.Items.Sum(ci => ci.Quantity * ci.Product.Price),
                Items = cart.Items.Select(ci => new CartItemDTO
                {
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    ImageUrl = ci.Product.Image,
                    ProductName = ci.Product.ProductName,
                    Price = ci.Product.Price
                }).ToList()
            };
            cartDto.TotalPrice = cart.Items.Sum(ci => ci.Quantity * ci.Product.Price);
            cartDto.Quantity = cart.Items.Sum(ci => ci.Quantity);

            // Return a list containing the updated CartDTO
            return new List<CartDTO> { cartDto };
        }

        //public async Task<CartDTO> UpdateCartByUserAsync(int userId, UpdateCartItemDTO updateCartItemDto)
        //{

        //    var cart = await _context.Carts
        //                            .Include(c => c.Items)
        //                            .ThenInclude(ci => ci.Product)
        //                            .FirstOrDefaultAsync(c => c.UserId == userId);


        //    if (cart == null)
        //        throw new Exception("Cart not found");


        //    var cartItem = cart.Items.FirstOrDefault(item => item.ProductId == updateCartItemDto.ProductId);

        //    if (cartItem == null)
        //        throw new Exception("Cart item not found");



        //    if (updateCartItemDto.Quantity > 0)
        //    {
        //        cartItem.Quantity = updateCartItemDto.Quantity;
        //    }
        //    else
        //    {
        //        _context.CartItems.Remove(cartItem);
        //    }

        //    await _context.SaveChangesAsync();
        //    var cartDto = _mapper.Map<CartDTO>(cart);
        //    cartDto.TotalPrice = cart.Items.Sum(ci => ci.Quantity * ci.Product.Price); // Update total price

        //    foreach (var item in cartDto.Items)
        //    {
        //        var product = cart.Items.FirstOrDefault(ci => ci.ProductId == item.ProductId)?.Product;
        //        if (product != null)
        //        {
        //            item.ImageUrl = product.Image;
        //            item.ProductName = product.ProductName;
        //            item.Price = product.Price;
        //        }
        //    }

        //    return cartDto;
        //}

        public async Task<bool> ClearCartItemByUserAsync(int userId)
        {
            var cart = await _context.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return false;

             _context.CartItems.RemoveRange(cart.Items);
            await _context.SaveChangesAsync();

            return true;


        }

    }
}

