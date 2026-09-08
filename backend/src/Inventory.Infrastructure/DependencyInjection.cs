using FluentValidation;
using Inventory.Application.Common;
using Inventory.Application.Items.DTOs;
using Inventory.Application.Items.Services;
using Inventory.Application.Items.Validators;
using Inventory.Application.MasterData.Services;
using Inventory.Application.Reports.Services;
using Inventory.Application.Stock.Services;
using Inventory.Application.StockTransactions.DTOs;
using Inventory.Application.StockTransactions.Services;
using Inventory.Application.StockTransactions.Validators;
using Inventory.Infrastructure.Persistence;
using Inventory.Infrastructure.Reports;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Inventory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string contentRootPath)
    {
        var useInMemory = bool.TryParse(configuration["UseInMemoryDatabase"], out var isMem) && isMem;
        if (useInMemory)
        {
            var dbName = configuration["InMemoryDbName"] ?? "InventoryTestDb_" + Guid.NewGuid();
            services.AddDbContext<InventoryDbContext>(options =>
                options.UseInMemoryDatabase(dbName));
        }
        else
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? "Server=(localdb)\\MSSQLLocalDB;Database=InventoryManagementDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

            services.AddDbContext<InventoryDbContext>(options =>
                options.UseSqlServer(connectionString));
        }

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<InventoryDbContext>());

        // Application Services
        services.AddScoped<IItemService, ItemService>();
        services.AddScoped<IMasterDataService, MasterDataService>();
        services.AddScoped<IStockService, StockService>();
        services.AddScoped<IStockTransactionService, StockTransactionService>();
        services.AddScoped<IReportQueryService, ReportQueryService>();

        // Validators
        services.AddScoped<IValidator<CreateItemDto>, CreateItemDtoValidator>();
        services.AddScoped<IValidator<UpdateItemDto>, UpdateItemDtoValidator>();
        services.AddScoped<IValidator<CreateStockTransactionDto>, CreateStockTransactionValidator>();
        services.AddScoped<IValidator<UpdateStockTransactionDto>, UpdateStockTransactionValidator>();

        // RDLC Report Generator
        var reportsPath = Path.Combine(contentRootPath, "reports");
        if (!Directory.Exists(reportsPath))
        {
            var slnReportsPath = Path.GetFullPath(Path.Combine(contentRootPath, "..", "..", "reports"));
            if (Directory.Exists(slnReportsPath))
            {
                reportsPath = slnReportsPath;
            }
        }

        services.AddSingleton<IRdlcReportGenerator>(_ => new RdlcReportGenerator(reportsPath));

        return services;
    }
}