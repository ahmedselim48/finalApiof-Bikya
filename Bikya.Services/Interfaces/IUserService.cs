using Bikya.Data.Response;
using Bikya.DTOs.AuthDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bikya.DTOs.UserDTOs;
namespace Bikya.Services.Interfaces
{
    public  interface  IUserService
    {
        Task<ApiResponse<UserProfileDto>> GetByIdAsync(int id);

        Task<ApiResponse<bool>> UpdateProfileAsync(int userId, UpdateProfileDto dto);

        Task<ApiResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordDto dto);
        Task<ApiResponse<bool>> DeactivateAccountAsync(int userId);



    }
}
