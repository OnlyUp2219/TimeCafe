using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Billing.TimeCafe.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddPaymentsTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Payments",
            columns: table => new
            {
                PaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                PaymentMethod = table.Column<int>(type: "integer", nullable: false),
                ExternalPaymentId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
            name: "IX_Payments_UserId",
            table: "Payments",
            column: "UserId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Payments");
    }
}
