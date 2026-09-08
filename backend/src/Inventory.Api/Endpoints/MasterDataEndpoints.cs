using Inventory.Application.Common;
using Inventory.Application.MasterData.DTOs;
using Inventory.Application.MasterData.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Api.Endpoints;

public static class MasterDataEndpoints
{
    public static RouteGroupBuilder MapMasterDataEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/categories", async ([FromQuery] bool? activeOnly, IMasterDataService service, CancellationToken ct) =>
        {
            var list = await service.GetCategoriesAsync(activeOnly, ct);
            return Results.Ok(ApiResponse<IReadOnlyList<CategoryDto>>.Ok(list));
        })
        .WithName("GetCategories");

        group.MapGet("/units", async ([FromQuery] bool? activeOnly, IMasterDataService service, CancellationToken ct) =>
        {
            var list = await service.GetUnitsAsync(activeOnly, ct);
            return Results.Ok(ApiResponse<IReadOnlyList<UnitDto>>.Ok(list));
        })
        .WithName("GetUnits");

        group.MapGet("/stores", async ([FromQuery] bool? activeOnly, IMasterDataService service, CancellationToken ct) =>
        {
            var list = await service.GetStoresAsync(activeOnly, ct);
            return Results.Ok(ApiResponse<IReadOnlyList<StoreDto>>.Ok(list));
        })
        .WithName("GetStores");

        return group;
    }
}