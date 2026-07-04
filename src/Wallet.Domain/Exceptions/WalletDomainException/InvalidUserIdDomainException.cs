using KO.BuildingBlocks.Domain.Exceptions;

namespace Wallet.Domain.Exceptions.WalletDomainException;

public class InvalidUserIdDomainException : DomainException
{
  public InvalidUserIdDomainException()
  {
  }

  public InvalidUserIdDomainException(string message) : base(message)
  {
  }
}
