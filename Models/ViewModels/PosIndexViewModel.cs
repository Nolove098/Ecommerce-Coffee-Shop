namespace SaleStore.Models.ViewModels;

public class PosIndexViewModel
{
    public string OperatorName { get; set; } = string.Empty;
    public bool CanAccessAdmin { get; set; }
    public IReadOnlyList<string> Categories { get; set; } = Array.Empty<string>();
    public IReadOnlyList<PosProductViewModel> Products { get; set; } = Array.Empty<PosProductViewModel>();
}

public class PosProductViewModel
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int Stock { get; set; }
}

public class PosCartItemInputModel
{
    public long ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

public class PosCheckoutRequest
{
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? TableNumber { get; set; }
    public string? Note { get; set; }
    public List<PosCartItemInputModel> Items { get; set; } = new();
}