using KO.BuildingBlocks.Application.Abstraction.Messaging;
using KO.BuildingBlocks.Domain.Exceptions;
using KO.BuildingBlocks.Domain.Repositories;
using KO.BuildingBlocks.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Wallet.Application.Helper;
using Wallet.Domain.Shared;
using Wallet.Domain.Wallets;
using Wallet.Domain.Wallets.Repositories;

namespace Wallet.Application.Wallets.Deposit;

public sealed class DepositCommandHandler : ICommandHandler<DepositCommand, Guid>
{
  private readonly IWalletRepository _walletRepository;
  private readonly IUnitOfWork _unitOfWork;

  public DepositCommandHandler(
      IWalletRepository walletRepository,
      IUnitOfWork unitOfWork)
  {
    _walletRepository = walletRepository;
    _unitOfWork = unitOfWork;
  }

  public async Task<Result<Guid>> Handle(
    DepositCommand request,
    CancellationToken cancellationToken)
  {
    var wallet = await _walletRepository.GetByIdAndUserIdAsync(request.WalletId, request.UserId, cancellationToken);
    if (wallet is null)
    {
      return Result.Failure<Guid>(WalletsErrors.NotFound);
    }

    var existingWalletTransaction = await _walletRepository
      .GetTransactionByWalletIdAndOperationIdAsync(request.WalletId, request.IdempotencyKey, cancellationToken);

    if (existingWalletTransaction is not null)
    {
      return GetWalletTransactionStatus(existingWalletTransaction, request);
    }

    var currency = Currency.FromName(request.CurrencyCode);
    var money = Money.Of(request.Amount, currency);

    try
    {
      wallet.Deposit(money, request.IdempotencyKey, request.Description);

      var createdTransaction = wallet.Transactions
          .Last(t => t.OperationId == request.IdempotencyKey);

      await _unitOfWork.SaveChangesAsync(cancellationToken);

      return Result.Success(createdTransaction.Id);
    }
    catch (DomainException ex)
    {
      return Result.Failure<Guid>(ex);
    }
    catch (DbUpdateConcurrencyException)
    {
      return Result.Failure<Guid>(WalletsErrors.ConcurrencyConflict);
    }
    catch (DbUpdateException ex) when (DatabaseExceptionHelper.IsUniqueViolation(ex, "uq_wallet_transactions_wallet_id_operation_id"))
    {
      existingWalletTransaction = await _walletRepository
        .GetTransactionByWalletIdAndOperationIdAsync(request.WalletId, request.IdempotencyKey, cancellationToken);

      if (existingWalletTransaction is null)
      {
        return Result.Failure<Guid>(WalletsErrors.IdempotencyStateNotFound);
      }

      return GetWalletTransactionStatus(existingWalletTransaction, request);
    }
  }

  private Result<Guid> GetWalletTransactionStatus(WalletTransaction existingWalletTransaction, DepositCommand request)
  {
    var currency = Currency.FromName(request.CurrencyCode);
    var money = Money.Of(request.Amount, currency);

    if (existingWalletTransaction.WalletId != request.WalletId
        || existingWalletTransaction.OperationId != request.IdempotencyKey
        || existingWalletTransaction.Type != WalletTransactionType.Deposit
        || existingWalletTransaction.Money != money)
    {
      return Result.Failure<Guid>(WalletsErrors.IdempotencyKeyAlreadyUsed);
    }

    return existingWalletTransaction.Status switch
    {
      var status when status.Equals(WalletTransactionStatus.Completed) =>
          Result.Success(existingWalletTransaction.Id),

      var status when status.Equals(WalletTransactionStatus.Pending) =>
          Result.Failure<Guid>(WalletsErrors.AlreadyDepositProcessing),

      var status when status.Equals(WalletTransactionStatus.Failed) =>
          Result.Failure<Guid>(WalletsErrors.AlreadyDepositFailed),

      var status when status.Equals(WalletTransactionStatus.Reversed) =>
          Result.Failure<Guid>(WalletsErrors.AlreadyDepositReversed),

      _ =>
          Result.Failure<Guid>(WalletsErrors.InvalidStatus)
    };
  }
}
