using Bikya.DTOs.UserDTOs;
using Bikya.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bikya.API.Areas.Identity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Area("Identity")]
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : ControllerBase
    {
        private readonly IUserAdminService _adminService;

        public AdminUsersController(IUserAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? status)
        {
            var result = await _adminService.GetAllUsersAsync(search, status);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
        {
            var result = await _adminService.GetActiveUsersAsync();
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("inactive")]
        public async Task<IActionResult> GetInactive()
        {
            var result = await _adminService.GetInactiveUsersAsync();
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetCount()
        {
            var result = await _adminService.GetUsersCountAsync();
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
        {
            var result = await _adminService.UpdateUserAsync(id, dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _adminService.DeleteUserAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("reactivate")]
        public async Task<IActionResult> Reactivate([FromQuery] string email)
        {
            var result = await _adminService.ReactivateUserAsync(email);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{id}/lock")]
        public async Task<IActionResult> Lock(int id)
        {
            var result = await _adminService.LockUserAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{id}/unlock")]
        public async Task<IActionResult> Unlock(int id)
        {
            var result = await _adminService.UnlockUserAsync(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}
