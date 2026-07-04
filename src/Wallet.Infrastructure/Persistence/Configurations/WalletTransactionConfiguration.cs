using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wallet.Domain.Shared;
using Wallet.Domain.Wallets;

namespace Wallet.Infrastructure.Persistence.Configurations;

internal sealed class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
{
  public void Configure(EntityTypeBuilder<WalletTransaction> builder)
  {
    builder.ToTable("WalletTransactions");

    builder.HasKey(transaction => transaction.Id)
      .HasName("pk_wallet_transaction");

    builder.Property(transaction => transaction.Id)
      .HasColumnType("binary(16)") //for using UUIDV7 with sorting.
      .ValueGeneratedNever();

    builder.Property(transaction => transaction.TransferId)
      .HasColumnType("binary(16)")
      .IsRequired();

    builder.HasIndex(x => new { x.WalletId, x.OperationId })
      .IsUnique()
      .HasDatabaseName("uq_wallet_transactions_wallet_id_operation_id");

    builder.Property(transaction => transaction.WalletId)
      .IsRequired();

    builder.OwnsOne(transaction => transaction.Money, moneyBuilder =>
    {
      moneyBuilder.Property(money => money.Amount)
          .HasColumnName("Amount")
          .HasColumnType("decimal(18,2)")
          .IsRequired();

      moneyBuilder.Property(money => money.Currency)
          .HasConversion(
              currency => currency.Id,
              id => Currency.FromValue(id))
          .HasColumnName("CurrencyId")
          .IsRequired();
    });

    builder.Property(transaction => transaction.Type)
        .HasConversion(
            type => type.Id,
            id => WalletTransactionType.FromValue(id))
        .HasColumnName("TypeId")
        .IsRequired();

    builder.Property(transaction => transaction.Status)
        .HasConversion(
            status => status.Id,
            id => WalletTransactionStatus.FromValue(id))
        .HasColumnName("StatusId")
        .IsRequired();

    builder.Property(transaction => transaction.Description)
        .HasMaxLength(500)
        .IsRequired(false);

    builder.Property(transaction => transaction.TransferId)
        .IsRequired(false);

    builder.Property(transaction => transaction.CreatedOnUtc)
        .IsRequired();

    builder.HasIndex(transaction => transaction.WalletId);

    builder.HasIndex(transaction => transaction.TransferId);

    builder.HasIndex(transaction => transaction.CreatedOnUtc);
  }
}