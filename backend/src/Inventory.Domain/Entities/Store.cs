namespace Inventory.Domain.Entities;

public class Store
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<StockTransaction> Transactions { get; set; } = new List<StockTransaction>();
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}
