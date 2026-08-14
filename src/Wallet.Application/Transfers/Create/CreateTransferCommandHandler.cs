using Ardalis.Result;
using KO.BuildingBlocks.Application.Abstraction.Messaging;
using KO.BuildingBlocks.Domain.Exceptions;
using KO.BuildingBlocks.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Wallet.Application.ExternalContractors;
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
  private readonly IExchangeRateProvider _exchangeRateProvider;

  public CreateTransferCommandHandler(
      IWalletRepository walletRepository,
      ITransferRepository transferRepository,
      IUnitOfWork unitOfWork,
      IExchangeRateProvider exchangeRateProvider)
  {
    _walletRepository = walletRepository;
    _transferRepository = transferRepository;
    _unitOfWork = unitOfWork;
    _exchangeRateProvider = exchangeRateProvider;
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

    var destinationCurrency = await _walletRepository.GetCurrencyById(request.DestinationWalletId, cancellationToken);
    var exchangeRate = await _exchangeRateProvider.GetExchangeRateAsync(
      Currency.FromName(request.CurrencyCode), destinationCurrency, cancellationToken);

    await _unitOfWork.BeginTransactionAsync(cancellationToken: cancellationToken);
    try
    {
      var sourceWallet = await _walletRepository.GetByIdAsync(request.SourceWalletId, cancellationToken);
      if (sourceWallet is null)
      {
        return Result<Guid>.NotFound(TransfersErrors.SourceWalletNotFound.Message);
      }

      var destinationWallet = await _walletRepository.GetByIdAsync(request.DestinationWalletId, cancellationToken);
      if (destinationWallet is null)
      {
        return Result<Guid>.NotFound(TransfersErrors.DestinationWalletNotFound.Message);
      }

      var currency = Currency.FromName(request.CurrencyCode);
      var money = Money.Of(request.Amount, currency);

      var transfer = Transfer.Initiate(
          request.UserId,
          request.SourceWalletId,
          request.DestinationWalletId,
          money,
          exchangeRate,
          request.Description);

      transfer.Complete(sourceWallet, destinationWallet, request.IdempotencyKey);

      await _transferRepository.AddAsync(transfer, cancellationToken);

      await _unitOfWork.CommitTransactionAsync(cancellationToken);

      return Result.Success(transfer.Id);
    }
    catch (DomainException ex)
    {
      await _unitOfWork.RollbackTransactionAsync();
      return Result<Guid>.Error(ex.Message);
    }
    catch (DbUpdateConcurrencyException)
    {
      await _unitOfWork.RollbackTransactionAsync();
      return Result.CriticalError(TransfersErrors.ConcurrencyConflict.Message);
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
        return Result.NotFound(TransfersErrors.IdempotencyStateNotFound.Message);
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
        return Result.NotFound(WalletsErrors.IdempotencyStateNotFound.Message);
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
        return Result.Error(WalletsErrors.IdempotencyKeyAlreadyUsed.Message);
      }

      return sourceWalletTransaction.Status switch
      {
        var status when status.Equals(WalletTransactionStatus.Completed) =>
            Result<Guid>.Success(sourceWalletTransaction.Id),

        var status when status.Equals(WalletTransactionStatus.Pending) =>
            Result.Error(WalletsErrors.AlreadyDepositProcessing.Message),

        var status when status.Equals(WalletTransactionStatus.Failed) =>
            Result.Error(WalletsErrors.AlreadyDepositFailed.Message),

        var status when status.Equals(WalletTransactionStatus.Reversed) =>
            Result.Error(WalletsErrors.AlreadyDepositReversed.Message),

        _ =>
            Result.Error(WalletsErrors.InvalidStatus.Message)
      };
    }

    if (destinationWalletTransaction is not null)
    {
      if ((destinationWalletTransaction.WalletId != request.SourceWalletId
        || destinationWalletTransaction.OperationId != request.IdempotencyKey
        || destinationWalletTransaction.Type != WalletTransactionType.Deposit
        || destinationWalletTransaction.Money != money))
      {
        return Result.Error(WalletsErrors.IdempotencyKeyAlreadyUsed.Message);
      }

      return destinationWalletTransaction.Status switch
      {
        var status when status.Equals(WalletTransactionStatus.Completed) =>
            Result.Success(destinationWalletTransaction.Id),

        var status when status.Equals(WalletTransactionStatus.Pending) =>
            Result.Error(WalletsErrors.AlreadyDepositProcessing.Message),

        var status when status.Equals(WalletTransactionStatus.Failed) =>
            Result.Error(WalletsErrors.AlreadyDepositFailed.Message),

        var status when status.Equals(WalletTransactionStatus.Reversed) =>
            Result.Error(WalletsErrors.AlreadyDepositReversed.Message),

        _ =>
            Result.Error(WalletsErrors.InvalidStatus.Message)
      };
    }

    return Result.Error(WalletsErrors.InvalidStatus.Message);
  }

  private Result<Guid> GetTransferStatus(Transfer existingTransfer, CreateTransferCommand request)
  {
    var currency = Currency.FromName(request.CurrencyCode);
    var money = Money.Of(request.Amount, currency);

    if (existingTransfer.SourceWalletId != request.SourceWalletId ||
          existingTransfer.DestinationWalletId != request.DestinationWalletId ||
          existingTransfer.Money != money)
    {
      return Result.Error(TransfersErrors.IdempotencyKeyAlreadyUsed.Message);
    }

    return existingTransfer.Status switch
    {
      var status when status.Equals(TransferStatus.Completed) =>
          Result.Success(existingTransfer.Id),

      var status when status.Equals(TransferStatus.Pending) =>
          Result.Error(TransfersErrors.AlreadyProcessing.Message),

      var status when status.Equals(TransferStatus.Failed) =>
          Result.Error(TransfersErrors.AlreadyFailed.Message),

      var status when status.Equals(TransferStatus.Cancelled) =>
          Result.Error(TransfersErrors.Cancelled.Message),

      _ =>
          Result.Error(TransfersErrors.InvalidStatus.Message)
    };
  }
}
