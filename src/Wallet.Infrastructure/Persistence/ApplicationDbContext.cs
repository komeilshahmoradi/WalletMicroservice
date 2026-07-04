using KO.BuildingBlocks.Infrastructure.Persistence.EF;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Wallet.Domain.Transfers;
using Wallet.Domain.Wallets;

namespace Wallet.Infrastructure.Persistence;

public sealed class ApplicationDbContext : BaseDbContext
{
  public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
      : base(options)
  {
  }

  public DbSet<Domain.Wallets.Wallet> Wallets => Set<Domain.Wallets.Wallet>();

  public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();

  public DbSet<Transfer> Transfers => Set<Transfer>();

  protected override void ApplyServiceConfigurations(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
  }
}
