namespace Wallet.Domain.Transfers;

public sealed class TransferDomainErrors
{
  public const string AmountMustBePositive = "Transfer amount must be positive.";
  public const string WalletsMustBeDifferent = "Source and destination wallets must be different.";
  public const string TransferAlreadyCompleted = "Transfer is already completed.";
  public const string TransferAlreadyFailed = "Transfer is already failed.";
  public const string TransferCannotBeCancelled = "Only pending transfers can be cancelled.";
}
