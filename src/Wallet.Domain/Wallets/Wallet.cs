using KO.BuildingBlocks.Domain.Common;
using KO.BuildingBlocks.Domain.Concurrency;
using Wallet.Domain.Exceptions.WalletDomainException;
using Wallet.Domain.Shared;

namespace Wallet.Domain.Wallets;

public sealed class Wallet : AggregateRoot<Guid>, IHasConcurrencyToken
{
  private readonly List<WalletTransaction> _transactions = [];

  public Guid UserId { get; private set; }

  public Currency Currency { get; private set; }

  public decimal Balance { get; private set; }

  public bool IsActive { get; private set; }

  public IReadOnlyCollection<WalletTransaction> Transactions => _transactions.AsReadOnly();

  private Wallet(Guid id) : base(id)
  {
    Currency = null!;
  }

  private Wallet(Guid id, Guid userId, Currency currency)
      : base(id)
  {
    if (userId == Guid.Empty)
      throw new InvalidUserIdDomainException("User id cannot be empty.");

    if (currency is null)
      throw new InvalidCurrencyDomainException("Currency can not be null");

    UserId = userId;
    Currency = currency;
    Balance = 0;
    IsActive = true;
  }

  public static Wallet Create(Guid userId, Currency currency)
  {
    return new Wallet(Guid.CreateVersion7(), userId, currency);
  }

  public Money GetBalance()
  {
    return Money.Of(Balance, Currency);
  }

  public void Deposit(Money money, Guid operationId, string? description = null)
  {
    EnsureActive();
    EnsurePositiveAmount(money);
    EnsureCurrencyMatches(money);
    EnsureOperationIdIsValid(operationId);

    Balance += money.Amount;

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
    EnsureCurrencyMatches(money);
    EnsureSufficientBalance(money);

    Balance -= money.Amount;

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
    EnsureCurrencyMatches(money);
    EnsureSufficientBalance(money);

    Balance -= money.Amount;

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
    EnsureCurrencyMatches(money);

    Balance += money.Amount;

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

  private void EnsureCurrencyMatches(Money money)
  {
    if (money.Currency != Currency)
      throw new CurrencyMismatchDomainException(WalletDomainErrors.CurrencyMismatch);
  }

  private void EnsureSufficientBalance(Money money)
  {
    if (Balance < money.Amount)
      throw new InsufficientBalanceDomainException(WalletDomainErrors.InsufficientBalance);
  }

  private static void EnsureOperationIdIsValid(Guid operationId)
  {
    if (operationId == Guid.Empty)
      throw new InvalidOperationIdDomainException("Operation id cannot be empty.");
  }
}
