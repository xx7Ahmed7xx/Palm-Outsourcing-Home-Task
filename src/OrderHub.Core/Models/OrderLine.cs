namespace OrderHub.Core.Models;

public sealed class OrderLine
{
    public required string Sku { get; init; }
    public int Quantity { get; init; }
    public string? Embroidery { get; init; }
}
