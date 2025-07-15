using Bikya.Data;
using Bikya.Data.Enums;
using Bikya.Data.Models;
using Bikya.Data.Response;
using Bikya.DTOs.Orderdto;
using Bikya.DTOs.ReviewDTOs;
using Bikya.DTOs.ShippingDTOs;
using Bikya.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Bikya.Services.Services
{
    public class OrderService : IOrderService
    {
        private readonly BikyaContext _context;

        private readonly UserManager<ApplicationUser> _userManager;

        public OrderService(BikyaContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<ApiResponse<OrderDTO>> CreateOrderAsync(CreateOrderDTO dto)
        {
            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product == null)
                return ApiResponse<OrderDTO>.ErrorResponse("Product not found", 404);

            var seller = await _context.Users.FindAsync(product.UserId);
            if (seller == null)
                return ApiResponse<OrderDTO>.ErrorResponse("Seller not found", 404);

            var order = new Order
            {
                ProductId = dto.ProductId,
                BuyerId = dto.BuyerId,
                SellerId = seller.Id,
                TotalAmount = product.Price,
                PlatformFee = product.Price * 0.05m,
                SellerAmount = product.Price * 0.95m,
                CreatedAt = DateTime.UtcNow,
                ShippingInfo = new ShippingInfo
                {
                    RecipientName = dto.ShippingInfo.RecipientName,
                    Address = dto.ShippingInfo.Address,
                    City = dto.ShippingInfo.City,
                    PostalCode = dto.ShippingInfo.PostalCode,
                    PhoneNumber = dto.ShippingInfo.PhoneNumber,
                    Status = ShippingStatus.Pending,
                    CreateAt = DateTime.UtcNow,
                }
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return ApiResponse<OrderDTO>.SuccessResponse(new OrderDTO
            {
                Id = order.Id,
                ProductId = product.Id,
                ProductTitle = product.Title,
                BuyerId = dto.BuyerId,
                BuyerName = "", // fetch if needed
                SellerId = seller.Id,
                SellerName = seller.FullName,
                TotalAmount = order.TotalAmount,
                PlatformFee = order.PlatformFee,
                SellerAmount = order.SellerAmount,
                Status = order.Status.ToString(),
                CreatedAt = order.CreatedAt,
                ShippingInfo = dto.ShippingInfo
            });
        }

        public async Task<ApiResponse<bool>> UpdateOrderStatusAsync(UpdateOrderStatusDTO dto)
        {
            var order = await _context.Orders.FindAsync(dto.OrderId);
            if (order == null)
                return ApiResponse<bool>.ErrorResponse("Order not found", 404);

            if (Enum.TryParse<OrderStatus>(dto.NewStatus, true, out var newStatus))
            {
                order.Status = newStatus;
                if (newStatus == OrderStatus.Paid)
                    order.PaidAt = DateTime.UtcNow;
                else if (newStatus == OrderStatus.Completed)
                    order.CompletedAt = DateTime.UtcNow;

                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResponse(true);
            }
            return ApiResponse<bool>.ErrorResponse("Invalid status", 400);
        }

        public async Task<ApiResponse<List<OrderSummaryDTO>>> GetOrdersByUserIdAsync(int userId)
        {
            var orders = await _context.Orders
                .Include(o => o.Product)
                .Include(o => o.Buyer)
                .Include(o => o.Seller)
                .Where(o => o.BuyerId == userId || o.SellerId == userId)
                .Select(o => new OrderSummaryDTO
                {
                    Id = o.Id,
                    ProductTitle = o.Product.Title,
                    BuyerName = o.Buyer.UserName,
                    SellerName = o.Seller.UserName,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    CreatedAt = o.CreatedAt
                }).ToListAsync();

            return ApiResponse<List<OrderSummaryDTO>>.SuccessResponse(orders);
        }

        public async Task<ApiResponse<List<OrderSummaryDTO>>> GetOrdersByBuyerIdAsync(int buyerId)
        {
            var orders = await _context.Orders
                .Include(o => o.Product)
                .Include(o => o.Buyer)
                .Include(o => o.Seller)
                .Where(o => o.BuyerId == buyerId)
                .Select(o => new OrderSummaryDTO
                {
                    Id = o.Id,
                    ProductTitle = o.Product.Title,
                    BuyerName = o.Buyer.UserName,
                    SellerName = o.Seller.UserName,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    CreatedAt = o.CreatedAt
                }).ToListAsync();

            return ApiResponse<List<OrderSummaryDTO>>.SuccessResponse(orders);
        }

        public async Task<ApiResponse<List<OrderSummaryDTO>>> GetOrdersBySellerIdAsync(int sellerId)
        {
            var orders = await _context.Orders
                .Include(o => o.Product)
                .Include(o => o.Buyer)
                .Include(o => o.Seller)
                .Where(o => o.SellerId == sellerId)
                .Select(o => new OrderSummaryDTO
                {
                    Id = o.Id,
                    ProductTitle = o.Product.Title,
                    BuyerName = o.Buyer.UserName,
                    SellerName = o.Seller.UserName,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    CreatedAt = o.CreatedAt
                }).ToListAsync();

            return ApiResponse<List<OrderSummaryDTO>>.SuccessResponse(orders);
        }

        public async Task<ApiResponse<List<OrderSummaryDTO>>> GetAllOrdersAsync()
        {
            var orders = await _context.Orders
                .Include(o => o.Product)
                .Include(o => o.Buyer)
                .Include(o => o.Seller)
                .Select(o => new OrderSummaryDTO
                {
                    Id = o.Id,
                    ProductTitle = o.Product.Title,
                    BuyerName = o.Buyer.UserName,
                    SellerName = o.Seller.UserName,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    CreatedAt = o.CreatedAt
                }).ToListAsync();

            return ApiResponse<List<OrderSummaryDTO>>.SuccessResponse(orders);
        }

        public async Task<ApiResponse<OrderDetailsDTO>> GetOrderByIdAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Product)
                .Include(o => o.Buyer)
                .Include(o => o.Seller)
                .Include(o => o.ShippingInfo)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return ApiResponse<OrderDetailsDTO>.ErrorResponse("Order not found", 404);

            return ApiResponse<OrderDetailsDTO>.SuccessResponse(new OrderDetailsDTO
            {
                Id = order.Id,
                ProductTitle = order.Product.Title,
                BuyerName = order.Buyer.UserName,
                SellerName = order.Seller.UserName,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                CreatedAt = order.CreatedAt,
                ShippingInfo = new ShippingDetailsDto
                {
                    ShippingId = order.ShippingInfo.ShippingId,
                    RecipientName = order.ShippingInfo.RecipientName,
                    Address = order.ShippingInfo.Address,
                    City = order.ShippingInfo.City,
                    PostalCode = order.ShippingInfo.PostalCode,
                    PhoneNumber = order.ShippingInfo.PhoneNumber,
                    Status = order.ShippingInfo.Status,
                    CreateAt = order.ShippingInfo.CreateAt,
                    OrderId = order.ShippingInfo.OrderId
                }

            });
        }

        public async Task<ApiResponse<bool>> CancelOrderAsync(int orderId, int buyerId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null || order.BuyerId != buyerId)
                return ApiResponse<bool>.ErrorResponse("Not authorized or order not found", 403);

            if (order.Status != OrderStatus.Pending)
                return ApiResponse<bool>.ErrorResponse("Only pending orders can be canceled", 400);

            order.Status = OrderStatus.Cancelled;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true);
        }

        public async Task<ApiResponse<bool>> UpdateShippingInfoAsync(int orderId, ShippingInfoDTO dto)
        {
            var order = await _context.Orders
                .Include(o => o.ShippingInfo)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null || order.ShippingInfo == null)
                return ApiResponse<bool>.ErrorResponse("Order or shipping info not found", 404);

            order.ShippingInfo.RecipientName = dto.RecipientName;
            order.ShippingInfo.Address = dto.Address;
            order.ShippingInfo.City = dto.City;
            order.ShippingInfo.PostalCode = dto.PostalCode;
            order.ShippingInfo.PhoneNumber = dto.PhoneNumber;

            _context.ShippingInfos.Update(order.ShippingInfo);
            await _context.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true);
        }


        //public async Task<ApiResponse<bool>> AddReviewToOrderAsync(int orderId, CreateReviewDTO dto, int reviewerId)
        //{
        //    var order = await _context.Orders
        //        .Include(o => o.Reviews)
        //        .FirstOrDefaultAsync(o => o.Id == orderId);

        //    if (order == null)
        //        return ApiResponse<bool>.ErrorResponse("Order not found", 404);

        //    if (order.BuyerId != reviewerId)
        //        return ApiResponse<bool>.ErrorResponse("You are not authorized to review this order", 403);

        //    if (order.Status != OrderStatus.Completed)
        //        return ApiResponse<bool>.ErrorResponse("Order must be completed before review", 400);

        //    if (order.Reviews != null && order.Reviews.Any(r => r.ReviewerId == reviewerId))
        //        return ApiResponse<bool>.ErrorResponse("You already reviewed this order", 409);

        //    var review = new Review
        //    {
        //        Rating = dto.Rating,
        //        Comment = dto.Comment,
        //        ReviewerId = reviewerId,
        //        SellerId = order.SellerId,
        //        ProductId = order.ProductId,
        //        OrderId = orderId,
        //        CreatedAt = DateTime.UtcNow,

        //        Seller = order.Seller,
        //        Reviewer = order.Buyer,
        //        Product = order.Product,
        //        Order = order
        //    };
           

        //    order.Reviews ??= new List<Review>();
        //    order.Reviews.Add(review);

        //    await _context.SaveChangesAsync();

          
        //    await UpdateSellerRatingAsync(dto.SellerId);

        //    return ApiResponse<bool>.SuccessResponse(true, "Review added successfully");
        //}


        //public async Task<ApiResponse<List<ReviewDTO>>> GetReviewsByOrderIdAsync(int orderId)
        //{
        //    var reviews = await _context.Reviews
        //        .Where(r => r.OrderId == orderId)
        //        .Select(r => new ReviewDTO
        //        {
        //            Rating = r.Rating,
        //            Comment = r.Comment,
        //            ReviewerId = r.ReviewerId,
        //            ReviewerName = r.Reviewer.FullName,
        //            CreatedAt = r.CreatedAt
        //        })
        //        .ToListAsync();

        //    return ApiResponse<List<ReviewDTO>>.SuccessResponse(reviews);
        //}

        //// ⏫ تحديث تقييم البائع
        //private async Task UpdateSellerRatingAsync(int sellerId)
        //{
        //    var ratings = await _context.Orders
        //        .Where(o => o.SellerId == sellerId && o.Reviews != null && o.Reviews.Any())
        //        .SelectMany(o => o.Reviews)
        //        .Select(r => r.Rating)
        //        .ToListAsync();

        //    double? averageRating = ratings.Any() ? ratings.Average() : null;

        //    var seller = await _userManager.FindByIdAsync(sellerId.ToString());
        //    if (seller != null)
        //    {
        //        seller.SellerRating = averageRating;
        //        await _userManager.UpdateAsync(seller);
        //    }
        //}
    }
}
