-- ==============================================================================
-- 004_CreateIndexes.sql
-- Indexes for Uniqueness and Performance
-- ==============================================================================

USE InventoryManagementDb;
GO

-- Unique Index on ItemCode
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UX_Items_ItemCode')
    CREATE UNIQUE NONCLUSTERED INDEX UX_Items_ItemCode ON Items(ItemCode);

-- Unique Index on Store Code
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UX_Stores_Code')
    CREATE UNIQUE NONCLUSTERED INDEX UX_Stores_Code ON Stores(Code);

-- Unique Index on TransactionNo
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UX_StockTransactions_TransactionNo')
    CREATE UNIQUE NONCLUSTERED INDEX UX_StockTransactions_TransactionNo ON StockTransactions(TransactionNo);

-- Indexes on StockTransactions
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_StockTransactions_TransactionDate')
    CREATE NONCLUSTERED INDEX IX_StockTransactions_TransactionDate ON StockTransactions(TransactionDate);

-- Performance Composite Indexes for Ledger and Reporting
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_StockMovements_Item_Store_Date')
    CREATE NONCLUSTERED INDEX IX_StockMovements_Item_Store_Date 
    ON StockMovements(ItemId, StoreId, TransactionDate)
    INCLUDE (Quantity, SignedQuantity, TransactionType);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_StockMovements_TransactionId')
    CREATE NONCLUSTERED INDEX IX_StockMovements_TransactionId 
    ON StockMovements(StockTransactionId);

PRINT 'Indexes created successfully.';
GO
