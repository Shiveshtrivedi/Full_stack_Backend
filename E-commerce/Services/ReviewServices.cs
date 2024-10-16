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


        public async Task<List<ReviewDTO>> GetReviewsByProductAsync(int productId)
        {
            var reviewList = await _context.Reviews.Where(p => p.ProductId == productId)
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

        public async Task<ReviewDTO> AddReviewAsync(ReviewDTO reviewDto)
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

            if(user!=null)
            reviewDto.UserName = user.UserName;

            return reviewDto;
        }

        public async Task<bool> DeleteReviewAsync(int reviewId, int userId)
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

    }
}
