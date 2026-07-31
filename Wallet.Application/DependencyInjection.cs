using Microsoft.Extensions.DependencyInjection;
using Wallet.Application.Features.Wallets.Commands;
using Wallet.Application.Features.Wallets.Validators;
using FluentValidation;

namespace Wallet.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IValidator<CreateWalletCommand>, CreateWalletCommandValidator>();
        services.AddScoped<IValidator<MoneyCommand>, CreditWalletCommandValidator>();
        services.AddScoped<IValidator<DebitCommand>, DebitWalletCommandValidator>();
        services.AddScoped<IValidator<TransferCommand>, TransferCommandValidator>();
        services.AddScoped<CreateWalletHandler>();
        services.AddScoped<CreditWalletHandler>();
        services.AddScoped<DebitWalletHandler>();
        services.AddScoped<TransferWalletHandler>();
        return services;
    }
}
