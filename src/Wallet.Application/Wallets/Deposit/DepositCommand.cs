using KO.BuildingBlocks.Application.Abstraction.Messaging;

namespace Wallet.Application.Wallets.Deposit;

public sealed record DepositCommand(
    Guid UserId,
    Guid WalletId,
    decimal Amount,
    string? Description,
    Guid IdempotencyKey) : ICommand<Guid>;
