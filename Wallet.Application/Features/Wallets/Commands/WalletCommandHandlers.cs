using FluentValidation;

namespace Wallet.Application.Features.Wallets.Commands;

public interface ICommandHandler<in TCommand, TResult>
{
    Task<TResult> Handle(TCommand command, CancellationToken cancellationToken);
}

public sealed class CreateWalletHandler(IWalletService service, IValidator<CreateWalletCommand> validator) : ICommandHandler<CreateWalletCommand, WalletResponse>
{ 
    public async Task<WalletResponse> Handle(CreateWalletCommand command, CancellationToken ct) { await validator.ValidateAndThrowAsync(command, ct); return await service.CreateAsync(command, ct); } 
}

public sealed class CreditWalletHandler(IWalletService service, IValidator<MoneyCommand> validator) : ICommandHandler<MoneyCommand, WalletResponse>
{ 
    public async Task<WalletResponse> Handle(MoneyCommand command, CancellationToken ct) { await validator.ValidateAndThrowAsync(command, ct); return await service.CreditAsync(command, ct); } 
}

public sealed class DebitWalletHandler(IWalletService service, IValidator<DebitCommand> validator) : ICommandHandler<DebitCommand, WalletResponse>
{ 
    public async Task<WalletResponse> Handle(DebitCommand command, CancellationToken ct) { await validator.ValidateAndThrowAsync(command, ct); return await service.DebitAsync(new(command.WalletId, command.Amount, command.ReferenceId), ct); } 
}

public sealed class TransferWalletHandler(IWalletService service, IValidator<TransferCommand> validator) : ICommandHandler<TransferCommand, WalletResponse>
{
    public async Task<WalletResponse> Handle(TransferCommand command, CancellationToken ct) { await validator.ValidateAndThrowAsync(command, ct); return await service.TransferAsync(command, ct); } 
}

public sealed record DebitCommand(Guid WalletId, decimal Amount, string ReferenceId);
