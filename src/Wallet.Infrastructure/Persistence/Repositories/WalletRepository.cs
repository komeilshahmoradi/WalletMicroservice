using KO.BuildingBlocks.Infrastructure.Persistence.EF;
using Microsoft.EntityFrameworkCore;
using Wallet.Domain.Shared;
using Wallet.Domain.Wallets;
using Wallet.Domain.Wallets.Repositories;

namespace Wallet.Infrastructure.Persistence.Repositories;

internal sealed class WalletRepository : EfRepository<Domain.Wallets.Wallet, Guid>, IWalletRepository
{
  public WalletRepository(ApplicationDbContext dbContext) : base(dbContext)
  {
  }

  public async Task<Domain.Wallets.Wallet?> GetByIdAndUserIdAsync(
    Guid id,
    Guid userId,
    CancellationToken cancellationToken = default)
  {
    var result = await _dbContext.Set<Domain.Wallets.Wallet>()
      .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);
    return result;
  }

  public async Task<Domain.Wallets.Wallet?> GetByUserIdAndCurrencyAsync(
    Guid userId,
    string currency,
    CancellationToken cancellationToken = default)
  {
    var result = await _dbContext.Set<Domain.Wallets.Wallet>()
      .FirstOrDefaultAsync(x => x.UserId == userId && x.Currency == Currency.FromName(currency), cancellationToken);
    return result;
  }

  public async Task<WalletTransaction?> GetTransactionByWalletIdAndOperationIdAsync(
    Guid walletId,
    Guid idempotencyKey,
    CancellationToken cancellationToken = default)
  {
    var result = await _dbContext.Set<WalletTransaction>()
      .FirstOrDefaultAsync(x => x.WalletId == walletId && x.OperationId == idempotencyKey, cancellationToken);
    return result;
  }

  public async Task<Guid?> GetTransactionIdAsync(
    Guid walletId,
    Guid idempotencyKey,
    CancellationToken cancellationToken = default)
  {
    var result = await _dbContext.Set<WalletTransaction>()
      .Where(x => x.WalletId == walletId && x.OperationId == idempotencyKey)
      .Select(x => x.Id)
      .FirstOrDefaultAsync();
    return result;
  }
}
