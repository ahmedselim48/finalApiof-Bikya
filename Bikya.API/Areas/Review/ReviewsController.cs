using Bikya.Data.Response;
using Bikya.DTOs.ReviewDTOs;
using Bikya.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Bikya.API.Areas.Review
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _service;

        public ReviewsController(IReviewService service)
        {
            _service = service;
        }

        // GET: api/Reviews
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var response = await _service.GetAllAsync();
            return StatusCode(response.StatusCode, response);
        }

        //GET: api/seller/id
        [HttpGet("seller/{sellerId}")]
        public async Task<IActionResult> GetReviewsForSeller(int sellerId)
        {
            var response = await _service.GetReviewsForSellerAsync(sellerId);
            return StatusCode(response.StatusCode, response);
        }

        // GET: api/Reviews/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _service.GetByIdAsync(id);
            return StatusCode(response.StatusCode, response);
        }


        //POST: api/Reviews
       [HttpPost]
        public async Task<IActionResult> Add([FromBody] CreateReviewDTO dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(ApiResponse<ReviewDTO>.ErrorResponse(string.Join(" | ", errors), 400));
            }

            var response = await _service.AddAsync(dto);
            return StatusCode(response.StatusCode, response);
        }

        // PUT: api/Reviews/5
        [HttpPut("{id:int}")]

        public async Task<IActionResult> Update([FromBody] UpdateReviewDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<ReviewDTO>.ErrorResponse("Invalid review data", 400));

            var response = await _service.UpdateAsync(dto);
            return StatusCode(response.StatusCode, response);
        }
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _service.DeleteAsync(id);
            return StatusCode(response.StatusCode, response);
        }


        [HttpGet("AverageRating/{sellerId}")]
        public async Task<ActionResult<ApiResponse<double>>> GetAverageRating(int sellerId)
        {
            var average = await _service.GetAverageRatingBySellerIdAsync(sellerId);

            return Ok(new ApiResponse<double>
            {
                Success = true,
                Message = "Average rating loaded successfully",
                Data = average
            });
        }


        // هنستخدم دول ليه اصلا؟؟

        //public async Task<IActionResult> Update(int id, [FromBody] UpdateReviewDTO dto)
        //{
        //    if (id != dto.Id)
        //        return BadRequest(ApiResponse<object>.ErrorResponse("ID mismatch", 400));

        //    if (!ModelState.IsValid)
        //    {
        //        var errors = ModelState.Values.SelectMany(v => v.Errors)
        //                                      .Select(e => e.ErrorMessage)
        //                                      .ToList();
        //        return BadRequest(ApiResponse<ReviewDTO>.ErrorResponse(string.Join(" | ", errors), 400));
        //    }

        //    var response = await _service.UpdateAsync(id, dto);
        //    return StatusCode(response.StatusCode, response);
        //}

        // DELETE: api/Reviews/5


        //// GET: api/Reviews/order/5
        //[HttpGet("order/{orderId:int}")]
        //public async Task<IActionResult> GetByOrderId(int orderId)
        //{
        //    var response = await _service.GetReviewsByOrderIdAsync(orderId);
        //    return StatusCode(response.StatusCode, response);
        //}

        //// GET: api/Reviews/seller/5
        //[HttpGet("seller/{sellerId:int}")]
        //public async Task<IActionResult> GetBySellerId(int sellerId)
        //{
        //    var response = await _service.GetReviewsBySellerIdAsync(sellerId);
        //    return StatusCode(response.StatusCode, response);
        //}

        //// GET: api/Reviews/user/5
        //[HttpGet("user/{reviewerId:int}")]
        //public async Task<IActionResult> GetByReviewerId(int reviewerId)
        //{
        //    var response = await _service.GetReviewsByReviewerIdAsync(reviewerId);
        //    return StatusCode(response.StatusCode, response);
        //}
    }
}
