using KO.BuildingBlocks.Domain.Enumerations;

namespace Wallet.Domain.Wallets;

public sealed class WalletTransactionType : Enumeration
{
  public static readonly WalletTransactionType Deposit = new(1, nameof(Deposit));
  public static readonly WalletTransactionType Withdraw = new(2, nameof(Withdraw));
  public static readonly WalletTransactionType TransferOut = new(3, nameof(TransferOut));
  public static readonly WalletTransactionType TransferIn = new(4, nameof(TransferIn));
  public static readonly WalletTransactionType Reversal = new(5, nameof(Reversal));

  public WalletTransactionType(int id, string name) : base(id, name)
  {
  }

  public static WalletTransactionType FromValue(int value)
  {
    return FromValue<WalletTransactionType>(value);
  }

  public static WalletTransactionType FromName(string name)
  {
    if (string.IsNullOrWhiteSpace(name))
      throw new ArgumentException("WalletTransactionType name cannot be empty.", nameof(name));

    return FromName<WalletTransactionType>(name.ToUpperInvariant());
  }
}
