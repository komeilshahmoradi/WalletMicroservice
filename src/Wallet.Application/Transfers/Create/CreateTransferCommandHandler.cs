using KO.BuildingBlocks.Application.Abstraction.Messaging;
using KO.BuildingBlocks.Domain.Exceptions;
using KO.BuildingBlocks.Domain.Repositories;
using KO.BuildingBlocks.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Wallet.Application.Helper;
using Wallet.Application.Wallets;
using Wallet.Domain.Shared;
using Wallet.Domain.Transfers;
using Wallet.Domain.Transfers.Repositories;
using Wallet.Domain.Wallets;
using Wallet.Domain.Wallets.Repositories;

namespace Wallet.Application.Transfers.Create;

internal sealed class CreateTransferCommandHandler : ICommandHandler<CreateTransferCommand, Guid>
{
  private readonly IWalletRepository _walletRepository;
  private readonly ITransferRepository _transferRepository;
  private readonly IUnitOfWork _unitOfWork;

  public CreateTransferCommandHandler(
      IWalletRepository walletRepository,
      ITransferRepository transferRepository,
      IUnitOfWork unitOfWork)
  {
    _walletRepository = walletRepository;
    _transferRepository = transferRepository;
    _unitOfWork = unitOfWork;
  }

  public async Task<Result<Guid>> Handle(CreateTransferCommand request, CancellationToken cancellationToken)
  {
    var existingTransfer = await _transferRepository.GetByIdAndUserIdAsync(
        request.IdempotencyKey,
        request.UserId,
        cancellationToken);

    if (existingTransfer is not null)
    {
      return GetTransferStatus(existingTransfer, request);
    }

    await _unitOfWork.BeginTransactionAsync(cancellationToken: cancellationToken);
    try
    {
      var sourceWallet = await _walletRepository.GetByIdAsync(request.SourceWalletId, cancellationToken);
      if (sourceWallet is null)
      {
        return Result.Failure<Guid>(TransfersErrors.SourceWalletNotFound);
      }

      var destinationWallet = await _walletRepository.GetByIdAsync(request.DestinationWalletId, cancellationToken);
      if (destinationWallet is null)
      {
        return Result.Failure<Guid>(TransfersErrors.DestinationWalletNotFound);
      }

      var currency = Currency.FromName(request.CurrencyCode);
      var money = Money.Of(request.Amount, currency);

      var transfer = Transfer.Initiate(
          request.UserId,
          request.SourceWalletId,
          request.DestinationWalletId,
          money,
          request.Description);

      transfer.Complete(sourceWallet, destinationWallet, request.IdempotencyKey);

      await _transferRepository.AddAsync(transfer, cancellationToken);

      await _unitOfWork.CommitTransactionAsync(cancellationToken);

      return Result.Success(transfer.Id);
    }
    catch (DomainException ex)
    {
      await _unitOfWork.RollbackTransactionAsync();
      return Result.Failure<Guid>(ex);
    }
    catch (DbUpdateConcurrencyException)
    {
      await _unitOfWork.RollbackTransactionAsync();
      return Result.Failure<Guid>(TransfersErrors.ConcurrencyConflict);
    }
    catch (DbUpdateException ex) when (DatabaseExceptionHelper.IsUniqueViolation(ex, "pk_transfers"))
    {
      await _unitOfWork.RollbackTransactionAsync();

      existingTransfer = await _transferRepository.GetByIdAndUserIdAsync(
          request.IdempotencyKey,
          request.UserId,
          cancellationToken);

      if (existingTransfer is null)
      {
        return Result.Failure<Guid>(TransfersErrors.IdempotencyStateNotFound);
      }

      return GetTransferStatus(existingTransfer, request);
    }
    catch (DbUpdateException ex) when (DatabaseExceptionHelper.IsUniqueViolation(ex, "uq_wallet_transactions_wallet_id_operation_id"))
    {
      await _unitOfWork.RollbackTransactionAsync();

      var existingSourceTransaction = await _walletRepository
        .GetTransactionByWalletIdAndOperationIdAsync(request.SourceWalletId, request.IdempotencyKey, cancellationToken);

      var existingDestinationTransaction = await _walletRepository
        .GetTransactionByWalletIdAndOperationIdAsync(request.DestinationWalletId, request.IdempotencyKey, cancellationToken);

      if (existingSourceTransaction is null && existingSourceTransaction is null)
      {
        return Result.Failure<Guid>(WalletsErrors.IdempotencyStateNotFound);
      }

      return GetWalletTransactionStatus(existingSourceTransaction, existingDestinationTransaction, request);
    }
    catch (Exception)
    {
      await _unitOfWork.RollbackTransactionAsync();
      throw;
    }
  }

  private Result<Guid> GetWalletTransactionStatus(
    WalletTransaction? sourceWalletTransaction,
    WalletTransaction? destinationWalletTransaction,
    CreateTransferCommand request)
  {
    var currency = Currency.FromName(request.CurrencyCode);
    var money = Money.Of(request.Amount, currency);

    if (sourceWalletTransaction is not null)
    {
      if ((sourceWalletTransaction.WalletId != request.SourceWalletId
        || sourceWalletTransaction.OperationId != request.IdempotencyKey
        || sourceWalletTransaction.Type != WalletTransactionType.Deposit
        || sourceWalletTransaction.Money != money))
      {
        return Result.Failure<Guid>(WalletsErrors.IdempotencyKeyAlreadyUsed);
      }

      return sourceWalletTransaction.Status switch
      {
        var status when status.Equals(WalletTransactionStatus.Completed) =>
            Result.Success(sourceWalletTransaction.Id),

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

    if (destinationWalletTransaction is not null)
    {
      if ((destinationWalletTransaction.WalletId != request.SourceWalletId
        || destinationWalletTransaction.OperationId != request.IdempotencyKey
        || destinationWalletTransaction.Type != WalletTransactionType.Deposit
        || destinationWalletTransaction.Money != money))
      {
        return Result.Failure<Guid>(WalletsErrors.IdempotencyKeyAlreadyUsed);
      }

      return destinationWalletTransaction.Status switch
      {
        var status when status.Equals(WalletTransactionStatus.Completed) =>
            Result.Success(destinationWalletTransaction.Id),

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

    return Result.Failure<Guid>(WalletsErrors.InvalidStatus);
  }

  private Result<Guid> GetTransferStatus(Transfer existingTransfer, CreateTransferCommand request)
  {
    var currency = Currency.FromName(request.CurrencyCode);
    var money = Money.Of(request.Amount, currency);

    if (existingTransfer.SourceWalletId != request.SourceWalletId ||
          existingTransfer.DestinationWalletId != request.DestinationWalletId ||
          existingTransfer.Money != money)
    {
      return Result.Failure<Guid>(TransfersErrors.IdempotencyKeyAlreadyUsed);
    }

    return existingTransfer.Status switch
    {
      var status when status.Equals(TransferStatus.Completed) =>
          Result.Success(existingTransfer.Id),

      var status when status.Equals(TransferStatus.Pending) =>
          Result.Failure<Guid>(TransfersErrors.AlreadyProcessing),

      var status when status.Equals(TransferStatus.Failed) =>
          Result.Failure<Guid>(TransfersErrors.AlreadyFailed),

      var status when status.Equals(TransferStatus.Cancelled) =>
          Result.Failure<Guid>(TransfersErrors.Cancelled),

      _ =>
          Result.Failure<Guid>(TransfersErrors.InvalidStatus)
    };
  }
}
