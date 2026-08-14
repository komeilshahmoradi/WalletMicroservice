using KO.BuildingBlocks.Domain.Pagination;
using KO.BuildingBlocks.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq.Expressions;
using Wallet.Application.ExternalContractors;
using Wallet.Application.Transfers;
using Wallet.Application.Transfers.Create;
using Wallet.Domain.Shared;
using Wallet.Domain.Transfers;
using Wallet.Domain.Transfers.Repositories;
using Wallet.Domain.Wallets;
using Wallet.Domain.Wallets.Repositories;
using DomainWallet = Wallet.Domain.Wallets.Wallet;

namespace Wallet.UnitTests;

public sealed class CreateTransferCommandHandlerIdempotencyTests
{
  [Fact]
  public async Task Handle_WhenMatchingTransferExists_ReturnsOriginalTransferId()
  {
    var scenario = CreateScenario();
    var transferRepository = new FakeTransferRepository(scenario.ExistingTransfer);
    var unitOfWork = new FakeUnitOfWork();
    var handler = CreateHandler(scenario, transferRepository, unitOfWork);

    var result = await handler.Handle(scenario.Command, CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Equal(scenario.ExistingTransfer.Id, result.Value);
    Assert.Equal(0, unitOfWork.BeginCount);
  }

  [Fact]
  public async Task Handle_WhenIdempotencyKeyIsReusedWithDifferentPayload_ReturnsConflict()
  {
    var scenario = CreateScenario();
    var transferRepository = new FakeTransferRepository(scenario.ExistingTransfer);
    var unitOfWork = new FakeUnitOfWork();
    var handler = CreateHandler(scenario, transferRepository, unitOfWork);
    var changedRequest = scenario.Command with { Description = "Different description" };

    var result = await handler.Handle(changedRequest, CancellationToken.None);

    Assert.False(result.IsSuccess);
    Assert.Equal(TransfersErrors.IdempotencyKeyAlreadyUsed.Message, result.Errors.Single());
    Assert.Equal(0, unitOfWork.BeginCount);
  }

  [Fact]
  public async Task Handle_WhenConcurrencyRaceHasCommittedWinner_ReturnsWinnerResult()
  {
    var scenario = CreateScenario();
    var transferRepository = new FakeTransferRepository(null, scenario.ExistingTransfer);
    var unitOfWork = new FakeUnitOfWork(new DbUpdateConcurrencyException());
    var handler = CreateHandler(scenario, transferRepository, unitOfWork);

    var result = await handler.Handle(scenario.Command, CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Equal(scenario.ExistingTransfer.Id, result.Value);
    Assert.Equal(1, unitOfWork.RollbackCount);
  }

  [Fact]
  public async Task Handle_WhenConcurrencyConflictHasNoCommittedWinner_ReturnsConcurrencyConflict()
  {
    var scenario = CreateScenario();
    var transferRepository = new FakeTransferRepository(null, null);
    var unitOfWork = new FakeUnitOfWork(new DbUpdateConcurrencyException());
    var handler = CreateHandler(scenario, transferRepository, unitOfWork);

    var result = await handler.Handle(scenario.Command, CancellationToken.None);

    Assert.False(result.IsSuccess);
    Assert.Equal(TransfersErrors.ConcurrencyConflict.Message, result.Errors.Single());
    Assert.Equal(1, unitOfWork.RollbackCount);
  }

  [Fact]
  public async Task ResolveAfterIdempotencyConflict_WhenKeyBelongsToAnotherOperation_ReturnsKeyAlreadyUsed()
  {
    var scenario = CreateScenario();
    var existingDeposit = scenario.SourceWallet.Transactions
      .Single(transaction => transaction.Type == WalletTransactionType.Deposit);
    var transferRepository = new FakeTransferRepository((Transfer?)null);
    var handler = CreateHandler(
      scenario,
      transferRepository,
      new FakeUnitOfWork(),
      existingDeposit);

    var result = await handler.ResolveAfterIdempotencyConflictAsync(
      scenario.Command,
      CancellationToken.None);

    Assert.False(result.IsSuccess);
    Assert.Equal(TransfersErrors.IdempotencyKeyAlreadyUsed.Message, result.Errors.Single());
  }

  private static CreateTransferCommandHandler CreateHandler(
    Scenario scenario,
    ITransferRepository transferRepository,
    IUnitOfWork unitOfWork,
    WalletTransaction? existingWalletTransaction = null)
  {
    return new CreateTransferCommandHandler(
      new FakeWalletRepository(
        existingWalletTransaction,
        scenario.SourceWallet,
        scenario.DestinationWallet),
      transferRepository,
      unitOfWork,
      new FakeExchangeRateProvider());
  }

  private static Scenario CreateScenario()
  {
    var userId = Guid.CreateVersion7();
    var idempotencyKey = Guid.CreateVersion7();
    var sourceWallet = DomainWallet.Create(userId, Currency.USD);
    var destinationWallet = DomainWallet.Create(Guid.CreateVersion7(), Currency.Rial);

    sourceWallet.Deposit(
      Money.Of(1_000m, Currency.USD),
      idempotencyKey);

    var command = new CreateTransferCommand(
      userId,
      sourceWallet.Id,
      destinationWallet.Id,
      10m,
      Currency.USD.Name,
      "Invoice 42",
      idempotencyKey);

    var existingTransfer = Transfer.Initiate(
      userId,
      sourceWallet.Id,
      destinationWallet.Id,
      Money.Of(command.Amount, Currency.USD),
      100m,
      command.Description);

    existingTransfer.Complete(sourceWallet, destinationWallet, idempotencyKey);

    return new Scenario(command, existingTransfer, sourceWallet, destinationWallet);
  }

  private sealed record Scenario(
    CreateTransferCommand Command,
    Transfer ExistingTransfer,
    DomainWallet SourceWallet,
    DomainWallet DestinationWallet);

  private sealed class FakeExchangeRateProvider : IExchangeRateProvider
  {
    public ValueTask<decimal> GetExchangeRateAsync(
      Currency sourceCurrency,
      Currency destinationCurrency,
      CancellationToken cancellationToken = default)
    {
      return ValueTask.FromResult(100m);
    }
  }

  private sealed class FakeUnitOfWork : IUnitOfWork
  {
    private readonly Exception? _commitException;

    public FakeUnitOfWork(Exception? commitException = null)
    {
      _commitException = commitException;
    }

    public int BeginCount { get; private set; }
    public int RollbackCount { get; private set; }

    public Task BeginTransactionAsync(
      IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
      CancellationToken cancellationToken = default)
    {
      BeginCount++;
      return Task.CompletedTask;
    }

    public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
      return _commitException is null
        ? Task.CompletedTask
        : Task.FromException(_commitException);
    }

    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
      RollbackCount++;
      return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
      return Task.FromResult(0);
    }
  }

  private sealed class FakeTransferRepository : ITransferRepository
  {
    private readonly Queue<Transfer?> _lookupResults;

    public FakeTransferRepository(params Transfer?[] lookupResults)
    {
      _lookupResults = new Queue<Transfer?>(lookupResults);
    }

    public Task<Transfer?> GetByIdempotencyKeyAsync(
      Guid sourceWalletId,
      Guid userId,
      Guid idempotencyKey,
      CancellationToken cancellationToken = default)
    {
      var result = _lookupResults.Count == 0 ? null : _lookupResults.Dequeue();
      return Task.FromResult(result);
    }

    public Task AddAsync(Transfer entity, CancellationToken cancellationToken = default)
    {
      return Task.CompletedTask;
    }

    public Task<Transfer?> GetByIdAndUserIdAsync(
      Guid id,
      Guid userId,
      CancellationToken cancellationToken = default)
    {
      throw new NotSupportedException();
    }

    public Task<Transfer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
      throw new NotSupportedException();
    }

    public Task<IReadOnlyList<Transfer>> ListAsync(CancellationToken cancellationToken = default)
    {
      throw new NotSupportedException();
    }

    public Task<PagedResult<Transfer>> PagedListAsync(
      Expression<Func<Transfer, bool>> expersion,
      int pageNumber,
      int pageSize,
      CancellationToken cancellationToken = default)
    {
      throw new NotSupportedException();
    }

    public void Remove(Transfer entity)
    {
      throw new NotSupportedException();
    }

    public void Update(Transfer entity)
    {
      throw new NotSupportedException();
    }
  }

  private sealed class FakeWalletRepository : IWalletRepository
  {
    private readonly IReadOnlyDictionary<Guid, DomainWallet> _wallets;
    private readonly WalletTransaction? _existingWalletTransaction;

    public FakeWalletRepository(
      WalletTransaction? existingWalletTransaction,
      params DomainWallet[] wallets)
    {
      _existingWalletTransaction = existingWalletTransaction;
      _wallets = wallets.ToDictionary(wallet => wallet.Id);
    }

    public Task<DomainWallet?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
      _wallets.TryGetValue(id, out var wallet);
      return Task.FromResult(wallet);
    }

    public Task<Currency> GetCurrencyById(Guid id, CancellationToken cancellationToken = default)
    {
      return Task.FromResult(_wallets[id].Currency);
    }

    public Task<WalletTransaction?> GetTransactionByWalletIdAndOperationIdAsync(
      Guid walletId,
      Guid idempotencyKey,
      CancellationToken cancellationToken = default)
    {
      var result = _existingWalletTransaction is not null &&
                   _existingWalletTransaction.WalletId == walletId &&
                   _existingWalletTransaction.OperationId == idempotencyKey
        ? _existingWalletTransaction
        : null;

      return Task.FromResult(result);
    }

    public Task AddAsync(DomainWallet entity, CancellationToken cancellationToken = default)
    {
      throw new NotSupportedException();
    }

    public Task<DomainWallet?> GetByIdAndUserIdAsync(
      Guid id,
      Guid userId,
      CancellationToken cancellationToken = default)
    {
      throw new NotSupportedException();
    }

    public Task<DomainWallet?> GetByUserIdAndCurrencyAsync(
      Guid userId,
      string currency,
      CancellationToken cancellationToken = default)
    {
      throw new NotSupportedException();
    }

    public Task<IReadOnlyList<DomainWallet>> ListAsync(CancellationToken cancellationToken = default)
    {
      throw new NotSupportedException();
    }

    public Task<PagedResult<DomainWallet>> PagedListAsync(
      Expression<Func<DomainWallet, bool>> expersion,
      int pageNumber,
      int pageSize,
      CancellationToken cancellationToken = default)
    {
      throw new NotSupportedException();
    }

    public void Remove(DomainWallet entity)
    {
      throw new NotSupportedException();
    }

    public void Update(DomainWallet entity)
    {
      throw new NotSupportedException();
    }
  }
}
