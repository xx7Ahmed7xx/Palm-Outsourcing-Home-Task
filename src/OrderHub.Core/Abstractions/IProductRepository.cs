namespace OrderHub.Core.Abstractions;

public interface IProductRepository
{
    Task<decimal?> GetBasePriceAsync(string sku, CancellationToken cancellationToken = default);
}
