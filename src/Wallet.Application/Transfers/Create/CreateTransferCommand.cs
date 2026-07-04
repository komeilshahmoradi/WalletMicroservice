using KO.BuildingBlocks.Application.Abstraction.Messaging;

namespace Wallet.Application.Transfers.Create;

public sealed record CreateTransferCommand(
    Guid UserId,
    Guid SourceWalletId,
    Guid DestinationWalletId,
    decimal Amount,
    string CurrencyCode,
    string? Description,
    Guid IdempotencyKey) : ICommand<Guid>;
