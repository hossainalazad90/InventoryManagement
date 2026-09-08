using Inventory.Application.Common;
using Inventory.Application.Stock.DTOs;
using Inventory.Application.Stock.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Api.Endpoints;

public static class StockEndpoints
{
    public static RouteGroupBuilder MapStockEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async ([FromQuery] int itemId, [FromQuery] int storeId, IStockService service, CancellationToken ct) =>
        {
            var balance = await service.GetStockBalanceAsync(itemId, storeId, ct);
            return Results.Ok(ApiResponse<StockBalanceDto>.Ok(balance));
        })
        .WithName("GetStockBalance")
        .WithSummary("Get available stock balance for an item in a specific store");

        group.MapGet("/balances", async ([FromQuery] int? storeId, IStockService service, CancellationToken ct) =>
        {
            var list = await service.GetAllStockBalancesAsync(storeId, ct);
            return Results.Ok(ApiResponse<IReadOnlyList<StockBalanceDto>>.Ok(list));
        })
        .WithName("GetAllStockBalances")
        .WithSummary("Get available stock balance list across all items and stores");

        return group;
    }
}
