using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Inventory.Application.Common;

public interface IApplicationDbContext
{
    DbSet<Item> Items { get; }
    DbSet<Category> Categories { get; }
    DbSet<Unit> Units { get; }
    DbSet<Store> Stores { get; }
    DbSet<StockTransaction> StockTransactions { get; }
    DbSet<StockTransactionDetail> StockTransactionDetails { get; }
    DbSet<StockMovement> StockMovements { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
