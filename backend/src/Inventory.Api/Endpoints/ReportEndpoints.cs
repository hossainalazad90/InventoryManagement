using Inventory.Application.Common;
using Inventory.Application.Reports.DTOs;
using Inventory.Application.Reports.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Api.Endpoints;

public static class ReportEndpoints
{
    public static RouteGroupBuilder MapReportEndpoints(this RouteGroupBuilder group)
    {

        group.MapGet("/stock-movement", async (
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int? storeId,
            [FromQuery] int? itemId,
            IReportQueryService service,
            CancellationToken ct) =>
        {
            var start = fromDate ?? DateTime.UtcNow.AddMonths(-1);
            var end = toDate ?? DateTime.UtcNow;

            var report = await service.GetStockMovementReportAsync(start, end, storeId, itemId, ct);
            return Results.Ok(ApiResponse<StockMovementReportDto>.Ok(report));
        })
        .WithName("GetStockMovementReport")
        .WithSummary("Get Stock Movement report data (Opening, Receive, Issue, Return, Closing)");

        group.MapGet("/stock-movement/export", async (
            [FromQuery] string? format,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int? storeId,
            [FromQuery] int? itemId,
            IReportQueryService queryService,
            IRdlcReportGenerator rdlcService,
            CancellationToken ct) =>
        {
            var start = fromDate ?? DateTime.UtcNow.AddMonths(-1);
            var end = toDate ?? DateTime.UtcNow;
            var fmt = string.Equals(format, "excel", StringComparison.OrdinalIgnoreCase) ? "Excel" : "Pdf";

            var report = await queryService.GetStockMovementReportAsync(start, end, storeId, itemId, ct);
            var bytes = rdlcService.GenerateStockMovementReport(report, fmt);

            var contentType = fmt == "Excel"
                ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                : "application/pdf";
            var fileName = $"StockMovement_{start:yyyyMMdd}_{end:yyyyMMdd}.{(fmt == "Excel" ? "xlsx" : "pdf")}";

            return Results.File(bytes, contentType, fileName);
        })
        .WithName("ExportStockMovementReport")
        .WithSummary("Export Stock Movement report to PDF or Excel via RDLC");

        group.MapGet("/transaction-details", async (
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int? storeId,
            [FromQuery] int? itemId,
            IReportQueryService service,
            CancellationToken ct) =>
        {
            var start = fromDate ?? DateTime.UtcNow.AddMonths(-1);
            var end = toDate ?? DateTime.UtcNow;

            var report = await service.GetTransactionDetailReportAsync(start, end, storeId, itemId, ct);
            return Results.Ok(ApiResponse<TransactionDetailReportDto>.Ok(report));
        })
        .WithName("GetTransactionDetailsReport")
        .WithSummary("Get Transaction Details report data with running stock levels");

        group.MapGet("/transaction-details/export", async (
            [FromQuery] string? format,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int? storeId,
            [FromQuery] int? itemId,
            IReportQueryService queryService,
            IRdlcReportGenerator rdlcService,
            CancellationToken ct) =>
        {
            var start = fromDate ?? DateTime.UtcNow.AddMonths(-1);
            var end = toDate ?? DateTime.UtcNow;
            var fmt = string.Equals(format, "excel", StringComparison.OrdinalIgnoreCase) ? "Excel" : "Pdf";

            var report = await queryService.GetTransactionDetailReportAsync(start, end, storeId, itemId, ct);
            var bytes = rdlcService.GenerateTransactionDetailReport(report, fmt);

            var contentType = fmt == "Excel"
                ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                : "application/pdf";
            var fileName = $"TransactionDetails_{start:yyyyMMdd}_{end:yyyyMMdd}.{(fmt == "Excel" ? "xlsx" : "pdf")}";

            return Results.File(bytes, contentType, fileName);
        })
        .WithName("ExportTransactionDetailsReport")
        .WithSummary("Export Transaction Details report to PDF or Excel via RDLC");

        return group;
    }
}
