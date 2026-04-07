using Microsoft.AspNetCore.Mvc;
using SaleStore.Services;

namespace SaleStore.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatBotController : ControllerBase
{
    private readonly IChatBotService _chatBot;

    public ChatBotController(IChatBotService chatBot)
    {
        _chatBot = chatBot;
    }

    [HttpPost]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        if (request.Messages == null || request.Messages.Count == 0)
            return BadRequest(new { error = "Tin nhắn không được để trống." });

        // Limit conversation history to last 20 messages to prevent abuse
        var history = request.Messages.TakeLast(20).ToList();

        var reply = await _chatBot.GetResponseAsync(history);
        return Ok(new { reply });
    }
}

public class ChatRequest
{
    public List<ChatMessage> Messages { get; set; } = new();
}
