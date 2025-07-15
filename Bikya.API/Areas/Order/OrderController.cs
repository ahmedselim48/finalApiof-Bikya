using Bikya.DTOs.Orderdto;
using Bikya.DTOs.ShippingDTOs;
using Bikya.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bikya.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDTO dto)
        {
            var result = await _orderService.CreateOrderAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("status")]
      //  [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateOrderStatusDTO dto)
        {
            var result = await _orderService.UpdateOrderStatusAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

       
        [HttpGet("my-orders/{userId}")]
        public async Task<IActionResult> GetMyOrders(int userId)
        {
            var result = await _orderService.GetOrdersByUserIdAsync(userId);
            return StatusCode(result.StatusCode, result);
        }

       
        [HttpGet("buyer/{buyerId}")]
        public async Task<IActionResult> GetOrdersByBuyer(int buyerId)
        {
            var result = await _orderService.GetOrdersByBuyerIdAsync(buyerId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("seller/{sellerId}")]
        public async Task<IActionResult> GetOrdersBySeller(int sellerId)
        {
            var result = await _orderService.GetOrdersBySellerIdAsync(sellerId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllOrders()
        {
            var result = await _orderService.GetAllOrdersAsync();
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderById(int orderId)
        {
            var result = await _orderService.GetOrderByIdAsync(orderId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{orderId}/cancel")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            int buyerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _orderService.CancelOrderAsync(orderId, buyerId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{orderId}/shipping")]
        public async Task<IActionResult> UpdateShipping(int orderId, [FromBody] ShippingInfoDTO dto)
        {
            var result = await _orderService.UpdateShippingInfoAsync(orderId, dto);
            return StatusCode(result.StatusCode, result);
        }
    }
}
