using KO.BuildingBlocks.Domain.Enumerations;

namespace Wallet.Domain.Transfers;

public sealed class TransferStatus : Enumeration
{
  public static readonly TransferStatus Pending = new(1, nameof(Pending));
  public static readonly TransferStatus Completed = new(2, nameof(Completed));
  public static readonly TransferStatus Failed = new(3, nameof(Failed));
  public static readonly TransferStatus Cancelled = new(4, nameof(Cancelled));

  private TransferStatus(int id, string name)
      : base(id, name)
  {
  }

  public static TransferStatus FromValue(int value)
  {
    return FromValue<TransferStatus>(value);
  }

  public static TransferStatus FromName(string name)
  {
    if (string.IsNullOrWhiteSpace(name))
      throw new ArgumentException("TransferStatus name cannot be empty.", nameof(name));

    return FromName<TransferStatus>(name.ToUpperInvariant());
  }
}
