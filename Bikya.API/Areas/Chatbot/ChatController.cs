using Bikya.Data.Models;
using Bikya.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bikya.API.Areas.Chatboot
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost]
        public async Task<ActionResult<ChatResponse>> AskBot([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest("الرسالة لا يمكن أن تكون فارغة.");

            var botReply = await _chatService.GetBotResponse(request.Message);

            return Ok(new ChatResponse { Response = botReply });
        }
    }
}
