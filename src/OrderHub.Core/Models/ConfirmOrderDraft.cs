namespace OrderHub.Core.Models;

public sealed class ConfirmOrderDraft
{
  public int SchoolId { get; init; }
  public required string SchoolName { get; init; }
  public IReadOnlyList<ConfirmOrderLineItem> Lines { get; init; } = [];
}

public sealed class ConfirmOrderLineItem
{
  public int Id { get; init; }
  public required string Sku { get; init; }
  public string? Embroidery { get; init; }
  public decimal UnitPrice { get; init; }
  public int Quantity { get; init; }
}

public sealed class ConfirmOrderLineUpdate
{
  public int LineId { get; init; }
  public int Quantity { get; init; }
}
