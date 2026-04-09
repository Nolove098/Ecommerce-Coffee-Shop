using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;
using Microsoft.EntityFrameworkCore;
using SaleStore.Data;
using SaleStore.Models;

namespace SaleStore.Services;

public class SalesForecastService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SalesForecastService> _logger;
    private readonly MLContext _mlContext;

    public SalesForecastService(IServiceScopeFactory scopeFactory, ILogger<SalesForecastService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _mlContext = new MLContext(seed: 0);
    }

    public class DailyRevenue
    {
        public DateTime Date { get; set; }
        public float Revenue { get; set; }
    }

    public class RevenuePrediction
    {
        public float[] ForecastedRevenue { get; set; } = Array.Empty<float>();
        public float[] LowerBound { get; set; } = Array.Empty<float>();
        public float[] UpperBound { get; set; } = Array.Empty<float>();
    }

    public class ForecastResult
    {
        public List<DailyRevenue> Historical { get; set; } = new();
        public List<ForecastPoint> Forecast { get; set; } = new();
        public string? Error { get; set; }
    }

    public class ForecastPoint
    {
        public DateTime Date { get; set; }
        public float Revenue { get; set; }
        public float LowerBound { get; set; }
        public float UpperBound { get; set; }
    }

    public async Task<ForecastResult> ForecastAsync(int horizonDays = 7)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Get daily revenue for the last 90 days
            var vnZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnZone);
            var startDate = TimeZoneInfo.ConvertTimeToUtc(now.Date.AddDays(-90), vnZone);

            var orders = await context.Orders
                .AsNoTracking()
                .Where(o => o.CreatedAt >= startDate && o.Status == OrderStatus.Delivered)
                .ToListAsync();

            // Group by date (in VN timezone)
            var dailyData = orders
                .GroupBy(o => TimeZoneInfo.ConvertTimeFromUtc(o.CreatedAt, vnZone).Date)
                .Select(g => new DailyRevenue
                {
                    Date = g.Key,
                    Revenue = (float)g.Sum(o => o.TotalAmount)
                })
                .OrderBy(d => d.Date)
                .ToList();

            // Fill missing dates with 0 revenue
            if (dailyData.Count > 0)
            {
                var allDates = new List<DailyRevenue>();
                var current = dailyData.First().Date;
                var end = now.Date;
                while (current <= end)
                {
                    var existing = dailyData.FirstOrDefault(d => d.Date == current);
                    allDates.Add(new DailyRevenue
                    {
                        Date = current,
                        Revenue = existing?.Revenue ?? 0f
                    });
                    current = current.AddDays(1);
                }
                dailyData = allDates;
            }

            if (dailyData.Count < 7)
            {
                return new ForecastResult
                {
                    Historical = dailyData,
                    Error = "Cần ít nhất 7 ngày dữ liệu để dự đoán. Hiện có " + dailyData.Count + " ngày."
                };
            }

            // Train SSA model
            var dataView = _mlContext.Data.LoadFromEnumerable(dailyData);

            var windowSize = Math.Min(dailyData.Count / 2, 30);
            var seriesLength = dailyData.Count;

            var pipeline = _mlContext.Forecasting.ForecastBySsa(
                outputColumnName: nameof(RevenuePrediction.ForecastedRevenue),
                inputColumnName: nameof(DailyRevenue.Revenue),
                windowSize: windowSize,
                seriesLength: seriesLength,
                trainSize: seriesLength,
                horizon: horizonDays,
                confidenceLevel: 0.95f,
                confidenceLowerBoundColumn: nameof(RevenuePrediction.LowerBound),
                confidenceUpperBoundColumn: nameof(RevenuePrediction.UpperBound));

            var model = pipeline.Fit(dataView);
            var engine = model.CreateTimeSeriesEngine<DailyRevenue, RevenuePrediction>(_mlContext);
            var prediction = engine.Predict();

            var forecastPoints = new List<ForecastPoint>();
            var lastDate = dailyData.Last().Date;
            for (int i = 0; i < horizonDays; i++)
            {
                forecastPoints.Add(new ForecastPoint
                {
                    Date = lastDate.AddDays(i + 1),
                    Revenue = Math.Max(0, prediction.ForecastedRevenue[i]),
                    LowerBound = Math.Max(0, prediction.LowerBound[i]),
                    UpperBound = Math.Max(0, prediction.UpperBound[i])
                });
            }

            return new ForecastResult
            {
                Historical = dailyData.TakeLast(30).ToList(),
                Forecast = forecastPoints
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sales forecast failed");
            return new ForecastResult { Error = "Lỗi dự đoán: " + ex.Message };
        }
    }
}
