using OrderHub.Application.Pricing;

namespace OrderHub.Tests.Pricing;

public class LinePriceCalculatorTests
{
  public static TheoryData<decimal, string, string?, decimal> PricingCases => new()
  {
    { 100m, "GOLD", null, 85.00m },
    { 100m, "SILVER", null, 92.00m },
    { 100m, "BRONZE", null, 100.00m },
    { 100m, "GOLD", "AB", 89.50m },
    { 100m, "GOLD", "ABCD", 93.00m }
  };

  [Theory]
  [MemberData(nameof(PricingCases))]
  public void CalculateUnitPrice_AppliesTierAndEmbroideryRules(
    decimal basePrice,
    string tierCode,
    string? embroidery,
    decimal expectedUnitPrice)
  {
    var unitPrice = LinePriceCalculator.CalculateUnitPrice(basePrice, tierCode, embroidery);

    Assert.Equal(expectedUnitPrice, unitPrice);
  }
}
