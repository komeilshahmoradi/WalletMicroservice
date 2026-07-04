using KO.BuildingBlocks.Domain.Exceptions;

namespace Wallet.Domain.Exceptions.WalletDomainException;

public class InvalidOperationIdDomainException : DomainException
{
  public InvalidOperationIdDomainException()
  {
  }

  public InvalidOperationIdDomainException(string message) : base(message)
  {
  }
}
