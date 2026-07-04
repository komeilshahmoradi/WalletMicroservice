using KO.BuildingBlocks.Domain.Exceptions;

namespace Wallet.Domain.Exceptions.TransferDomainException;

public class TransferAlreadyCompletedDomainException : DomainException
{
  public TransferAlreadyCompletedDomainException()
  {
  }

  public TransferAlreadyCompletedDomainException(string message) : base(message)
  {
  }
}
