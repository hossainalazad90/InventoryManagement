using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task InitializeAsync(InventoryDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (await context.Categories.AnyAsync())
        {
            return; // DB has been seeded
        }

        // 1. Seed Categories
        var categories = new[]
        {
            new Category { Name = "Raw Materials", IsActive = true },
            new Category { Name = "Finished Goods", IsActive = true },
            new Category { Name = "Packaging", IsActive = true },
            new Category { Name = "Spare Parts", IsActive = true },
            new Category { Name = "Office Supplies", IsActive = true }
        };
        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();

        // 2. Seed Units
        var units = new[]
        {
            new Unit { Name = "PCS", IsActive = true },
            new Unit { Name = "BOX", IsActive = true },
            new Unit { Name = "KG", IsActive = true },
            new Unit { Name = "LTR", IsActive = true },
            new Unit { Name = "SET", IsActive = true }
        };
        await context.Units.AddRangeAsync(units);
        await context.SaveChangesAsync();

        // 3. Seed Stores
        var stores = new[]
        {
            new Store { Code = "MAIN-01", Name = "Main Warehouse", IsActive = true },
            new Store { Code = "RAW-01", Name = "Raw Material Store", IsActive = true },
            new Store { Code = "PROD-01", Name = "Production Floor Store", IsActive = true }
        };
        await context.Stores.AddRangeAsync(stores);
        await context.SaveChangesAsync();

        // 4. Seed Items
        var items = new[]
        {
            new Item
            {
                ItemCode = "ITEM-001",
                ItemName = "Industrial Steel Bearing 6205",
                CategoryId = categories[3].Id, // Spare Parts
                UnitId = units[0].Id, // PCS
                ReorderLevel = 20,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new Item
            {
                ItemCode = "ITEM-002",
                ItemName = "Hydraulic Oil ISO 46 (20L)",
                CategoryId = categories[0].Id, // Raw Materials
                UnitId = units[3].Id, // LTR
                ReorderLevel = 50,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new Item
            {
                ItemCode = "ITEM-003",
                ItemName = "Corrugated Cardboard Box 12x12x12",
                CategoryId = categories[2].Id, // Packaging
                UnitId = units[1].Id, // BOX
                ReorderLevel = 100,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new Item
            {
                ItemCode = "ITEM-004",
                ItemName = "Thermal Eco Sensor Assembly",
                CategoryId = categories[1].Id, // Finished Goods
                UnitId = units[4].Id, // SET
                ReorderLevel = 15,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new Item
            {
                ItemCode = "ITEM-005",
                ItemName = "Safety Protective Gloves Nitrile",
                CategoryId = categories[4].Id, // Office Supplies
                UnitId = units[4].Id, // SET
                ReorderLevel = 30,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            }
        };
        await context.Items.AddRangeAsync(items);
        await context.SaveChangesAsync();

        // 5. Seed Initial Stock Receive Transactions
        var date1 = DateTime.UtcNow.AddDays(-20);
        var tx1 = new StockTransaction
        {
            TransactionNo = "RCV-20260818-0001",
            TransactionDate = date1,
            TransactionType = TransactionType.Receive,
            StoreId = stores[0].Id,
            Remarks = "Initial stock intake from vendor",
            CreatedAt = date1
        };
        context.StockTransactions.Add(tx1);
        await context.SaveChangesAsync();

        var detail1 = new StockTransactionDetail
        {
            StockTransactionId = tx1.Id,
            ItemId = items[0].Id, // ITEM-001
            Quantity = 100,
            UnitId = items[0].UnitId,
            Remarks = "Batch A1",
            DetailDate = date1,
            IsChecked = true
        };
        var detail2 = new StockTransactionDetail
        {
            StockTransactionId = tx1.Id,
            ItemId = items[1].Id, // ITEM-002
            Quantity = 200,
            UnitId = items[1].UnitId,
            Remarks = "Sealed drums",
            DetailDate = date1,
            IsChecked = true
        };
        var detail3 = new StockTransactionDetail
        {
            StockTransactionId = tx1.Id,
            ItemId = items[2].Id, // ITEM-003
            Quantity = 500,
            UnitId = items[2].UnitId,
            Remarks = "Pallet 1",
            DetailDate = date1,
            IsChecked = false
        };

        context.StockTransactionDetails.AddRange(detail1, detail2, detail3);
        await context.SaveChangesAsync();

        context.StockMovements.AddRange(
            new StockMovement
            {
                StockTransactionId = tx1.Id,
                StockTransactionDetailId = detail1.Id,
                ItemId = detail1.ItemId,
                StoreId = tx1.StoreId,
                TransactionDate = date1,
                TransactionType = TransactionType.Receive,
                Quantity = detail1.Quantity,
                SignedQuantity = detail1.Quantity,
                CreatedAt = date1
            },
            new StockMovement
            {
                StockTransactionId = tx1.Id,
                StockTransactionDetailId = detail2.Id,
                ItemId = detail2.ItemId,
                StoreId = tx1.StoreId,
                TransactionDate = date1,
                TransactionType = TransactionType.Receive,
                Quantity = detail2.Quantity,
                SignedQuantity = detail2.Quantity,
                CreatedAt = date1
            },
            new StockMovement
            {
                StockTransactionId = tx1.Id,
                StockTransactionDetailId = detail3.Id,
                ItemId = detail3.ItemId,
                StoreId = tx1.StoreId,
                TransactionDate = date1,
                TransactionType = TransactionType.Receive,
                Quantity = detail3.Quantity,
                SignedQuantity = detail3.Quantity,
                CreatedAt = date1
            }
        );
        await context.SaveChangesAsync();

        // 6. Seed Sample Issue Transaction
        var date2 = DateTime.UtcNow.AddDays(-10);
        var tx2 = new StockTransaction
        {
            TransactionNo = "ISS-20260828-0001",
            TransactionDate = date2,
            TransactionType = TransactionType.Issue,
            StoreId = stores[0].Id,
            Remarks = "Maintenance workshop requisition",
            CreatedAt = date2
        };
        context.StockTransactions.Add(tx2);
        await context.SaveChangesAsync();

        var detail4 = new StockTransactionDetail
        {
            StockTransactionId = tx2.Id,
            ItemId = items[0].Id,
            Quantity = 25,
            UnitId = items[0].UnitId,
            Remarks = "For Line 1 overhaul",
            DetailDate = date2,
            IsChecked = true
        };
        context.StockTransactionDetails.Add(detail4);
        await context.SaveChangesAsync();

        context.StockMovements.Add(new StockMovement
        {
            StockTransactionId = tx2.Id,
            StockTransactionDetailId = detail4.Id,
            ItemId = detail4.ItemId,
            StoreId = tx2.StoreId,
            TransactionDate = date2,
            TransactionType = TransactionType.Issue,
            Quantity = detail4.Quantity,
            SignedQuantity = -detail4.Quantity,
            CreatedAt = date2
        });
        await context.SaveChangesAsync();
    }
}
