using KO.BuildingBlocks.Domain.Repositories;

namespace Wallet.Domain.Transfers.Repositories;

public interface ITransferRepository : IRepository<Transfer, Guid>
{
  Task<Transfer?> GetByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
}
