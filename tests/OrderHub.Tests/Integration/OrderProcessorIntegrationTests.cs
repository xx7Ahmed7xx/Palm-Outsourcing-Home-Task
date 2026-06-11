using Microsoft.Data.Sqlite;
using OrderHub.Application;
using OrderHub.Core.Abstractions;
using OrderHub.Core.Models;
using OrderHub.Infrastructure.Repositories;
using OrderHub.Infrastructure.Services;
using OrderHub.Infrastructure.Sqlite;

namespace OrderHub.Tests.Integration;

public class OrderProcessorIntegrationTests : IAsyncLifetime
{
  private SqliteConnection _connection = null!;
  private OrderProcessor _processor = null!;

  public async Task InitializeAsync()
  {
    _connection = new SqliteConnection("Data Source=orderhub-tests;Mode=Memory;Cache=Shared");
    await _connection.OpenAsync();

    var factory = new SqliteConnectionFactory(_connection.ConnectionString);
    var initializer = new SqliteDatabaseInitializer(factory);
    await initializer.InitializeAsync();

    _processor = new OrderProcessor(
      new SqliteSchoolRepository(factory),
      new SqliteProductRepository(factory),
      new SqliteStockRepository(factory),
      new LoggingPaymentGateway(Microsoft.Extensions.Logging.Abstractions.NullLogger<LoggingPaymentGateway>.Instance),
      new LoggingOrderNotifier(Microsoft.Extensions.Logging.Abstractions.NullLogger<LoggingOrderNotifier>.Instance));
  }

  public async Task DisposeAsync()
  {
    await _connection.DisposeAsync();
  }

  [Fact]
  public async Task ProcessOrderAsync_UsesSqliteData_AppliesGoldTierAndEmbroidery()
  {
    var result = await _processor.ProcessOrderAsync(
      schoolId: 1,
      lines:
      [
        new OrderLine { Sku = "BLAZER-42", Quantity = 1, Embroidery = "AB" },
        new OrderLine { Sku = "TIE-RED", Quantity = 2 }
      ],
      parentEmail: "parent@example.com");

    Assert.True(result.IsSuccess);
    // BLAZER: 50 * 0.85 + 4.50 = 47.00; TIE: 8 * 0.85 * 2 = 13.60
    Assert.Equal(60.60m, result.Subtotal);
  }

  [Fact]
  public async Task ProcessOrderAsync_WhenOutOfStock_ReturnsFailure()
  {
    var result = await _processor.ProcessOrderAsync(
      schoolId: 1,
      lines: [new OrderLine { Sku = "BLAZER-42", Quantity = 99 }],
      parentEmail: "parent@example.com");

    Assert.False(result.IsSuccess);
    Assert.Contains("out of stock", result.Message, StringComparison.OrdinalIgnoreCase);
  }
}
