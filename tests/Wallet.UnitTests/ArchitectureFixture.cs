using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Wallet.Application.DependencyInjection;
using Wallet.Infrastructure.Persistence;

namespace Wallet.UnitTests;

public abstract class ArchitectureFixture
{
  protected static readonly System.Reflection.Assembly DomainAssembly = typeof(Wallet.Domain.Wallets.Wallet).Assembly;
  protected static readonly System.Reflection.Assembly ApplicationAssembly = typeof(ServiceCollectionExtensions).Assembly;
  protected static readonly System.Reflection.Assembly InfrastructureAssembly = typeof(ApplicationDbContext).Assembly;
  protected static readonly System.Reflection.Assembly PresentationAssembly = typeof(Program).Assembly;

  protected static readonly Architecture Architecture = new ArchLoader()
      .LoadAssemblies(
          DomainAssembly,
          ApplicationAssembly,
          InfrastructureAssembly,
          PresentationAssembly)
      .Build();
}
