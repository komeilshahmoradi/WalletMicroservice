using KO.BuildingBlocks.Application.Abstraction.Messaging;

namespace Wallet.Application.Wallets.Deposit;

public sealed record DepositCommand(
    Guid WalletId,
    Guid UserId,
    decimal Amount,
    string CurrencyCode,
    string? Description,
    Guid IdempotencyKey) : ICommand<Guid>;
