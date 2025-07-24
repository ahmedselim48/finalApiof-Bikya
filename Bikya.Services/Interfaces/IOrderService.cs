using Bikya.Data.Response;
using Bikya.DTOs.Orderdto;
using Bikya.DTOs.ReviewDTOs;
using Bikya.DTOs.ShippingDTOs;


namespace Bikya.Services.Interfaces
{
    public interface IOrderService
    {
       
        Task<ApiResponse<OrderDTO>> CreateOrderAsync(CreateOrderDTO dto);

        Task<ApiResponse<bool>> UpdateOrderStatusAsync(UpdateOrderStatusDTO dto);

        Task<ApiResponse<List<OrderSummaryDTO>>> GetOrdersByUserIdAsync(int userId);

        Task<ApiResponse<List<OrderSummaryDTO>>> GetOrdersByBuyerIdAsync(int buyerId);

        
        Task<ApiResponse<List<OrderSummaryDTO>>> GetOrdersBySellerIdAsync(int sellerId);

    
        Task<ApiResponse<List<OrderSummaryDTO>>> GetAllOrdersAsync();

  
        Task<ApiResponse<OrderDetailsDTO>> GetOrderByIdAsync(int orderId);

      
        Task<ApiResponse<bool>> CancelOrderAsync(int orderId, int buyerId);

      
        Task<ApiResponse<bool>> UpdateShippingInfoAsync(int orderId, ShippingInfoDTO dto);

        Task<int> CountUserSalesAsync(int userId);

        Task<int> CountUserOrdersAsync(int userId);

    //    Task<ApiResponse<bool>> AddReviewToOrderAsync(int orderId, CreateReviewDTO dto, int reviewerId);


        //    Task<ApiResponse<List<ReviewDTO>>> GetReviewsByOrderIdAsync(int orderId);
        //
    }
}
