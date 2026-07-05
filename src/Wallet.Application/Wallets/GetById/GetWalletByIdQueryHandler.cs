using KO.BuildingBlocks.Application.Abstraction.Messaging;
using KO.BuildingBlocks.Domain.Results;
using Wallet.Domain.Wallets.Repositories;

namespace Wallet.Application.Wallets.GetById;

internal sealed class GetWalletByIdQueryHandler : IQueryHandler<GetWalletByIdQuery, GetWalletByIdDto>
{
  private readonly IWalletRepository _walletRepository;

  public GetWalletByIdQueryHandler(IWalletRepository walletRepository)
  {
    _walletRepository = walletRepository;
  }

  public async Task<Result<GetWalletByIdDto>> Handle(GetWalletByIdQuery request, CancellationToken cancellationToken)
  {
    var wallet = await _walletRepository.GetByIdAsync(request.WalletId, cancellationToken);
    if (wallet is null)
    {
      return Result.Failure<GetWalletByIdDto>(WalletsErrors.NotFound);
    }

    return Result.Success(new GetWalletByIdDto
    {
      Id = wallet.Id,
      UserId = wallet.UserId,
      Currency = wallet.Currency.Name,
      Balance = wallet.Balance,
      IsActive = wallet.IsActive
    });
  }
}
