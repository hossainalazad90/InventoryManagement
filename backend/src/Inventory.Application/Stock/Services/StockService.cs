using Inventory.Application.Common;
using Inventory.Application.Stock.DTOs;
using Inventory.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Stock.Services;

public class StockService : IStockService
{
    private readonly IApplicationDbContext _context;

    public StockService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<decimal> GetAvailableQuantityAsync(int itemId, int storeId, CancellationToken ct = default)
    {
        return await _context.StockMovements
            .Where(m => m.ItemId == itemId && m.StoreId == storeId)
            .SumAsync(m => (decimal?)m.SignedQuantity, ct) ?? 0m;
    }

    public async Task<StockBalanceDto> GetStockBalanceAsync(int itemId, int storeId, CancellationToken ct = default)
    {
        var item = await _context.Items
            .Include(i => i.Unit)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == itemId, ct);

        if (item == null)
        {
            throw new EntityNotFoundException(nameof(Domain.Entities.Item), itemId);
        }

        var store = await _context.Stores
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == storeId, ct);

        if (store == null)
        {
            throw new EntityNotFoundException(nameof(Domain.Entities.Store), storeId);
        }

        var availableQty = await GetAvailableQuantityAsync(itemId, storeId, ct);

        return new StockBalanceDto(
            item.Id,
            item.ItemCode,
            item.ItemName,
            store.Id,
            store.Name,
            item.UnitId,
            item.Unit?.Name ?? string.Empty,
            availableQty,
            item.ReorderLevel,
            availableQty <= item.ReorderLevel
        );
    }

    public async Task<IReadOnlyList<StockBalanceDto>> GetAllStockBalancesAsync(int? storeId = null, CancellationToken ct = default)
    {
        var query = from item in _context.Items.Include(i => i.Unit)
                    from store in _context.Stores
                    where (!storeId.HasValue || store.Id == storeId.Value) && item.IsActive && store.IsActive
                    select new
                    {
                        Item = item,
                        Store = store,
                        AvailableQuantity = _context.StockMovements
                            .Where(m => m.ItemId == item.Id && m.StoreId == store.Id)
                            .Sum(m => (decimal?)m.SignedQuantity) ?? 0m
                    };

        var list = await query.ToListAsync(ct);

        return list.Select(x => new StockBalanceDto(
            x.Item.Id,
            x.Item.ItemCode,
            x.Item.ItemName,
            x.Store.Id,
            x.Store.Name,
            x.Item.UnitId,
            x.Item.Unit?.Name ?? string.Empty,
            x.AvailableQuantity,
            x.Item.ReorderLevel,
            x.AvailableQuantity <= x.Item.ReorderLevel
        )).ToList();
    }
}
