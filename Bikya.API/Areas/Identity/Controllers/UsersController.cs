using Bikya.DTOs.UserDTOs;
using Bikya.Services.Interfaces;
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

        public UsersController(IUserService userService)
        {
            _userService = userService;
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

    }
}
