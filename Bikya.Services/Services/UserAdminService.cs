using Bikya.Data.Models;
using Bikya.Data.Response;
using Bikya.DTOs.AuthDTOs;
using Bikya.DTOs.UserDTOs;
using Bikya.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bikya.Services.Services
{
    public class UserAdminService : IUserAdminService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserAdminService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ApiResponse<List<UserProfileDto>>> GetAllUsersAsync(string? search, string? status)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));

            if (status == "active")
                query = query.Where(u => !u.LockoutEnabled && !u.IsDeleted);
            else if (status == "inactive")
                query = query.Where(u => u.LockoutEnabled || u.IsDeleted);

            var users = await query.Select(u => new UserProfileDto
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.FullName,
                UserName = u.UserName
            }).ToListAsync();

            return ApiResponse<List<UserProfileDto>>.SuccessResponse(users);
        }

        public async Task<ApiResponse<List<UserProfileDto>>> GetActiveUsersAsync()
        {
            return await GetAllUsersAsync(null, "active");
        }

        public async Task<ApiResponse<List<UserProfileDto>>> GetInactiveUsersAsync()
        {
            return await GetAllUsersAsync(null, "inactive");
        }

        public async Task<ApiResponse<int>> GetUsersCountAsync()
        {
            int count = await _userManager.Users.CountAsync();
            return ApiResponse<int>.SuccessResponse(count);
        }

        public async Task<ApiResponse<string>> UpdateUserAsync(int id, UpdateUserDto dto)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                return ApiResponse<string>.ErrorResponse("User not found", 404);

            user.FullName = dto.FullName ?? user.FullName;
            user.PhoneNumber = dto.PhoneNumber ?? user.PhoneNumber;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = updateResult.Errors.Select(e => e.Description).ToList();
                return ApiResponse<string>.ErrorResponse("Failed to update user.", 400, errors);
            }

            // Role update
            if (!string.IsNullOrWhiteSpace(dto.Role))
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    var errors = removeResult.Errors.Select(e => e.Description).ToList();
                    return ApiResponse<string>.ErrorResponse("Failed to remove old roles.", 400, errors);
                }

                var addResult = await _userManager.AddToRoleAsync(user, dto.Role);
                if (!addResult.Succeeded)
                {
                    var errors = addResult.Errors.Select(e => e.Description).ToList();
                    return ApiResponse<string>.ErrorResponse("Failed to assign new role.", 400, errors);
                }
            }

            return ApiResponse<string>.SuccessResponse("User updated successfully.");
        }


        public async Task<ApiResponse<string>> DeleteUserAsync(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return ApiResponse<string>.ErrorResponse("User not found", 404);

            user.IsDeleted = true;
            await _userManager.UpdateAsync(user);

            return ApiResponse<string>.SuccessResponse("User deleted.");
        }

        public async Task<ApiResponse<string>> ReactivateUserAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return ApiResponse<string>.ErrorResponse("User not found", 404);

            user.IsDeleted = false;
            user.LockoutEnd = null;
            await _userManager.UpdateAsync(user);

            return ApiResponse<string>.SuccessResponse("User reactivated.");
        }

        public async Task<ApiResponse<string>> LockUserAsync(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return ApiResponse<string>.ErrorResponse("User not found", 404);

            user.LockoutEnd = DateTimeOffset.MaxValue;
            await _userManager.UpdateAsync(user);
            return ApiResponse<string>.SuccessResponse("User locked.");
        }

        public async Task<ApiResponse<string>> UnlockUserAsync(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return ApiResponse<string>.ErrorResponse("User not found", 404);

            user.LockoutEnd = null;
            await _userManager.UpdateAsync(user);
            return ApiResponse<string>.SuccessResponse("User unlocked.");
        }
    }
}
