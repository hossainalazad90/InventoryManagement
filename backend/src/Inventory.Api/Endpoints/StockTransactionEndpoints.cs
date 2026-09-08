using Inventory.Application.Common;
using Inventory.Application.StockTransactions.DTOs;
using Inventory.Application.StockTransactions.Services;
using Inventory.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Api.Endpoints;

public static class StockTransactionEndpoints
{
    public static RouteGroupBuilder MapStockTransactionEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (
            [FromQuery] TransactionType? type,
            [FromQuery] int? storeId,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            IStockTransactionService service,
            CancellationToken ct) =>
        {
            var transactions = await service.GetAllAsync(type, storeId, fromDate, toDate, ct);
            return Results.Ok(ApiResponse<IReadOnlyList<StockTransactionDto>>.Ok(transactions));
        })
        .WithName("GetStockTransactions")
        .WithSummary("Get list of stock transactions with filters");

        group.MapGet("/{id:int}", async (int id, IStockTransactionService service, CancellationToken ct) =>
        {
            var tx = await service.GetByIdAsync(id, ct);
            return tx is not null
                ? Results.Ok(ApiResponse<StockTransactionDto>.Ok(tx))
                : Results.NotFound(ApiResponse.Fail("ENTITY_NOT_FOUND", $"Transaction {id} not found."));
        })
        .WithName("GetStockTransactionById")
        .WithSummary("Get single transaction details by ID");

        group.MapGet("/next-no", async ([FromQuery] TransactionType type, IStockTransactionService service, CancellationToken ct) =>
        {
            var nextNo = await service.GenerateTransactionNoAsync(type, ct);
            return Results.Ok(ApiResponse<string>.Ok(nextNo));
        })
        .WithName("GetNextTransactionNo")
        .WithSummary("Preview next generated transaction number");

        group.MapPost("/", async ([FromBody] CreateStockTransactionDto dto, IStockTransactionService service, CancellationToken ct) =>
        {
            var created = await service.CreateAsync(dto, ct);
            return Results.Created($"/api/v1/stock-transactions/{created.Id}", ApiResponse<StockTransactionDto>.Ok(created, "Stock transaction saved successfully."));
        })
        .WithName("CreateStockTransaction")
        .WithSummary("Create new stock transaction (Receive or Issue) with multi-row details");

        group.MapPut("/{id:int}", async (int id, [FromBody] UpdateStockTransactionDto dto, IStockTransactionService service, CancellationToken ct) =>
        {
            var updated = await service.UpdateAsync(id, dto, ct);
            return Results.Ok(ApiResponse<StockTransactionDto>.Ok(updated, "Stock transaction updated successfully."));
        })
        .WithName("UpdateStockTransaction")
        .WithSummary("Update transaction with delta detection for new/modified/deleted details");

        group.MapDelete("/{id:int}", async (int id, IStockTransactionService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return Results.Ok(ApiResponse.Ok("Stock transaction deleted and ledger reversed successfully."));
        })
        .WithName("DeleteStockTransaction")
        .WithSummary("Delete transaction and reverse ledger atomically");

        return group;
    }
}
