using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wallet.Application;
using Wallet.Domain.Entities;
using Wallet.Infrastructure.DataAccessManager.Persistence;

namespace Wallet.Infrastructure;


public sealed class WalletService(WalletDbContext db, ILogger<WalletService> logger) : IWalletService
{
    public async Task<WalletResponse> CreateAsync(CreateWalletCommand c, CancellationToken ct)
    {
        if (c.InitialBalance < 0) 
            throw new DomainException("Initial balance cannot be negative.");

        var email = c.Email.Trim().ToLowerInvariant(); var mobile = c.MobileNumber.Trim();
       
        if (await db.Wallets.AnyAsync(x => x.Email == email || x.MobileNumber == mobile, ct)) 
            throw new DomainException("Email or mobile number is already registered.", 409);
        
        var w = new WalletAccount { Name = c.Name.Trim(), Email = email, MobileNumber = mobile, Balance = c.InitialBalance };
        db.Wallets.Add(w); 
        
        if (c.InitialBalance > 0) 
            db.Transactions.Add(NewTransaction(w, TransactionType.Credit, c.InitialBalance, "INITIAL-" + w.Id));
        await SaveAsync(ct); return Map(w);
    }
    public Task<WalletResponse> CreditAsync(MoneyCommand c, CancellationToken ct) 
        => ChangeBalanceAsync(c, TransactionType.Credit, false, ct);
    public Task<WalletResponse> DebitAsync(MoneyCommand c, CancellationToken ct) 
        => ChangeBalanceAsync(c, TransactionType.Debit, true, ct);
    private async Task<WalletResponse> ChangeBalanceAsync(MoneyCommand c, TransactionType type, bool debit, CancellationToken ct)
    {
        Validate(c.Amount, c.ReferenceId); 
        
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        
        if (await db.Transactions.AsNoTracking().AnyAsync(x => x.ReferenceId == c.ReferenceId, ct)) 
            throw new DomainException("Reference ID has already been processed.", 409);

        var w = await db.Wallets.SingleOrDefaultAsync(x => x.Id == c.WalletId, ct) ?? throw new DomainException("Wallet not found.", 404);
        
        if (debit && w.Balance < c.Amount) 
            throw new DomainException("Insufficient wallet balance.");
        
        w.Balance += debit ? -c.Amount : c.Amount; w.UpdatedAt = DateTimeOffset.UtcNow; w.Version++;
        db.Transactions.Add(NewTransaction(w, type, c.Amount, c.ReferenceId)); await SaveAsync(ct); await tx.CommitAsync(ct);
        logger.LogInformation("{Type} processed for wallet {WalletId}, ref {ReferenceId}", type, w.Id, c.ReferenceId); return Map(w);
    }
    public async Task<WalletResponse> TransferAsync(TransferCommand c, CancellationToken ct)
    {
        Validate(c.Amount, c.ReferenceId); 
        
        if (c.ReferenceId.Length > 93) 
            throw new DomainException("Transfer reference ID must be 93 characters or fewer."); if (c.FromWalletId == c.ToWalletId) throw new DomainException("Source and destination wallets must differ.");

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        
        if (await db.Transactions.AsNoTracking().AnyAsync(x => x.ReferenceId == c.ReferenceId, ct)) 
            throw new DomainException("Reference ID has already been processed.", 409);
        
        var ids = new[] { c.FromWalletId, c.ToWalletId }.Order().ToArray(); var wallets = await db.Wallets.Where(x => ids.Contains(x.Id)).OrderBy(x => x.Id).ToListAsync(ct);
        
        if (wallets.Count != 2) 
            throw new DomainException("One or both wallets were not found.", 404);

        var from = wallets.Single(x => x.Id == c.FromWalletId); var to = wallets.Single(x => x.Id == c.ToWalletId);

        if (from.Balance < c.Amount) 
            throw new DomainException("Insufficient wallet balance.");

        from.Balance -= c.Amount; 
        to.Balance += c.Amount; 
        var now = DateTimeOffset.UtcNow; 
        from.UpdatedAt = to.UpdatedAt = now; 
        from.Version++; to.Version++;
        db.Transactions.Add(NewTransaction(from, TransactionType.TransferDebit, c.Amount, c.ReferenceId));
        db.Transactions.Add(NewTransaction(to, TransactionType.TransferCredit, c.Amount, c.ReferenceId + ":CREDIT"));
        await SaveAsync(ct); await tx.CommitAsync(ct); 
        logger.LogInformation("Transfer {ReferenceId} completed", c.ReferenceId); return Map(from);
    }
    public async Task<WalletResponse?> GetAsync(Guid id, CancellationToken ct) 
        =>(await db.Wallets.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct)) is { } w ? Map(w) : null;
    public async Task<PagedResponse<TransactionResponse>> GetHistoryAsync(Guid id, TransactionType? type, DateTimeOffset? from, DateTimeOffset? to, int page, int size, CancellationToken ct)
    {
        if (!await db.Wallets.AsNoTracking().AnyAsync(x => x.Id == id, ct)) 
            throw new DomainException("Wallet not found.", 404);

        var q = db.Transactions.AsNoTracking().Where(x => x.WalletId == id); 
        
        if (type.HasValue) 
            q = q.Where(x => x.Type == type); 
        
        if (from.HasValue) 
            q = q.Where(x => x.CreatedAt >= from); 
        
        if (to.HasValue) 
            q = q.Where(x => x.CreatedAt <= to);

        var total = await q.CountAsync(ct); 
        var items = await q.OrderByDescending(x => x.CreatedAt).Skip((page - 1) * size).Take(size).Select(x => new TransactionResponse(x.Id,x.WalletId,x.Type,x.Amount,x.BalanceBefore,x.BalanceAfter,x.ReferenceId,x.Status,x.CreatedAt)).ToListAsync(ct);
        return new(items, page, size, total);
    }
    private static void Validate(decimal amount, string reference) 
    { 
        if (amount <= 0) throw new DomainException("Amount must be greater than zero."); 
        
        if (string.IsNullOrWhiteSpace(reference) || reference.Length > 100) 
            throw new DomainException("Reference ID is required and must be 100 characters or fewer."); 
    }
    private static WalletTransaction NewTransaction(WalletAccount w, TransactionType type, decimal amount, string reference) 
        => new() { WalletId = w.Id, Type = type, Amount = amount, BalanceBefore = type is TransactionType.Debit or TransactionType.TransferDebit ? w.Balance + amount : w.Balance - amount, BalanceAfter = w.Balance, ReferenceId = reference };
    private static WalletResponse Map(WalletAccount w) 
        => new(w.Id, w.Name, w.Balance, w.UpdatedAt);
    private async Task SaveAsync(CancellationToken ct) 
    { 
        try {
            await db.SaveChangesAsync(ct); 
        } 
        catch (DbUpdateConcurrencyException) 
        {
            throw new DomainException("Wallet was updated concurrently; please retry.", 409); 
        } 
        catch (DbUpdateException ex) 
        { 
            logger.LogWarning(ex, "Database constraint violation"); 
            throw new DomainException("Request conflicts with existing data.", 409); 
        } 
    }
}
