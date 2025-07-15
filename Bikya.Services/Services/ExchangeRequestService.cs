using Bikya.Data;
using Bikya.Data.Enums;
using Bikya.Data.Models;
using Bikya.Data.Response;
using Bikya.DTOs.ExchangeRequestDTOs;
using Bikya.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bikya.Services.Services
{
    public class ExchangeRequestService : IExchangeRequestService
    {
        private readonly BikyaContext _context;

        public ExchangeRequestService(BikyaContext context)
        {
            _context = context;
        }

      
        public async Task<ApiResponse<ExchangeRequestDTO>> CreateAsync(CreateExchangeRequestDTO dto, int senderUserId)
        {
            var offeredProduct = await _context.Products.FindAsync(dto.OfferedProductId);
            var requestedProduct = await _context.Products.FindAsync(dto.RequestedProductId);

            if (offeredProduct == null || requestedProduct == null)
                return ApiResponse<ExchangeRequestDTO>.ErrorResponse("One or both products not found", 404);

            if (offeredProduct.UserId != senderUserId)
                return ApiResponse<ExchangeRequestDTO>.ErrorResponse("You can only offer your own product", 403);

            var request = new ExchangeRequest
            {
                OfferedProductId = dto.OfferedProductId,
                RequestedProductId = dto.RequestedProductId,
                Message = dto.Message,
                Status = ExchangeStatus.Pending,
                RequestedAt = DateTime.UtcNow
            };

            await _context.ExchangeRequests.AddAsync(request);
            await _context.SaveChangesAsync();

            return ApiResponse<ExchangeRequestDTO>.SuccessResponse(ToDTO(request), "Exchange request created successfully", 201);
        }

        public async Task<ApiResponse<ExchangeRequestDTO>> GetByIdAsync(int id)
        {
            var request = await _context.ExchangeRequests
                .Include(e => e.OfferedProduct)
                .Include(e => e.RequestedProduct)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (request == null)
                return ApiResponse<ExchangeRequestDTO>.ErrorResponse("Request not found", 404);

            return ApiResponse<ExchangeRequestDTO>.SuccessResponse(ToDTO(request));
        }

        public async Task<ApiResponse<List<ExchangeRequestDTO>>> GetAllAsync()
        {
            var requests = await _context.ExchangeRequests
                .Include(e => e.OfferedProduct)
                .Include(e => e.RequestedProduct)
                .ToListAsync();

            return ApiResponse<List<ExchangeRequestDTO>>.SuccessResponse(requests.Select(ToDTO).ToList());
        }

        public async Task<ApiResponse<List<ExchangeRequestDTO>>> GetSentRequestsAsync(int senderUserId)
        {
            var requests = await _context.ExchangeRequests
                .Include(e => e.OfferedProduct)
                .Include(e => e.RequestedProduct)
                .Where(e => e.OfferedProduct.UserId == senderUserId)
                .ToListAsync();

            return ApiResponse<List<ExchangeRequestDTO>>.SuccessResponse(requests.Select(ToDTO).ToList());
        }

        public async Task<ApiResponse<List<ExchangeRequestDTO>>> GetReceivedRequestsAsync(int receiverUserId)
        {
            var requests = await _context.ExchangeRequests
                .Include(e => e.OfferedProduct)
                .Include(e => e.RequestedProduct)
                .Where(e => e.RequestedProduct.UserId == receiverUserId)
                .ToListAsync();

            return ApiResponse<List<ExchangeRequestDTO>>.SuccessResponse(requests.Select(ToDTO).ToList());
        }

        public async Task<ApiResponse<ExchangeRequestDTO>> ApproveRequestAsync(int requestId, int currentUserId)
        {
            var request = await _context.ExchangeRequests
                .Include(e => e.RequestedProduct)
                .FirstOrDefaultAsync(e => e.Id == requestId);

            if (request == null)
                return ApiResponse<ExchangeRequestDTO>.ErrorResponse("Request not found", 404);

            if (request.RequestedProduct.UserId != currentUserId)
                return ApiResponse<ExchangeRequestDTO>.ErrorResponse("You are not authorized to approve this request", 403);

            request.Status = ExchangeStatus.Accepted;
            await _context.SaveChangesAsync();

            return ApiResponse<ExchangeRequestDTO>.SuccessResponse(ToDTO(request), "Request approved");
        }

       
        public async Task<ApiResponse<ExchangeRequestDTO>> RejectRequestAsync(int requestId, int currentUserId)
        {
            var request = await _context.ExchangeRequests
                .Include(e => e.RequestedProduct)
                .FirstOrDefaultAsync(e => e.Id == requestId);

            if (request == null)
                return ApiResponse<ExchangeRequestDTO>.ErrorResponse("Request not found", 404);

            if (request.RequestedProduct.UserId != currentUserId)
                return ApiResponse<ExchangeRequestDTO>.ErrorResponse("You are not authorized to reject this request", 403);

            request.Status = ExchangeStatus.Rejected;
            await _context.SaveChangesAsync();

            return ApiResponse<ExchangeRequestDTO>.SuccessResponse(ToDTO(request), "Request rejected");
        }

       
        public async Task<ApiResponse<bool>> DeleteAsync(int requestId, int currentUserId)
        {
            var request = await _context.ExchangeRequests
                .Include(e => e.OfferedProduct)
                .Include(e => e.RequestedProduct)
                .FirstOrDefaultAsync(e => e.Id == requestId);

            if (request == null)
                return ApiResponse<bool>.ErrorResponse("Request not found", 404);

            if (request.OfferedProduct.UserId != currentUserId && request.RequestedProduct.UserId != currentUserId)
                return ApiResponse<bool>.ErrorResponse("You are not authorized to delete this request", 403);

            _context.ExchangeRequests.Remove(request);
            await _context.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Request deleted");
        }

      
        private ExchangeRequestDTO ToDTO(ExchangeRequest request)
        {
            return new ExchangeRequestDTO
            {
                Id = request.Id,
                OfferedProductId = request.OfferedProductId,
                RequestedProductId = request.RequestedProductId,
                Status = request.Status.ToString(),
                Message = request.Message,
                RequestedAt = request.RequestedAt
            };
        }
    }
}
