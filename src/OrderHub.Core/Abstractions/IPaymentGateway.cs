namespace OrderHub.Core.Abstractions;

public interface IPaymentGateway
{
    Task<bool> CreatePaymentIntentAsync(decimal amount, string parentEmail, CancellationToken cancellationToken = default);
}
