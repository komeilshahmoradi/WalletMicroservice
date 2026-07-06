using Wallet.Domain.Shared;

namespace Wallet.Application.ExternalContractors;

public interface IExchangeRateProvider
{
  ValueTask<decimal> GetExchangeRateAsync(
    Currency sourceCurrency,
    Currency destinationCurrency,
    CancellationToken cancellationToken = default);
}
