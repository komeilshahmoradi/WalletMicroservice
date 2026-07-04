using KO.BuildingBlocks.Application.Abstraction.Messaging;

namespace Wallet.Application.Wallets.Withdraw;

public sealed record WithdrawCommand(
    Guid WalletId,
    Guid UserId,
    decimal Amount,
    string CurrencyCode,
    string? Description,
    Guid IdempotencyKey) : ICommand<Guid>;
