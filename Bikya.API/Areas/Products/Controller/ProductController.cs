using Bikya.Data.Models;
using Bikya.Data.Response;
using Bikya.DTOs.ProductDTO;
using Bikya.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bikya.API.Areas.Products.Controller
{
    [Area("Products")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        #region Services
        private readonly ProductService productService;
        private readonly ProductImageService productImageService;
        private readonly IWebHostEnvironment env;

        public ProductController(IWebHostEnvironment env, ProductService productService, ProductImageService productImageService)
        {
            this.productService = productService;
            this.productImageService = productImageService;
            this.env = env;
        }
        #endregion

        #region Helper
        private bool TryGetUserId(out int userId)
        {
            return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
        }
        #endregion

        #region Admin
        [Authorize(Roles = "Admin")]
        [HttpGet("AllProduct")]
        public async Task<IActionResult> GetAllProductsWithImages()
        {
            try
            {
                var products = await productService.GetAllProductsWithImagesAsync();
                return Ok(ApiResponse<IEnumerable<Product>>.SuccessResponse(products));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResponse("Server error", 500, new List<string> { ex.Message }));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("NotApprovedProducts")]
        public async Task<IActionResult> GetNotApprovedProductsWithImages()
        {
            try
            {
                var products = await productService.GetNotApprovedProductsWithImagesAsync();
                return Ok(ApiResponse<IEnumerable<Product>>.SuccessResponse(products));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResponse("Server error", 500, new List<string> { ex.Message }));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("ApproveProduct/{id}")]
        public async Task<IActionResult> ApproveProductAsync(int id)
        {
            try
            {
                await productService.ApproveProductAsync(id);
                return Ok(ApiResponse<bool>.SuccessResponse(true));
            }
            catch (ArgumentException ex)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse(ex.Message, 404));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResponse("Server error", 500, new List<string> { ex.Message }));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("RejectProduct/{id}")]
        public async Task<IActionResult> RejectProductAsync(int id)
        {
            try
            {
                await productService.RejectProductAsync(id);
                return Ok(ApiResponse<bool>.SuccessResponse(true));
            }
            catch (ArgumentException ex)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse(ex.Message, 404));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResponse("Server error", 500, new List<string> { ex.Message }));
            }
        }
        #endregion

        #region GET
        [HttpGet("ApprovedProducts")]
        public async Task<IActionResult> GetApprovedProductsWithImages()
        {
            try
            {
                var products = await productService.GetApprovedProductsWithImagesAsync();
                return Ok(ApiResponse<IEnumerable<Product>>.SuccessResponse(products));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResponse("Server error", 500, new List<string> { ex.Message }));
            }
        }

        [HttpGet("ApprovedProducts/{id}")]
        public async Task<IActionResult> GetProductByIdAsync(int id)
        {
            try
            {
                var product = await productService.GetProductWithImagesByIdAsync(id);
                return Ok(ApiResponse<Product>.SuccessResponse(product));
            }
            catch (ArgumentException ex)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse(ex.Message, 404));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResponse("Server error", 500, new List<string> { ex.Message }));
            }
        }

        [HttpGet("UserProduct/{userId}")]
        public async Task<IActionResult> GetProductByUserAsync(int userId)
        {
            try
            {
                var products = await productService.GetProductsByUserAsync(userId);
                return Ok(ApiResponse<IEnumerable<Product>>.SuccessResponse(products));
            }
            catch (ArgumentException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResponse(ex.Message, 404));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResponse("Server error", 500, new List<string> { ex.Message }));
            }
        }

        [HttpGet("UserNotApprovedProduct/{userId}")]
        public async Task<IActionResult> GetNotApprovedProductByUserAsync(int userId)
        {
            try
            {
                var products = await productService.GetNotApprovedProductByUserAsync(userId);
                return Ok(ApiResponse<IEnumerable<Product>>.SuccessResponse(products));
            }
            catch (ArgumentException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResponse(ex.Message, 404));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResponse("Server error", 500, new List<string> { ex.Message }));
            }
        }

        [HttpGet("CategoryProducts/{id}")]
        public async Task<IActionResult> GetProductsByCategoryAsync(int id)
        {
            var products = await productService.GetProductsByCategoryAsync(id);
            return Ok(ApiResponse<IEnumerable<Product>>.SuccessResponse(products));
        }
        #endregion

        #region CRUD
        [Authorize]
        [HttpPost("Add")]
        public async Task<IActionResult> CreateProductAsync([FromBody] ProductDTO product)
        {
            if (!TryGetUserId(out int userId))
                return Unauthorized(ApiResponse<string>.ErrorResponse("Unauthorized", 401));

            try
            {
                await productService.CreateProductAsync(product, userId);
                return Ok(ApiResponse<bool>.SuccessResponse(true));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message, 400));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResponse("Server error", 500, new List<string> { ex.Message }));
            }
        }

        [Authorize]
        [Consumes("multipart/form-data")]
        [HttpPost("AddWithImages")]
        public async Task<IActionResult> CreateProductWithImagesAsync([FromForm] CreateProductWithimagesDTO productDTO)
        {
            if (!TryGetUserId(out int userId))
                return Unauthorized(ApiResponse<string>.ErrorResponse("Unauthorized", 401));

            try
            {
                var product = new ProductDTO
                {
                    Title = productDTO.Title,
                    Description = productDTO.Description,
                    Price = productDTO.Price,
                    IsForExchange = productDTO.IsForExchange,
                    Condition = productDTO.Condition,
                    CategoryId = productDTO.CategoryId
                };

                var createdProduct = await productService.CreateProductAsync(product, userId);
                var rootPath = env.WebRootPath;

                if (productDTO.MainImage != null)
                {
                    await productImageService.AddProductImageAsync(new ProductImageDTO
                    {
                        ProductId = createdProduct.Id,
                        Image = productDTO.MainImage,
                        IsMain = true
                    }, rootPath);
                }

                if (productDTO.AdditionalImages?.Any() == true)
                {
                    foreach (var image in productDTO.AdditionalImages)
                    {
                        await productImageService.AddProductImageAsync(new ProductImageDTO
                        {
                            ProductId = createdProduct.Id,
                            Image = image,
                            IsMain = false
                        }, rootPath);
                    }
                }

                return Ok(ApiResponse<bool>.SuccessResponse(true));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message, 400));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResponse("Server error", 500, new List<string> { ex.Message }));
            }
        }

        [Authorize]
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> UpdateProductAsync(int id, [FromBody] ProductDTO product)
        {
            if (!TryGetUserId(out int userId))
                return Unauthorized(ApiResponse<string>.ErrorResponse("Unauthorized", 401));

            try
            {
                await productService.UpdateProductAsync(id, product, userId);
                return Ok(ApiResponse<bool>.SuccessResponse(true));
            }
            catch (ArgumentException ex)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse(ex.Message, 404));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<bool>.ErrorResponse(ex.Message, 401));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResponse("Server error", 500, new List<string> { ex.Message }));
            }
        }

        [Authorize]
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteProductAsync(int id)
        {
            if (!TryGetUserId(out int userId))
                return Unauthorized(ApiResponse<string>.ErrorResponse("Unauthorized", 401));

            try
            {
                var rootPath = env.WebRootPath;
                await productService.DeleteProductAsync(id, userId, rootPath);
                return Ok(ApiResponse<bool>.SuccessResponse(true));
            }
            catch (ArgumentException ex)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse(ex.Message, 404));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<bool>.ErrorResponse(ex.Message, 401));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResponse("Server error", 500, new List<string> { ex.Message }));
            }
        }
        #endregion

        #region Images
        [Authorize]
        [HttpPost("Product/{productId}/AddImage")]
        public async Task<IActionResult> AddImageAsync(int productId, [FromForm] CreateImageDTO dto)
        {
            try
            {
                var rootPath = env.WebRootPath;

                if (dto.Image != null)
                {
                    await productImageService.AddProductImageAsync(new ProductImageDTO
                    {
                        ProductId = productId,
                        Image = dto.Image,
                        IsMain = dto.IsMain
                    }, rootPath);
                }

                return Ok(ApiResponse<bool>.SuccessResponse(true));
            }
            catch (ArgumentException ex)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse(ex.Message, 404));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResponse("Server error", 500, new List<string> { ex.Message }));
            }
        }

        [Authorize]
        [HttpDelete("DeleteImage/{id}")]
        public async Task<IActionResult> DeleteProductImageAsync(int id)
        {
            if (!TryGetUserId(out int userId))
                return Unauthorized(ApiResponse<string>.ErrorResponse("Unauthorized", 401));

            try
            {
                var rootPath = env.WebRootPath;
                await productImageService.DeleteProductImageAsync(id, userId, rootPath);
                return Ok(ApiResponse<bool>.SuccessResponse(true));
            }
            catch (ArgumentException ex)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse(ex.Message, 404));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResponse("Server error", 500, new List<string> { ex.Message }));
            }
        }
        #endregion
    }
}
