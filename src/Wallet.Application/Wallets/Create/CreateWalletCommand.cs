using KO.BuildingBlocks.Application.Abstraction.Messaging;

namespace Wallet.Application.Wallets.Create;

public sealed record CreateWalletCommand(
    Guid UserId,
    string CurrencyCode) : ICommand<Guid>;
