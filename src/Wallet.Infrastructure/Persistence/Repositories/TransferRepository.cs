
using KO.BuildingBlocks.Infrastructure.Persistence.EF;
using Microsoft.EntityFrameworkCore;
using Wallet.Domain.Transfers;
using Wallet.Domain.Transfers.Repositories;
using Wallet.Domain.Wallets;

namespace Wallet.Infrastructure.Persistence.Repositories;

internal sealed class TransferRepository : EfRepository<Transfer, Guid>, ITransferRepository
{
  public TransferRepository(ApplicationDbContext dbContext) : base(dbContext)
  {
  }

  public async Task<Transfer?> GetByIdAndUserIdAsync(
    Guid id,
    Guid userId,
    CancellationToken cancellationToken = default)
  {
    var result = await _dbContext.Set<Transfer>()
      .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == id, cancellationToken);
    return result;
  }

  public async Task<Transfer?> GetByIdempotencyKeyAsync(
    Guid sourceWalletId,
    Guid userId,
    Guid idempotencyKey,
    CancellationToken cancellationToken = default)
  {
    return await _dbContext.Set<WalletTransaction>()
      .Where(transaction =>
        transaction.OperationId == idempotencyKey &&
        transaction.WalletId == sourceWalletId &&
        transaction.Type == WalletTransactionType.TransferOut)
      .Join(
        _dbContext.Set<Transfer>(),
        transaction => transaction.TransferId,
        transfer => transfer.Id,
        (_, transfer) => transfer)
      .Where(transfer => transfer.UserId == userId)
      .AsNoTracking()
      .SingleOrDefaultAsync(cancellationToken);
  }
}
