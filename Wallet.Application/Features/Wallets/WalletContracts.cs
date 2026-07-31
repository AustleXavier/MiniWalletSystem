using Wallet.Domain.Entities;

namespace Wallet.Application;


public sealed record CreateWalletCommand(string Name, string Email, string MobileNumber, decimal InitialBalance = 0);
public sealed record MoneyCommand(Guid WalletId, decimal Amount, string ReferenceId);
public sealed record TransferCommand(Guid FromWalletId, Guid ToWalletId, decimal Amount, string ReferenceId);
public sealed record WalletResponse(Guid WalletId, string Name, decimal Balance, DateTimeOffset LastUpdatedAt);
public sealed record TransactionResponse(Guid TransactionId, Guid WalletId, TransactionType Type, decimal Amount, decimal BalanceBefore, decimal BalanceAfter, string ReferenceId, TransactionStatus Status, DateTimeOffset CreatedAt);
public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int PageNumber, int PageSize, int TotalCount);

public interface IWalletService
{
    Task<WalletResponse> CreateAsync(CreateWalletCommand command, CancellationToken ct);
    Task<WalletResponse> CreditAsync(MoneyCommand command, CancellationToken ct);
    Task<WalletResponse> DebitAsync(MoneyCommand command, CancellationToken ct);
    Task<WalletResponse> TransferAsync(TransferCommand command, CancellationToken ct);
    Task<WalletResponse?> GetAsync(Guid walletId, CancellationToken ct);
    Task<PagedResponse<TransactionResponse>> GetHistoryAsync(Guid walletId, TransactionType? type, DateTimeOffset? from, DateTimeOffset? to, int pageNumber, int pageSize, CancellationToken ct);
    Task<PagedResponse<WalletResponse>> GetWalletAsync(DateTimeOffset? from, DateTimeOffset? to, int pageNumber, int pageSize, CancellationToken ct);
}

public sealed class DomainException(string message, int statusCode = 400) : Exception(message)
{ 
    public int StatusCode { get; } = statusCode; 
}
