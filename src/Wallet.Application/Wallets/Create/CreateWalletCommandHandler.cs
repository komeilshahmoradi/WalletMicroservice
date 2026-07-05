using KO.BuildingBlocks.Application.Abstraction.Messaging;
using KO.BuildingBlocks.Domain.Exceptions;
using KO.BuildingBlocks.Domain.Repositories;
using KO.BuildingBlocks.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Wallet.Application.Helper;
using Wallet.Domain.Shared;
using Wallet.Domain.Wallets.Repositories;

namespace Wallet.Application.Wallets.Create;

internal sealed class CreateWalletCommandHandler : ICommandHandler<CreateWalletCommand, Guid>
{
  private readonly IWalletRepository _walletRepository;
  private readonly IUnitOfWork _unitOfWork;

  public CreateWalletCommandHandler(
      IWalletRepository walletRepository,
      IUnitOfWork unitOfWork)
  {
    _walletRepository = walletRepository;
    _unitOfWork = unitOfWork;
  }

  public async Task<Result<Guid>> Handle(
    CreateWalletCommand request,
    CancellationToken cancellationToken)
  {
    var existingWallet = await _walletRepository
      .GetByUserIdAndCurrencyAsync(request.UserId, request.CurrencyCode, cancellationToken);

    if (existingWallet is not null)
    {
      return Result.Failure<Guid>(WalletsErrors.AlreadyExistsWithCurrency);
    }

    var currency = Currency.FromName(request.CurrencyCode);

    try
    {
      var wallet = Domain.Wallets.Wallet.Create(request.UserId, currency);

      await _walletRepository.AddAsync(wallet, cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);

      return Result.Success(wallet.Id);
    }
    catch (DomainException ex)
    {
      return Result.Failure<Guid>(ex);
    }
    catch (DbUpdateException ex) when (DatabaseExceptionHelper.IsUniqueViolation(ex, "pk_wallets"))
    {
      existingWallet = await _walletRepository
        .GetByUserIdAndCurrencyAsync(request.UserId, request.CurrencyCode, cancellationToken);

      if (existingWallet is null)
      {
        return Result.Failure<Guid>(WalletsErrors.IdempotencyStateNotFound);
      }

      return Result.Success(existingWallet.Id);
    }
  }
}
