using Bikya.DTOs.ShippingDTOs;
using Bikya.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bikya.API.Areas.Shipping
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // يتطلب مستخدم مسجل
    public class ShippingController : ControllerBase
    {
        private readonly IShippingService shippingService;

        public ShippingController(IShippingService shippingService)
        {
            this.shippingService = shippingService;
        }

        // ✅ المستخدم ينشئ شحنة لأوردر يخصه فقط – التحقق يتم داخل السيرفيس
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateShippingDto dto)
        {
            var result = await shippingService.CreateAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        // ✅ المستخدم يجيب شحنة بالـ ID – ممكن تزود تحقق بالداخل لو حبيت
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await shippingService.GetByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        // ✅ للمسؤول فقط
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await shippingService.GetAllAsync();
            return StatusCode(result.StatusCode, result);
        }

        // ✅ للمسؤول فقط – تحديث حالة الشحنة
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateShippingStatusDto dto)
        {
            var result = await shippingService.UpdateStatusAsync(id, dto);
            return StatusCode(result.StatusCode, result);
        }

        // ✅ للمسؤول فقط – حذف شحنة
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await shippingService.DeleteAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        // ✅ تتبع شحنة – متاح للجميع (حتى غير المسجلين)
        [AllowAnonymous]
        [HttpGet("track/{trackingNumber}")]
        public async Task<IActionResult> Track(string trackingNumber)
        {
            var result = await shippingService.TrackAsync(trackingNumber);
            return StatusCode(result.StatusCode, result);
        }

        // ✅ حساب تكلفة – متاح للجميع
        [AllowAnonymous]
        [HttpPost("calculate")]
        public async Task<IActionResult> CalculateCost([FromBody] ShippingCostRequestDto dto)
        {
            var result = await shippingService.CalculateCostAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        // ✅ ربط مع مزود شحن – للمسؤول فقط
        [Authorize(Roles = "Admin")]
        [HttpPost("integrate/{provider}")]
        public async Task<IActionResult> Integrate(string provider, [FromBody] ThirdPartyShippingRequestDto dto)
        {
            dto.Provider = provider;
            var result = await shippingService.IntegrateWithProviderAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        // ✅ Webhook – متاحة بدون تسجيل
        [AllowAnonymous]
        [HttpPost("webhook/{provider}")]
        public async Task<IActionResult> Webhook(string provider, [FromBody] ShippingWebhookDto dto)
        {
            var result = await shippingService.HandleWebhookAsync(provider, dto);
            return StatusCode(result.StatusCode, result);
        }
    }
}
