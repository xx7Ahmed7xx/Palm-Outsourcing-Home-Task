using Microsoft.Extensions.DependencyInjection;
using OrderHub.Core.Abstractions;
using OrderHub.Infrastructure.Repositories;
using OrderHub.Infrastructure.Services;
using OrderHub.Infrastructure.Sqlite;

namespace OrderHub.Infrastructure;

public static class DependencyInjection
{
  public static IServiceCollection AddOrderHubInfrastructure(
    this IServiceCollection services,
    string connectionString)
  {
    services.AddSingleton(new SqliteConnectionFactory(connectionString));
    services.AddSingleton<SqliteDatabaseInitializer>();

    services.AddScoped<ISchoolRepository, SqliteSchoolRepository>();
    services.AddScoped<IProductRepository, SqliteProductRepository>();
    services.AddScoped<IStockRepository, SqliteStockRepository>();
    services.AddScoped<IConfirmOrderRepository, SqliteConfirmOrderRepository>();
    services.AddScoped<IPaymentGateway, LoggingPaymentGateway>();
    services.AddScoped<IOrderNotifier, LoggingOrderNotifier>();

    return services;
  }
}
