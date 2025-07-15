using Bikya.Data.Models;
using Bikya.Data.Response;
using Bikya.DTOs.AuthDTOs;
using Bikya.DTOs.UserDTOs;
using Bikya.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using System.Threading.Tasks;

namespace Bikya.Services.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly JwtSettings _jwtSettings;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager,
            IOptions<JwtSettings> jwtSettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto dto)
        {
            if (dto.Password != dto.ConfirmPassword)
                return ApiResponse<AuthResponseDto>.ErrorResponse("Passwords do not match", 400);

            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return ApiResponse<AuthResponseDto>.ErrorResponse("User already exists", 400);

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ApiResponse<AuthResponseDto>.ErrorResponse("Registration failed", 400, errors);
            }

            if (!await _roleManager.RoleExistsAsync("User"))
                await _roleManager.CreateAsync(new ApplicationRole { Name = "User" });

            await _userManager.AddToRoleAsync(user, "User");

            var token = await GenerateJwtToken(user);
            var response = new AuthResponseDto
            {
                Token = token,
                FullName = user.FullName,
                Email = user.Email,
                UserId = user.Id,
                UserName = user.UserName ?? ""
            };

            return ApiResponse<AuthResponseDto>.SuccessResponse(response, "Registration successful");
        }


        public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return ApiResponse<AuthResponseDto>.ErrorResponse("Invalid login credentials", 401);

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded)
                return ApiResponse<AuthResponseDto>.ErrorResponse("Invalid login credentials", 401);

            var token = await GenerateJwtToken(user);
            var response = new AuthResponseDto
            {
                Token = token,
                FullName = user.FullName,
                Email = user.Email,
                UserId = user.Id,
                UserName = user.UserName ?? ""
            };

            return ApiResponse<AuthResponseDto>.SuccessResponse(response, "Login successful");
        }
        public async Task<ApiResponse<UserProfileDto>> GetProfileAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
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
                ProfileImageUrl = user.ProfileImageUrl,
                IsVerified = user.IsVerified,
                PhoneNumber = user.PhoneNumber,
                SellerRating = user.SellerRating,


                Roles = roles.ToList()
            });
        }
        public async Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(RefreshTokenDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return ApiResponse<AuthResponseDto>.ErrorResponse("Invalid refresh attempt", 401);

            var token = await GenerateJwtToken(user);
            var roles = await _userManager.GetRolesAsync(user);

            return ApiResponse<AuthResponseDto>.SuccessResponse(new AuthResponseDto
            {
                Token = token,
                FullName = user.FullName,
                Email = user.Email,
                UserName = user.UserName,
                UserId = user.Id,
                Roles = roles.ToList(),
                Expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes)
            });
        }


        public async Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                return ApiResponse<string>.SuccessResponse("If the email exists, a reset link has been sent.");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var resetUrl = $"http://localhost:4200/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(dto.Email)}";

            Console.WriteLine($"Reset Link: {resetUrl}");

            return ApiResponse<string>.SuccessResponse("Reset password link sent.");
        }
        public async Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return ApiResponse<string>.ErrorResponse("Invalid email.", 400);

            var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ApiResponse<string>.ErrorResponse("Reset failed.", 400, errors);
            }

            return ApiResponse<string>.SuccessResponse("Password reset successfully.");
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Name, user.UserName ?? ""),
                new Claim("UserId", user.Id.ToString()),
                new Claim("FullName", user.FullName ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(tokenDescriptor);
            return handler.WriteToken(token);
        }
    }
}
