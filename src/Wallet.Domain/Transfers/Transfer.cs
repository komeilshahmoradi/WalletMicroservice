using KO.BuildingBlocks.Domain.Common;
using KO.BuildingBlocks.Domain.Concurrency;
using Wallet.Domain.Exceptions.TransferDomainException;
using Wallet.Domain.Shared;

namespace Wallet.Domain.Transfers;

public sealed class Transfer : AggregateRoot<Guid>, IHasConcurrencyToken
{
  public Guid UserId { get; private set; }
  public Guid SourceWalletId { get; private set; }
  public Guid DestinationWalletId { get; private set; }
  public Money Money { get; private set; }
  public decimal ExchangeRate { get; set; }
  public TransferStatus Status { get; private set; }
  public string? Description { get; private set; }
  public DateTimeOffset CreatedOnUtc { get; private set; }
  public DateTimeOffset? CompletedOnUtc { get; private set; }
  public DateTimeOffset? FailedOnUtc { get; private set; }
  public string? FailureReason { get; private set; }

  private Transfer(Guid id) : base(id)
  {
    Money = null!;
    Status = null!;
  }

  private Transfer(
      Guid id,
      Guid userId,
      Guid sourceWalletId,
      Guid destinationWalletId,
      Money money,
      decimal exchangeRate,
      string? description)
      : base(id)
  {
    if (userId == Guid.Empty)
      throw new InvalidUserIdDomainException("user id cannot be empty.");

    if (sourceWalletId == Guid.Empty)
      throw new InvalidSourceWalletIdDomainException("Source wallet id cannot be empty.");

    if (destinationWalletId == Guid.Empty)
      throw new InvalidDestinationWalletIdDomainException("Destination wallet id cannot be empty.");

    if (sourceWalletId == destinationWalletId)
      throw new SourceIdAndDesinitionIdAreSameValueDomainException(TransferDomainErrors.WalletsMustBeDifferent);

    if (money is null)
      throw new InvalidCurrencyDomainException("Money is invalid.");

    if (!money.IsPositive())
      throw new AmountMustBePositiveDomainException(TransferDomainErrors.AmountMustBePositive);

    if (exchangeRate < 0)
      throw new ExchangeRateMustBeEqualOrAboveZeroDomainException(TransferDomainErrors.ExchangeRateMustBeEqualOrAboveZero);

    UserId = userId;
    SourceWalletId = sourceWalletId;
    DestinationWalletId = destinationWalletId;
    Money = money;
    ExchangeRate = exchangeRate;
    Status = TransferStatus.Pending;
    Description = description;
    CreatedOnUtc = DateTimeOffset.UtcNow;
  }

  public static Transfer Initiate(
      Guid userId,
      Guid sourceWalletId,
      Guid destinationWalletId,
      Money money,
      decimal exchangeRate,
      string? description = null)
  {
    return new Transfer(
        Guid.CreateVersion7(),
        userId,
        sourceWalletId,
        destinationWalletId,
        money,
        exchangeRate,
        description);
  }

  public void Complete(Wallets.Wallet sourceWallet, Wallets.Wallet destinationWallet, Guid operationId)
  {
    if (Status == TransferStatus.Completed)
      throw new TransferAlreadyCompletedDomainException(TransferDomainErrors.TransferAlreadyCompleted);

    if (Status == TransferStatus.Failed)
      throw new TransferAlreadyFailedDomainException(TransferDomainErrors.TransferAlreadyFailed);

    if (sourceWallet.Id != SourceWalletId)
      throw new InvalidSourceWalletIdDomainException("Invalid source wallet.");

    if (destinationWallet.Id != DestinationWalletId)
      throw new InvalidDestinationWalletIdDomainException("Invalid destination wallet.");

    if (sourceWallet.Currency == destinationWallet.Currency && sourceWallet.UserId == destinationWallet.UserId)
    {
      throw new InvalidTransferCurrencyDomainException("Currency of source and destination wallet, for the same user should be different.");
    }

    sourceWallet.ApplyTransferOut(
        Money,
        Id,
        operationId,
        $"Transfer out {Money.Amount} {Money.Currency} to wallet {DestinationWalletId}.OperationId: {operationId} | Description: {Description}");

    var destinationMoney = ApplyExchangeRate(sourceWallet.Currency, destinationWallet.Currency, SourceWalletId, DestinationWalletId);

    destinationWallet.ApplyTransferIn(
        destinationMoney,
        Id,
        operationId,
        $"Transfer in {destinationMoney.Amount} {destinationMoney.Currency} from wallet {SourceWalletId}.OperationId: {operationId} | Description: {Description}");

    Status = TransferStatus.Completed;
    CompletedOnUtc = DateTimeOffset.UtcNow;
  }

  private Money ApplyExchangeRate(
    Currency sourceCurrency,
    Currency destinationCurrency,
    Guid sourceUserId,
    Guid destinationUserId)
  {
    if (sourceCurrency == destinationCurrency && sourceUserId != destinationUserId)
    {
      ExchangeRate = 0;
      return Money;
    }

    if (destinationCurrency == Currency.Rial)
    {
      return Money.Of(Money.Amount * ExchangeRate, destinationCurrency);
    }
    else
    {
      return Money.Of(Money.Amount / ExchangeRate, destinationCurrency);
    }
  }

  public void Fail(string reason)
  {
    if (Status == TransferStatus.Completed)
      throw new TransferAlreadyCompletedDomainException(TransferDomainErrors.TransferAlreadyCompleted);

    if (Status == TransferStatus.Failed)
      return;

    Status = TransferStatus.Failed;
    FailureReason = reason;
    FailedOnUtc = DateTimeOffset.UtcNow;
  }

  public void Cancel()
  {
    if (Status != TransferStatus.Pending)
      throw new TransferCannotBeCancelledDomainException(TransferDomainErrors.TransferCannotBeCancelled);

    Status = TransferStatus.Cancelled;
  }
}
