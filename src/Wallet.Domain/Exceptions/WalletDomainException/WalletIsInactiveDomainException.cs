using KO.BuildingBlocks.Domain.Exceptions;

namespace Wallet.Domain.Exceptions.WalletDomainException;

public class WalletIsInactiveDomainException : DomainException
{
  public WalletIsInactiveDomainException()
  {
  }

  public WalletIsInactiveDomainException(string message) : base(message)
  {
  }
}
