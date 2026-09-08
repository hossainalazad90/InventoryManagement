namespace Inventory.Application.MasterData.DTOs;

public record CategoryDto(int Id, string Name, bool IsActive);
public record UnitDto(int Id, string Name, bool IsActive);
public record StoreDto(int Id, string Code, string Name, bool IsActive);
