using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Inventory.Application.Common;
using Inventory.Application.Items.DTOs;
using Xunit;

namespace Inventory.IntegrationTests;

public class ItemIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ItemIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllItems_ReturnsSuccessAndList()
    {
        var response = await _client.GetAsync("/api/v1/items");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<ItemDto>>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateItem_ValidPayload_CreatesSuccessfully()
    {
        var uniqueCode = $"TEST-{Guid.NewGuid():N}"[..12].ToUpper();
        var dto = new CreateItemDto(uniqueCode, "Integration Test Item", 1, 1, 10m, true);

        var response = await _client.PostAsJsonAsync("/api/v1/items", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ItemDto>>();
        result.Should().NotBeNull();
        result!.Data!.ItemCode.Should().Be(uniqueCode);
    }

    [Fact]
    public async Task CreateItem_DuplicateCode_ReturnsBadRequest()
    {
        var uniqueCode = $"DUP-{Guid.NewGuid():N}"[..12].ToUpper();
        var dto = new CreateItemDto(uniqueCode, "Duplicate Test Item", 1, 1, 10m, true);

        var res1 = await _client.PostAsJsonAsync("/api/v1/items", dto);
        res1.StatusCode.Should().Be(HttpStatusCode.Created);

        var res2 = await _client.PostAsJsonAsync("/api/v1/items", dto);
        res2.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var err = await res2.Content.ReadFromJsonAsync<ApiResponse<object>>();
        err!.Code.Should().Be("VALIDATION_ERROR");
    }
}
