using Bikya.Data;
using Bikya.Data.Models;
using Bikya.Data.Response;
using Bikya.DTOs.ShippingDTOs;
using Bikya.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bikya.Services.Services
{
    public class ShippingService : IShippingService
    {
        private readonly BikyaContext context;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public ShippingService(BikyaContext context, IHttpContextAccessor httpContextAccessor)
        {
            this.context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ApiResponse<List<ShippingDetailsDto>>> GetAllAsync()
        {
            var data = await context.ShippingInfos
                .Select(s => new ShippingDetailsDto
                {
                    ShippingId = s.ShippingId,
                    RecipientName = s.RecipientName,
                    Address = s.Address,
                    City = s.City,
                    PostalCode = s.PostalCode,
                    PhoneNumber = s.PhoneNumber,
                    Status = s.Status,
                    CreateAt = s.CreateAt,
                    OrderId = s.OrderId
                })
                .ToListAsync();

            return ApiResponse<List<ShippingDetailsDto>>.SuccessResponse(data);
        }

        public async Task<ApiResponse<ShippingDetailsDto>> GetByIdAsync(int id)
        {
            var s = await context.ShippingInfos.FindAsync(id);
            if (s == null)
                return ApiResponse<ShippingDetailsDto>.ErrorResponse("Shipping not found", 404);

            var dto = new ShippingDetailsDto
            {
                ShippingId = s.ShippingId,
                RecipientName = s.RecipientName,
                Address = s.Address,
                City = s.City,
                PostalCode = s.PostalCode,
                PhoneNumber = s.PhoneNumber,
                Status = s.Status,
                CreateAt = s.CreateAt,
                OrderId = s.OrderId
            };

            return ApiResponse<ShippingDetailsDto>.SuccessResponse(dto);
        }

        public async Task<ApiResponse<ShippingDetailsDto>> CreateAsync(CreateShippingDto dto)
        {
            // 1. تحقق من وجود الأوردر
            var order = await context.Orders.FindAsync(dto.OrderId);
            if (order == null)
                return ApiResponse<ShippingDetailsDto>.ErrorResponse("Order not found", 404);

            // 2. استخراج UserId من الـ Claims
            var userIdStr = _httpContextAccessor?.HttpContext?.User?.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdStr, out var userId))
                return ApiResponse<ShippingDetailsDto>.ErrorResponse("Unauthorized", 401);

            // 3. تحقق إن الأوردر تابع للمستخدم اللي بيطلب الشحن
            if (order.BuyerId != userId)
                return ApiResponse<ShippingDetailsDto>.ErrorResponse("You are not allowed to create a shipment for this order", 403);

            // 4. أنشئ الشحنة
            var shipping = new ShippingInfo
            {
                RecipientName = dto.RecipientName,
                Address = dto.Address,
                City = dto.City,
                PostalCode = dto.PostalCode,
                PhoneNumber = dto.PhoneNumber,
                OrderId = dto.OrderId,
                Status = ShippingStatus.Pending,
                CreateAt = DateTime.UtcNow
            };

            context.ShippingInfos.Add(shipping);
            await context.SaveChangesAsync();

            var result = new ShippingDetailsDto
            {
                ShippingId = shipping.ShippingId,
                RecipientName = shipping.RecipientName,
                Address = shipping.Address,
                City = shipping.City,
                PostalCode = shipping.PostalCode,
                PhoneNumber = shipping.PhoneNumber,
                Status = shipping.Status,
                CreateAt = shipping.CreateAt,
                OrderId = shipping.OrderId
            };

            return ApiResponse<ShippingDetailsDto>.SuccessResponse(result, "Shipping created successfully", 201);
        }


        public async Task<ApiResponse<bool>> UpdateStatusAsync(int id, UpdateShippingStatusDto dto)
        {
            var shipping = await context.ShippingInfos.FindAsync(id);
            if (shipping == null)
                return ApiResponse<bool>.ErrorResponse("Shipping not found", 404);

            shipping.Status = dto.Status;
            await context.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Shipping status updated");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var shipping = await context.ShippingInfos.FindAsync(id);
            if (shipping == null)
                return ApiResponse<bool>.ErrorResponse("Shipping not found", 404);

            context.ShippingInfos.Remove(shipping);
            await context.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Shipping deleted");
        }

        public async Task<ApiResponse<TrackShipmentDto>> TrackAsync(string trackingNumber)
        {
            var shipping = await context.ShippingInfos
                .FirstOrDefaultAsync(s => s.ShippingId.ToString() == trackingNumber);

            if (shipping == null)
                return ApiResponse<TrackShipmentDto>.ErrorResponse("Tracking number not found", 404);

            var dto = new TrackShipmentDto
            {
                TrackingNumber = trackingNumber,
                Status = shipping.Status,
                LastLocation = "Warehouse", // تقدر تحدثها بناءً على جدول منفصل لو عندك
                EstimatedArrival = DateTime.UtcNow.AddDays(3)
            };

            return ApiResponse<TrackShipmentDto>.SuccessResponse(dto);
        }

        public async Task<ApiResponse<ShippingCostResponseDto>> CalculateCostAsync(ShippingCostRequestDto dto)
        {
            double ratePerKg = dto.Method == "Express" ? 20.0 : 10.0;
            double cost = dto.Weight * ratePerKg;

            var result = new ShippingCostResponseDto
            {
                Cost = cost,
                EstimatedDeliveryDate = DateTime.UtcNow.AddDays(dto.Method == "Express" ? 1 : 4)
            };

            return ApiResponse<ShippingCostResponseDto>.SuccessResponse(result);
        }

        public async Task<ApiResponse<bool>> IntegrateWithProviderAsync(ThirdPartyShippingRequestDto dto)
        {
            // تمثيل فقط – تقدر تبني هنا ربط حقيقي مع شركة شحن
            if (dto.Provider.ToLower() == "aramex")
            {
                // simulate sending request
                return ApiResponse<bool>.SuccessResponse(true, "Integrated with Aramex");
            }

            return ApiResponse<bool>.ErrorResponse("Provider not supported", 400);
        }

        public async Task<ApiResponse<bool>> HandleWebhookAsync(string provider, ShippingWebhookDto dto)
        {
            var shipping = await context.ShippingInfos
                .FirstOrDefaultAsync(s => s.ShippingId.ToString() == dto.TrackingNumber);

            if (shipping == null)
                return ApiResponse<bool>.ErrorResponse("Shipping not found for webhook", 404);

            shipping.Status = dto.NewStatus;
            await context.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Shipping status updated via webhook");
        }
    }
}
