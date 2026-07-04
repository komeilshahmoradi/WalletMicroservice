using KO.BuildingBlocks.Domain.Exceptions;

namespace Wallet.Domain.Exceptions.TransferDomainException;

public class InvalidSourceWalletIdDomainException : DomainException
{
  public InvalidSourceWalletIdDomainException()
  {
  }

  public InvalidSourceWalletIdDomainException(string message) : base(message)
  {
  }
}
