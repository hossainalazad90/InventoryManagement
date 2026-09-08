using Inventory.Domain.Enums;

namespace Inventory.Domain.Entities;

public class StockMovement
{
    public int Id { get; set; }
    public int StockTransactionId { get; set; }
    public StockTransaction? StockTransaction { get; set; }
    public int StockTransactionDetailId { get; set; }
    public StockTransactionDetail? StockTransactionDetail { get; set; }
    public int ItemId { get; set; }
    public Item? Item { get; set; }
    public int StoreId { get; set; }
    public Store? Store { get; set; }
    public DateTime TransactionDate { get; set; }
    public TransactionType TransactionType { get; set; }
    public decimal Quantity { get; set; }
    public decimal SignedQuantity { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public static decimal CalculateSignedQuantity(TransactionType type, decimal quantity)
    {
        return type switch
        {
            TransactionType.Receive => quantity,
            TransactionType.Issue => -quantity,
            TransactionType.Return => quantity,
            _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unsupported transaction type: {type}")
        };
    }
}
