using KO.BuildingBlocks.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Wallet.Domain.Transfers.Repositories;
using Wallet.Domain.Wallets.Repositories;
using Wallet.Infrastructure.Persistence;
using Wallet.Infrastructure.Persistence.Repositories;

namespace Wallet.Infrastructure;

public static class DependencyInjection
{
  public static IServiceCollection AddInfrastructure(
      this IServiceCollection services,
      IConfiguration configuration)
  {
    return services.AddBuildingBlocksLogging()
      .AddBuildingBlocksDbContext<ApplicationDbContext>(configuration, Assembly.GetExecutingAssembly().FullName!, useGuidV7GeneratorInterceptor: true)
      .AddBuildingBlocksRepository()
      .AddCustomRepositories()
      .AddBuildingBlocksServices();
  }

  private static IServiceCollection AddCustomRepositories(this IServiceCollection services)
  {
    return services.AddScoped<IWalletRepository, WalletRepository>()
      .AddScoped<ITransferRepository, TransferRepository>();
  }
}