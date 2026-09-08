using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.IsActive).HasDefaultValue(true);
    }
}

public class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> builder)
    {
        builder.ToTable("Units");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Name).IsRequired().HasMaxLength(50);
        builder.Property(u => u.IsActive).HasDefaultValue(true);
    }
}

public class StoreConfiguration : IEntityTypeConfiguration<Store>
{
    public void Configure(EntityTypeBuilder<Store> builder)
    {
        builder.ToTable("Stores");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Code).IsRequired().HasMaxLength(20);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(100);
        builder.Property(s => s.IsActive).HasDefaultValue(true);
        builder.HasIndex(s => s.Code).IsUnique();
    }
}

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.ToTable("Items");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.ItemCode).IsRequired().HasMaxLength(50);
        builder.Property(i => i.ItemName).IsRequired().HasMaxLength(200);
        builder.Property(i => i.ReorderLevel).HasColumnType("decimal(18, 4)").HasDefaultValue(0);
        builder.Property(i => i.IsActive).HasDefaultValue(true);
        builder.Property(i => i.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(i => i.ItemCode).IsUnique();

        builder.HasOne(i => i.Category)
            .WithMany(c => c.Items)
            .HasForeignKey(i => i.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Unit)
            .WithMany(u => u.Items)
            .HasForeignKey(i => i.UnitId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class StockTransactionConfiguration : IEntityTypeConfiguration<StockTransaction>
{
    public void Configure(EntityTypeBuilder<StockTransaction> builder)
    {
        builder.ToTable("StockTransactions");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.TransactionNo).IsRequired().HasMaxLength(50);
        builder.Property(t => t.TransactionDate).IsRequired();
        builder.Property(t => t.TransactionType).IsRequired().HasConversion<int>();
        builder.Property(t => t.Remarks).HasMaxLength(500);
        builder.Property(t => t.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        builder.Property(t => t.RowVersion).IsRowVersion();

        builder.HasIndex(t => t.TransactionNo).IsUnique();
        builder.HasIndex(t => t.TransactionDate);

        builder.HasOne(t => t.Store)
            .WithMany(s => s.Transactions)
            .HasForeignKey(t => t.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.Details)
            .WithOne(d => d.StockTransaction)
            .HasForeignKey(d => d.StockTransactionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class StockTransactionDetailConfiguration : IEntityTypeConfiguration<StockTransactionDetail>
{
    public void Configure(EntityTypeBuilder<StockTransactionDetail> builder)
    {
        builder.ToTable("StockTransactionDetails");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Quantity).IsRequired().HasColumnType("decimal(18, 4)");
        builder.Property(d => d.Remarks).HasMaxLength(500);

        builder.HasOne(d => d.Item)
            .WithMany(i => i.TransactionDetails)
            .HasForeignKey(d => d.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Unit)
            .WithMany()
            .HasForeignKey(d => d.UnitId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.TransactionDate).IsRequired();
        builder.Property(m => m.TransactionType).IsRequired().HasConversion<int>();
        builder.Property(m => m.Quantity).IsRequired().HasColumnType("decimal(18, 4)");
        builder.Property(m => m.SignedQuantity).IsRequired().HasColumnType("decimal(18, 4)");
        builder.Property(m => m.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(m => new { m.ItemId, m.StoreId, m.TransactionDate });
        builder.HasIndex(m => m.StockTransactionId);

        builder.HasOne(m => m.StockTransaction)
            .WithMany(t => t.Movements)
            .HasForeignKey(m => m.StockTransactionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.StockTransactionDetail)
            .WithMany(d => d.Movements)
            .HasForeignKey(m => m.StockTransactionDetailId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(m => m.Item)
            .WithMany(i => i.StockMovements)
            .HasForeignKey(m => m.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Store)
            .WithMany(s => s.StockMovements)
            .HasForeignKey(m => m.StoreId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
