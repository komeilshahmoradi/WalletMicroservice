using KO.BuildingBlocks.Domain.Results;

namespace Wallet.Application.Wallets;

public static class WalletsErrors
{
  public static readonly Error NotFound =
    new("Wallet.NotFound", "Wallet not found.");

  public static readonly Error Inactive =
      new("Wallet.Inactive", "Wallet is not active.");

  public static readonly Error AlreadyDepositProcessing =
      new("Wallet.AlreadyDepositProcessing", "Wallet deposit is being processed.");

  public static readonly Error AlreadyWithdrawProcessing =
    new("Wallet.AlreadyWithdrawProcessing", "Wallet withdraw is being processed.");

  public static readonly Error AlreadyDepositFailed =
      new("Wallet.AlreadyDepositFailed", "Wallet deposit is already Failed.");

  public static readonly Error AlreadyWithdrawFailed =
      new("Wallet.AlreadyWithdrawFailed", "Wallet withdraw is already Failed.");

  public static readonly Error AlreadyDepositReversed =
      new("Wallet.AlreadyDepositReversed", "Wallet deposit is already Reversed");

  public static readonly Error AlreadyWithdrawReversed =
      new("Wallet.AlreadyDepositReversed", "Wallet deposit is already Reversed");

  public static readonly Error InvalidStatus =
    new("Wallet.InvalidStatus", "Invalid wallet status.");

  public static readonly Error CurrencyMismatch =
      new("Wallet.CurrencyMismatch", "Wallet currency does not match.");

  public static readonly Error AlreadyExistsWithCurrency =
    new("Wallet.AlreadyExistsWithCurrency", $"Current user has wallet with the provided currency.");

  public static readonly Error InsufficientBalance =
      new("Wallet.InsufficientBalance", "Wallet has insufficient balance.");

  public static readonly Error ConcurrencyConflict =
      new("Wallet.ConcurrencyConflict", "Wallet was modified by another request. Please retry.");

  public static readonly Error IdempotencyKeyAlreadyUsed =
    new("Transfer.IdempotencyKeyAlreadyUsed", "Idempotency key was already used with different request data.");

  public static readonly Error IdempotencyStateNotFound =
  new("Transfer.IdempotencyStateNotFound", "Idempotency state not found.");
}
