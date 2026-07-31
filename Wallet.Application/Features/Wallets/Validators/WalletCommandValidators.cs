using FluentValidation;
using Wallet.Application.Features.Wallets.Commands;

namespace Wallet.Application.Features.Wallets.Validators;

public sealed class CreateWalletCommandValidator : AbstractValidator<CreateWalletCommand>
{
    public CreateWalletCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(x => x.MobileNumber).NotEmpty().MaximumLength(30);
        RuleFor(x => x.InitialBalance).GreaterThanOrEqualTo(0);
    }
}

public sealed class CreditWalletCommandValidator : AbstractValidator<MoneyCommand>
{
    public CreditWalletCommandValidator()
    {
        RuleFor(x => x.WalletId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.ReferenceId).NotEmpty().MaximumLength(100);
    }
}

public sealed class DebitWalletCommandValidator : AbstractValidator<DebitCommand>
{
    public DebitWalletCommandValidator()
    {
        RuleFor(x => x.WalletId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.ReferenceId).NotEmpty().MaximumLength(100);
    }
}

public sealed class TransferCommandValidator : AbstractValidator<TransferCommand>
{
    public TransferCommandValidator()
    {
        RuleFor(x => x.FromWalletId).NotEmpty();
        RuleFor(x => x.ToWalletId).NotEmpty().NotEqual(x => x.FromWalletId);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.ReferenceId).NotEmpty().MaximumLength(93);
    }
}
