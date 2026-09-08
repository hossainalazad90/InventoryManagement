using Inventory.Domain.Enums;

namespace Inventory.Domain.Entities;

public class StockTransaction
{
    public int Id { get; set; }
    public string TransactionNo { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public TransactionType TransactionType { get; set; }
    public int StoreId { get; set; }
    public Store? Store { get; set; }
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public byte[]? RowVersion { get; set; }

    public ICollection<StockTransactionDetail> Details { get; set; } = new List<StockTransactionDetail>();
    public ICollection<StockMovement> Movements { get; set; } = new List<StockMovement>();
}
