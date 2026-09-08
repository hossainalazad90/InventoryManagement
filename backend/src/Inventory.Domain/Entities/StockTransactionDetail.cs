namespace Inventory.Domain.Entities;

public class StockTransactionDetail
{
    public int Id { get; set; }
    public int StockTransactionId { get; set; }
    public StockTransaction? StockTransaction { get; set; }
    public int ItemId { get; set; }
    public Item? Item { get; set; }
    public decimal Quantity { get; set; }
    public int UnitId { get; set; }
    public Unit? Unit { get; set; }
    public string? Remarks { get; set; }
    public DateTime? DetailDate { get; set; }
    public bool IsChecked { get; set; }

    public ICollection<StockMovement> Movements { get; set; } = new List<StockMovement>();
}
