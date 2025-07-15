using Bikya.Data.Models;
using Bikya.Data.Response;
using Bikya.DTOs.AuthDTOs;
using Bikya.DTOs.UserDTOs;
using Bikya.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bikya.Services.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ApiResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return ApiResponse<bool>.ErrorResponse("User not found", 404);

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
                return ApiResponse<bool>.ErrorResponse("Incorrect password or invalid new one", 400);

            return ApiResponse<bool>.SuccessResponse(true, "Password changed");
        }

        public async Task<ApiResponse<bool>> DeactivateAccountAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return ApiResponse<bool>.ErrorResponse("User not found", 404);

            user.IsDeleted = true;
            await _userManager.UpdateAsync(user);
            return ApiResponse<bool>.SuccessResponse(true, "Account deactivated");
        }

        public async Task<ApiResponse<UserProfileDto>> GetByIdAsync(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return ApiResponse<UserProfileDto>.ErrorResponse("User not found", 404);

            var roles = await _userManager.GetRolesAsync(user);

            return ApiResponse<UserProfileDto>.SuccessResponse(new UserProfileDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                UserName = user.UserName,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber,
                SellerRating = user.SellerRating,


                ProfileImageUrl = user.ProfileImageUrl,
                IsVerified = user.IsVerified,
                Roles = roles.ToList()
            });
        }

        public async Task<ApiResponse<bool>> UpdateProfileAsync(int userId, UpdateProfileDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return ApiResponse<bool>.ErrorResponse("User not found", 404);

            user.FullName = dto.FullName;
            user.ProfileImageUrl = dto.ProfileImageUrl;
            user.Address = dto.Address;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return ApiResponse<bool>.ErrorResponse("Failed to update", 400);

            return ApiResponse<bool>.SuccessResponse(true, "Profile updated");
        }
    }
}
