using Microsoft.AspNetCore.Mvc;
using SaleStore.Controllers;
using SaleStore.Services;
using System.Text.Json;

namespace SaleStore.Tests.Controllers;

public class ChatBotControllerTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Chat_WhenMessagesMissing_RejectsWithoutCallingService(bool useNull)
    {
        var service = new RecordingChatService();
        var controller = new ChatBotController(service);

        var result = await controller.Chat(new ChatRequest { Messages = useNull ? null! : [] });

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Null(service.ReceivedHistory);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(20)]
    [InlineData(25)]
    public async Task Chat_WhenHistoryProvided_ForwardsLatestTwentyInOrderAndReturnsReply(int count)
    {
        var service = new RecordingChatService();
        var controller = new ChatBotController(service);
        var messages = Enumerable.Range(1, count)
            .Select(i => new ChatMessage { Role = "user", Content = $"Synthetic message {i}" }).ToList();
        var original = messages.ToArray();

        var result = Assert.IsType<OkObjectResult>(await controller.Chat(new ChatRequest { Messages = messages }));

        Assert.Equal(original.Skip(Math.Max(0, count - 20)), service.ReceivedHistory);
        Assert.Equal(original, messages);
        Assert.Equal("Synthetic coffee reply", JsonSerializer.SerializeToElement(result.Value).GetProperty("reply").GetString());
    }

    private sealed class RecordingChatService : IChatBotService
    {
        public List<ChatMessage>? ReceivedHistory { get; private set; }

        public Task<string> GetResponseAsync(List<ChatMessage> history)
        {
            ReceivedHistory = history;
            return Task.FromResult("Synthetic coffee reply");
        }

        public Task<string> GenerateAsync(string systemPrompt, string userMessage) =>
            throw new NotSupportedException("Chat must use the conversation service boundary.");
    }
}
