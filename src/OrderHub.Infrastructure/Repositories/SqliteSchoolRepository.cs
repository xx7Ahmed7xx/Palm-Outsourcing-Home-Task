using Microsoft.Data.Sqlite;
using OrderHub.Core.Abstractions;
using OrderHub.Infrastructure.Sqlite;

namespace OrderHub.Infrastructure.Repositories;

public sealed class SqliteSchoolRepository(SqliteConnectionFactory connectionFactory) : ISchoolRepository
{
  public async Task<string?> GetTierCodeAsync(int schoolId, CancellationToken cancellationToken = default)
  {
    await using var connection = connectionFactory.CreateConnection();
    await connection.OpenAsync(cancellationToken);

    await using var command = connection.CreateCommand();
    command.CommandText = "SELECT TierCode FROM Schools WHERE Id = $schoolId;";
    command.Parameters.AddWithValue("$schoolId", schoolId);

    var result = await command.ExecuteScalarAsync(cancellationToken);
    return result is null or DBNull ? null : (string)result;
  }
}
