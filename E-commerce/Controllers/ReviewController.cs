using E_commerce.DTOs;
using E_commerce.Services;
using E_commerce.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace E_commerce.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/review")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewServices _reviewServices;
        public ReviewController(IReviewServices reviewServices)
        {
            _reviewServices = reviewServices;
        }

        [HttpGet("getAllReview")]
        public async Task<ActionResult<List<ReviewDTO>>> GetAllReviews()
        {
            var reviews = await _reviewServices.GetAllReviewsAsync();
            return Ok(reviews);
        }

        [HttpGet("{productId}/getReviewByProductId")]
        public async Task<IActionResult> GetReview(int productId)
        {
            var review = await _reviewServices.GetReviewsByProductAsync(productId);

            if (review == null)
            {
                return BadRequest("no review found");
            }
            return Ok(review);

        }

        [HttpPost("addReview")]
        public async Task<IActionResult> AddReview(ReviewDTO reviewDTO)
        {
            var addReview = await _reviewServices.AddReviewAsync(reviewDTO);
            return Ok(addReview);

        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new ArgumentNullException(nameof(userIdClaim), "User ID cannot be null or empty.");
            }

            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }

            throw new FormatException("User ID format is invalid.");
        }


        [HttpDelete("{reviewId}/deleteReviewById")]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            int currentUserId = GetCurrentUserId();

            try
            {

                var result = await _reviewServices.DeleteReviewAsync(reviewId, currentUserId);

                if (result)
                {
                    return Ok("Review deleted successfully.");
                }

                return NotFound("Review not found or you do not have permission to delete this review.");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

    }
}
