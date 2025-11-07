using Microsoft.AspNetCore.Mvc;
using project.Services;

namespace project.Controllers;

[Route("api/ai/chat")]
[ApiController]
public class ChatController : Controller
{
    private readonly ChatBotService _chatBotService;

    public ChatController(ChatBotService chatBotService)
    {
        _chatBotService = chatBotService;
    }

    public class MessageRequest
    {
        public string Message { get; set; }
    }

    [HttpPost]
    public async Task<IActionResult> Send([FromBody] MessageRequest request)
    {
        var reply = await _chatBotService.SendMessageAsync(request.Message);
        return Ok(new { reply });
    }
}