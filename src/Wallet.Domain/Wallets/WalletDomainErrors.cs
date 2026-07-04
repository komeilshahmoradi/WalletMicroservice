namespace Wallet.Domain.Wallets;

public sealed class WalletDomainErrors
{
  public const string AmountMustBePositive = "Wallet amount must be positive.";
  public const string InsufficientBalance = "Wallet does not have sufficient balance.";
  public const string CurrencyMismatch = "Wallet currency must match money currency.";
  public const string WalletIsInactive = "Wallet is inactive.";
}
