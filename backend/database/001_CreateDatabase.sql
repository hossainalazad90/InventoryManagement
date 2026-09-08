-- ==============================================================================
-- 001_CreateDatabase.sql
-- Inventory Stock Management Database Creation Script
-- ==============================================================================

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'InventoryManagementDb')
BEGIN
    CREATE DATABASE InventoryManagementDb;
    PRINT 'Database InventoryManagementDb created successfully.';
END
ELSE
BEGIN
    PRINT 'Database InventoryManagementDb already exists.';
END
GO

USE InventoryManagementDb;
GO

-- Enable Read Committed Snapshot for high-concurrency consistency
ALTER DATABASE InventoryManagementDb SET READ_COMMITTED_SNAPSHOT ON;
GO
