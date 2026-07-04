using KO.BuildingBlocks.Application.Abstraction.Messaging;

namespace Wallet.Application.Wallets.GetById;

public sealed record GetWalletByIdQuery(Guid WalletId) : IQuery<GetWalletByIdDto>;
