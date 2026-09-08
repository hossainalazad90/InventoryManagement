-- ==============================================================================
-- 005_SeedData.sql
-- Master Data and Initial Transaction Seed Script
-- ==============================================================================

USE InventoryManagementDb;
GO

SET NOCOUNT ON;

-- 1. Seed Categories
IF NOT EXISTS (SELECT 1 FROM Categories)
BEGIN
    INSERT INTO Categories (Name, IsActive) VALUES
    ('Raw Materials', 1),
    ('Finished Goods', 1),
    ('Packaging', 1),
    ('Spare Parts', 1),
    ('Office Supplies', 1);
    PRINT 'Categories seeded.';
END

-- 2. Seed Units
IF NOT EXISTS (SELECT 1 FROM Units)
BEGIN
    INSERT INTO Units (Name, IsActive) VALUES
    ('PCS', 1),
    ('BOX', 1),
    ('KG', 1),
    ('LTR', 1),
    ('SET', 1);
    PRINT 'Units seeded.';
END

-- 3. Seed Stores
IF NOT EXISTS (SELECT 1 FROM Stores)
BEGIN
    INSERT INTO Stores (Code, Name, IsActive) VALUES
    ('MAIN-01', 'Main Warehouse', 1),
    ('RAW-01', 'Raw Material Store', 1),
    ('PROD-01', 'Production Floor Store', 1);
    PRINT 'Stores seeded.';
END

-- 4. Seed Items
IF NOT EXISTS (SELECT 1 FROM Items)
BEGIN
    DECLARE @catRaw INT = (SELECT TOP 1 Id FROM Categories WHERE Name = 'Raw Materials');
    DECLARE @catFin INT = (SELECT TOP 1 Id FROM Categories WHERE Name = 'Finished Goods');
    DECLARE @catPkg INT = (SELECT TOP 1 Id FROM Categories WHERE Name = 'Packaging');
    DECLARE @catSpr INT = (SELECT TOP 1 Id FROM Categories WHERE Name = 'Spare Parts');
    DECLARE @catOff INT = (SELECT TOP 1 Id FROM Categories WHERE Name = 'Office Supplies');

    DECLARE @uPcs INT = (SELECT TOP 1 Id FROM Units WHERE Name = 'PCS');
    DECLARE @uBox INT = (SELECT TOP 1 Id FROM Units WHERE Name = 'BOX');
    DECLARE @uKg  INT = (SELECT TOP 1 Id FROM Units WHERE Name = 'KG');
    DECLARE @uLtr INT = (SELECT TOP 1 Id FROM Units WHERE Name = 'LTR');
    DECLARE @uSet INT = (SELECT TOP 1 Id FROM Units WHERE Name = 'SET');

    INSERT INTO Items (ItemCode, ItemName, CategoryId, UnitId, ReorderLevel, IsActive, CreatedAt) VALUES
    ('ITEM-001', 'Industrial Steel Bearing 6205', @catSpr, @uPcs, 20.0000, 1, DATEADD(day, -30, GETUTCDATE())),
    ('ITEM-002', 'Hydraulic Oil ISO 46 (20L)', @catRaw, @uLtr, 50.0000, 1, DATEADD(day, -30, GETUTCDATE())),
    ('ITEM-003', 'Corrugated Cardboard Box 12x12x12', @catPkg, @uBox, 100.0000, 1, DATEADD(day, -30, GETUTCDATE())),
    ('ITEM-004', 'Thermal Eco Sensor Assembly', @catFin, @uSet, 15.0000, 1, DATEADD(day, -30, GETUTCDATE())),
    ('ITEM-005', 'Safety Protective Gloves Nitrile', @catOff, @uSet, 30.0000, 1, DATEADD(day, -30, GETUTCDATE()));

    PRINT 'Items seeded.';
END

-- 5. Seed Initial Stock Receive and Issue Transactions
IF NOT EXISTS (SELECT 1 FROM StockTransactions)
BEGIN
    DECLARE @storeMain INT = (SELECT TOP 1 Id FROM Stores WHERE Code = 'MAIN-01');
    DECLARE @itemId1 INT = (SELECT TOP 1 Id FROM Items WHERE ItemCode = 'ITEM-001');
    DECLARE @itemId2 INT = (SELECT TOP 1 Id FROM Items WHERE ItemCode = 'ITEM-002');
    DECLARE @itemId3 INT = (SELECT TOP 1 Id FROM Items WHERE ItemCode = 'ITEM-003');

    DECLARE @unitId1 INT = (SELECT TOP 1 UnitId FROM Items WHERE Id = @itemId1);
    DECLARE @unitId2 INT = (SELECT TOP 1 UnitId FROM Items WHERE Id = @itemId2);
    DECLARE @unitId3 INT = (SELECT TOP 1 UnitId FROM Items WHERE Id = @itemId3);

    -- Transaction 1: Initial Receive
    DECLARE @date1 DATETIME2 = DATEADD(day, -20, GETUTCDATE());
    INSERT INTO StockTransactions (TransactionNo, TransactionDate, TransactionType, StoreId, Remarks, CreatedAt)
    VALUES ('RCV-20260818-0001', @date1, 1, @storeMain, 'Initial purchase order delivery', @date1);
    DECLARE @tx1 INT = SCOPE_IDENTITY();

    INSERT INTO StockTransactionDetails (StockTransactionId, ItemId, Quantity, UnitId, Remarks, DetailDate, IsChecked)
    VALUES 
    (@tx1, @itemId1, 100.0000, @unitId1, 'Batch A1', @date1, 1),
    (@tx1, @itemId2, 200.0000, @unitId2, 'Sealed drums', @date1, 1),
    (@tx1, @itemId3, 500.0000, @unitId3, 'Pallet 1', @date1, 0);

    DECLARE @dt1 INT = (SELECT Id FROM StockTransactionDetails WHERE StockTransactionId = @tx1 AND ItemId = @itemId1);
    DECLARE @dt2 INT = (SELECT Id FROM StockTransactionDetails WHERE StockTransactionId = @tx1 AND ItemId = @itemId2);
    DECLARE @dt3 INT = (SELECT Id FROM StockTransactionDetails WHERE StockTransactionId = @tx1 AND ItemId = @itemId3);

    INSERT INTO StockMovements (StockTransactionId, StockTransactionDetailId, ItemId, StoreId, TransactionDate, TransactionType, Quantity, SignedQuantity, CreatedAt)
    VALUES
    (@tx1, @dt1, @itemId1, @storeMain, @date1, 1, 100.0000, 100.0000, @date1),
    (@tx1, @dt2, @itemId2, @storeMain, @date1, 1, 200.0000, 200.0000, @date1),
    (@tx1, @dt3, @itemId3, @storeMain, @date1, 1, 500.0000, 500.0000, @date1);

    -- Transaction 2: Sample Issue
    DECLARE @date2 DATETIME2 = DATEADD(day, -10, GETUTCDATE());
    INSERT INTO StockTransactions (TransactionNo, TransactionDate, TransactionType, StoreId, Remarks, CreatedAt)
    VALUES ('ISS-20260828-0001', @date2, 2, @storeMain, 'Workshop maintenance issue', @date2);
    DECLARE @tx2 INT = SCOPE_IDENTITY();

    INSERT INTO StockTransactionDetails (StockTransactionId, ItemId, Quantity, UnitId, Remarks, DetailDate, IsChecked)
    VALUES (@tx2, @itemId1, 25.0000, @unitId1, 'Overhaul line 1', @date2, 1);
    DECLARE @dt4 INT = SCOPE_IDENTITY();

    INSERT INTO StockMovements (StockTransactionId, StockTransactionDetailId, ItemId, StoreId, TransactionDate, TransactionType, Quantity, SignedQuantity, CreatedAt)
    VALUES (@tx2, @dt4, @itemId1, @storeMain, @date2, 2, 25.0000, -25.0000, @date2);

    PRINT 'Transactions and stock movements seeded successfully.';
END
GO
