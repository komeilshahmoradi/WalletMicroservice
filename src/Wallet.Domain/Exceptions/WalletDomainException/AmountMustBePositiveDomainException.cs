using KO.BuildingBlocks.Domain.Exceptions;

namespace Wallet.Domain.Exceptions.WalletDomainException;

public class AmountMustBePositiveDomainException : DomainException
{
  public AmountMustBePositiveDomainException()
  {
  }

  public AmountMustBePositiveDomainException(string message) : base(message)
  {
  }
}
