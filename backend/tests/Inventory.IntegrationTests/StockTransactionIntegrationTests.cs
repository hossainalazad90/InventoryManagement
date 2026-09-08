using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Inventory.Application.Common;
using Inventory.Application.Stock.DTOs;
using Inventory.Application.StockTransactions.DTOs;
using Inventory.Domain.Enums;
using Xunit;

namespace Inventory.IntegrationTests;

public class StockTransactionIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public StockTransactionIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateReceiveTransaction_IncreasesAvailableStock()
    {
        // 1. Get initial balance of Item 2 in Store 1
        var balBefore = await _client.GetFromJsonAsync<ApiResponse<StockBalanceDto>>("/api/v1/stock?itemId=2&storeId=1");
        var initialQty = balBefore!.Data!.AvailableQuantity;

        // 2. Post Receive of 50 units
        var createDto = new CreateStockTransactionDto(
            DateTime.UtcNow,
            TransactionType.Receive,
            1,
            "Integration Test Receive",
            new List<CreateStockTransactionDetailDto>
            {
                new(2, 50m, 4, "Batch Recv", DateTime.UtcNow, true)
            }
        );

        var response = await _client.PostAsJsonAsync("/api/v1/stock-transactions", createDto);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // 3. Verify stock increased by 50
        var balAfter = await _client.GetFromJsonAsync<ApiResponse<StockBalanceDto>>("/api/v1/stock?itemId=2&storeId=1");
        balAfter!.Data!.AvailableQuantity.Should().Be(initialQty + 50m);
    }

    [Fact]
    public async Task CreateIssueTransaction_BeyondAvailableStock_ReturnsInsufficientStockError()
    {
        // Available for Item 1 in Store 1 is 75 (100 initial - 25 issued)
        var bal = await _client.GetFromJsonAsync<ApiResponse<StockBalanceDto>>("/api/v1/stock?itemId=1&storeId=1");
        var available = bal!.Data!.AvailableQuantity;

        // Attempt to issue more than available
        var excessiveQty = available + 1000m;
        var createDto = new CreateStockTransactionDto(
            DateTime.UtcNow,
            TransactionType.Issue,
            1,
            "Should fail due to negative stock prevention",
            new List<CreateStockTransactionDetailDto>
            {
                new(1, excessiveQty, 1, "Exceeds stock", DateTime.UtcNow, true)
            }
        );

        var response = await _client.PostAsJsonAsync("/api/v1/stock-transactions", createDto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var err = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        err.Should().NotBeNull();
        err!.Code.Should().Be("INSUFFICIENT_STOCK");
        err.Message.Should().Contain("Available stock is");

        // Verify balance remained unchanged
        var balAfter = await _client.GetFromJsonAsync<ApiResponse<StockBalanceDto>>("/api/v1/stock?itemId=1&storeId=1");
        balAfter!.Data!.AvailableQuantity.Should().Be(available);
    }

    [Fact]
    public async Task UpdateStockTransaction_MultiDetailDelta_HandlesNewModifiedAndDeletedCorrectly()
    {
        // 1. Create transaction with 2 details (Receive 100 of Item 3 and 100 of Item 4)
        var createDto = new CreateStockTransactionDto(
            DateTime.UtcNow,
            TransactionType.Receive,
            1,
            "Initial multi-detail",
            new List<CreateStockTransactionDetailDto>
            {
                new(3, 100m, 2, "Row 1", DateTime.UtcNow, true),
                new(4, 100m, 5, "Row 2", DateTime.UtcNow, false)
            }
        );

        var createRes = await _client.PostAsJsonAsync("/api/v1/stock-transactions", createDto);
        createRes.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdTx = (await createRes.Content.ReadFromJsonAsync<ApiResponse<StockTransactionDto>>())!.Data!;

        var row1 = createdTx.Details.First(d => d.ItemId == 3);
        var row2 = createdTx.Details.First(d => d.ItemId == 4);

        // 2. Prepare Update:
        // - Row 1 modified (quantity changed from 100 to 150)
        // - Row 2 deleted (omitted from Details and listed in DeletedDetailIds)
        // - New Row added (Id = 0, Item 5 with 30 units)
        var updateDto = new UpdateStockTransactionDto(
            createdTx.Id,
            createdTx.TransactionDate,
            TransactionType.Receive,
            1,
            "Updated multi-detail",
            new List<UpdateStockTransactionDetailDto>
            {
                new(row1.Id, row1.ItemId, 150m, row1.UnitId, "Modified Row 1", row1.DetailDate, true),
                new(0, 5, 30m, 5, "New Row 3", DateTime.UtcNow, true)
            },
            new List<int> { row2.Id }
        );

        var updateRes = await _client.PutAsJsonAsync($"/api/v1/stock-transactions/{createdTx.Id}", updateDto);
        updateRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedTx = (await updateRes.Content.ReadFromJsonAsync<ApiResponse<StockTransactionDto>>())!.Data!;

        // 3. Verify detail count and items
        updatedTx.Details.Should().HaveCount(2);
        updatedTx.Details.Should().NotContain(d => d.Id == row2.Id);
        updatedTx.Details.First(d => d.Id == row1.Id).Quantity.Should().Be(150m);
        updatedTx.Details.First(d => d.ItemId == 5).Quantity.Should().Be(30m);
    }
}
