using Bikya.Data.Response;
using Bikya.DTOs.UserDTOs;
using Bikya.Services.Interfaces;
using Bikya.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bikya.API.Areas.Identity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ProductService _productService;
        private readonly IOrderService _orderService;

        public UsersController(IUserService userService , IOrderService orderService)
        {
            _userService = userService;
            _orderService = orderService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var result = await _userService.GetByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }
        [Authorize]


        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(UpdateProfileDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _userService.UpdateProfileAsync(userId, dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _userService.ChangePasswordAsync(userId, dto);
            return StatusCode(result.StatusCode, result);
        }

       
        [HttpDelete("deactivate")]
        public async Task<IActionResult> Deactivate()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized("User not authenticated");

            if (!int.TryParse(userIdStr, out int userId))
                return BadRequest("Invalid user ID");

            var result = await _userService.DeactivateAccountAsync(userId);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("stats/{userId}")]
        public async Task<ActionResult<ApiResponse<UserStatsDTO>>> GetUserStats(int userId)
        {
            var totalProducts = await _productService.CountUserProductsAsync(userId);
            var totalOrders = await _orderService.CountUserOrdersAsync(userId);
            var totalSales = await _orderService.CountUserSalesAsync(userId);

            var stats = new UserStatsDTO
            {
                TotalProducts = totalProducts,
                TotalOrders = totalOrders,
                TotalSales = totalSales
            };

            return Ok(new ApiResponse<UserStatsDTO>
            {
                Success = true,
                Message = "User stats loaded",
                Data = stats
            });
        }

    }
}
