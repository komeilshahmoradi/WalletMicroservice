using KO.BuildingBlocks.Domain.Exceptions;

namespace Wallet.Domain.Exceptions.WalletDomainException;

public class CurrencyMismatchDomainException : DomainException
{
  public CurrencyMismatchDomainException()
  {
  }

  public CurrencyMismatchDomainException(string message) : base(message)
  {
  }
}
