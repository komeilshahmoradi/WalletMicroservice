namespace Wallet.Infrastructure.ExternalServices.ExchangeRate;

public sealed class ExchangeRateOptions
{
  public const string SectionName = "ExternalApis:ExchangeRate";

  public string BaseUrl { get; set; } = string.Empty;
  public string USDApiKey { get; set; } = string.Empty;
  public string EuroApiKey { get; set; } = string.Empty;
  public int RetryCount { get; set; } = 3;
  public int TimeoutSeconds { get; set; } = 5;
}

