using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Inventory.Application.Common;
using Inventory.Application.Stock.DTOs;
using Inventory.Application.StockTransactions.DTOs;
using Inventory.Domain.Enums;
using Xunit;

namespace Inventory.BDDTests;

public class StockTransactionScenarios : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public StockTransactionScenarios(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Scenario_17_1_ReceiveStockSuccessfully()
    {
        // GIVEN item "ITEM-002" has initial available stock in Store 1
        var balRes = await _client.GetFromJsonAsync<ApiResponse<StockBalanceDto>>("/api/v1/stock?itemId=2&storeId=1");
        var initialStock = balRes!.Data!.AvailableQuantity;

        // WHEN the user receives 50 units
        var receiveDto = new CreateStockTransactionDto(
            DateTime.UtcNow,
            TransactionType.Receive,
            1,
            "BDD 17.1 Receive",
            new List<CreateStockTransactionDetailDto>
            {
                new(2, 50m, 4, "Receive test", DateTime.UtcNow, true)
            }
        );
        var postRes = await _client.PostAsJsonAsync("/api/v1/stock-transactions", receiveDto);

        // THEN the transaction should be saved
        postRes.StatusCode.Should().Be(HttpStatusCode.Created);

        // AND the available stock should increase by 50 units
        var afterRes = await _client.GetFromJsonAsync<ApiResponse<StockBalanceDto>>("/api/v1/stock?itemId=2&storeId=1");
        afterRes!.Data!.AvailableQuantity.Should().Be(initialStock + 50m);
    }

    [Fact]
    public async Task Scenario_17_2_PreventNegativeStock()
    {
        // GIVEN item "ITEM-001" has available stock in Store 1
        var balRes = await _client.GetFromJsonAsync<ApiResponse<StockBalanceDto>>("/api/v1/stock?itemId=1&storeId=1");
        var available = balRes!.Data!.AvailableQuantity;

        // WHEN the user attempts to issue more than available stock
        var excessiveQty = available + 100m;
        var issueDto = new CreateStockTransactionDto(
            DateTime.UtcNow,
            TransactionType.Issue,
            1,
            "BDD 17.2 Excessive Issue",
            new List<CreateStockTransactionDetailDto>
            {
                new(1, excessiveQty, 1, "Should be rejected", DateTime.UtcNow, true)
            }
        );
        var postRes = await _client.PostAsJsonAsync("/api/v1/stock-transactions", issueDto);

        // THEN the transaction should be rejected
        postRes.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await postRes.Content.ReadFromJsonAsync<ApiResponse<object>>();
        error!.Code.Should().Be("INSUFFICIENT_STOCK");

        // AND the available stock should remain unchanged
        var afterRes = await _client.GetFromJsonAsync<ApiResponse<StockBalanceDto>>("/api/v1/stock?itemId=1&storeId=1");
        afterRes!.Data!.AvailableQuantity.Should().Be(available);
    }

    [Fact]
    public async Task Scenario_17_3_MultiDetailUpdate()
    {
        // GIVEN an existing transaction has 3 details
        var createDto = new CreateStockTransactionDto(
            DateTime.UtcNow,
            TransactionType.Receive,
            1,
            "BDD 17.3 Multi Detail Initial",
            new List<CreateStockTransactionDetailDto>
            {
                new(1, 10m, 1, "D1", DateTime.UtcNow, true),
                new(2, 20m, 4, "D2", DateTime.UtcNow, true),
                new(3, 30m, 2, "D3", DateTime.UtcNow, false)
            }
        );
        var res = await _client.PostAsJsonAsync("/api/v1/stock-transactions", createDto);
        var tx = (await res.Content.ReadFromJsonAsync<ApiResponse<StockTransactionDto>>())!.Data!;
        tx.Details.Should().HaveCount(3);

        var d1 = tx.Details[0];
        var d2 = tx.Details[1];
        var d3 = tx.Details[2];

        // WHEN one detail is modified, one is deleted, and one new detail is added
        var updateDto = new UpdateStockTransactionDto(
            tx.Id,
            tx.TransactionDate,
            TransactionType.Receive,
            tx.StoreId,
            "BDD 17.3 Updated",
            new List<UpdateStockTransactionDetailDto>
            {
                new(d1.Id, d1.ItemId, 25m, d1.UnitId, "Modified D1", d1.DetailDate, true), // Modified
                new(0, 4, 15m, 5, "New D4", DateTime.UtcNow, true) // New
            },
            new List<int> { d2.Id, d3.Id } // Deleted d2 and d3
        );

        var updateRes = await _client.PutAsJsonAsync($"/api/v1/stock-transactions/{tx.Id}", updateDto);
        updateRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedTx = (await updateRes.Content.ReadFromJsonAsync<ApiResponse<StockTransactionDto>>())!.Data!;

        // THEN the modified detail should be updated
        updatedTx.Details.First(d => d.Id == d1.Id).Quantity.Should().Be(25m);

        // AND the deleted details should be removed
        updatedTx.Details.Should().NotContain(d => d.Id == d2.Id);
        updatedTx.Details.Should().NotContain(d => d.Id == d3.Id);

        // AND the new detail should be inserted
        updatedTx.Details.Should().Contain(d => d.ItemId == 4 && d.Quantity == 15m);
    }

    [Fact]
    public async Task Scenario_17_4_RollbackOnFailure()
    {
        // GIVEN initial stock of items
        var bal1 = (await _client.GetFromJsonAsync<ApiResponse<StockBalanceDto>>("/api/v1/stock?itemId=1&storeId=1"))!.Data!.AvailableQuantity;
        var bal2 = (await _client.GetFromJsonAsync<ApiResponse<StockBalanceDto>>("/api/v1/stock?itemId=2&storeId=1"))!.Data!.AvailableQuantity;

        // WHEN a transaction receives 50 units of ITEM-001 AND attempts to issue 99999 units of ITEM-002
        // Since Issue cannot be mixed in Receive, or if an issue row has insufficient stock:
        var badTx = new CreateStockTransactionDto(
            DateTime.UtcNow,
            TransactionType.Issue,
            1,
            "BDD 17.4 Atomic Rollback Test",
            new List<CreateStockTransactionDetailDto>
            {
                new(1, 10m, 1, "Valid item issue", DateTime.UtcNow, true),
                new(2, 999999m, 4, "Excessive item issue", DateTime.UtcNow, true)
            }
        );

        var response = await _client.PostAsJsonAsync("/api/v1/stock-transactions", badTx);

        // THEN the transaction should fail
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // AND all balances should remain completely intact (no partial commit)
        var afterBal1 = (await _client.GetFromJsonAsync<ApiResponse<StockBalanceDto>>("/api/v1/stock?itemId=1&storeId=1"))!.Data!.AvailableQuantity;
        var afterBal2 = (await _client.GetFromJsonAsync<ApiResponse<StockBalanceDto>>("/api/v1/stock?itemId=2&storeId=1"))!.Data!.AvailableQuantity;

        afterBal1.Should().Be(bal1);
        afterBal2.Should().Be(bal2);
    }
}
