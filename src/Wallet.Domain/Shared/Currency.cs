using KO.BuildingBlocks.Domain.Enumerations;

namespace Wallet.Domain.Shared;

public sealed class Currency : Enumeration
{
  public static readonly Currency Rial = new(1, nameof(Rial), 0);
  public static readonly Currency USD = new(2, nameof(USD), 2);
  public static readonly Currency Euro = new(3, nameof(Euro), 2);

  public int DecimalPlaces { get; private set; }

  private Currency(int id, string name, byte decimalPlaces) : base(id, name)
  {
    DecimalPlaces = decimalPlaces;
  }

  public static Currency FromValue(int value)
  {
    return FromValue<Currency>(value);
  }

  public static Currency FromName(string name)
  {
    if (string.IsNullOrWhiteSpace(name))
      throw new ArgumentException("Currency name cannot be empty.", nameof(name));

    return FromName<Currency>(name.ToUpperInvariant());
  }

  public decimal Round(decimal amount) =>
      Math.Round(amount, DecimalPlaces, MidpointRounding.ToEven);
}
