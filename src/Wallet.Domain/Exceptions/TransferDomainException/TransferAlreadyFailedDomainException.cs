using KO.BuildingBlocks.Domain.Exceptions;

namespace Wallet.Domain.Exceptions.TransferDomainException;

public class TransferAlreadyFailedDomainException : DomainException
{
  public TransferAlreadyFailedDomainException()
  {
  }

  public TransferAlreadyFailedDomainException(string message) : base(message)
  {
  }
}
