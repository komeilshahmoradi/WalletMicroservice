using KO.BuildingBlocks.Domain.Exceptions;

namespace Wallet.Domain.Exceptions.TransferDomainException;

public class TransferCannotBeCancelledDomainException : DomainException
{
  public TransferCannotBeCancelledDomainException()
  {
  }

  public TransferCannotBeCancelledDomainException(string message) : base(message)
  {
  }
}
