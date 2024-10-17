using E_commerce.Services;
using E_commerce.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/wishlist")]
    public class WishListController : ControllerBase
    {
        private readonly IWishListServices _wishListServices;
        public WishListController(IWishListServices wishListServices)
        {
            _wishListServices = wishListServices;
        }

        [HttpGet("{userId}/getAllItemsOfWishlist")]
        public async Task<IActionResult> GetWishListByUserId(int userId)
        {
            var wishList = await _wishListServices.GetUserWishlist(userId);
            if (wishList == null)
            {
                return BadRequest(new { message = "wishlist is empty" });
            }
            return Ok(wishList);
        }

        [HttpPost("{userId}/add/{productId}/addProductToWishList")]
        public async Task<IActionResult> AddToWishList(int userId, int productId)
        {
            try
            {
                var wishList = await _wishListServices.AddProductToWishList(userId, productId);
                return Ok(wishList);
            }

            catch (Exception ex)
            {
                return BadRequest(ex.Message);

            }
        }
        [HttpDelete("{userId}/remove/{productId}/removeProductFromWishList")]
        public async Task<IActionResult> RemoveFromWishList(int userId, int productId)
        {
            try
            {
                var result = await _wishListServices.DeleteProductFromWishList(userId, productId);
                return result ? Ok() : BadRequest("Failed to remove product from wishlist");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }


    }
}
