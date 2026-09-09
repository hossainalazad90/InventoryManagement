using Inventory.Application.Common;
using Inventory.Application.Reports.DTOs;
using Inventory.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Reports.Services;

public class ReportQueryService : IReportQueryService
{
    private readonly IApplicationDbContext _context;

    public ReportQueryService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StockMovementReportDto> GetStockMovementReportAsync(
        DateTime fromDate,
        DateTime toDate,
        int? storeId = null,
        int? itemId = null,
        CancellationToken ct = default)
    {
        var start = fromDate.Date;
        var end = toDate.Date.AddDays(1).AddTicks(-1);
        
        var itemsQuery = _context.Items.Include(i => i.Unit).AsNoTracking();
        if (itemId.HasValue) itemsQuery = itemsQuery.Where(i => i.Id == itemId.Value);
        var itemsList = await itemsQuery.ToListAsync(ct);

        var storesQuery = _context.Stores.AsNoTracking();
        if (storeId.HasValue) storesQuery = storesQuery.Where(s => s.Id == storeId.Value);
        var storesList = await storesQuery.ToListAsync(ct);

        var reportItems = new List<StockMovementReportItemDto>();

        foreach (var item in itemsList)
        {
            foreach (var store in storesList)
            {
                // Opening = sum of signed quantity before start date
                var opening = await _context.StockMovements
                    .Where(m => m.ItemId == item.Id && m.StoreId == store.Id && m.TransactionDate < start)
                    .SumAsync(m => (decimal?)m.SignedQuantity, ct) ?? 0m;

                // Movements within the period
                var periodMovements = await _context.StockMovements
                    .Where(m => m.ItemId == item.Id && m.StoreId == store.Id && m.TransactionDate >= start && m.TransactionDate <= end)
                    .ToListAsync(ct);

                var receive = periodMovements.Where(m => m.TransactionType == TransactionType.Receive).Sum(m => m.Quantity);
                var issue = periodMovements.Where(m => m.TransactionType == TransactionType.Issue).Sum(m => m.Quantity);
                var ret = periodMovements.Where(m => m.TransactionType == TransactionType.Return).Sum(m => m.Quantity);

                // Closing = Opening + Receive - Issue + Return
                var closing = opening + receive - issue + ret;

                // Only include if there is any movement or non-zero stock
                if (opening != 0 || receive != 0 || issue != 0 || ret != 0 || closing != 0)
                {
                    reportItems.Add(new StockMovementReportItemDto(
                        item.Id,
                        item.ItemCode,
                        item.ItemName,
                        store.Id,
                        store.Name,
                        item.Unit?.Name ?? string.Empty,
                        opening,
                        receive,
                        issue,
                        ret,
                        closing
                    ));
                }
            }
        }

        string? storeName = storeId.HasValue ? storesList.FirstOrDefault(s => s.Id == storeId.Value)?.Name : "All Stores";
        string? itemName = itemId.HasValue ? itemsList.FirstOrDefault(i => i.Id == itemId.Value)?.ItemName : "All Items";

        return new StockMovementReportDto(
            fromDate,
            toDate,
            storeId,
            storeName,
            itemId,
            itemName,
            reportItems.OrderBy(x => x.ItemCode).ThenBy(x => x.StoreName).ToList()
        );
    }

    public async Task<TransactionDetailReportDto> GetTransactionDetailReportAsync(
        DateTime fromDate,
        DateTime toDate,
        int? storeId = null,
        int? itemId = null,
        CancellationToken ct = default)
    {
        var start = fromDate.Date;
        var end = toDate.Date.AddDays(1).AddTicks(-1);

        var query = _context.StockMovements
            .Include(m => m.StockTransaction)
            .Include(m => m.Store)
            .Include(m => m.Item)
                .ThenInclude(i => i!.Unit)
            .AsNoTracking()
            .Where(m => m.TransactionDate >= start && m.TransactionDate <= end);

        if (storeId.HasValue) query = query.Where(m => m.StoreId == storeId.Value);
        if (itemId.HasValue) query = query.Where(m => m.ItemId == itemId.Value);

        var movements = await query
            .OrderBy(m => m.TransactionDate)
            .ThenBy(m => m.Id)
            .ToListAsync(ct);
        
        var runningBalances = new Dictionary<(int ItemId, int StoreId), decimal>();
        
        var itemStorePairs = movements.Select(m => (m.ItemId, m.StoreId)).Distinct().ToList();
        foreach (var pair in itemStorePairs)
        {
            var initialOpening = await _context.StockMovements
                .Where(m => m.ItemId == pair.ItemId && m.StoreId == pair.StoreId && m.TransactionDate < start)
                .SumAsync(m => (decimal?)m.SignedQuantity, ct) ?? 0m;

            runningBalances[pair] = initialOpening;
        }

        var rows = new List<TransactionDetailReportRowDto>();
        foreach (var m in movements)
        {
            var key = (m.ItemId, m.StoreId);
            var opening = runningBalances.GetValueOrDefault(key, 0m);

            decimal receiveQty = m.TransactionType == TransactionType.Receive ? m.Quantity : 0m;
            decimal issueQty = m.TransactionType == TransactionType.Issue ? m.Quantity : 0m;
            decimal returnQty = m.TransactionType == TransactionType.Return ? m.Quantity : 0m;

            decimal closing = opening + m.SignedQuantity;
            runningBalances[key] = closing;

            rows.Add(new TransactionDetailReportRowDto(
                m.TransactionDate,
                m.StockTransaction?.TransactionNo ?? "N/A",
                m.TransactionType,
                m.Store?.Name ?? "N/A",
                m.Item?.ItemCode ?? "N/A",
                m.Item?.ItemName ?? "N/A",
                m.Item?.Unit?.Name ?? "PCS",
                opening,
                receiveQty,
                issueQty,
                returnQty,
                closing
            ));
        }

        string? storeName = storeId.HasValue ? (await _context.Stores.FindAsync(new object[] { storeId.Value }, ct))?.Name : "All Stores";
        string? itemName = itemId.HasValue ? (await _context.Items.FindAsync(new object[] { itemId.Value }, ct))?.ItemName : "All Items";

        return new TransactionDetailReportDto(
            fromDate,
            toDate,
            storeId,
            storeName,
            itemId,
            itemName,
            rows
        );
    }
}
