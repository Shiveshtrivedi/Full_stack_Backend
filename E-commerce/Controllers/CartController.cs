using E_commerce.DTOs;
using E_commerce.Utils;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.Controllers
{
    [ApiController]
    [Route("api/cart")]
    public class CartController:ControllerBase
    {
        private readonly ICartServices _cartServices;
        public CartController(ICartServices cartServices)
        {
            _cartServices = cartServices;
        }

        [HttpGet("{userId}/getCartItems")]
        public async Task<IActionResult> GetCartItem(int userId)
         {
            var cartItem = await _cartServices.GetAllProductsFromCartAsync(userId);
            Console.WriteLine($"cart item {cartItem}");
            if (cartItem == null || !cartItem.Any())  
            {
                return NotFound(new { message = "Cart is empty or user does not exist." });
            }

            return Ok(cartItem);
        }

        [HttpPost("{userId}/addCartItems")]
        public async Task<IActionResult> AddCartItem(int userId, [FromBody] AddCartItemDTO addCartItemDto)
        {
            var result = await _cartServices.AddProductInCartAsync(userId, addCartItemDto);
            if(result)
            {
                return Ok(new { Message = "Item added to cart successfully" });
            }

            return BadRequest(new { Message = "Failed to add item in cart" });
        }
        public class UpdateCartItemsDTO
        {
            public List<UpdateCartItemDTO> Items { get; set; }
        }


        [HttpPut("{userId}/updateCartItem")]
        public async Task<IActionResult> UpdateCartItems(int userId, [FromBody] List<UpdateCartItemDTO> updateCartItemsDto)
        {
            try
            {
                // Get updated cart as a list of CartDTO
                var updatedCart = await _cartServices.UpdateCartByUserAsync(userId, updateCartItemsDto);
                return Ok(updatedCart); // Return the updated cart items as a list of CartDTO
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }


        //[HttpPut("{userId}")]
        //public async Task<IActionResult> UpdateCartItem(int userId, [FromBody] UpdateCartItemDTO updateCartItemDto)
        //{
        //    try
        //    {
        //        var updatedCart = await _cartServices.UpdateCartByUserAsync(userId, updateCartItemDto);
        //        return Ok(updatedCart); // Return the updated cart details
        //    }
        //    catch (Exception ex)
        //    {
        //        return NotFound(new { message = ex.Message });
        //    }
        //}



        [HttpDelete("clear/{userId}/deletItemsInCart")]
        public async Task<IActionResult> ClearCart(int userId)
        {
            var result = await _cartServices.ClearCartItemByUserAsync(userId);
            Console.WriteLine($"result ${result}");
            if(result)
            {
                return Ok("Cart cleared successfully");
            }
            else
            {
                return BadRequest("No item are found");
            }    
        }

    }
}
