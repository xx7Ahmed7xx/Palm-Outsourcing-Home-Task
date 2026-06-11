using Microsoft.Extensions.Logging;
using OrderHub.Core.Abstractions;

namespace OrderHub.Infrastructure.Services;

public sealed class LoggingOrderNotifier(ILogger<LoggingOrderNotifier> logger) : IOrderNotifier
{
  public Task SendOrderConfirmationAsync(string parentEmail, decimal subtotal, CancellationToken cancellationToken = default)
  {
    logger.LogInformation(
      "Order confirmation email sent to {Email} (total {Subtotal:C})",
      parentEmail,
      subtotal);
    return Task.CompletedTask;
  }
}
