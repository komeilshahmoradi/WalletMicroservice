using KO.BuildingBlocks.Domain.Exceptions;

namespace Wallet.Domain.Exceptions.WalletTransactionDomainException;

public sealed class InvalidWalletIdDomainException : DomainException
{
  public InvalidWalletIdDomainException()
  {
  }

  public InvalidWalletIdDomainException(string message) : base(message)
  {
  }
}
