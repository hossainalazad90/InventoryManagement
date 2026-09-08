using Inventory.Application.Items.DTOs;

namespace Inventory.Application.Items.Services;

public interface IItemService
{
    Task<IReadOnlyList<ItemDto>> GetAllAsync(bool? activeOnly = null, string? search = null, CancellationToken ct = default);
    Task<ItemDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ItemDto> CreateAsync(CreateItemDto dto, CancellationToken ct = default);
    Task<ItemDto> UpdateAsync(int id, UpdateItemDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
