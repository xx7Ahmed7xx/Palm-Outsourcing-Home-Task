using Microsoft.Data.Sqlite;

namespace OrderHub.Infrastructure.Sqlite;

public sealed class SqliteDatabaseInitializer(SqliteConnectionFactory connectionFactory)
{
  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    await using var connection = connectionFactory.CreateConnection();
    await connection.OpenAsync(cancellationToken);

    await ExecuteAsync(connection, """
      CREATE TABLE IF NOT EXISTS Schools (
        Id       INTEGER PRIMARY KEY,
        Name     TEXT    NOT NULL,
        TierCode TEXT    NOT NULL
      );

      CREATE TABLE IF NOT EXISTS Products (
        Sku       TEXT PRIMARY KEY,
        BasePrice REAL NOT NULL
      );

      CREATE TABLE IF NOT EXISTS Stock (
        Sku TEXT PRIMARY KEY,
        Qty INTEGER NOT NULL,
        FOREIGN KEY (Sku) REFERENCES Products (Sku)
      );

      CREATE TABLE IF NOT EXISTS ConfirmOrderLines (
        Id         INTEGER PRIMARY KEY,
        SchoolId   INTEGER NOT NULL,
        Sku        TEXT    NOT NULL,
        Embroidery TEXT,
        UnitPrice  REAL    NOT NULL,
        Quantity   INTEGER NOT NULL,
        FOREIGN KEY (SchoolId) REFERENCES Schools (Id)
      );
      """, cancellationToken);

    if (!await HasSeedDataAsync(connection, cancellationToken))
    {
      await SeedAsync(connection, cancellationToken);
    }
  }

  private static async Task<bool> HasSeedDataAsync(SqliteConnection connection, CancellationToken cancellationToken)
  {
    await using var command = connection.CreateCommand();
    command.CommandText = "SELECT COUNT(*) FROM Schools;";
    var count = (long)(await command.ExecuteScalarAsync(cancellationToken) ?? 0L);
    return count > 0;
  }

  private static async Task SeedAsync(SqliteConnection connection, CancellationToken cancellationToken)
  {
    await ExecuteAsync(connection, """
      INSERT INTO Schools (Id, Name, TierCode) VALUES
        (1, 'St. Brindleford Academy', 'GOLD');

      INSERT INTO Products (Sku, BasePrice) VALUES
        ('BLAZER-42', 50.00),
        ('TIE-RED', 8.00);

      INSERT INTO Stock (Sku, Qty) VALUES
        ('BLAZER-42', 10),
        ('TIE-RED', 50);

      INSERT INTO ConfirmOrderLines (Id, SchoolId, Sku, Embroidery, UnitPrice, Quantity) VALUES
        (1, 1, 'BLAZER-42', 'J.S.', 42.50, 1),
        (2, 1, 'TIE-RED', NULL, 8.00, 2);
      """, cancellationToken);
  }

  private static async Task ExecuteAsync(SqliteConnection connection, string sql, CancellationToken cancellationToken)
  {
    await using var command = connection.CreateCommand();
    command.CommandText = sql;
    await command.ExecuteNonQueryAsync(cancellationToken);
  }
}
