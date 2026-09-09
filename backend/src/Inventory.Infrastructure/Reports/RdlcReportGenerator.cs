using System.Data;
using Inventory.Application.Reports.DTOs;
using Inventory.Application.Reports.Services;
using Microsoft.Reporting.NETCore;

namespace Inventory.Infrastructure.Reports;

public class RdlcReportGenerator : IRdlcReportGenerator
{
    private readonly string _reportsDirectory;

    public RdlcReportGenerator(string reportsDirectory)
    {
        _reportsDirectory = reportsDirectory;
    }

    public byte[] GenerateStockMovementReport(StockMovementReportDto report, string format)
    {
        var rdlcPath = Path.Combine(_reportsDirectory, "StockMovement.rdlc");
        if (!File.Exists(rdlcPath))
        {
            throw new FileNotFoundException($"RDLC report definition not found at: {rdlcPath}");
        }

        using var localReport = new LocalReport();        
        using var rdlcStream = File.OpenRead(rdlcPath);
        localReport.LoadReportDefinition(rdlcStream);

        var dataTable = new DataTable("StockMovementDataSet");
        dataTable.Columns.Add("ItemId", typeof(int));
        dataTable.Columns.Add("ItemCode", typeof(string));
        dataTable.Columns.Add("ItemName", typeof(string));
        dataTable.Columns.Add("StoreId", typeof(int));
        dataTable.Columns.Add("StoreName", typeof(string));
        dataTable.Columns.Add("UnitName", typeof(string));
        dataTable.Columns.Add("Opening", typeof(decimal));
        dataTable.Columns.Add("Receive", typeof(decimal));
        dataTable.Columns.Add("Issue", typeof(decimal));
        dataTable.Columns.Add("Return", typeof(decimal));
        dataTable.Columns.Add("Closing", typeof(decimal));

        foreach (var item in report.Items)
        {
            dataTable.Rows.Add(
                item.ItemId,
                item.ItemCode,
                item.ItemName,
                item.StoreId,
                item.StoreName,
                item.UnitName,
                item.Opening,
                item.Receive,
                item.Issue,
                item.Return,
                item.Closing
            );
        }

        localReport.DataSources.Add(new ReportDataSource("StockMovementDataSet", dataTable));
        localReport.SetParameters(new[]
        {
            new ReportParameter("FromDate", report.FromDate.ToString("yyyy-MM-dd")),
            new ReportParameter("ToDate", report.ToDate.ToString("yyyy-MM-dd")),
            new ReportParameter("StoreName", report.StoreName ?? "All Stores"),
            new ReportParameter("ItemName", report.ItemName ?? "All Items")
        });

        var renderFormat = format.Equals("Excel", StringComparison.OrdinalIgnoreCase) ? "EXCELOPENXML" : "PDF";
        return localReport.Render(renderFormat);
    }

    public byte[] GenerateTransactionDetailReport(TransactionDetailReportDto report, string format)
    {
        var rdlcPath = Path.Combine(_reportsDirectory, "TransactionDetails.rdlc");
        if (!File.Exists(rdlcPath))
        {
            throw new FileNotFoundException($"RDLC report definition not found at: {rdlcPath}");
        }

        using var localReport = new LocalReport();
        using var rdlcStream = File.OpenRead(rdlcPath);
        localReport.LoadReportDefinition(rdlcStream);

        var dataTable = new DataTable("TransactionDetailDataSet");
        dataTable.Columns.Add("TransactionDate", typeof(DateTime));
        dataTable.Columns.Add("TransactionNo", typeof(string));
        dataTable.Columns.Add("TransactionType", typeof(string));
        dataTable.Columns.Add("StoreName", typeof(string));
        dataTable.Columns.Add("ItemCode", typeof(string));
        dataTable.Columns.Add("ItemName", typeof(string));
        dataTable.Columns.Add("UnitName", typeof(string));
        dataTable.Columns.Add("OpeningQuantity", typeof(decimal));
        dataTable.Columns.Add("ReceiveQuantity", typeof(decimal));
        dataTable.Columns.Add("IssueQuantity", typeof(decimal));
        dataTable.Columns.Add("ReturnQuantity", typeof(decimal));
        dataTable.Columns.Add("ClosingQuantity", typeof(decimal));

        foreach (var row in report.Rows)
        {
            dataTable.Rows.Add(
                row.TransactionDate,
                row.TransactionNo,
                row.TransactionType.ToString(),
                row.StoreName,
                row.ItemCode,
                row.ItemName,
                row.UnitName,
                row.OpeningQuantity,
                row.ReceiveQuantity,
                row.IssueQuantity,
                row.ReturnQuantity,
                row.ClosingQuantity
            );
        }

        localReport.DataSources.Add(new ReportDataSource("TransactionDetailDataSet", dataTable));
        localReport.SetParameters(new[]
        {
            new ReportParameter("FromDate", report.FromDate.ToString("yyyy-MM-dd")),
            new ReportParameter("ToDate", report.ToDate.ToString("yyyy-MM-dd")),
            new ReportParameter("StoreName", report.StoreName ?? "All Stores"),
            new ReportParameter("ItemName", report.ItemName ?? "All Items")
        });

        var renderFormat = format.Equals("Excel", StringComparison.OrdinalIgnoreCase) ? "EXCELOPENXML" : "PDF";
        return localReport.Render(renderFormat);
    }
}
