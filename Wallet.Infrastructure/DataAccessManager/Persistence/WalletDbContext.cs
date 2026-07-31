using Microsoft.EntityFrameworkCore;
using Wallet.Domain.Entities;

namespace Wallet.Infrastructure.DataAccessManager.Persistence;


public sealed class WalletDbContext(DbContextOptions<WalletDbContext> options) : DbContext(options)
{
    public DbSet<WalletAccount> Wallets => Set<WalletAccount>();
    public DbSet<WalletTransaction> Transactions => Set<WalletTransaction>();
    protected override void OnModelCreating(ModelBuilder model)
    {
        var wallet = model.Entity<WalletAccount>();
        wallet.ToTable("Wallets"); wallet.HasKey(x => x.Id);
        wallet.Property(x => x.Name).HasMaxLength(200).IsRequired();
        wallet.Property(x => x.Email).HasMaxLength(320).IsRequired(); wallet.HasIndex(x => x.Email).IsUnique();
        wallet.Property(x => x.MobileNumber).HasMaxLength(30).IsRequired(); wallet.HasIndex(x => x.MobileNumber).IsUnique();
        wallet.Property(x => x.Balance).HasPrecision(18, 2); wallet.Property(x => x.Version).IsConcurrencyToken();
        
        var transaction = model.Entity<WalletTransaction>();
        transaction.ToTable("WalletTransactions"); transaction.HasKey(x => x.Id);
        transaction.Property(x => x.Amount).HasPrecision(18, 2); transaction.Property(x => x.BalanceBefore).HasPrecision(18, 2); transaction.Property(x => x.BalanceAfter).HasPrecision(18, 2);
        transaction.Property(x => x.ReferenceId).HasMaxLength(100).IsRequired(); transaction.HasIndex(x => x.ReferenceId).IsUnique();
        transaction.HasIndex(x => new { x.WalletId, x.CreatedAt });
        transaction.HasOne(x => x.Wallet).WithMany(x => x.Transactions).HasForeignKey(x => x.WalletId);
    }
}
