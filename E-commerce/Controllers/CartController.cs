using E_commerce.DTOs;
using E_commerce.Services;
using E_commerce.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/cart")]
    public class CartController : ControllerBase
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

            if (cartItem == null || !cartItem.Any())
            {
                return NotFound(new { message = "Cart is empty or user does not exist." });
            }

            return Ok(cartItem);
        }



        [HttpPost("{userId}/addCartItems")]
        public async Task<IActionResult> AddToCart(int userId, [FromBody] AddCartItemDTO addCartItemDTO)
        {
            try
            {
                if (addCartItemDTO == null || userId <= 0 || addCartItemDTO.ProductId <= 0 || addCartItemDTO.Quantity <= 0)
                {
                    return BadRequest("Invalid cart data.");
                }

                var cart = await _cartServices.AddToCartAsync(userId, addCartItemDTO.ProductId, addCartItemDTO.Quantity);

                return Ok(cart);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
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
                var updatedCart = await _cartServices.UpdateCartByUserAsync(userId, updateCartItemsDto);
                return Ok(updatedCart);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }




        [HttpDelete("clear/{userId}/deletItemsInCart")]
        public async Task<IActionResult> ClearCart(int userId)
        {
            var result = await _cartServices.ClearCartItemByUserAsync(userId);
            if (result)
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



