using Inventory.Application.Common;
using Inventory.Application.MasterData.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.MasterData.Services;

public class MasterDataService : IMasterDataService
{
    private readonly IApplicationDbContext _context;

    public MasterDataService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(bool? activeOnly = null, CancellationToken ct = default)
    {
        var q = _context.Categories.AsNoTracking();
        if (activeOnly.HasValue && activeOnly.Value) q = q.Where(c => c.IsActive);
        return await q.OrderBy(c => c.Name).Select(c => new CategoryDto(c.Id, c.Name, c.IsActive)).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<UnitDto>> GetUnitsAsync(bool? activeOnly = null, CancellationToken ct = default)
    {
        var q = _context.Units.AsNoTracking();
        if (activeOnly.HasValue && activeOnly.Value) q = q.Where(u => u.IsActive);
        return await q.OrderBy(u => u.Name).Select(u => new UnitDto(u.Id, u.Name, u.IsActive)).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<StoreDto>> GetStoresAsync(bool? activeOnly = null, CancellationToken ct = default)
    {
        var q = _context.Stores.AsNoTracking();
        if (activeOnly.HasValue && activeOnly.Value) q = q.Where(s => s.IsActive);
        return await q.OrderBy(s => s.Code).Select(s => new StoreDto(s.Id, s.Code, s.Name, s.IsActive)).ToListAsync(ct);
    }
}
