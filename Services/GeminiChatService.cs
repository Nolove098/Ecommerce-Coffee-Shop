using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SaleStore.Services;

public interface IChatBotService
{
    Task<string> GetResponseAsync(List<ChatMessage> history);
}

public class ChatMessage
{
    public string Role { get; set; } = null!;   // "user" or "model"
    public string Content { get; set; } = null!;
}

public class GeminiChatService : IChatBotService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly string _systemPrompt;
    private readonly ILogger<GeminiChatService> _logger;

    public GeminiChatService(IConfiguration config, ILogger<GeminiChatService> logger)
    {
        _http = new HttpClient();
        _logger = logger;
        _apiKey = config["Gemini:ApiKey"] ?? "";
        _systemPrompt = @"Bạn là trợ lý tư vấn AI của SaleStore – một cửa hàng cà phê trực tuyến.

QUY TẮC BẮT BUỘC:
1. Chỉ trả lời các câu hỏi liên quan đến: cà phê, trà, thức uống, menu của quán, nguyên liệu pha chế, cách pha cà phê, văn hóa cà phê, đặt hàng, giao hàng, thanh toán trên SaleStore.
2. Nếu khách hỏi bất kỳ chủ đề nào KHÔNG liên quan đến cà phê/thức uống/SaleStore, hãy từ chối lịch sự: ""Xin lỗi, tôi chỉ có thể tư vấn về cà phê và các sản phẩm của SaleStore. Bạn có câu hỏi gì về cà phê không?"".
3. Trả lời bằng tiếng Việt, thân thiện, ngắn gọn (tối đa 3-4 câu).
4. Không bịa đặt thông tin về giá cả cụ thể trừ khi được cung cấp. Hãy gợi ý khách xem menu trên website.
5. Có thể tư vấn về: loại cà phê, cách chọn cà phê phù hợp, cách pha chế, so sánh các loại cà phê, gợi ý thức uống.

THÔNG TIN CỬA HÀNG:
- Tên: SaleStore
- Loại hình: Cửa hàng cà phê & thức uống trực tuyến
- Thanh toán: Hỗ trợ VNPay và thanh toán khi nhận hàng (COD)
- Website: Khách có thể xem menu tại trang Menu của website";
    }

    public async Task<string> GetResponseAsync(List<ChatMessage> history)
    {
        if (string.IsNullOrEmpty(_apiKey))
            return "Chatbot chưa được cấu hình. Vui lòng liên hệ quản trị viên.";

        try
        {
            var contents = new List<object>();

            // Gemma không hỗ trợ system_instruction, nhúng vào đầu cuộc hội thoại
            contents.Add(new
            {
                role = "user",
                parts = new[] { new { text = _systemPrompt } }
            });
            contents.Add(new
            {
                role = "model",
                parts = new[] { new { text = "Tôi hiểu. Tôi sẽ tuân thủ đúng các quy tắc trên." } }
            });

            foreach (var msg in history)
            {
                contents.Add(new
                {
                    role = msg.Role == "user" ? "user" : "model",
                    parts = new[] { new { text = msg.Content } }
                });
            }

            var body = new
            {
                contents,
                generationConfig = new
                {
                    temperature = 0.7,
                    maxOutputTokens = 300,
                    topP = 0.9
                }
            };

            var json = JsonConvert.SerializeObject(body);
            var request = new HttpRequestMessage(HttpMethod.Post,
                $"https://generativelanguage.googleapis.com/v1beta/models/gemma-3-27b-it:generateContent?key={_apiKey}");
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Gemini API error {StatusCode}: {Body}", (int)response.StatusCode, responseBody);
                // Hiển thị lỗi gốc từ Google để debug
                try
                {
                    var errObj = JObject.Parse(responseBody);
                    var errMsg = errObj["error"]?["message"]?.ToString() ?? responseBody;
                    return $"[Lỗi API {(int)response.StatusCode}]: {errMsg}";
                }
                catch
                {
                    return $"[Lỗi API {(int)response.StatusCode}]: {responseBody}";
                }
            }

            var result = JObject.Parse(responseBody);
            var text = result["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();
            return text ?? "Xin lỗi, tôi không thể trả lời lúc này.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chatbot error");
            return "Xin lỗi, đã có lỗi xảy ra. Vui lòng thử lại sau.";
        }
    }
}
