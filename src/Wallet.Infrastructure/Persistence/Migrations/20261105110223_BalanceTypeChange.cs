using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wallet.Infrastructure.Persistence.Migrations
{
  /// <inheritdoc />
  public partial class BalanceTypeChange : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropIndex(
          name: "IX_Wallets_UserId_CurrencyId",
          table: "Wallets");

      migrationBuilder.CreateIndex(
          name: "IX_Wallets_UserId_CurrencyId",
          table: "Wallets",
          columns: new[] { "UserId", "CurrencyId" },
          unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropIndex(
          name: "IX_Wallets_UserId_CurrencyId",
          table: "Wallets");
    }
  }
}
