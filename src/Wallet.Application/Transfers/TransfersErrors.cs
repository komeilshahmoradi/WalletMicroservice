using KO.BuildingBlocks.Domain.Results;

namespace Wallet.Application.Transfers;

public static class TransfersErrors
{
  public static readonly Error IdempotencyKeyAlreadyUsed =
    new("Transfer.IdempotencyKeyAlreadyUsed", "Idempotency key was already used with different request data.");

  public static readonly Error IdempotencyStateNotFound =
      new("Transfer.IdempotencyStateNotFound", "Idempotency state not found.");

  public static readonly Error AlreadyProcessing =
      new("Transfer.AlreadyProcessing", "Transfer is already being processed.");

  public static readonly Error AlreadyFailed =
      new("Transfer.AlreadyFailed", "Transfer has already failed.");

  public static readonly Error InvalidStatus =
      new("Transfer.InvalidStatus", "Invalid transfer status.");

  public static readonly Error Cancelled =
    new("Transfer.AlreadyCancelled", "Transfer has already Cancelled.");

  public static readonly Error SourceWalletNotFound =
      new("Transfer.SourceWalletNotFound", "Source wallet not found.");

  public static readonly Error DestinationWalletNotFound =
      new("Transfer.DestinationWalletNotFound", "Destination wallet not found.");

  public static readonly Error ConcurrencyConflict =
      new("Transfer.ConcurrencyConflict", "Transfer could not be completed because data was modified by another request.");
}
