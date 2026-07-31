namespace Wallet.Domain.Entities;


public enum TransactionType { 
    Credit, 
    Debit,
    TransferCredit,
    TransferDebit 
}
public enum TransactionStatus { 
    Completed 
}

public sealed class WalletAccount
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string MobileNumber { get; set; } = null!;
    public decimal Balance { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public long Version { get; set; }
    public List<WalletTransaction> Transactions { get; set; } = [];
}

public sealed class WalletTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WalletId { get; set; }
    public WalletAccount Wallet { get; set; } = null!;
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
    public string ReferenceId { get; set; } = null!;
    public TransactionStatus Status { get; set; } = TransactionStatus.Completed;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
