using Microsoft.Data.Sqlite;
using OrderHub.Core.Abstractions;
using OrderHub.Core.Models;
using OrderHub.Infrastructure.Sqlite;

namespace OrderHub.Infrastructure.Repositories;

public sealed class SqliteConfirmOrderRepository(SqliteConnectionFactory connectionFactory) : IConfirmOrderRepository
{
  public async Task<ConfirmOrderDraft?> GetDraftAsync(int schoolId, CancellationToken cancellationToken = default)
  {
    await using var connection = connectionFactory.CreateConnection();
    await connection.OpenAsync(cancellationToken);

    var schoolName = await GetSchoolNameAsync(connection, schoolId, cancellationToken);
    if (schoolName is null)
    {
      return null;
    }

    var lines = new List<ConfirmOrderLineItem>();

    await using var command = connection.CreateCommand();
    command.CommandText = """
      SELECT Id, Sku, Embroidery, UnitPrice, Quantity
      FROM ConfirmOrderLines
      WHERE SchoolId = $schoolId
      ORDER BY Id;
      """;
    command.Parameters.AddWithValue("$schoolId", schoolId);

    await using var reader = await command.ExecuteReaderAsync(cancellationToken);
    while (await reader.ReadAsync(cancellationToken))
    {
      lines.Add(new ConfirmOrderLineItem
      {
        Id = reader.GetInt32(0),
        Sku = reader.GetString(1),
        Embroidery = reader.IsDBNull(2) ? null : reader.GetString(2),
        UnitPrice = reader.GetDecimal(3),
        Quantity = reader.GetInt32(4)
      });
    }

    return new ConfirmOrderDraft
    {
      SchoolId = schoolId,
      SchoolName = schoolName,
      Lines = lines
    };
  }

  public async Task UpdateQuantitiesAsync(
    int schoolId,
    IReadOnlyList<ConfirmOrderLineUpdate> updates,
    CancellationToken cancellationToken = default)
  {
    await using var connection = connectionFactory.CreateConnection();
    await connection.OpenAsync(cancellationToken);
    await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);

    foreach (var update in updates)
    {
      await using var command = connection.CreateCommand();
      command.Transaction = transaction;
      command.CommandText = """
        UPDATE ConfirmOrderLines
        SET Quantity = $quantity
        WHERE Id = $lineId AND SchoolId = $schoolId;
        """;
      command.Parameters.AddWithValue("$quantity", update.Quantity);
      command.Parameters.AddWithValue("$lineId", update.LineId);
      command.Parameters.AddWithValue("$schoolId", schoolId);
      await command.ExecuteNonQueryAsync(cancellationToken);
    }

    await transaction.CommitAsync(cancellationToken);
  }

  private static async Task<string?> GetSchoolNameAsync(
    SqliteConnection connection,
    int schoolId,
    CancellationToken cancellationToken)
  {
    await using var command = connection.CreateCommand();
    command.CommandText = "SELECT Name FROM Schools WHERE Id = $schoolId;";
    command.Parameters.AddWithValue("$schoolId", schoolId);

    var result = await command.ExecuteScalarAsync(cancellationToken);
    return result is null or DBNull ? null : (string)result;
  }
}
