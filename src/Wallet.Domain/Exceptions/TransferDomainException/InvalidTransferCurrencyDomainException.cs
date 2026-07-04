using KO.BuildingBlocks.Domain.Exceptions;

namespace Wallet.Domain.Exceptions.TransferDomainException;

public sealed class InvalidTransferCurrencyDomainException : DomainException
{
  public InvalidTransferCurrencyDomainException()
  {
  }

  public InvalidTransferCurrencyDomainException(string message) : base(message)
  {
  }
}
