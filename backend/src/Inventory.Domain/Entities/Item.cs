namespace Inventory.Domain.Entities;

public class Item
{
    public int Id { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public int UnitId { get; set; }
    public Unit? Unit { get; set; }
    public decimal ReorderLevel { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<StockTransactionDetail> TransactionDetails { get; set; } = new List<StockTransactionDetail>();
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}
