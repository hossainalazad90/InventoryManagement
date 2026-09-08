using Inventory.Application.Stock.DTOs;

namespace Inventory.Application.Stock.Services;

public interface IStockService
{
    Task<StockBalanceDto> GetStockBalanceAsync(int itemId, int storeId, CancellationToken ct = default);
    Task<decimal> GetAvailableQuantityAsync(int itemId, int storeId, CancellationToken ct = default);
    Task<IReadOnlyList<StockBalanceDto>> GetAllStockBalancesAsync(int? storeId = null, CancellationToken ct = default);
}
