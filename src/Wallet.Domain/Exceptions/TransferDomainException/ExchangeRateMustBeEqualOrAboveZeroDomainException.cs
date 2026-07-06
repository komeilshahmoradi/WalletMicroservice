using KO.BuildingBlocks.Domain.Exceptions;

namespace Wallet.Domain.Exceptions.TransferDomainException;

public class ExchangeRateMustBeEqualOrAboveZeroDomainException : DomainException
{
  public ExchangeRateMustBeEqualOrAboveZeroDomainException()
  {
  }

  public ExchangeRateMustBeEqualOrAboveZeroDomainException(string message) : base(message)
  {
  }
}
