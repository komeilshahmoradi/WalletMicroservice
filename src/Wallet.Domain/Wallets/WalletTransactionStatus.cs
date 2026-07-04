using KO.BuildingBlocks.Domain.Enumerations;

namespace Wallet.Domain.Wallets;

public sealed class WalletTransactionStatus : Enumeration
{
  public static readonly WalletTransactionStatus Pending = new(1, nameof(Pending));
  public static readonly WalletTransactionStatus Completed = new(2, nameof(Completed));
  public static readonly WalletTransactionStatus Failed = new(3, nameof(Failed));
  public static readonly WalletTransactionStatus Reversed = new(4, nameof(Reversed));

  private WalletTransactionStatus(int id, string name)
      : base(id, name)
  {
  }

  public static WalletTransactionStatus FromValue(int value)
  {
    return FromValue<WalletTransactionStatus>(value);
  }

  public static WalletTransactionStatus FromName(string name)
  {
    if (string.IsNullOrWhiteSpace(name))
      throw new ArgumentException("WalletTransactionStatus name cannot be empty.", nameof(name));

    return FromName<WalletTransactionStatus>(name.ToUpperInvariant());
  }
}
