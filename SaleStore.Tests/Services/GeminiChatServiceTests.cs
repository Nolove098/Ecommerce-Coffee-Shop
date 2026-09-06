using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SaleStore.Services;

namespace SaleStore.Tests.Services;

public class GeminiChatServiceTests
{
    private const string SyntheticMarker = "synthetic-provider-detail-not-for-users";

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Request_WhenApiKeyMissing_ReturnsFallbackWithoutTransport(bool generate)
    {
        using var handler = new FakeHandler();
        using var client = new HttpClient(handler);
        var service = CreateService(client, new RecordingLogger(), configured: false);

        var result = await Invoke(service, generate);

        Assert.Equal(generate ? "" : "Chatbot chưa được cấu hình. Vui lòng liên hệ quản trị viên.", result);
        Assert.Equal(0, handler.Calls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Request_WhenSuccessful_ReturnsProviderTextAndSendsConversation(bool generate)
    {
        using var handler = new FakeHandler
        {
            Body = """{"candidates":[{"content":{"parts":[{"text":"Synthetic coffee answer"}]}}]}"""
        };
        using var client = new HttpClient(handler);
        var service = CreateService(client, new RecordingLogger());

        var result = await Invoke(service, generate);

        Assert.Equal("Synthetic coffee answer", result);
        Assert.Equal(1, handler.Calls);
        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Equal("application/json", handler.MediaType);
        using var payload = JsonDocument.Parse(handler.RequestBody!);
        var contents = payload.RootElement.GetProperty("contents");
        Assert.Equal("Synthetic coffee question", contents[contents.GetArrayLength() - 1].GetProperty("parts")[0].GetProperty("text").GetString());
        Assert.Equal("user", contents[0].GetProperty("role").GetString());
        Assert.Equal("model", contents[1].GetProperty("role").GetString());
        if (generate)
            Assert.Equal("Synthetic system prompt", contents[0].GetProperty("parts")[0].GetProperty("text").GetString());
    }

    [Fact]
    public async Task GetResponseAsync_WhenHistoryContainsNonUserRole_NormalizesItToModel()
    {
        using var handler = new FakeHandler();
        using var client = new HttpClient(handler);
        var service = CreateService(client, new RecordingLogger());

        await service.GetResponseAsync([
            new() { Role = "system", Content = "Synthetic prior answer" },
            new() { Role = "user", Content = "Synthetic follow-up" }
        ]);

        using var payload = JsonDocument.Parse(handler.RequestBody!);
        var contents = payload.RootElement.GetProperty("contents");
        Assert.Equal("model", contents[2].GetProperty("role").GetString());
        Assert.Equal("Synthetic prior answer", contents[2].GetProperty("parts")[0].GetProperty("text").GetString());
        Assert.Equal("user", contents[3].GetProperty("role").GetString());
    }

    [Theory]
    [InlineData(false, 429)]
    [InlineData(true, 429)]
    [InlineData(false, 500)]
    [InlineData(true, 500)]
    public async Task Request_WhenProviderRejects_DoesNotExposeProviderBodyOrKey(bool generate, int status)
    {
        using var handler = new FakeHandler { Status = (HttpStatusCode)status, Body = SyntheticMarker };
        using var client = new HttpClient(handler);
        var logger = new RecordingLogger();
        var service = CreateService(client, logger);

        var result = await Invoke(service, generate);

        Assert.Equal(generate ? $"[ERROR:{status}] " : $"[Lỗi API {status}]: ", result);
        Assert.Single(logger.Messages);
        Assert.Contains(status.ToString(), logger.Messages[0]);
        Assert.DoesNotContain(SyntheticMarker, string.Join("\n", logger.Messages));
        Assert.DoesNotContain("dummy-api-key", string.Join("\n", logger.Messages));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Request_WhenTransportThrows_ReturnsSafeFallbackAndLogsOnlyExceptionType(bool generate)
    {
        using var handler = new FakeHandler { Failure = new HttpRequestException(SyntheticMarker) };
        using var client = new HttpClient(handler);
        var logger = new RecordingLogger();

        var result = await Invoke(CreateService(client, logger), generate);

        Assert.Equal(generate ? "" : "Xin lỗi, đã có lỗi xảy ra. Vui lòng thử lại sau.", result);
        Assert.Single(logger.Messages);
        Assert.Contains(nameof(HttpRequestException), logger.Messages[0]);
        Assert.DoesNotContain(SyntheticMarker, logger.Messages[0]);
        Assert.All(logger.Exceptions, exception => Assert.Null(exception));
    }

    [Theory]
    [InlineData(false, "{}", "Xin lỗi, tôi không thể trả lời lúc này.")]
    [InlineData(true, "{}", "")]
    [InlineData(false, "invalid-json", "Xin lỗi, đã có lỗi xảy ra. Vui lòng thử lại sau.")]
    [InlineData(true, "invalid-json", "")]
    [InlineData(false, "{\"candidates\":[]}", "Xin lỗi, đã có lỗi xảy ra. Vui lòng thử lại sau.")]
    [InlineData(true, "{\"candidates\":[]}", "")]
    public async Task Request_WhenResponseMissingOrMalformed_ReturnsExistingFallback(bool generate, string body, string expected)
    {
        using var handler = new FakeHandler { Body = body };
        using var client = new HttpClient(handler);

        var result = await Invoke(CreateService(client, new RecordingLogger()), generate);

        Assert.Equal(expected, result);
    }

    private static GeminiChatService CreateService(HttpClient client, RecordingLogger logger, bool configured = true)
    {
        // Deliberately excludes environment variables, appsettings, and User Secrets.
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(
            configured ? new Dictionary<string, string?> { ["Gemini:ApiKey"] = "dummy-api-key" } : []).Build();
        return new GeminiChatService(client, configuration, logger);
    }

    private static Task<string> Invoke(GeminiChatService service, bool generate) => generate
        ? service.GenerateAsync("Synthetic system prompt", "Synthetic coffee question")
        : service.GetResponseAsync([new() { Role = "user", Content = "Synthetic coffee question" }]);

    private sealed class FakeHandler : HttpMessageHandler
    {
        public HttpStatusCode Status { get; init; } = HttpStatusCode.OK;
        public string Body { get; init; } = "{}";
        public Exception? Failure { get; init; }
        public int Calls { get; private set; }
        public HttpMethod? Method { get; private set; }
        public string? MediaType { get; private set; }
        public string? RequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            Method = request.Method;
            MediaType = request.Content?.Headers.ContentType?.MediaType;
            RequestBody = request.Content == null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            if (Failure != null) throw Failure;
            return new HttpResponseMessage(Status) { Content = new StringContent(Body, Encoding.UTF8, "application/json") };
        }
    }

    private sealed class RecordingLogger : ILogger<GeminiChatService>
    {
        public List<string> Messages { get; } = [];
        public List<Exception?> Exceptions { get; } = [];
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
            Exceptions.Add(exception);
        }
    }
}
