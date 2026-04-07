using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaleStore.Data;
using SaleStore.Services;

namespace SaleStore.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiController : ControllerBase
{
    private readonly IChatBotService _ai;
    private readonly ApplicationDbContext _context;

    public AiController(IChatBotService ai, ApplicationDbContext context)
    {
        _ai = ai;
        _context = context;
    }

    /// <summary>
    /// AI Gợi ý thức uống theo tâm trạng/sở thích
    /// </summary>
    [HttpPost("recommend")]
    public async Task<IActionResult> Recommend([FromBody] RecommendRequest req)
    {
        // Lấy toàn bộ sản phẩm đang bán
        var products = await _context.Products
            .AsNoTracking()
            .Where(p => p.IsActive)
            .Select(p => new { p.Id, p.Name, p.Category, p.Price, p.Description })
            .ToListAsync();

        var productList = string.Join("\n", products.Select(p =>
            $"- ID:{p.Id} | {p.Name} | {p.Category} | {p.Price:N0}₫ | {p.Description ?? ""}"));

        var systemPrompt = @"Bạn là chuyên gia tư vấn thức uống của SaleStore – quán cà phê trực tuyến.
Dựa trên tâm trạng và sở thích của khách, hãy gợi ý 3 sản phẩm PHÙ HỢP NHẤT từ danh sách sản phẩm bên dưới.

DANH SÁCH SẢN PHẨM:
" + productList + @"

QUY TẮC:
- Chỉ gợi ý sản phẩm CÓ TRONG danh sách trên
- Trả về JSON array gồm đúng 3 object: [{""id"": <số>, ""name"": ""tên"", ""reason"": ""lý do ngắn gọn""}]
- Lý do phải liên quan đến tâm trạng/sở thích khách đã nêu
- CHỈ trả về JSON, không thêm text nào khác";

        var userMsg = $"Tâm trạng: {req.Mood ?? "bình thường"}\nSở thích: {req.Preference ?? "không rõ"}\nThời tiết: {req.Weather ?? "bình thường"}";

        var reply = await _ai.GenerateAsync(systemPrompt, userMsg);

        // Parse JSON từ AI response
        try
        {
            // Tìm JSON array trong response
            var start = reply.IndexOf('[');
            var end = reply.LastIndexOf(']');
            if (start >= 0 && end > start)
            {
                var jsonStr = reply.Substring(start, end - start + 1);
                var recommendations = Newtonsoft.Json.JsonConvert.DeserializeObject<List<RecommendItem>>(jsonStr);
                
                // Enrich with product data
                var result = new List<object>();
                foreach (var rec in recommendations ?? new())
                {
                    var product = products.FirstOrDefault(p => p.Id == rec.Id);
                    if (product != null)
                    {
                        result.Add(new
                        {
                            product.Id,
                            product.Name,
                            product.Category,
                            product.Price,
                            rec.Reason
                        });
                    }
                }
                if (result.Any())
                    return Ok(new { recommendations = result });
            }
        }
        catch { }

        return Ok(new { recommendations = new List<object>(), raw = reply });
    }

    /// <summary>
    /// AI Tìm kiếm thông minh
    /// </summary>
    [HttpPost("search")]
    public async Task<IActionResult> SmartSearch([FromBody] SearchRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Query))
            return BadRequest(new { error = "Query is required" });

        var products = await _context.Products
            .AsNoTracking()
            .Where(p => p.IsActive)
            .Select(p => new { p.Id, p.Name, p.Category, p.Price, p.Description })
            .ToListAsync();

        var productList = string.Join("\n", products.Select(p =>
            $"- ID:{p.Id} | {p.Name} | {p.Category} | {p.Price:N0}₫ | {p.Description ?? ""}"));

        var systemPrompt = @"Bạn là hệ thống tìm kiếm thông minh cho SaleStore – quán cà phê trực tuyến.
Khách hàng sẽ mô tả thức uống họ muốn bằng ngôn ngữ tự nhiên. Hãy tìm các sản phẩm phù hợp nhất từ danh sách.

DANH SÁCH SẢN PHẨM:
" + productList + @"

QUY TẮC:
- Chỉ trả về sản phẩm CÓ TRONG danh sách
- Trả về JSON array: [{""id"": <số>, ""relevance"": ""lý do phù hợp ngắn gọn""}]
- Sắp xếp từ phù hợp nhất đến ít phù hợp nhất
- Tối đa 6 sản phẩm
- Nếu không tìm thấy gì phù hợp, trả về []
- CHỈ trả về JSON, không thêm text nào khác";

        var reply = await _ai.GenerateAsync(systemPrompt, req.Query);

        try
        {
            var start = reply.IndexOf('[');
            var end = reply.LastIndexOf(']');
            if (start >= 0 && end > start)
            {
                var jsonStr = reply.Substring(start, end - start + 1);
                var searchResults = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SearchItem>>(jsonStr);

                var result = new List<object>();
                foreach (var item in searchResults ?? new())
                {
                    var product = products.FirstOrDefault(p => p.Id == item.Id);
                    if (product != null)
                    {
                        result.Add(new
                        {
                            product.Id,
                            product.Name,
                            product.Category,
                            product.Price,
                            product.Description,
                            item.Relevance
                        });
                    }
                }
                return Ok(new { results = result, query = req.Query });
            }
        }
        catch { }

        return Ok(new { results = new List<object>(), query = req.Query, debug = reply });
    }

    /// <summary>
    /// AI Phân tích doanh thu cho Admin
    /// </summary>
    [HttpGet("admin/insights")]
    public async Task<IActionResult> AdminInsights()
    {
        // Collect data
        var vnZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnZone);
        var last7Days = TimeZoneInfo.ConvertTimeToUtc(now.Date.AddDays(-7), vnZone);
        var last30Days = TimeZoneInfo.ConvertTimeToUtc(now.Date.AddDays(-30), vnZone);

        var recentOrders = await _context.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
            .Where(o => o.CreatedAt >= last30Days)
            .ToListAsync();

        var totalRevenue30d = recentOrders.Where(o => o.Status == Models.OrderStatus.Delivered).Sum(o => o.TotalAmount);
        var totalRevenue7d = recentOrders.Where(o => o.Status == Models.OrderStatus.Delivered && o.CreatedAt >= last7Days).Sum(o => o.TotalAmount);
        var orderCount30d = recentOrders.Count;
        var orderCount7d = recentOrders.Count(o => o.CreatedAt >= last7Days);

        // Top products
        var topProducts = recentOrders
            .Where(o => o.Status == Models.OrderStatus.Delivered)
            .SelectMany(o => o.OrderItems)
            .GroupBy(oi => oi.Product?.Name ?? "Unknown")
            .Select(g => new { Name = g.Key, Qty = g.Sum(x => x.Quantity), Revenue = g.Sum(x => x.Quantity * x.UnitPrice) })
            .OrderByDescending(x => x.Revenue)
            .Take(5)
            .ToList();

        var pendingOrders = await _context.Orders.CountAsync(o => o.Status == Models.OrderStatus.Pending);

        var dataForAI = $@"DỮ LIỆU KINH DOANH SALESTORE (cập nhật {now:dd/MM/yyyy HH:mm}):

30 ngày gần đây:
- Tổng đơn hàng: {orderCount30d}
- Doanh thu (đơn đã giao): {totalRevenue30d:N0}₫

7 ngày gần đây:
- Tổng đơn hàng: {orderCount7d}
- Doanh thu: {totalRevenue7d:N0}₫

Top 5 sản phẩm bán chạy (30 ngày):
{string.Join("\n", topProducts.Select((p, i) => $"{i + 1}. {p.Name}: {p.Qty} ly, doanh thu {p.Revenue:N0}₫"))}

Đơn hàng đang chờ xử lý: {pendingOrders}";

        var systemPrompt = @"Bạn là chuyên gia phân tích kinh doanh cho SaleStore – cửa hàng cà phê trực tuyến.
Dựa trên dữ liệu được cung cấp, hãy đưa ra nhận xét và gợi ý kinh doanh.

QUY TẮC:
- Trả lời bằng tiếng Việt, chuyên nghiệp nhưng dễ hiểu
- Đưa ra 3-4 nhận xét chính về tình hình kinh doanh
- Đưa ra 2-3 gợi ý cải thiện cụ thể
- Dùng emoji phù hợp để sinh động
- Giới hạn tối đa 200 từ";

        var reply = await _ai.GenerateAsync(systemPrompt, dataForAI);

        return Ok(new { insights = string.IsNullOrEmpty(reply) ? "AI không trả về kết quả." : reply, updatedAt = now.ToString("HH:mm dd/MM/yyyy") });
    }
}

public class RecommendRequest
{
    public string? Mood { get; set; }
    public string? Preference { get; set; }
    public string? Weather { get; set; }
}

public class RecommendItem
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? Reason { get; set; }
}

public class SearchRequest
{
    public string Query { get; set; } = "";
}

public class SearchItem
{
    public long Id { get; set; }
    public string? Relevance { get; set; }
}
