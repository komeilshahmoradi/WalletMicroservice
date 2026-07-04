using KO.BuildingBlocks.Domain.Exceptions;

namespace Wallet.Domain.Exceptions.WalletDomainException;

public class InsufficientBalanceDomainException : DomainException
{
  public InsufficientBalanceDomainException(string message) : base(message)
  {
  }
}
