using Inventory.Application.Reports.DTOs;

namespace Inventory.Application.Reports.Services;

public interface IReportQueryService
{
    Task<StockMovementReportDto> GetStockMovementReportAsync(
        DateTime fromDate, 
        DateTime toDate, 
        int? storeId = null, 
        int? itemId = null, 
        CancellationToken ct = default);

    Task<TransactionDetailReportDto> GetTransactionDetailReportAsync(
        DateTime fromDate, 
        DateTime toDate, 
        int? storeId = null, 
        int? itemId = null, 
        CancellationToken ct = default);
}
