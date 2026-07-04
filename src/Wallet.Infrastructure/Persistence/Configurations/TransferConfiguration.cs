using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wallet.Domain.Shared;
using Wallet.Domain.Transfers;

namespace Wallet.Infrastructure.Persistence.Configurations;

internal sealed class TransferConfiguration : IEntityTypeConfiguration<Transfer>
{
  public void Configure(EntityTypeBuilder<Transfer> builder)
  {
    builder.ToTable("Transfers");

    builder.HasKey(transfer => transfer.Id).HasName("pk_transfers");

    builder.Property(transfer => transfer.Id)
      .HasColumnType("binary(16)")
      .ValueGeneratedNever();

    builder.Property(transfer => transfer.UserId)
    .IsRequired();

    builder.Property(transfer => transfer.SourceWalletId)
        .HasColumnType("binary(16)")
        .IsRequired();

    builder.Property(transfer => transfer.DestinationWalletId)
        .HasColumnType("binary(16)")
        .IsRequired();

    builder.OwnsOne(transfer => transfer.Money, moneyBuilder =>
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

    builder.Property(transfer => transfer.Status)
        .HasConversion(
            status => status.Id,
            id => TransferStatus.FromValue(id))
        .HasColumnName("StatusId")
        .IsRequired();

    builder.Property(transfer => transfer.Description)
        .HasMaxLength(500);

    builder.Property(transfer => transfer.CreatedOnUtc)
        .IsRequired();

    builder.Property(transfer => transfer.CompletedOnUtc)
        .IsRequired(false);

    builder.Property(transfer => transfer.FailedOnUtc)
        .IsRequired(false);

    builder.Property(transfer => transfer.FailureReason)
      .HasMaxLength(1000)
      .IsRequired(false);

    builder.Property(p => p.Version)
      .IsRowVersion();

    builder.HasIndex(transfer => transfer.UserId);

    builder.HasIndex(transfer => transfer.SourceWalletId);

    builder.HasIndex(transfer => transfer.DestinationWalletId);

    builder.HasIndex(transfer => transfer.Status);

    builder.HasIndex(transfer => transfer.CreatedOnUtc);
  }
}
