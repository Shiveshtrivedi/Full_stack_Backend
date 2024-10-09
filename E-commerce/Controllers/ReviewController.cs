using E_commerce.DTOs;
using E_commerce.Services;
using E_commerce.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace E_commerce.Controllers
{
    [ApiController]
    [Route("api/review")]
    public class ReviewController:ControllerBase
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

            if(review==null)
            { 
                 return BadRequest("no review found");
            }
                return Ok(review);

        }

        [HttpPost("addReview")]
        public async Task <IActionResult> AddReview(ReviewDTO reviewDTO)
        {
            var addReview = await _reviewServices.AddReviewAsync(reviewDTO);
            return Ok(addReview);

        }

        private int GetCurrentUserId()
        {
            // Assuming you're storing user ID in a claim or similar storage
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                // Handle the case where the user ID is null or empty
                throw new ArgumentNullException(nameof(userIdClaim), "User ID cannot be null or empty.");
            }

            // Attempt to parse the user ID to an integer
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId; // Return the parsed user ID
            }

            // If parsing fails, you can throw an exception or return a default value
            throw new FormatException("User ID format is invalid.");
        }


        [HttpDelete("{reviewId}/deleteReviewById")]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            int currentUserId = GetCurrentUserId();

                Console.WriteLine("current user id "+currentUserId);
            try
            {

            var result = await _reviewServices.DeleteReviewAsync(reviewId, currentUserId);

            if (result)
            {
                return Ok("Review deleted successfully.");
            }

            return NotFound("Review not found or you do not have permission to delete this review.");

            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

    }
}
