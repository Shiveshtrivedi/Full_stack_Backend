using AutoMapper;
using E_commerce.Context;
using E_commerce.DTOs;
using E_commerce.Models;
using E_commerce.Utils;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Services
{
    public class CartServices : ICartServices
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public CartServices(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CartDTO>> GetAllProductsFromCartAsync(int userId)
        {
            try
            {
                var cart = await _context.Carts
                                     .Include(c => c.Items)
                                     .ThenInclude(ci => ci.Product)
                                     .Where(c => c.UserId == userId)
                                     .FirstOrDefaultAsync();

                if (cart == null)
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

                return new List<CartDTO> { cartDto };
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine($"Error retrieving products from cart: {ex.Message}");
                throw;
            }

        }

        public async Task<CartDTO[]> AddToCartAsync(int userId, int productId, int quantity)
        {
            try
            {
                var cart = await _context.Carts
                                      .Include(c => c.Items)
                                      .ThenInclude(ci => ci.Product)
                                      .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    cart = new Cart { UserId = userId, Items = new List<CartItem>() };
                    _context.Carts.Add(cart);
                }

                var existingCartItem = cart.Items.FirstOrDefault(ci => ci.ProductId == productId);

                if (existingCartItem != null)
                {
                    existingCartItem.Quantity += quantity;
                }
                else
                {
                    var product = await _context.Products.FindAsync(productId);
                    if (product == null)
                    {
                        throw new Exception("Product not found");
                    }

                    var newCartItem = new CartItem
                    {
                        ProductId = productId,
                        Quantity = quantity,
                        Product = product
                    };
                    cart.Items.Add(newCartItem);
                }

                await _context.SaveChangesAsync();

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
                cartDto.Quantity = cart.Items.Sum(ci => ci.Quantity);

                return new CartDTO[] { cartDto };
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine($"Error adding to cart: {ex.Message}");
                throw;
            }
        }


        public async Task<List<CartDTO>> UpdateCartByUserAsync(int userId, List<UpdateCartItemDTO> updateCartItemsDto)
        {
            try
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

                return new List<CartDTO> { cartDto };
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine($"Error updating cart: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> ClearCartItemByUserAsync(int userId)
        {
            try
            {
                var cart = await _context.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                    return false;

                _context.CartItems.RemoveRange(cart.Items);
                await _context.SaveChangesAsync();

                return true;
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine($"Error clearing cart: {ex.Message}");
                throw;
            }


        }

    }
}

