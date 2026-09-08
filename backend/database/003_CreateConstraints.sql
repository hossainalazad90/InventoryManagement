-- ==============================================================================
-- 003_CreateConstraints.sql
-- Foreign Keys, Check Constraints, and Rules
-- ==============================================================================

USE InventoryManagementDb;
GO

-- Items Foreign Keys
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Items_Categories_CategoryId')
    ALTER TABLE Items ADD CONSTRAINT FK_Items_Categories_CategoryId 
        FOREIGN KEY (CategoryId) REFERENCES Categories (Id);

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Items_Units_UnitId')
    ALTER TABLE Items ADD CONSTRAINT FK_Items_Units_UnitId 
        FOREIGN KEY (UnitId) REFERENCES Units (Id);

-- StockTransactions Foreign Keys
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_StockTransactions_Stores_StoreId')
    ALTER TABLE StockTransactions ADD CONSTRAINT FK_StockTransactions_Stores_StoreId 
        FOREIGN KEY (StoreId) REFERENCES Stores (Id);

-- StockTransactionDetails Foreign Keys & Checks
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_StockTransactionDetails_StockTransactions_StockTransactionId')
    ALTER TABLE StockTransactionDetails ADD CONSTRAINT FK_StockTransactionDetails_StockTransactions_StockTransactionId 
        FOREIGN KEY (StockTransactionId) REFERENCES StockTransactions (Id) ON DELETE CASCADE;

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_StockTransactionDetails_Items_ItemId')
    ALTER TABLE StockTransactionDetails ADD CONSTRAINT FK_StockTransactionDetails_Items_ItemId 
        FOREIGN KEY (ItemId) REFERENCES Items (Id);

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_StockTransactionDetails_Units_UnitId')
    ALTER TABLE StockTransactionDetails ADD CONSTRAINT FK_StockTransactionDetails_Units_UnitId 
        FOREIGN KEY (UnitId) REFERENCES Units (Id);

IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_StockTransactionDetails_Quantity')
    ALTER TABLE StockTransactionDetails ADD CONSTRAINT CK_StockTransactionDetails_Quantity 
        CHECK (Quantity > 0);

-- StockMovements Foreign Keys
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_StockMovements_StockTransactions_StockTransactionId')
    ALTER TABLE StockMovements ADD CONSTRAINT FK_StockMovements_StockTransactions_StockTransactionId 
        FOREIGN KEY (StockTransactionId) REFERENCES StockTransactions (Id) ON DELETE CASCADE;

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_StockMovements_Items_ItemId')
    ALTER TABLE StockMovements ADD CONSTRAINT FK_StockMovements_Items_ItemId 
        FOREIGN KEY (ItemId) REFERENCES Items (Id);

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_StockMovements_Stores_StoreId')
    ALTER TABLE StockMovements ADD CONSTRAINT FK_StockMovements_Stores_StoreId 
        FOREIGN KEY (StoreId) REFERENCES Stores (Id);

PRINT 'Constraints created successfully.';
GO
