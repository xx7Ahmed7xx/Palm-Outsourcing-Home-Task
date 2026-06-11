namespace OrderHub.Application.Pricing;

public static class LinePriceCalculator
{
  private const decimal GoldTierMultiplier = 0.85m;
  private const decimal SilverTierMultiplier = 0.92m;
  private const decimal ShortEmbroideryFee = 4.50m;
  private const decimal LongEmbroideryFee = 8.00m;
  private const int ShortEmbroideryMaxLength = 3;

  public static decimal CalculateUnitPrice(decimal basePrice, string? tierCode, string? embroidery)
  {
    var price = ApplyTierDiscount(basePrice, tierCode);
    return price + GetEmbroideryFee(embroidery);
  }

  private static decimal ApplyTierDiscount(decimal basePrice, string? tierCode) =>
    tierCode switch
    {
      "GOLD" => basePrice * GoldTierMultiplier,
      "SILVER" => basePrice * SilverTierMultiplier,
      _ => basePrice
    };

  private static decimal GetEmbroideryFee(string? embroidery)
  {
    if (string.IsNullOrEmpty(embroidery))
    {
      return 0m;
    }

    return embroidery.Length <= ShortEmbroideryMaxLength
      ? ShortEmbroideryFee
      : LongEmbroideryFee;
  }
}
