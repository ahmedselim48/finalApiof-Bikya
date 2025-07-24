using Bikya.Data.Response;
using Bikya.DTOs.ReviewDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bikya.Services.Interfaces
{
    public interface IReviewService
    {
        Task<ApiResponse<List<ReviewDTO>>> GetAllAsync();
        Task<ApiResponse<ReviewDTO>> GetByIdAsync(int id);
        Task<ApiResponse<ReviewDTO>> AddAsync(CreateReviewDTO dto);
        Task<ApiResponse<ReviewDTO>> UpdateAsync(UpdateReviewDTO dto);
        Task<ApiResponse<object>> DeleteAsync(int id);
        Task<ApiResponse<List<ReviewDTO>>> GetReviewsForSellerAsync(int sellerId);
        Task<double> GetAverageRatingBySellerIdAsync(int sellerId);

        //Task<ApiResponse<List<ReviewDTO>>> GetReviewsBySellerIdAsync(int sellerId);
        //Task<ApiResponse<List<ReviewDTO>>> GetReviewsByReviewerIdAsync(int reviewerId);
        //Task<ApiResponse<List<ReviewDTO>>> GetReviewsByOrderIdAsync(int orderId);




    }
}
