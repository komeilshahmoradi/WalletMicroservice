using KO.BuildingBlocks.Domain.Exceptions;

namespace Wallet.Domain.Exceptions.WalletTransactionDomainException;

public sealed class InvalidOperationIdDomainException : DomainException
{
  public InvalidOperationIdDomainException()
  {
  }

  public InvalidOperationIdDomainException(string message) : base(message)
  {
  }
}
