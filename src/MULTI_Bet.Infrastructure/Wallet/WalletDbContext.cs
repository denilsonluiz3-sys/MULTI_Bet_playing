using Microsoft.EntityFrameworkCore;

namespace MULTI_Bet.Infrastructure.Wallet;

public sealed class WalletDbContext(DbContextOptions<WalletDbContext> options) : DbContext(options)
{
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.ToTable("wallets");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Currency).HasMaxLength(3).IsRequired();
            entity.HasIndex(x => x.UserId).IsUnique();
        });

        modelBuilder.Entity<WalletTransaction>(entity =>
        {
            entity.ToTable("wallet_transactions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.Currency).HasMaxLength(3).IsRequired();
            entity.Property(x => x.PixTxId).HasMaxLength(100);
            entity.HasIndex(x => x.PixTxId).IsUnique();
            entity.HasIndex(x => new { x.WalletId, x.CreatedAt });
            entity.HasOne<Wallet>()
                .WithMany()
                .HasForeignKey(x => x.WalletId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
