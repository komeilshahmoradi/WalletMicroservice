using FluentValidation;

namespace Wallet.Application.Wallets.Withdraw;

public sealed class WithdrawCommandValidator : AbstractValidator<WithdrawCommand>
{
  public WithdrawCommandValidator()
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
