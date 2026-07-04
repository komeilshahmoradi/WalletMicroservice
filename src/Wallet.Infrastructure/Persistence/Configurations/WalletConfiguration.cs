using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wallet.Domain.Shared;

namespace Wallet.Infrastructure.Persistence.Configurations;

internal sealed class WalletConfiguration : IEntityTypeConfiguration<Domain.Wallets.Wallet>
{
  public void Configure(EntityTypeBuilder<Domain.Wallets.Wallet> builder)
  {
    builder.ToTable("Wallets");

    builder.HasKey(wallet => wallet.Id)
        .HasName("pk_wallets");

    builder.Property(wallet => wallet.Id)
        .HasColumnType("binary(16)")
        .ValueGeneratedNever();

    builder.Property(wallet => wallet.UserId)
        .IsRequired();

    builder.HasIndex(wallet => new
    {
      wallet.UserId,
      wallet.Currency
    }).IsUnique();

    builder.Property(wallet => wallet.Currency)
        .HasConversion(
            currency => currency.Id,
            id => Currency.FromValue(id))
        .HasColumnName("CurrencyId")
        .IsRequired();

    builder.Property(wallet => wallet.Balance)
        .HasColumnType("decimal(18,2)")
        .IsRequired();

    builder.Property(wallet => wallet.IsActive)
        .IsRequired();

    builder.HasMany(wallet => wallet.Transactions)
        .WithOne()
        .HasForeignKey(transaction => transaction.WalletId)
        .OnDelete(DeleteBehavior.NoAction);

    builder.Navigation(wallet => wallet.Transactions)
        .HasField("_transactions")
        .UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.Property(p => p.Version)
       .IsRowVersion();

    builder.HasIndex(wallet => wallet.UserId);
  }
}
