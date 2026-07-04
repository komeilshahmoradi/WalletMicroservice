using KO.BuildingBlocks.Domain.Common;
using Wallet.Domain.Exceptions.WalletDomainException;

namespace Wallet.Domain.Shared;

public sealed class Money : ValueObject
{
  public decimal Amount { get; private set; }
  public Currency Currency { get; private set; }

  private Money()
  {
    Currency = null!;
  }

  private Money(decimal amount, Currency currency)
  {
    if (currency is null)
      throw new ArgumentNullException(nameof(currency));

    Amount = currency.Round(amount);
    Currency = currency;
  }

  public static Money Of(decimal amount, Currency currency)
  {
    return new Money(amount, currency);
  }

  public static Money Zero(Currency currency)
  {
    return new Money(0, currency);
  }

  public Money Add(Money other)
  {
    EnsureSameCurrency(other);

    return new Money(Amount + other.Amount, Currency);
  }

  public Money Subtract(Money other)
  {
    EnsureSameCurrency(other);

    return new Money(Amount - other.Amount, Currency);
  }

  public bool IsPositive()
  {
    return Amount > 0;
  }

  public bool IsNegative()
  {
    return Amount < 0;
  }

  public bool IsZero()
  {
    return Amount == 0;
  }

  public bool IsGreaterThan(Money other)
  {
    EnsureSameCurrency(other);

    return Amount > other.Amount;
  }

  public bool IsGreaterThanOrEqual(Money other)
  {
    EnsureSameCurrency(other);

    return Amount >= other.Amount;
  }

  private void EnsureSameCurrency(Money other)
  {
    if (other is null)
      throw new ArgumentNullException(nameof(other));

    if (Currency != other.Currency)
      throw new CurrencyMismatchDomainException("Money currencies must be the same.");
  }

  public override IEnumerable<object?> GetEqualityComponents()
  {
    yield return Amount;
    yield return Currency;
  }

  public override string ToString()
  {
    return $"{Amount} {Currency.Name}";
  }
}
