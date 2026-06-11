using OrderHub.Application;
using OrderHub.Infrastructure;
using OrderHub.Infrastructure.Sqlite;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

var databasePath = Path.Combine(builder.Environment.ContentRootPath, "orderhub.db");
builder.Services.AddOrderHubInfrastructure($"Data Source={databasePath}");
builder.Services.AddScoped<OrderProcessor>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
  await scope.ServiceProvider.GetRequiredService<SqliteDatabaseInitializer>().InitializeAsync();
}

if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Error");
  app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

app.Run();
