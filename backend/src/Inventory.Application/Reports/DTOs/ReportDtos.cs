using Inventory.Domain.Enums;

namespace Inventory.Application.Reports.DTOs;

public record StockMovementReportItemDto(
    int ItemId,
    string ItemCode,
    string ItemName,
    int StoreId,
    string StoreName,
    string UnitName,
    decimal Opening,
    decimal Receive,
    decimal Issue,
    decimal Return,
    decimal Closing
);

public record StockMovementReportDto(
    DateTime FromDate,
    DateTime ToDate,
    int? StoreId,
    string? StoreName,
    int? ItemId,
    string? ItemName,
    IReadOnlyList<StockMovementReportItemDto> Items
);

public record TransactionDetailReportRowDto(
    DateTime TransactionDate,
    string TransactionNo,
    TransactionType TransactionType,
    string StoreName,
    string ItemCode,
    string ItemName,
    string UnitName,
    decimal OpeningQuantity,
    decimal ReceiveQuantity,
    decimal IssueQuantity,
    decimal ReturnQuantity,
    decimal ClosingQuantity
);

public record TransactionDetailReportDto(
    DateTime FromDate,
    DateTime ToDate,
    int? StoreId,
    string? StoreName,
    int? ItemId,
    string? ItemName,
    IReadOnlyList<TransactionDetailReportRowDto> Rows
);
