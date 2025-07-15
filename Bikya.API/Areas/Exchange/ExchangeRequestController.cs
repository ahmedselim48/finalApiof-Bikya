using Bikya.DTOs.ExchangeRequestDTOs;
using Bikya.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bikya.API.Areas.Exchange
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ExchangeRequestController : ControllerBase
    {
        private readonly IExchangeRequestService _service;

        public ExchangeRequestController(IExchangeRequestService service)
        {
            _service = service;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateExchangeRequestDTO dto)
        {
            var response = await _service.CreateAsync(dto, GetUserId());
            return StatusCode(response.StatusCode, response);
        }

      
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _service.GetByIdAsync(id);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var response = await _service.GetAllAsync();
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("sent")]
        public async Task<IActionResult> GetSent()
        {
            var response = await _service.GetSentRequestsAsync(GetUserId());
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("received")]
        public async Task<IActionResult> GetReceived()
        {
            var response = await _service.GetReceivedRequestsAsync(GetUserId());
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("{id}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            var response = await _service.ApproveRequestAsync(id, GetUserId());
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("{id}/reject")]
        public async Task<IActionResult> Reject(int id)
        {
            var response = await _service.RejectRequestAsync(id, GetUserId());
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _service.DeleteAsync(id, GetUserId());
            return StatusCode(response.StatusCode, response);
        }
    }
}
