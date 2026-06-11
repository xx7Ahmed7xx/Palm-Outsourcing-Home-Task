using Microsoft.Extensions.Logging;
using OrderHub.Core.Abstractions;

namespace OrderHub.Infrastructure.Services;

public sealed class LoggingPaymentGateway(ILogger<LoggingPaymentGateway> logger) : IPaymentGateway
{
  public Task<bool> CreatePaymentIntentAsync(decimal amount, string parentEmail, CancellationToken cancellationToken = default)
  {
    if (parentEmail.Contains("fail-payment", StringComparison.OrdinalIgnoreCase))
    {
      logger.LogWarning("Simulated payment failure for {Email}", parentEmail);
      return Task.FromResult(false);
    }

    logger.LogInformation("Payment intent created for {Email} amount {Amount:C}", parentEmail, amount);
    return Task.FromResult(true);
  }
}
