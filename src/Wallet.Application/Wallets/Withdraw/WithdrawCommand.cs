using KO.BuildingBlocks.Application.Abstraction.Messaging;

namespace Wallet.Application.Wallets.Withdraw;

public sealed record WithdrawCommand(
    Guid UserId,
    Guid WalletId,
    decimal Amount,
    string? Description,
    Guid IdempotencyKey) : ICommand<Guid>;
