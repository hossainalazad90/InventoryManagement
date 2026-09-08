namespace Inventory.Application.Stock.DTOs;

public record StockBalanceDto(
    int ItemId,
    string ItemCode,
    string ItemName,
    int StoreId,
    string StoreName,
    int UnitId,
    string UnitName,
    decimal AvailableQuantity,
    decimal ReorderLevel,
    bool IsBelowReorderLevel
);
