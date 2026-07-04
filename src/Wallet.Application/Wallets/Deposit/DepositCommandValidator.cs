using FluentValidation;

namespace Wallet.Application.Wallets.Deposit;

public sealed class DepositCommandValidator : AbstractValidator<DepositCommand>
{
  public DepositCommandValidator()
  {
    RuleFor(x => x.WalletId)
        .NotEmpty();

    RuleFor(x => x.Amount)
        .GreaterThan(0);

    RuleFor(x => x.CurrencyCode)
        .NotEmpty()
        .MaximumLength(10);
  }
}
