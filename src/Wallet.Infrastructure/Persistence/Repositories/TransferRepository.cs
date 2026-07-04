
using KO.BuildingBlocks.Infrastructure.Persistence.EF;
using Microsoft.EntityFrameworkCore;
using Wallet.Domain.Transfers;
using Wallet.Domain.Transfers.Repositories;

namespace Wallet.Infrastructure.Persistence.Repositories;

internal sealed class TransferRepository : EfRepository<Transfer, Guid>, ITransferRepository
{
  public TransferRepository(ApplicationDbContext dbContext) : base(dbContext)
  {
  }

  public async Task<Transfer?> GetByIdAndUserIdAsync(
    Guid id,
    Guid userId,
    CancellationToken cancellationToken = default)
  {
    var result = await _dbContext.Set<Transfer>()
      .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == id, cancellationToken);
    return result;
  }
}
