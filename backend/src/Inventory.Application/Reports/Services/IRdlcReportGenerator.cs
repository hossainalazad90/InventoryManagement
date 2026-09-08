using Inventory.Application.Reports.DTOs;

namespace Inventory.Application.Reports.Services;

public interface IRdlcReportGenerator
{
    byte[] GenerateStockMovementReport(StockMovementReportDto report, string format);
    byte[] GenerateTransactionDetailReport(TransactionDetailReportDto report, string format);
}
