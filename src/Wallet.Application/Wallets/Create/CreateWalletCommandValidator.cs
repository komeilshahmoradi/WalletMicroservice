using FluentValidation;

namespace Wallet.Application.Wallets.Create;

public sealed class CreateWalletCommandValidator : AbstractValidator<CreateWalletCommand>
{
  public CreateWalletCommandValidator()
  {
    RuleFor(x => x.UserId)
        .NotEmpty();

    RuleFor(x => x.CurrencyCode)
        .NotEmpty()
        .MaximumLength(10);
  }
}
