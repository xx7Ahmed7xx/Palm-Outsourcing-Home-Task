namespace OrderHub.Core.Abstractions;

public interface IStockRepository
{
    Task<int> GetAvailableQuantityAsync(string sku, CancellationToken cancellationToken = default);
}
