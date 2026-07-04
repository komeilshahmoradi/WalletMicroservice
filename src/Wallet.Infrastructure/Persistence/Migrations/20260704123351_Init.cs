using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wallet.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Transfers",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceWalletId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    DestinationWalletId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CompletedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    FailedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transfers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Wallets",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_wallets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WalletTransactions",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    WalletId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    OperationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    TypeId = table.Column<int>(type: "int", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TransferId = table.Column<byte[]>(type: "binary(16)", nullable: true),
                    CreatedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_wallet_transaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WalletTransactions_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "Wallets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_CreatedOnUtc",
                table: "Transfers",
                column: "CreatedOnUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_DestinationWalletId",
                table: "Transfers",
                column: "DestinationWalletId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_SourceWalletId",
                table: "Transfers",
                column: "SourceWalletId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_StatusId",
                table: "Transfers",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_UserId",
                table: "Transfers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_UserId",
                table: "Wallets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_UserId_CurrencyId",
                table: "Wallets",
                columns: new[] { "UserId", "CurrencyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_CreatedOnUtc",
                table: "WalletTransactions",
                column: "CreatedOnUtc");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_TransferId",
                table: "WalletTransactions",
                column: "TransferId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_WalletId",
                table: "WalletTransactions",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "uq_wallet_transactions_wallet_id_operation_id",
                table: "WalletTransactions",
                columns: new[] { "WalletId", "OperationId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transfers");

            migrationBuilder.DropTable(
                name: "WalletTransactions");

            migrationBuilder.DropTable(
                name: "Wallets");
        }
    }
}
