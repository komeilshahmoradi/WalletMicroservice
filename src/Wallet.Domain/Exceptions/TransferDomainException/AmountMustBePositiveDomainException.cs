using KO.BuildingBlocks.Domain.Exceptions;

namespace Wallet.Domain.Exceptions.TransferDomainException;

public class AmountMustBePositiveDomainException : DomainException
{
  public AmountMustBePositiveDomainException()
  {
  }

  public AmountMustBePositiveDomainException(string message) : base(message)
  {
  }
}
