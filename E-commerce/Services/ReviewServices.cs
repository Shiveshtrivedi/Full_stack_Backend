using E_commerce.Context;
using E_commerce.DTOs;
using E_commerce.Models;
using E_commerce.Utils;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Services
{
    public class ReviewServices : IReviewServices
    {
        private readonly DataContext _context;

        public ReviewServices(DataContext context)
        {
            _context = context;
        }

        public async Task<List<ReviewDTO>> GetAllReviewsAsync()
        {
            try
            {
                var allReviews = await _context.Reviews
                    .Include(r => r.User)
                    .Select(r => new ReviewDTO
                    {
                        ProductId = r.ProductId,
                        UserId = r.UserId,
                        UserName = r.User.UserName,
                        Rating = r.Rating,
                        Comment = r.Comment
                    })
                    .ToListAsync();

                return allReviews;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving all reviews: {ex.Message}");
                return new List<ReviewDTO>();       
            }
        }

        public async Task<List<ReviewDTO>> GetReviewsByProductAsync(int productId)
        {
            try
            {
                var reviewList = await _context.Reviews
                    .Where(p => p.ProductId == productId)
                    .Include(r => r.User)
                    .Select(r => new ReviewDTO
                    {
                        ProductId = r.ProductId,
                        UserId = r.UserId,
                        UserName = r.User.UserName,
                        Rating = r.Rating,
                        Comment = r.Comment
                    })
                    .ToListAsync();

                return reviewList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving reviews for product {productId}: {ex.Message}");
                return new List<ReviewDTO>();       
            }
        }

        public async Task<ReviewDTO> AddReviewAsync(ReviewDTO reviewDto)
        {
            try
            {
                var review = new Review
                {
                    ProductId = reviewDto.ProductId,
                    UserId = reviewDto.UserId,
                    Rating = reviewDto.Rating,
                    Comment = reviewDto.Comment,
                    ReviewDate = reviewDto.ReviewDate
                };

                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();

                var user = await _context.Users.FindAsync(reviewDto.UserId);

                if (user != null)
                    reviewDto.UserName = user.UserName;

                return reviewDto;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding review: {ex.Message}");
                return null;        
            }
        }

        public async Task<bool> DeleteReviewAsync(int reviewId, int userId)
        {
            try
            {
                var review = await _context.Reviews.FirstOrDefaultAsync(r => r.ReviewId == reviewId);
                if (review == null || review.UserId != userId)
                {
                    return false;
                }

                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting review {reviewId}: {ex.Message}");
                return false;        
            }
        }
    }
}
