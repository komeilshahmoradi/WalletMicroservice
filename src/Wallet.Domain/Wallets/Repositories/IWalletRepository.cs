using KO.BuildingBlocks.Domain.Repositories;
using Wallet.Domain.Shared;

namespace Wallet.Domain.Wallets.Repositories;

public interface IWalletRepository : IRepository<Wallet, Guid>
{
  Task<Wallet?> GetByUserIdAndCurrencyAsync(
    Guid userId,
    string currency,
    CancellationToken cancellationToken = default);

  Task<Wallet?> GetByIdAndUserIdAsync(
    Guid id,
    Guid userId,
    CancellationToken cancellationToken = default);

  Task<WalletTransaction?> GetTransactionByWalletIdAndOperationIdAsync(
    Guid walletId,
    Guid idempotencyKey,
    CancellationToken cancellationToken = default);

  Task<Guid?> GetTransactionIdAsync(
    Guid walletId,
    Guid idempotencyKey,
    CancellationToken cancellationToken = default);

  Task<Currency> GetCurrencyById(
    Guid id,
    CancellationToken cancellationToken = default);
}
