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

    builder.ComplexProperty(wallet => wallet.Balance, balanceBuilder =>
    {
      balanceBuilder.Property(b => b.Currency)
          .HasConversion(
              currency => currency.Id,
              id => Currency.FromValue(id))
          .HasColumnName("CurrencyId")
          .IsRequired();

      balanceBuilder.Property(b => b.Amount)
          .HasColumnType("decimal(18,2)")
          .HasColumnName("Balance")
          .IsRequired();
    });

    builder.HasIndex(w => w.UserId).HasDatabaseName("IX_Wallet_User_Currency");// Change migration manually after add migration.

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
