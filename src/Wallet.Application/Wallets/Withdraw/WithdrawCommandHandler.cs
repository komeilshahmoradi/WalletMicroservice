using Ardalis.Result;
using KO.BuildingBlocks.Application.Abstraction.Messaging;
using KO.BuildingBlocks.Domain.Exceptions;
using KO.BuildingBlocks.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Wallet.Application.Helper;
using Wallet.Domain.Shared;
using Wallet.Domain.Wallets;
using Wallet.Domain.Wallets.Repositories;

namespace Wallet.Application.Wallets.Withdraw;

internal sealed class WithdrawCommandHandler : ICommandHandler<WithdrawCommand, Guid>
{
  private readonly IWalletRepository _walletRepository;
  private readonly IUnitOfWork _unitOfWork;

  public WithdrawCommandHandler(
      IWalletRepository walletRepository,
      IUnitOfWork unitOfWork)
  {
    _walletRepository = walletRepository;
    _unitOfWork = unitOfWork;
  }

  public async Task<Result<Guid>> Handle(WithdrawCommand request, CancellationToken cancellationToken)
  {
    var wallet = await _walletRepository.GetByIdAndUserIdAsync(request.WalletId, request.UserId, cancellationToken);
    if (wallet is null)
    {
      return Result.NotFound(WalletsErrors.NotFound.Message);
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
      wallet.Withdraw(money, request.IdempotencyKey, request.Description);

      var createdTransaction = wallet.Transactions
          .Last(t => t.OperationId == request.IdempotencyKey);

      await _unitOfWork.SaveChangesAsync(cancellationToken);
      return Result.Success(createdTransaction.Id);
    }
    catch (DomainException ex)
    {
      return Result.Error(ex.Message);
    }
    catch (DbUpdateConcurrencyException)
    {
      return Result.CriticalError(WalletsErrors.ConcurrencyConflict.Message);
    }
    catch (DbUpdateException ex) when (DatabaseExceptionHelper.IsUniqueViolation(ex, "uq_wallet_transactions_wallet_id_operation_id"))
    {
      existingWalletTransaction = await _walletRepository
        .GetTransactionByWalletIdAndOperationIdAsync(request.WalletId, request.IdempotencyKey, cancellationToken);

      if (existingWalletTransaction is null)
      {
        return Result.NotFound(WalletsErrors.IdempotencyStateNotFound.Message);
      }

      return GetWalletTransactionStatus(existingWalletTransaction, request);
    }
    catch
    {
      throw;
    }
  }

  private Result<Guid> GetWalletTransactionStatus(WalletTransaction existingWalletTransaction, WithdrawCommand request)
  {
    var currency = Currency.FromName(request.CurrencyCode);
    var money = Money.Of(request.Amount, currency);

    if (existingWalletTransaction.WalletId != request.WalletId
        || existingWalletTransaction.OperationId != request.IdempotencyKey
        || existingWalletTransaction.Type != WalletTransactionType.Withdraw
        || existingWalletTransaction.Money != money)
    {
      return Result.Error(WalletsErrors.IdempotencyKeyAlreadyUsed.Message);
    }

    return existingWalletTransaction.Status switch
    {
      var status when status.Equals(WalletTransactionStatus.Completed) =>
          Result.Success(existingWalletTransaction.Id),

      var status when status.Equals(WalletTransactionStatus.Pending) =>
          Result.Error(WalletsErrors.AlreadyWithdrawProcessing.Message),

      var status when status.Equals(WalletTransactionStatus.Failed) =>
          Result.Error(WalletsErrors.AlreadyWithdrawFailed.Message),

      var status when status.Equals(WalletTransactionStatus.Reversed) =>
          Result.Error(WalletsErrors.AlreadyWithdrawReversed.Message),

      _ =>
          Result.Error(WalletsErrors.InvalidStatus.Message)
    };
  }
}
