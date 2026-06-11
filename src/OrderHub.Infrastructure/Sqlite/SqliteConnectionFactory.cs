using Microsoft.Data.Sqlite;

namespace OrderHub.Infrastructure.Sqlite;

public sealed class SqliteConnectionFactory(string connectionString)
{
  public string ConnectionString { get; } = connectionString;

  public SqliteConnection CreateConnection() => new(ConnectionString);
}
