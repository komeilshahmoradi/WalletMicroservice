using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using Wallet.Application.ExternalContractors;
using Wallet.Domain.Shared;

namespace Wallet.Infrastructure.ExternalServices.ExchangeRate;

internal sealed class ExchangeRateProvider : IExchangeRateProvider
{
  private readonly HttpClient _httpClient;
  private readonly IOptions<ExchangeRateOptions> _option;

  public ExchangeRateProvider(
    IOptions<ExchangeRateOptions> options,
    HttpClient httpClient)
  {
    _option = options;
    _httpClient = httpClient;
  }

  public async ValueTask<decimal> GetExchangeRateAsync(
    Currency sourceCurrency,
    Currency destinationCurrency,
    CancellationToken cancellationToken = default)
  {
    if (sourceCurrency == destinationCurrency)
    {
      return 0;
    }

    var apiKey = string.Empty;
    if (destinationCurrency == Currency.Rial)
    {
      apiKey = GetApiKeyByCurrency(sourceCurrency);
    }
    else
    {
      apiKey = GetApiKeyByCurrency(destinationCurrency);
    }

    using var request = new HttpRequestMessage(HttpMethod.Get, apiKey);

    var response = await _httpClient.SendAsync(request, cancellationToken);

    response.EnsureSuccessStatusCode();

    var exchangeRateResponse = await response.Content.ReadFromJsonAsync<ExchangeRateResponse>(
    cancellationToken: cancellationToken);

    decimal.TryParse(
      exchangeRateResponse?.Response.Indicators.Select(x => x.Price).FirstOrDefault(), out var rate);

    return rate;
  }

  private string GetApiKeyByCurrency(Currency currency)
  {
    return currency switch
    {
      var curr when currency.Equals(Currency.Euro) => _option.Value.EuroApiKey,
      var curr when currency.Equals(Currency.USD) => _option.Value.USDApiKey,
      _ => throw new ArgumentOutOfRangeException(nameof(currency))
    };
  }
}
