using KO.BuildingBlocks.Domain.Exceptions;

namespace Wallet.Domain.Exceptions.TransferDomainException;

public class SourceIdAndDesinitionIdAreSameValueDomainException : DomainException
{
  public SourceIdAndDesinitionIdAreSameValueDomainException()
  {
  }

  public SourceIdAndDesinitionIdAreSameValueDomainException(string message) : base(message)
  {
  }
}
