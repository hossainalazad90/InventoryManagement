using Inventory.Domain.Enums;

namespace Inventory.Application.StockTransactions.DTOs;

public record StockTransactionDetailDto(
    int Id,
    int StockTransactionId,
    int ItemId,
    string ItemCode,
    string ItemName,
    decimal Quantity,
    int UnitId,
    string UnitName,
    string? Remarks,
    DateTime? DetailDate,
    bool IsChecked
);

public record StockTransactionDto(
    int Id,
    string TransactionNo,
    DateTime TransactionDate,
    TransactionType TransactionType,
    int StoreId,
    string StoreName,
    string? Remarks,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<StockTransactionDetailDto> Details
);

public record CreateStockTransactionDetailDto(
    int ItemId,
    decimal Quantity,
    int UnitId,
    string? Remarks,
    DateTime? DetailDate,
    bool IsChecked
);

public record CreateStockTransactionDto(
    DateTime TransactionDate,
    TransactionType TransactionType,
    int StoreId,
    string? Remarks,
    List<CreateStockTransactionDetailDto> Details
);

public record UpdateStockTransactionDetailDto(
    int Id, // 0 if new, existing ID if modified
    int ItemId,
    decimal Quantity,
    int UnitId,
    string? Remarks,
    DateTime? DetailDate,
    bool IsChecked
);

public record UpdateStockTransactionDto(
    int TransactionId,
    DateTime TransactionDate,
    TransactionType TransactionType,
    int StoreId,
    string? Remarks,
    List<UpdateStockTransactionDetailDto> Details,
    List<int>? DeletedDetailIds = null
);
