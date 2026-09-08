namespace Inventory.Application.Items.DTOs;

public record ItemDto(
    int Id,
    string ItemCode,
    string ItemName,
    int CategoryId,
    string CategoryName,
    int UnitId,
    string UnitName,
    decimal ReorderLevel,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateItemDto(
    string ItemCode,
    string ItemName,
    int CategoryId,
    int UnitId,
    decimal ReorderLevel,
    bool IsActive = true
);

public record UpdateItemDto(
    string ItemCode,
    string ItemName,
    int CategoryId,
    int UnitId,
    decimal ReorderLevel,
    bool IsActive
);
