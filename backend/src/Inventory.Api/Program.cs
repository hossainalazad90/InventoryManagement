using Inventory.Api.Endpoints;
using Inventory.Api.Middleware;
using Inventory.Infrastructure;
using Inventory.Infrastructure.Persistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddInfrastructure(builder.Configuration, builder.Environment.ContentRootPath);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<InventoryDbContext>();
        await DbInitializer.InitializeAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
    }
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("Inventory Management API")
            .WithTheme(ScalarTheme.Kepler);
    });
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();

app.MapGroup("/api/v1/items").MapItemEndpoints().WithTags("Items");
app.MapGroup("/api/v1/stock").MapStockEndpoints().WithTags("Stock");
app.MapGroup("/api/v1/stock-transactions").MapStockTransactionEndpoints().WithTags("Stock Transactions");
app.MapGroup("/api/v1/reports").MapReportEndpoints().WithTags("Reports");
app.MapGroup("/api/v1").MapMasterDataEndpoints().WithTags("Master Data");



app.Run();

public partial class Program { }