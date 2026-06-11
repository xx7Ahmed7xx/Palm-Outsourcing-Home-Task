using Microsoft.Data.Sqlite;
using OrderHub.Core.Abstractions;
using OrderHub.Infrastructure.Sqlite;

namespace OrderHub.Infrastructure.Repositories;

public sealed class SqliteStockRepository(SqliteConnectionFactory connectionFactory) : IStockRepository
{
  public async Task<int> GetAvailableQuantityAsync(string sku, CancellationToken cancellationToken = default)
  {
    await using var connection = connectionFactory.CreateConnection();
    await connection.OpenAsync(cancellationToken);

    await using var command = connection.CreateCommand();
    command.CommandText = "SELECT Qty FROM Stock WHERE Sku = $sku;";
    command.Parameters.AddWithValue("$sku", sku);

    var result = await command.ExecuteScalarAsync(cancellationToken);
    return result is null or DBNull ? 0 : Convert.ToInt32(result);
  }
}
