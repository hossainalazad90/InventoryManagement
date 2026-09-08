namespace Inventory.Domain.Exceptions;

public class InsufficientStockException : DomainException
{
    public override string Code => "INSUFFICIENT_STOCK";

    public int ItemId { get; }
    public string ItemCode { get; }
    public string UnitName { get; }
    public decimal RequestedQuantity { get; }
    public decimal AvailableQuantity { get; }

    public InsufficientStockException(int itemId, string itemCode, string unitName, decimal requestedQuantity, decimal availableQuantity)
        : base($"Cannot issue {requestedQuantity:G29} {unitName} of {itemCode}. Available stock is {availableQuantity:G29} {unitName}.")
    {
        ItemId = itemId;
        ItemCode = itemCode;
        UnitName = unitName;
        RequestedQuantity = requestedQuantity;
        AvailableQuantity = availableQuantity;
    }
}
