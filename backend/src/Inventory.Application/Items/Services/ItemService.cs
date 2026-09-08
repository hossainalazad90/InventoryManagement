using FluentValidation;
using Inventory.Application.Common;
using Inventory.Application.Items.DTOs;
using Inventory.Domain.Entities;
using Inventory.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using ValidationException = Inventory.Domain.Exceptions.ValidationException;

namespace Inventory.Application.Items.Services;

public class ItemService : IItemService
{
    private readonly IApplicationDbContext _context;
    private readonly IValidator<CreateItemDto> _createValidator;
    private readonly IValidator<UpdateItemDto> _updateValidator;

    public ItemService(
        IApplicationDbContext context,
        IValidator<CreateItemDto> createValidator,
        IValidator<UpdateItemDto> updateValidator)
    {
        _context = context;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyList<ItemDto>> GetAllAsync(bool? activeOnly = null, string? search = null, CancellationToken ct = default)
    {
        var query = _context.Items
            .Include(i => i.Category)
            .Include(i => i.Unit)
            .AsNoTracking();

        if (activeOnly.HasValue && activeOnly.Value)
        {
            query = query.Where(i => i.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(i => i.ItemCode.ToLower().Contains(s) || i.ItemName.ToLower().Contains(s));
        }

        var items = await query
            .OrderBy(i => i.ItemCode)
            .Select(i => new ItemDto(
                i.Id,
                i.ItemCode,
                i.ItemName,
                i.CategoryId,
                i.Category != null ? i.Category.Name : string.Empty,
                i.UnitId,
                i.Unit != null ? i.Unit.Name : string.Empty,
                i.ReorderLevel,
                i.IsActive,
                i.CreatedAt,
                i.UpdatedAt
            ))
            .ToListAsync(ct);

        return items;
    }

    public async Task<ItemDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var item = await _context.Items
            .Include(i => i.Category)
            .Include(i => i.Unit)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id, ct);

        if (item == null) return null;

        return new ItemDto(
            item.Id,
            item.ItemCode,
            item.ItemName,
            item.CategoryId,
            item.Category?.Name ?? string.Empty,
            item.UnitId,
            item.Unit?.Name ?? string.Empty,
            item.ReorderLevel,
            item.IsActive,
            item.CreatedAt,
            item.UpdatedAt
        );
    }

    public async Task<ItemDto> CreateAsync(CreateItemDto dto, CancellationToken ct = default)
    {
        var valResult = await _createValidator.ValidateAsync(dto, ct);
        if (!valResult.IsValid)
        {
            throw new ValidationException(valResult.ToDictionary());
        }

        var duplicate = await _context.Items.AnyAsync(i => i.ItemCode.ToLower() == dto.ItemCode.Trim().ToLower(), ct);
        if (duplicate)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { nameof(dto.ItemCode), new[] { $"Item Code '{dto.ItemCode}' already exists." } }
            });
        }

        var item = new Item
        {
            ItemCode = dto.ItemCode.Trim().ToUpper(),
            ItemName = dto.ItemName.Trim(),
            CategoryId = dto.CategoryId,
            UnitId = dto.UnitId,
            ReorderLevel = dto.ReorderLevel,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.Items.Add(item);
        await _context.SaveChangesAsync(ct);

        return (await GetByIdAsync(item.Id, ct))!;
    }

    public async Task<ItemDto> UpdateAsync(int id, UpdateItemDto dto, CancellationToken ct = default)
    {
        var valResult = await _updateValidator.ValidateAsync(dto, ct);
        if (!valResult.IsValid)
        {
            throw new ValidationException(valResult.ToDictionary());
        }

        var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == id, ct);
        if (item == null)
        {
            throw new EntityNotFoundException(nameof(Item), id);
        }

        var duplicate = await _context.Items.AnyAsync(i => i.Id != id && i.ItemCode.ToLower() == dto.ItemCode.Trim().ToLower(), ct);
        if (duplicate)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { nameof(dto.ItemCode), new[] { $"Item Code '{dto.ItemCode}' already exists." } }
            });
        }

        item.ItemCode = dto.ItemCode.Trim().ToUpper();
        item.ItemName = dto.ItemName.Trim();
        item.CategoryId = dto.CategoryId;
        item.UnitId = dto.UnitId;
        item.ReorderLevel = dto.ReorderLevel;
        item.IsActive = dto.IsActive;
        item.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return (await GetByIdAsync(item.Id, ct))!;
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == id, ct);
        if (item == null)
        {
            throw new EntityNotFoundException(nameof(Item), id);
        }

        var hasTransactions = await _context.StockTransactionDetails.AnyAsync(d => d.ItemId == id, ct);
        if (hasTransactions)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "Item", new[] { "Cannot delete Item because it has associated stock transaction history. You can mark it as Inactive instead." } }
            });
        }

        _context.Items.Remove(item);
        await _context.SaveChangesAsync(ct);
    }
}
