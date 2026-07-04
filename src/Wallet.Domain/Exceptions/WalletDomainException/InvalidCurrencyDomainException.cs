using KO.BuildingBlocks.Domain.Exceptions;

namespace Wallet.Domain.Exceptions.WalletDomainException;

public sealed class InvalidCurrencyDomainException : DomainException
{
  public InvalidCurrencyDomainException()
  {
  }

  public InvalidCurrencyDomainException(string message) : base(message)
  {
  }
}
