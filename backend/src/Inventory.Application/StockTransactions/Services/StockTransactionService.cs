using FluentValidation;
using Inventory.Application.Common;
using Inventory.Application.StockTransactions.DTOs;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Inventory.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using ValidationException = Inventory.Domain.Exceptions.ValidationException;

namespace Inventory.Application.StockTransactions.Services;

public class StockTransactionService : IStockTransactionService
{
    private readonly IApplicationDbContext _context;
    private readonly IValidator<CreateStockTransactionDto> _createValidator;
    private readonly IValidator<UpdateStockTransactionDto> _updateValidator;

    public StockTransactionService(
        IApplicationDbContext context,
        IValidator<CreateStockTransactionDto> createValidator,
        IValidator<UpdateStockTransactionDto> updateValidator)
    {
        _context = context;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<string> GenerateTransactionNoAsync(TransactionType type, CancellationToken ct = default)
    {
        var prefix = type switch
        {
            TransactionType.Receive => "RCV",
            TransactionType.Issue => "ISS",
            TransactionType.Return => "RET",
            _ => "TRX"
        };

        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var countToday = await _context.StockTransactions
            .CountAsync(t => t.TransactionNo.StartsWith($"{prefix}-{datePart}"), ct);

        return $"{prefix}-{datePart}-{(countToday + 1):D4}";
    }

    public async Task<IReadOnlyList<StockTransactionDto>> GetAllAsync(
        TransactionType? type = null, 
        int? storeId = null, 
        DateTime? fromDate = null, 
        DateTime? toDate = null, 
        CancellationToken ct = default)
    {
        var query = _context.StockTransactions
            .Include(t => t.Store)
            .Include(t => t.Details)
                .ThenInclude(d => d.Item)
            .Include(t => t.Details)
                .ThenInclude(d => d.Unit)
            .AsNoTracking();

        if (type.HasValue) query = query.Where(t => t.TransactionType == type.Value);
        if (storeId.HasValue) query = query.Where(t => t.StoreId == storeId.Value);
        if (fromDate.HasValue) query = query.Where(t => t.TransactionDate >= fromDate.Value.Date);
        if (toDate.HasValue) query = query.Where(t => t.TransactionDate <= toDate.Value.Date.AddDays(1).AddTicks(-1));

        var transactions = await query
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.Id)
            .ToListAsync(ct);

        return transactions.Select(MapToDto).ToList();
    }

    public async Task<StockTransactionDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var t = await _context.StockTransactions
            .Include(t => t.Store)
            .Include(t => t.Details)
                .ThenInclude(d => d.Item)
            .Include(t => t.Details)
                .ThenInclude(d => d.Unit)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, ct);

        return t == null ? null : MapToDto(t);
    }

    public async Task<StockTransactionDto> CreateAsync(CreateStockTransactionDto dto, CancellationToken ct = default)
    {
        var valResult = await _createValidator.ValidateAsync(dto, ct);
        if (!valResult.IsValid)
        {
            throw new ValidationException(valResult.ToDictionary());
        }

        // Validate store exists & active
        var store = await _context.Stores.FirstOrDefaultAsync(s => s.Id == dto.StoreId, ct);
        if (store == null || !store.IsActive)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { nameof(dto.StoreId), new[] { "Selected store does not exist or is inactive." } }
            });
        }

        // Validate items & units
        var itemIds = dto.Details.Select(d => d.ItemId).Distinct().ToList();
        var items = await _context.Items.Include(i => i.Unit).Where(i => itemIds.Contains(i.Id)).ToDictionaryAsync(i => i.Id, ct);
        foreach (var d in dto.Details)
        {
            if (!items.TryGetValue(d.ItemId, out var item) || !item.IsActive)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "Details", new[] { $"Item ID {d.ItemId} is invalid or inactive." } }
                });
            }
        }

        using var transaction = await _context.BeginTransactionAsync(ct);
        try
        {
            // If Issue, validate sufficient stock per item
            if (dto.TransactionType == TransactionType.Issue)
            {
                // Group requested quantities by ItemId in case multiple rows have the same item
                var requestedByItem = dto.Details
                    .GroupBy(d => d.ItemId)
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

                foreach (var (itemId, requestedQty) in requestedByItem)
                {
                    var available = await _context.StockMovements
                        .Where(m => m.ItemId == itemId && m.StoreId == dto.StoreId)
                        .SumAsync(m => (decimal?)m.SignedQuantity, ct) ?? 0m;

                    if (requestedQty > available)
                    {
                        var item = items[itemId];
                        throw new InsufficientStockException(
                            itemId,
                            item.ItemCode,
                            item.Unit?.Name ?? "PCS",
                            requestedQty,
                            available);
                    }
                }
            }

            var transactionNo = await GenerateTransactionNoAsync(dto.TransactionType, ct);

            var stockTransaction = new StockTransaction
            {
                TransactionNo = transactionNo,
                TransactionDate = dto.TransactionDate,
                TransactionType = dto.TransactionType,
                StoreId = dto.StoreId,
                Remarks = dto.Remarks?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.StockTransactions.Add(stockTransaction);
            await _context.SaveChangesAsync(ct);

            // Add details and stock movements
            foreach (var d in dto.Details)
            {
                var detail = new StockTransactionDetail
                {
                    StockTransactionId = stockTransaction.Id,
                    ItemId = d.ItemId,
                    Quantity = d.Quantity,
                    UnitId = d.UnitId,
                    Remarks = d.Remarks?.Trim(),
                    DetailDate = d.DetailDate,
                    IsChecked = d.IsChecked
                };

                _context.StockTransactionDetails.Add(detail);
                await _context.SaveChangesAsync(ct); // persist to get detail.Id

                var movement = new StockMovement
                {
                    StockTransactionId = stockTransaction.Id,
                    StockTransactionDetailId = detail.Id,
                    ItemId = d.ItemId,
                    StoreId = dto.StoreId,
                    TransactionDate = dto.TransactionDate,
                    TransactionType = dto.TransactionType,
                    Quantity = d.Quantity,
                    SignedQuantity = StockMovement.CalculateSignedQuantity(dto.TransactionType, d.Quantity),
                    CreatedAt = DateTime.UtcNow
                };

                _context.StockMovements.Add(movement);
            }

            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return (await GetByIdAsync(stockTransaction.Id, ct))!;
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<StockTransactionDto> UpdateAsync(int id, UpdateStockTransactionDto dto, CancellationToken ct = default)
    {
        if (id != dto.TransactionId)
        {
            throw new ValidationException("Route ID does not match transaction payload ID.");
        }

        var valResult = await _updateValidator.ValidateAsync(dto, ct);
        if (!valResult.IsValid)
        {
            throw new ValidationException(valResult.ToDictionary());
        }

        using var dbTransaction = await _context.BeginTransactionAsync(ct);
        try
        {
            var existingTx = await _context.StockTransactions
                .Include(t => t.Details)
                .FirstOrDefaultAsync(t => t.Id == id, ct);

            if (existingTx == null)
            {
                throw new EntityNotFoundException(nameof(StockTransaction), id);
            }

            var store = await _context.Stores.FirstOrDefaultAsync(s => s.Id == dto.StoreId, ct);
            if (store == null || !store.IsActive)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { nameof(dto.StoreId), new[] { "Selected store does not exist or is inactive." } }
                });
            }

            // Step 1: Temporarily remove all existing StockMovements for this transaction
            var existingMovements = await _context.StockMovements
                .Where(m => m.StockTransactionId == id)
                .ToListAsync(ct);

            _context.StockMovements.RemoveRange(existingMovements);
            await _context.SaveChangesAsync(ct);

            // Step 2: Validate available stock for the new state if TransactionType is Issue
            var itemIds = dto.Details.Select(d => d.ItemId).Distinct().ToList();
            var items = await _context.Items.Include(i => i.Unit).Where(i => itemIds.Contains(i.Id)).ToDictionaryAsync(i => i.Id, ct);

            if (dto.TransactionType == TransactionType.Issue)
            {
                var requestedByItem = dto.Details
                    .GroupBy(d => d.ItemId)
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

                foreach (var (itemId, requestedQty) in requestedByItem)
                {
                    var available = await _context.StockMovements
                        .Where(m => m.ItemId == itemId && m.StoreId == dto.StoreId)
                        .SumAsync(m => (decimal?)m.SignedQuantity, ct) ?? 0m;

                    if (requestedQty > available)
                    {
                        var item = items[itemId];
                        throw new InsufficientStockException(
                            itemId,
                            item.ItemCode,
                            item.Unit?.Name ?? "PCS",
                            requestedQty,
                            available);
                    }
                }
            }

            // Step 3: Update header
            existingTx.TransactionDate = dto.TransactionDate;
            existingTx.TransactionType = dto.TransactionType;
            existingTx.StoreId = dto.StoreId;
            existingTx.Remarks = dto.Remarks?.Trim();
            existingTx.UpdatedAt = DateTime.UtcNow;

            // Step 4: Identify Delta: New, Modified, Deleted details
            var incomingDetailIds = dto.Details.Where(d => d.Id > 0).Select(d => d.Id).ToHashSet();
            var existingDetailMap = existingTx.Details.ToDictionary(d => d.Id);

            // Deleted details: either explicitly requested in DeletedDetailIds or existing details missing from incoming Details
            var toDelete = existingTx.Details
                .Where(d => !incomingDetailIds.Contains(d.Id) || (dto.DeletedDetailIds != null && dto.DeletedDetailIds.Contains(d.Id)))
                .ToList();

            foreach (var del in toDelete)
            {
                _context.StockTransactionDetails.Remove(del);
                existingDetailMap.Remove(del.Id);
            }

            // Save deletions before applying modifications/insertions
            await _context.SaveChangesAsync(ct);

            // Modified and New details
            foreach (var detailDto in dto.Details)
            {
                StockTransactionDetail detailEntity;

                if (detailDto.Id > 0 && existingDetailMap.TryGetValue(detailDto.Id, out var existingDetail))
                {
                    // Modified Detail
                    detailEntity = existingDetail;
                    detailEntity.ItemId = detailDto.ItemId;
                    detailEntity.Quantity = detailDto.Quantity;
                    detailEntity.UnitId = detailDto.UnitId;
                    detailEntity.Remarks = detailDto.Remarks?.Trim();
                    detailEntity.DetailDate = detailDto.DetailDate;
                    detailEntity.IsChecked = detailDto.IsChecked;
                }
                else
                {
                    // New Detail (Id == 0 or newly added)
                    detailEntity = new StockTransactionDetail
                    {
                        StockTransactionId = existingTx.Id,
                        ItemId = detailDto.ItemId,
                        Quantity = detailDto.Quantity,
                        UnitId = detailDto.UnitId,
                        Remarks = detailDto.Remarks?.Trim(),
                        DetailDate = detailDto.DetailDate,
                        IsChecked = detailDto.IsChecked
                    };
                    _context.StockTransactionDetails.Add(detailEntity);
                }

                await _context.SaveChangesAsync(ct); // persist to ensure detailEntity.Id is populated

                // Re-create Stock Movement
                var movement = new StockMovement
                {
                    StockTransactionId = existingTx.Id,
                    StockTransactionDetailId = detailEntity.Id,
                    ItemId = detailDto.ItemId,
                    StoreId = dto.StoreId,
                    TransactionDate = dto.TransactionDate,
                    TransactionType = dto.TransactionType,
                    Quantity = detailDto.Quantity,
                    SignedQuantity = StockMovement.CalculateSignedQuantity(dto.TransactionType, detailDto.Quantity),
                    CreatedAt = DateTime.UtcNow
                };

                _context.StockMovements.Add(movement);
            }

            await _context.SaveChangesAsync(ct);
            await dbTransaction.CommitAsync(ct);

            return (await GetByIdAsync(existingTx.Id, ct))!;
        }
        catch
        {
            await dbTransaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        using var transaction = await _context.BeginTransactionAsync(ct);
        try
        {
            var tx = await _context.StockTransactions
                .Include(t => t.Details)
                .FirstOrDefaultAsync(t => t.Id == id, ct);

            if (tx == null)
            {
                throw new EntityNotFoundException(nameof(StockTransaction), id);
            }

            // Remove associated movements
            var movements = await _context.StockMovements.Where(m => m.StockTransactionId == id).ToListAsync(ct);
            _context.StockMovements.RemoveRange(movements);

            // Remove details
            _context.StockTransactionDetails.RemoveRange(tx.Details);

            // Remove header
            _context.StockTransactions.Remove(tx);

            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    private static StockTransactionDto MapToDto(StockTransaction t)
    {
        var details = t.Details.Select(d => new StockTransactionDetailDto(
            d.Id,
            d.StockTransactionId,
            d.ItemId,
            d.Item?.ItemCode ?? string.Empty,
            d.Item?.ItemName ?? string.Empty,
            d.Quantity,
            d.UnitId,
            d.Unit?.Name ?? string.Empty,
            d.Remarks,
            d.DetailDate,
            d.IsChecked
        )).ToList();

        return new StockTransactionDto(
            t.Id,
            t.TransactionNo,
            t.TransactionDate,
            t.TransactionType,
            t.StoreId,
            t.Store?.Name ?? string.Empty,
            t.Remarks,
            t.CreatedAt,
            t.UpdatedAt,
            details
        );
    }
}
