using Inventory.Application.StockTransactions.DTOs;
using Inventory.Domain.Enums;

namespace Inventory.Application.StockTransactions.Services;

public interface IStockTransactionService
{
    Task<IReadOnlyList<StockTransactionDto>> GetAllAsync(TransactionType? type = null, int? storeId = null, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken ct = default);
    Task<StockTransactionDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<StockTransactionDto> CreateAsync(CreateStockTransactionDto dto, CancellationToken ct = default);
    Task<StockTransactionDto> UpdateAsync(int id, UpdateStockTransactionDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<string> GenerateTransactionNoAsync(TransactionType type, CancellationToken ct = default);
}
