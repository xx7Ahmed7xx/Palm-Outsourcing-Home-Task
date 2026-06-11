using Microsoft.Data.Sqlite;
using OrderHub.Core.Abstractions;
using OrderHub.Infrastructure.Sqlite;

namespace OrderHub.Infrastructure.Repositories;

public sealed class SqliteProductRepository(SqliteConnectionFactory connectionFactory) : IProductRepository
{
  public async Task<decimal?> GetBasePriceAsync(string sku, CancellationToken cancellationToken = default)
  {
    await using var connection = connectionFactory.CreateConnection();
    await connection.OpenAsync(cancellationToken);

    await using var command = connection.CreateCommand();
    command.CommandText = "SELECT BasePrice FROM Products WHERE Sku = $sku;";
    command.Parameters.AddWithValue("$sku", sku);

    var result = await command.ExecuteScalarAsync(cancellationToken);
    return result is null or DBNull ? null : Convert.ToDecimal(result);
  }
}
