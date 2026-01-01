using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Billing.TimeCafe.Infrastructure.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Balances",
            columns: table => new
            {
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                CurrentBalance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                TotalDeposited = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                TotalSpent = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                Debt = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                LastUpdated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Balances", x => x.UserId);
            });

        migrationBuilder.CreateTable(
            name: "Transactions",
            columns: table => new
            {
                TransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                Type = table.Column<int>(type: "integer", nullable: false),
                Source = table.Column<int>(type: "integer", nullable: false),
                SourceId = table.Column<Guid>(type: "uuid", nullable: true),
                Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                Comment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                BalanceAfter = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Transactions", x => x.TransactionId);
                table.ForeignKey(
                    name: "FK_Transactions_Balances_UserId",
                    column: x => x.UserId,
                    principalTable: "Balances",
                    principalColumn: "UserId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Payments",
            columns: table => new
            {
                PaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                PaymentMethod = table.Column<int>(type: "integer", nullable: false),
                ExternalPaymentId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                TransactionId = table.Column<Guid>(type: "uuid", nullable: true),
                ExternalData = table.Column<string>(type: "jsonb", nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                ErrorMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Payments", x => x.PaymentId);
                table.ForeignKey(
                    name: "FK_Payments_Balances_UserId",
                    column: x => x.UserId,
                    principalTable: "Balances",
                    principalColumn: "UserId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Payments_Transactions_TransactionId",
                    column: x => x.TransactionId,
                    principalTable: "Transactions",
                    principalColumn: "TransactionId",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Balances_Debt",
            table: "Balances",
            column: "Debt",
            filter: "\"Debt\" > 0");

        migrationBuilder.CreateIndex(
            name: "IX_Balances_LastUpdated",
            table: "Balances",
            column: "LastUpdated");

        migrationBuilder.CreateIndex(
            name: "IX_Payments_CreatedAt",
            table: "Payments",
            column: "CreatedAt");

        migrationBuilder.CreateIndex(
            name: "IX_Payments_ExternalPaymentId",
            table: "Payments",
            column: "ExternalPaymentId");

        migrationBuilder.CreateIndex(
            name: "IX_Payments_Status",
            table: "Payments",
            column: "Status");

        migrationBuilder.CreateIndex(
            name: "IX_Payments_TransactionId",
            table: "Payments",
            column: "TransactionId");

        migrationBuilder.CreateIndex(
            name: "IX_Payments_UserId",
            table: "Payments",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_Transactions_CreatedAt",
            table: "Transactions",
            column: "CreatedAt");

        migrationBuilder.CreateIndex(
            name: "IX_Transactions_Source_SourceId",
            table: "Transactions",
            columns: new[] { "Source", "SourceId" });

        migrationBuilder.CreateIndex(
            name: "IX_Transactions_SourceId",
            table: "Transactions",
            column: "SourceId");

        migrationBuilder.CreateIndex(
            name: "IX_Transactions_Type",
            table: "Transactions",
            column: "Type");

        migrationBuilder.CreateIndex(
            name: "IX_Transactions_UserId",
            table: "Transactions",
            column: "UserId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Payments");

        migrationBuilder.DropTable(
            name: "Transactions");

        migrationBuilder.DropTable(
            name: "Balances");
    }
}
