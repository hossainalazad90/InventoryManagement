using Inventory.Application.Reports.DTOs;
using Inventory.Infrastructure.Reports;

namespace Inventory.IntegrationTests;

public class ReportRenderTests
{
    [Fact]
    public void Renders_stock_movement_report()
    {
        var reports = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "reports"));
        var generator = new RdlcReportGenerator(reports);
        var report = new StockMovementReportDto(
            DateTime.Today.AddDays(-1), DateTime.Today, null, null, null, null,
            new[] { new StockMovementReportItemDto(1, "ITEM-1", "Test item", 1, "Main", "Each", 0, 2, 0, 0, 2) });

        var result = generator.GenerateStockMovementReport(report, "Pdf");

        Assert.NotEmpty(result);
    }

    [Fact]
    public void Renders_transaction_detail_report()
    {
        var reports = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "reports"));
        var generator = new RdlcReportGenerator(reports);
        var report = new TransactionDetailReportDto(
            DateTime.Today.AddDays(-1), DateTime.Today, null, null, null, null,
            new[] { new TransactionDetailReportRowDto(DateTime.Today, "REC-001", Inventory.Domain.Enums.TransactionType.Receive, "Main", "ITEM-1", "Test item", "Each", 0, 2, 0, 0, 2) });

        var result = generator.GenerateTransactionDetailReport(report, "Excel");

        Assert.NotEmpty(result);
    }
}
