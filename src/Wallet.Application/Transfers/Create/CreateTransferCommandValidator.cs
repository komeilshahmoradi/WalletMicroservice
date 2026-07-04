using FluentValidation;

namespace Wallet.Application.Transfers.Create;

public sealed class CreateTransferCommandValidator : AbstractValidator<CreateTransferCommand>
{
  public CreateTransferCommandValidator()
  {
    RuleFor(x => x.SourceWalletId)
        .NotEmpty();

    RuleFor(x => x.DestinationWalletId)
        .NotEmpty();

    RuleFor(x => x.Amount)
        .GreaterThan(0);

    RuleFor(x => x.CurrencyCode)
        .NotEmpty()
        .MaximumLength(10);

    RuleFor(x => x)
        .Must(x => x.SourceWalletId != x.DestinationWalletId)
        .WithMessage("Source and destination wallets must be different.");
  }
}
