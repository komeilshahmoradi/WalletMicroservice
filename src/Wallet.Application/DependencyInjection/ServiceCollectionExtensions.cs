using KO.BuildingBlocks.Application.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Wallet.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddApplication(this IServiceCollection services)
  {
    var assembly = Assembly.GetExecutingAssembly();
    return services.AddBuildingBlockValidators(assembly)
      .AddBuildingBlockMediatR(assembly);
  }
}
