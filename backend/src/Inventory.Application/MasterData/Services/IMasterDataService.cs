using Inventory.Application.MasterData.DTOs;

namespace Inventory.Application.MasterData.Services;

public interface IMasterDataService
{
    Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(bool? activeOnly = null, CancellationToken ct = default);
    Task<IReadOnlyList<UnitDto>> GetUnitsAsync(bool? activeOnly = null, CancellationToken ct = default);
    Task<IReadOnlyList<StoreDto>> GetStoresAsync(bool? activeOnly = null, CancellationToken ct = default);
}
