using KO.BuildingBlocks.Domain.Common;
using KO.BuildingBlocks.Domain.Concurrency;
using Wallet.Domain.Exceptions.WalletDomainException;
using Wallet.Domain.Shared;

namespace Wallet.Domain.Wallets;

public sealed class Wallet : AggregateRoot<Guid>, IHasConcurrencyToken
{
  private readonly List<WalletTransaction> _transactions = [];

  public Guid UserId { get; private set; }

  public Money Balance { get; private set; }

  public bool IsActive { get; private set; }

  public IReadOnlyCollection<WalletTransaction> Transactions => _transactions.AsReadOnly();

  private Wallet(Guid id) : base(id)
  {
    Balance = null!;
  }

  private Wallet(Guid id, Guid userId, Currency currency)
      : base(id)
  {
    if (userId == Guid.Empty)
      throw new InvalidUserIdDomainException("User id cannot be empty.");

    if (currency is null)
      throw new InvalidCurrencyDomainException("Currency can not be null");

    UserId = userId;
    Balance = Money.Of(0, currency);
    IsActive = true;
  }

  public static Wallet Create(Guid userId, Currency currency)
  {
    return new Wallet(Guid.CreateVersion7(), userId, currency);
  }

  public void Deposit(Money money, Guid operationId, string? description = null)
  {
    EnsureActive();
    EnsurePositiveAmount(money);
    EnsureOperationIdIsValid(operationId);

    Balance = Balance.Add(money);

    var transaction = WalletTransaction.CreateDeposit(
        Id,
        operationId,
        money,
        description);

    _transactions.Add(transaction);
  }

  public void Withdraw(Money money, Guid operationId, string? description = null)
  {
    EnsureActive();
    EnsurePositiveAmount(money);
    EnsureSufficientBalance(money);

    Balance = Balance.Subtract(money);

    var transaction = WalletTransaction.CreateWithdraw(
        Id,
        operationId,
        money,
        description);

    _transactions.Add(transaction);
  }

  public void ApplyTransferOut(Money money, Guid transferId, Guid operationId, string? description = null)
  {
    EnsureActive();
    EnsurePositiveAmount(money);
    EnsureSufficientBalance(money);

    Balance = Balance.Subtract(money);

    var transaction = WalletTransaction.CreateTransferOut(
        Id,
        operationId,
        money,
        transferId,
        description);

    _transactions.Add(transaction);
  }

  public void ApplyTransferIn(Money money, Guid transferId, Guid operationId, string? description = null)
  {
    EnsureActive();
    EnsurePositiveAmount(money);

    Balance = Balance.Add(money);

    var transaction = WalletTransaction.CreateTransferIn(
        Id,
        operationId,
        money,
        transferId,
        description);

    _transactions.Add(transaction);
  }

  public void Activate()
  {
    IsActive = true;
  }

  public void Deactivate()
  {
    IsActive = false;
  }

  private void EnsureActive()
  {
    if (!IsActive)
      throw new WalletIsInactiveDomainException(WalletDomainErrors.WalletIsInactive);
  }

  private void EnsurePositiveAmount(Money money)
  {
    if (!money.IsPositive())
      throw new AmountMustBePositiveDomainException(WalletDomainErrors.AmountMustBePositive);
  }

  private void EnsureSufficientBalance(Money money)
  {
    if (money.IsGreaterThan(Balance))
      throw new InsufficientBalanceDomainException(WalletDomainErrors.InsufficientBalance);
  }

  private static void EnsureOperationIdIsValid(Guid operationId)
  {
    if (operationId == Guid.Empty)
      throw new InvalidOperationIdDomainException("Operation id cannot be empty.");
  }
}
