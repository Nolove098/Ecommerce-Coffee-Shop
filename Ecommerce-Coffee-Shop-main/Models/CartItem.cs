using System.Text.Json;
using System.Text.Json.Serialization;

namespace SaleStore.Models;

public class CartItem
{
    [JsonPropertyName("productId")]
    public int ProductId { get; set; }

    [JsonPropertyName("productName")]
    public string ProductName { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("imageUrl")]
    [JsonConverter(typeof(SafeStringConverter))]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonIgnore]
    public decimal Subtotal => Price * Quantity;
}

public class SafeStringConverter : JsonConverter<string?>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String) return reader.GetString();
        if (reader.TokenType == JsonTokenType.Null) return null;
        
        using var doc = JsonDocument.ParseValue(ref reader);
        return doc.RootElement.ToString(); // Converted anything (object, array, number, bool) to string as fallback
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}
