using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wallet.Infrastructure.DataAccessManager.Persistence;
using Wallet.Application;

namespace Wallet.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<WalletDbContext>(options => 
        options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<IWalletService, WalletService>();
        return services;
    }
}
