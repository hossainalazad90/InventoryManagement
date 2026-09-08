using Inventory.Application.Common;
using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Inventory.Infrastructure.Persistence;

public class InventoryDbContext : DbContext, IApplicationDbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
    {
    }

    public DbSet<Item> Items => Set<Item>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();
    public DbSet<StockTransactionDetail> StockTransactionDetails => Set<StockTransactionDetail>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return Database.BeginTransactionAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);
    }
}
