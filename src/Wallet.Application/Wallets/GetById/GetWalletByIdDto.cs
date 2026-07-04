namespace Wallet.Application.Wallets.GetById;

public sealed record GetWalletByIdDto
{
  public Guid Id { get; init; }
  public Guid UserId { get; init; }
  public string Currency { get; init; } = default!;
  public decimal Balance { get; init; }
  public bool IsActive { get; init; }
}
