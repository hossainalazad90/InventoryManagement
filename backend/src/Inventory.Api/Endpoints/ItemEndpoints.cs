using Inventory.Application.Common;
using Inventory.Application.Items.DTOs;
using Inventory.Application.Items.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Api.Endpoints;

public static class ItemEndpoints
{
    public static RouteGroupBuilder MapItemEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (
            [FromQuery] bool? activeOnly,
            [FromQuery] string? search,
            IItemService service,
            CancellationToken ct) =>
        {
            var items = await service.GetAllAsync(activeOnly, search, ct);
            return Results.Ok(ApiResponse<IReadOnlyList<ItemDto>>.Ok(items));
        })
        .WithName("GetItems")
        .WithSummary("Get list of items with optional active filter and search");

        group.MapGet("/{id:int}", async (int id, IItemService service, CancellationToken ct) =>
        {
            var item = await service.GetByIdAsync(id, ct);
            return item is not null
                ? Results.Ok(ApiResponse<ItemDto>.Ok(item))
                : Results.NotFound(ApiResponse.Fail("ENTITY_NOT_FOUND", $"Item with ID {id} not found."));
        })
        .WithName("GetItemById")
        .WithSummary("Get single item by ID");

        group.MapPost("/", async ([FromBody] CreateItemDto dto, IItemService service, CancellationToken ct) =>
        {
            var created = await service.CreateAsync(dto, ct);
            return Results.Created($"/api/v1/items/{created.Id}", ApiResponse<ItemDto>.Ok(created, "Item created successfully."));
        })
        .WithName("CreateItem")
        .WithSummary("Create new item in Item Master");

        group.MapPut("/{id:int}", async (int id, [FromBody] UpdateItemDto dto, IItemService service, CancellationToken ct) =>
        {
            var updated = await service.UpdateAsync(id, dto, ct);
            return Results.Ok(ApiResponse<ItemDto>.Ok(updated, "Item updated successfully."));
        })
        .WithName("UpdateItem")
        .WithSummary("Update existing item in Item Master");

        group.MapDelete("/{id:int}", async (int id, IItemService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return Results.Ok(ApiResponse.Ok("Item deleted successfully."));
        })
        .WithName("DeleteItem")
        .WithSummary("Delete item (if no transaction history exists)");

        return group;
    }
}
