namespace OrderHub.Core.Abstractions;

public interface IOrderNotifier
{
    Task SendOrderConfirmationAsync(string parentEmail, decimal subtotal, CancellationToken cancellationToken = default);
}
