using Bikya.Data;
using Bikya.Data.Response;
using Bikya.DTOs.ReviewDTOs;
using Bikya.Services.Interfaces;
using Bikya.Data.Models;
using Microsoft.EntityFrameworkCore;
using Bikya.Data.Enums;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;


public class ReviewService : IReviewService
{
    private readonly BikyaContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ReviewService(BikyaContext context , IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }
    public async Task<ApiResponse<List<ReviewDTO>>> GetAllAsync()
    {
        var reviews = await _context.Reviews
            .Include(r => r.Reviewer)
            .Include(r => r.Seller)
            .Include(r => r.Order)
            .ToListAsync();

        var result = reviews.Select(ToReviewDTO).ToList();
        return ApiResponse<List<ReviewDTO>>.SuccessResponse(result, "Reviews retrieved successfully");
    }

    public async Task<ApiResponse<List<ReviewDTO>>> GetReviewsForSellerAsync(int sellerId)
    {
        var reviews = await _context.Reviews
            .Where(r => r.SellerId == sellerId)
            .Select(r => new ReviewDTO
            {
                ReviewerId = r.ReviewerId,
                SellerId = r.SellerId,
                Rating = r.Rating,
                Comment = r.Comment,
                OrderId = r.OrderId,

            })
            .ToListAsync();

        return ApiResponse<List<ReviewDTO>>.SuccessResponse(reviews);
    }
    public async Task<ApiResponse<ReviewDTO>> GetByIdAsync(int id)
    {
        var review = await _context.Reviews
            .Include(r => r.Reviewer)
            .Include(r => r.Seller)
            .Include(r => r.Order)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (review == null)
            return ApiResponse<ReviewDTO>.ErrorResponse("Review not found", 404);

        return ApiResponse<ReviewDTO>.SuccessResponse(ToReviewDTO(review), "Review retrieved successfully");
    }

    public async Task<ApiResponse<ReviewDTO>> AddAsync(CreateReviewDTO dto)
    {
        var currentUserId = int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (dto.ReviewerId != currentUserId)
            return ApiResponse<ReviewDTO>.ErrorResponse("You are not authorized to add this review", 403);

        var reviewer = await _context.Users.FindAsync(dto.ReviewerId);
        if (reviewer == null)
            return ApiResponse<ReviewDTO>.ErrorResponse("Reviewer not found", 404);

        var order = await _context.Orders.FirstOrDefaultAsync(o =>
             o.Id == dto.OrderId &&
             o.BuyerId == dto.ReviewerId &&
             o.SellerId == dto.SellerId &&
             o.OrderStatus == OrderStatus.Completed
         );

        if (order == null)
            return ApiResponse<ReviewDTO>.ErrorResponse("You can only review sellers you've bought from.", 403);

        var seller = await _context.Users.FindAsync(dto.SellerId);
        if (seller == null)
            return ApiResponse<ReviewDTO>.ErrorResponse("Seller not found", 404);

        var existingReview = await _context.Reviews
          .AnyAsync(r => r.OrderId == dto.OrderId && r.ReviewerId == dto.ReviewerId);

        if (existingReview)
            return ApiResponse<ReviewDTO>.ErrorResponse("You have already reviewed this order.", 409);

        var review = new Review
        {
            Rating = dto.Rating,
            Comment = dto.Comment,
            ReviewerId = dto.ReviewerId,
            Reviewer = reviewer,
            SellerId = dto.SellerId,
            Seller = seller,
            OrderId = dto.OrderId,
            Order = order,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Reviews.AddAsync(review);
        await _context.SaveChangesAsync();

        return ApiResponse<ReviewDTO>.SuccessResponse(ToReviewDTO(review), "Review created successfully", 201);
    }

    public async Task<ApiResponse<ReviewDTO>> UpdateAsync(UpdateReviewDTO dto)
    {
        var currentUserId = int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var review = await _context.Reviews.FindAsync(dto.Id);
        if (review == null)
            return ApiResponse<ReviewDTO>.ErrorResponse("Review not found", 404);

        if (review.ReviewerId != currentUserId)
            return ApiResponse<ReviewDTO>.ErrorResponse("You are not authorized to update this review", 403);

        
        review.Rating = dto.Rating;
        review.Comment = dto.Comment;
        review.CreatedAt = DateTime.UtcNow; 

        _context.Reviews.Update(review);
        await _context.SaveChangesAsync();

        return ApiResponse<ReviewDTO>.SuccessResponse(ToReviewDTO(review), "Review updated successfully");
    }

    public async Task<ApiResponse<object>> DeleteAsync(int reviewId)
{
    var currentUserId = int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    var review = await _context.Reviews.FindAsync(reviewId);
    if (review == null)
        return ApiResponse<object>.ErrorResponse("Review not found", 404);

    if (review.ReviewerId != currentUserId)
        return ApiResponse<object>.ErrorResponse("You are not authorized to delete this review", 403);

    _context.Reviews.Remove(review);
    await _context.SaveChangesAsync();

    return ApiResponse<object>.SuccessResponse(null, "Review deleted successfully");
}
    public async Task<double> GetAverageRatingBySellerIdAsync(int sellerId)
    {
        var reviews = await _context.Reviews
            .Where(r => r.SellerId == sellerId)
            .ToListAsync();

        if (reviews.Count == 0)
            return 0;

        return reviews.Average(r => r.Rating);
    }


    public ReviewDTO ToReviewDTO(Review review)
    {
        return new ReviewDTO
        {
            Id = review.Id,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt,
            ReviewerId = review.ReviewerId,
            SellerId = review.SellerId,
            OrderId = review.OrderId,
            BuyerName = review.Reviewer?.UserName,
            SellerName = review.Seller?.UserName,
        };
    }
}
