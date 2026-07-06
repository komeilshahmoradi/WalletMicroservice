using KO.BuildingBlocks.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Reflection;
using Wallet.Application.ExternalContractors;
using Wallet.Domain.Transfers.Repositories;
using Wallet.Domain.Wallets.Repositories;
using Wallet.Infrastructure.ExternalServices.ExchangeRate;
using Wallet.Infrastructure.Persistence;
using Wallet.Infrastructure.Persistence.Repositories;

namespace Wallet.Infrastructure;

public static class DependencyInjection
{
  public static IServiceCollection AddInfrastructure(
      this IServiceCollection services,
      IConfiguration configuration)
  {
    ConfigureOptions(services, configuration);
    return services.AddBuildingBlocksLogging()
      .AddBuildingBlocksDbContext<ApplicationDbContext>(configuration, Assembly.GetExecutingAssembly().FullName!, useGuidV7GeneratorInterceptor: true)
      .AddBuildingBlocksRepository()
      .AddCustomRepositories()
      .AddBuildingBlocksServices()
      .AddCustomServices();
  }

  private static void ConfigureOptions(IServiceCollection services, IConfiguration configuration)
  {
    services.Configure<ExchangeRateOptions>(configuration.GetSection(ExchangeRateOptions.SectionName));
  }

  private static IServiceCollection AddCustomRepositories(this IServiceCollection services)
  {
    return services.AddScoped<IWalletRepository, WalletRepository>()
      .AddScoped<ITransferRepository, TransferRepository>();
  }

  private static IServiceCollection AddCustomServices(this IServiceCollection services)
  {
    services.AddHttpClient<IExchangeRateProvider, ExchangeRateProvider>((sp, client) =>
    {
      var options = sp.GetRequiredService<IOptions<ExchangeRateOptions>>().Value;

      client.BaseAddress = new Uri(options.BaseUrl);
      client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
      client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
      client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
    });

    return services;
  }
}