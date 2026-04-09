using Microsoft.ML;
using Microsoft.ML.Trainers;
using Microsoft.EntityFrameworkCore;
using SaleStore.Data;

namespace SaleStore.Services;

public class ProductRecommendService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ProductRecommendService> _logger;
    private readonly MLContext _mlContext;
    private ITransformer? _model;
    private DateTime _lastTrained = DateTime.MinValue;
    private readonly object _lock = new();

    public ProductRecommendService(IServiceScopeFactory scopeFactory, ILogger<ProductRecommendService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _mlContext = new MLContext(seed: 0);
    }

    public class ProductRating
    {
        public float UserId { get; set; }
        public float ProductId { get; set; }
        public float Label { get; set; } // purchase count as implicit rating
    }

    public class ProductRatingPrediction
    {
        public float Score { get; set; }
    }

    public class RecommendedProduct
    {
        public long ProductId { get; set; }
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public float Score { get; set; }
    }

    private async Task TrainModelAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Get purchase history: user → product co-purchase patterns
        var purchaseData = await context.Orders
            .AsNoTracking()
            .Where(o => o.Status == Models.OrderStatus.Delivered)
            .Include(o => o.OrderItems)
            .SelectMany(o => o.OrderItems.Select(oi => new
            {
                UserId = (float)o.CustomerId,
                ProductId = (float)oi.ProductId,
                Quantity = oi.Quantity
            }))
            .ToListAsync();

        if (purchaseData.Count < 5)
        {
            _logger.LogWarning("Not enough purchase data to train recommendation model ({Count} records)", purchaseData.Count);
            return;
        }

        // Aggregate purchases: sum quantities per user-product pair as implicit rating
        var ratings = purchaseData
            .GroupBy(p => new { p.UserId, p.ProductId })
            .Select(g => new ProductRating
            {
                UserId = g.Key.UserId,
                ProductId = g.Key.ProductId,
                Label = Math.Min(g.Sum(x => x.Quantity), 10f) // cap at 10
            })
            .ToList();

        var dataView = _mlContext.Data.LoadFromEnumerable(ratings);

        var options = new MatrixFactorizationTrainer.Options
        {
            MatrixColumnIndexColumnName = nameof(ProductRating.UserId),
            MatrixRowIndexColumnName = nameof(ProductRating.ProductId),
            LabelColumnName = nameof(ProductRating.Label),
            NumberOfIterations = 20,
            ApproximationRank = 8,
            LearningRate = 0.1
        };

        var pipeline = _mlContext.Transforms.Conversion
            .MapValueToKey(nameof(ProductRating.UserId))
            .Append(_mlContext.Transforms.Conversion.MapValueToKey(nameof(ProductRating.ProductId)))
            .Append(_mlContext.Recommendation().Trainers.MatrixFactorization(options));

        lock (_lock)
        {
            _model = pipeline.Fit(dataView);
            _lastTrained = DateTime.UtcNow;
        }

        _logger.LogInformation("Recommendation model trained with {Count} user-product pairs", ratings.Count);
    }

    public async Task<List<RecommendedProduct>> GetRecommendationsAsync(long userId, long? currentProductId = null, int count = 4)
    {
        try
        {
            // userId == 0 nghĩa là user chưa có lịch sử → dùng fallback best-sellers
            if (userId == 0)
            {
                return await GetFallbackRecommendationsAsync(currentProductId, count);
            }

            // Re-train if model is stale (older than 1 hour) or not trained
            if (_model == null || (DateTime.UtcNow - _lastTrained).TotalHours > 1)
            {
                await TrainModelAsync();
            }

            if (_model == null)
            {
                return await GetFallbackRecommendationsAsync(currentProductId, count);
            }

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Get all active products
            var products = await context.Products
                .AsNoTracking()
                .Where(p => p.IsActive)
                .ToListAsync();

            // Get products the user already bought
            var boughtProductIds = await context.Orders
                .AsNoTracking()
                .Where(o => o.CustomerId == userId && o.Status == Models.OrderStatus.Delivered)
                .SelectMany(o => o.OrderItems.Select(oi => oi.ProductId))
                .Distinct()
                .ToListAsync();

            PredictionEngine<ProductRating, ProductRatingPrediction> engine;
            lock (_lock)
            {
                engine = _mlContext.Model.CreatePredictionEngine<ProductRating, ProductRatingPrediction>(_model);
            }

            // Score each product the user hasn't bought
            var scored = new List<RecommendedProduct>();
            foreach (var product in products)
            {
                if (product.Id == currentProductId) continue;

                var prediction = engine.Predict(new ProductRating
                {
                    UserId = userId,
                    ProductId = product.Id
                });

                if (float.IsNaN(prediction.Score) || float.IsInfinity(prediction.Score))
                    continue;

                scored.Add(new RecommendedProduct
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    Category = product.Category,
                    Price = product.Price,
                    ImageUrl = product.ImageUrl,
                    Score = prediction.Score
                });
            }

            var results = scored
                .OrderByDescending(s => s.Score)
                .Take(count)
                .ToList();

            // If ML returns too few, supplement with fallback
            if (results.Count < count)
            {
                var fallback = await GetFallbackRecommendationsAsync(currentProductId, count - results.Count,
                    results.Select(r => r.ProductId).ToList());
                results.AddRange(fallback);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Product recommendation failed for user {UserId}", userId);
            return await GetFallbackRecommendationsAsync(currentProductId, count);
        }
    }

    private async Task<List<RecommendedProduct>> GetFallbackRecommendationsAsync(long? excludeProductId, int count, List<long>? excludeIds = null)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var excludeAll = new List<long>();
        if (excludeProductId.HasValue) excludeAll.Add(excludeProductId.Value);
        if (excludeIds != null) excludeAll.AddRange(excludeIds);

        // Fallback: best-selling products
        var topProducts = await context.OrderItems
            .AsNoTracking()
            .Where(oi => !excludeAll.Contains(oi.ProductId))
            .GroupBy(oi => oi.ProductId)
            .Select(g => new { ProductId = g.Key, TotalQty = g.Sum(x => x.Quantity) })
            .OrderByDescending(x => x.TotalQty)
            .Take(count)
            .ToListAsync();

        var productIds = topProducts.Select(x => x.ProductId).ToList();
        var products = await context.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id) && p.IsActive)
            .ToListAsync();

        return products.Select(p => new RecommendedProduct
        {
            ProductId = p.Id,
            Name = p.Name,
            Category = p.Category,
            Price = p.Price,
            ImageUrl = p.ImageUrl,
            Score = 0
        }).ToList();
    }
}
