using IotDashboard.Api.Services;
using IotDashboard.Api.Util;
using IotDashboard.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IotDashboard.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] ChatRequestVM request, CancellationToken cancellationToken)
        {
            var result = await _chatService.AskAsync(request, cancellationToken);
            return result.ToResponse();
        }
    }
}
