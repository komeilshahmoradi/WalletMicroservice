using KO.BuildingBlocks.Domain.Common;
using Wallet.Domain.Exceptions.WalletTransactionDomainException;
using Wallet.Domain.Shared;

namespace Wallet.Domain.Wallets;

public sealed class WalletTransaction : Entity<Guid>
{
  public Guid WalletId { get; private set; }

  public Guid OperationId { get; private set; }

  public Money Money { get; private set; }

  public WalletTransactionType Type { get; private set; }

  public WalletTransactionStatus Status { get; private set; }

  public string? Description { get; private set; }

  public Guid? TransferId { get; private set; }

  public DateTimeOffset CreatedOnUtc { get; private set; }

  private WalletTransaction(Guid id) : base(id)
  {
    Money = null!;
    Type = null!;
    Status = null!;
  }

  private WalletTransaction(
      Guid id,
      Guid operationId,
      Guid walletId,
      Money money,
      WalletTransactionType type,
      WalletTransactionStatus status,
      string? description,
      Guid? transferId)
      : base(id)
  {
    if (walletId == Guid.Empty)
      throw new InvalidWalletIdDomainException("Wallet id cannot be empty.");

    if (operationId == Guid.Empty)
      throw new Exceptions.WalletTransactionDomainException.InvalidOperationIdDomainException("Operation id cannot be empty.");

    WalletId = walletId;
    OperationId = operationId;
    Money = money;
    Type = type;
    Status = status;
    Description = description;
    TransferId = transferId;
    CreatedOnUtc = DateTimeOffset.UtcNow;
  }

  internal static WalletTransaction CreateDeposit(
      Guid walletId,
      Guid operationId,
      Money money,
      string? description = null)
  {
    return new WalletTransaction(
        Guid.CreateVersion7(),
        operationId,
        walletId,
        money,
        WalletTransactionType.Deposit,
        WalletTransactionStatus.Completed,
        description,
        transferId: null);
  }

  internal static WalletTransaction CreateWithdraw(
      Guid walletId,
      Guid operationId,
      Money money,
      string? description = null)
  {
    return new WalletTransaction(
        Guid.CreateVersion7(),
        operationId,
        walletId,
        money,
        WalletTransactionType.Withdraw,
        WalletTransactionStatus.Completed,
        description,
        transferId: null);
  }

  internal static WalletTransaction CreateTransferIn(
      Guid walletId,
      Guid operationId,
      Money money,
      Guid transferId,
      string? description = null)
  {
    return new WalletTransaction(
        Guid.CreateVersion7(),
        operationId,
        walletId,
        money,
        WalletTransactionType.TransferIn,
        WalletTransactionStatus.Completed,
        description,
        transferId);
  }

  internal static WalletTransaction CreateTransferOut(
      Guid walletId,
      Guid operationId,
      Money money,
      Guid transferId,
      string? description = null)
  {
    return new WalletTransaction(
        Guid.CreateVersion7(),
        operationId,
        walletId,
        money,
        WalletTransactionType.TransferOut,
        WalletTransactionStatus.Completed,
        description,
        transferId);
  }

  internal void Reverse()
  {
    if (Status == WalletTransactionStatus.Reversed)
      return;

    Status = WalletTransactionStatus.Reversed;
  }
}
