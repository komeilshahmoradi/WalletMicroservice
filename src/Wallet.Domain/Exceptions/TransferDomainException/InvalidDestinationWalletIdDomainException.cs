using KO.BuildingBlocks.Domain.Exceptions;

namespace Wallet.Domain.Exceptions.TransferDomainException;

public class InvalidDestinationWalletIdDomainException : DomainException
{
  public InvalidDestinationWalletIdDomainException()
  {
  }

  public InvalidDestinationWalletIdDomainException(string message) : base(message)
  {
  }
}
