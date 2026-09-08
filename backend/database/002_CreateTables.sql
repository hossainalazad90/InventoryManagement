-- ==============================================================================
-- 002_CreateTables.sql
-- DDL Tables Creation Script
-- ==============================================================================

USE InventoryManagementDb;
GO

-- 1. Categories
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Categories')
BEGIN
    CREATE TABLE Categories (
        Id INT IDENTITY(1,1) NOT NULL,
        Name NVARCHAR(100) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CONSTRAINT PK_Categories PRIMARY KEY CLUSTERED (Id)
    );
    PRINT 'Table Categories created.';
END
GO

-- 2. Units
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Units')
BEGIN
    CREATE TABLE Units (
        Id INT IDENTITY(1,1) NOT NULL,
        Name NVARCHAR(50) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CONSTRAINT PK_Units PRIMARY KEY CLUSTERED (Id)
    );
    PRINT 'Table Units created.';
END
GO

-- 3. Stores
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Stores')
BEGIN
    CREATE TABLE Stores (
        Id INT IDENTITY(1,1) NOT NULL,
        Code NVARCHAR(20) NOT NULL,
        Name NVARCHAR(100) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CONSTRAINT PK_Stores PRIMARY KEY CLUSTERED (Id)
    );
    PRINT 'Table Stores created.';
END
GO

-- 4. Items (Item Master)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Items')
BEGIN
    CREATE TABLE Items (
        Id INT IDENTITY(1,1) NOT NULL,
        ItemCode NVARCHAR(50) NOT NULL,
        ItemName NVARCHAR(200) NOT NULL,
        CategoryId INT NOT NULL,
        UnitId INT NOT NULL,
        ReorderLevel DECIMAL(18, 4) NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT PK_Items PRIMARY KEY CLUSTERED (Id)
    );
    PRINT 'Table Items created.';
END
GO

-- 5. StockTransactions (Transaction Header)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'StockTransactions')
BEGIN
    CREATE TABLE StockTransactions (
        Id INT IDENTITY(1,1) NOT NULL,
        TransactionNo NVARCHAR(50) NOT NULL,
        TransactionDate DATETIME2 NOT NULL,
        TransactionType INT NOT NULL, -- 1 = Receive, 2 = Issue, 3 = Return
        StoreId INT NOT NULL,
        Remarks NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        RowVersion ROWVERSION NOT NULL,
        CONSTRAINT PK_StockTransactions PRIMARY KEY CLUSTERED (Id)
    );
    PRINT 'Table StockTransactions created.';
END
GO

-- 6. StockTransactionDetails (Transaction Details / Multi-Row Grid)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'StockTransactionDetails')
BEGIN
    CREATE TABLE StockTransactionDetails (
        Id INT IDENTITY(1,1) NOT NULL,
        StockTransactionId INT NOT NULL,
        ItemId INT NOT NULL,
        Quantity DECIMAL(18, 4) NOT NULL,
        UnitId INT NOT NULL,
        Remarks NVARCHAR(500) NULL,
        DetailDate DATETIME2 NULL,
        IsChecked BIT NOT NULL DEFAULT 0,
        CONSTRAINT PK_StockTransactionDetails PRIMARY KEY CLUSTERED (Id)
    );
    PRINT 'Table StockTransactionDetails created.';
END
GO

-- 7. StockMovements (Historical Stock Ledger)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'StockMovements')
BEGIN
    CREATE TABLE StockMovements (
        Id INT IDENTITY(1,1) NOT NULL,
        StockTransactionId INT NOT NULL,
        StockTransactionDetailId INT NOT NULL,
        ItemId INT NOT NULL,
        StoreId INT NOT NULL,
        TransactionDate DATETIME2 NOT NULL,
        TransactionType INT NOT NULL,
        Quantity DECIMAL(18, 4) NOT NULL,
        SignedQuantity DECIMAL(18, 4) NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT PK_StockMovements PRIMARY KEY CLUSTERED (Id)
    );
    PRINT 'Table StockMovements created.';
END
GO
