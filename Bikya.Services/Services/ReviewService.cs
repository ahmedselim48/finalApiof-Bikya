using Bikya.Data;
using Bikya.Data.Response;
using Bikya.DTOs.ReviewDTOs;
using Bikya.Services.Interfaces;
using Bikya.Data.Models;
using Microsoft.EntityFrameworkCore;


public class ReviewService : IReviewService
{
    private readonly BikyaContext _context;

    public ReviewService(BikyaContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<ReviewDTO>>> GetAllAsync()
    {
        var reviews = await _context.Reviews
            .Include(r => r.Reviewer)
            .Include(r => r.Seller)
            .Include(r => r.Order).ThenInclude(o => o.Product)
            .ToListAsync();

        var result = reviews.Select(ToReviewDTO).ToList();
        return ApiResponse<List<ReviewDTO>>.SuccessResponse(result, "Reviews retrieved successfully");
    }

    public async Task<ApiResponse<ReviewDTO>> GetByIdAsync(int id)
    {
        var review = await _context.Reviews
            .Include(r => r.Reviewer)
            .Include(r => r.Seller)
            .Include(r => r.Order).ThenInclude(o => o.Product)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (review == null)
            return ApiResponse<ReviewDTO>.ErrorResponse("Review not found", 404);

        return ApiResponse<ReviewDTO>.SuccessResponse(ToReviewDTO(review), "Review retrieved successfully");
    }

    public async Task<ApiResponse<ReviewDTO>> AddAsync(CreateReviewDTO dto)
    {
        var reviewer = await _context.Users.FindAsync(dto.ReviewerId);
        if (reviewer == null)
            return ApiResponse<ReviewDTO>.ErrorResponse("Reviewer not found", 404);

        var seller = await _context.Users.FindAsync(dto.SellerId);
        if (seller == null)
            return ApiResponse<ReviewDTO>.ErrorResponse("Seller not found", 404);

        var order = await _context.Orders
            .Include(o => o.Product) 
            .FirstOrDefaultAsync(o => o.Id == dto.OrderId);

        if (order == null)
            return ApiResponse<ReviewDTO>.ErrorResponse("Order not found", 404);

        if (order.BuyerId != dto.ReviewerId)
            return ApiResponse<ReviewDTO>.ErrorResponse("You are not authorized to review this order", 403);

        var existingReview = await _context.Reviews.FirstOrDefaultAsync(r => r.OrderId == dto.OrderId);
        if (existingReview != null)
            return ApiResponse<ReviewDTO>.ErrorResponse("Review already exists for this order", 400);

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


    public async Task<ApiResponse<ReviewDTO>> UpdateAsync(int id, UpdateReviewDTO dto)
    {
        var review = await _context.Reviews
            .Include(r => r.Reviewer)
            .Include(r => r.Seller)
            .Include(r => r.Order).ThenInclude(o => o.Product)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (review == null)
            return ApiResponse<ReviewDTO>.ErrorResponse("Review not found", 404);

        if (review.ReviewerId != dto.ReviewerId)
            return ApiResponse<ReviewDTO>.ErrorResponse("You are not authorized to update this review", 403);

        review.Rating = dto.Rating;
        review.Comment = dto.Comment;

        await _context.SaveChangesAsync();

        return ApiResponse<ReviewDTO>.SuccessResponse(ToReviewDTO(review), "Review updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review == null)
            return ApiResponse<bool>.ErrorResponse("Review not found", 404);

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, "Review deleted successfully");
    }

    public async Task<ApiResponse<List<ReviewDTO>>> GetReviewsBySellerIdAsync(int sellerId)
    {
        var reviews = await _context.Reviews
            .Include(r => r.Reviewer)
            .Include(r => r.Seller)
            .Include(r => r.Order).ThenInclude(o => o.Product)
            .Where(r => r.SellerId == sellerId)
            .ToListAsync();

        var result = reviews.Select(ToReviewDTO).ToList();
        return ApiResponse<List<ReviewDTO>>.SuccessResponse(result, "Reviews for seller retrieved successfully");
    }

    public async Task<ApiResponse<List<ReviewDTO>>> GetReviewsByReviewerIdAsync(int reviewerId)
    {
        var reviews = await _context.Reviews
            .Include(r => r.Reviewer)
            .Include(r => r.Seller)
            .Include(r => r.Order).ThenInclude(o => o.Product)
            .Where(r => r.ReviewerId == reviewerId)
            .ToListAsync();

        var result = reviews.Select(ToReviewDTO).ToList();
        return ApiResponse<List<ReviewDTO>>.SuccessResponse(result, "Reviews by reviewer retrieved successfully");
    }
    public async Task<ApiResponse<List<ReviewDTO>>> GetReviewsByOrderIdAsync(int orderId)
    {
        var reviews = await _context.Reviews
            .Include(r => r.Reviewer)
            .Include(r => r.Seller)
            .Include(r => r.Order).ThenInclude(o => o.Product)
            .Where(r => r.OrderId == orderId)
            .ToListAsync();

        if (!reviews.Any())
            return ApiResponse<List<ReviewDTO>>.ErrorResponse("No reviews found for this order", 404);

        var result = reviews.Select(ToReviewDTO).ToList();
        return ApiResponse<List<ReviewDTO>>.SuccessResponse(result, "Reviews for order retrieved successfully");
    }


    private ReviewDTO ToReviewDTO(Review review)
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
            BuyerName = review.Reviewer?.UserName ?? "",
            SellerName = review.Seller?.UserName ?? "",
            ProductName = review.Order?.Product?.Title ?? string.Empty

        };
    }
}
