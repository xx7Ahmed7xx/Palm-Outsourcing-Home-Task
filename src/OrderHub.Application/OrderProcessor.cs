using OrderHub.Application.Pricing;
using OrderHub.Core.Abstractions;
using OrderHub.Core.Models;

namespace OrderHub.Application;

public sealed class OrderProcessor(
  ISchoolRepository schoolRepository,
  IProductRepository productRepository,
  IStockRepository stockRepository,
  IPaymentGateway paymentGateway,
  IOrderNotifier orderNotifier)
{
  public async Task<ProcessOrderResult> ProcessOrderAsync(
    int schoolId,
    IReadOnlyList<OrderLine> lines,
    string parentEmail,
    CancellationToken cancellationToken = default)
  {
    var tierCode = await schoolRepository.GetTierCodeAsync(schoolId, cancellationToken);
    if (tierCode is null)
    {
      return ProcessOrderResult.Failure("FAIL: school not found");
    }

    decimal subtotal = 0m;

    foreach (var line in lines)
    {
      var basePrice = await productRepository.GetBasePriceAsync(line.Sku, cancellationToken);
      if (basePrice is null)
      {
        return ProcessOrderResult.Failure($"FAIL: product not found {line.Sku}");
      }

      var availableStock = await stockRepository.GetAvailableQuantityAsync(line.Sku, cancellationToken);
      if (availableStock < line.Quantity)
      {
        return ProcessOrderResult.Failure($"FAIL: out of stock {line.Sku}");
      }

      var unitPrice = LinePriceCalculator.CalculateUnitPrice(basePrice.Value, tierCode, line.Embroidery);
      subtotal += unitPrice * line.Quantity;
    }

    var paymentSucceeded = await paymentGateway.CreatePaymentIntentAsync(subtotal, parentEmail, cancellationToken);
    if (!paymentSucceeded)
    {
      return ProcessOrderResult.Failure("FAIL: payment");
    }

    try
    {
      await orderNotifier.SendOrderConfirmationAsync(parentEmail, subtotal, cancellationToken);
    }
    catch
    {
      // Legacy behaviour: confirmation email failure must not fail the order.
    }

    return ProcessOrderResult.Success(subtotal);
  }
}
