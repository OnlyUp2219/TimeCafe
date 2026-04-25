using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Billing.TimeCafe.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVisitBillingSagaAndIdempotency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_Source_SourceId",
                table: "Transactions");

            migrationBuilder.CreateTable(
                name: "VisitBillingSagas",
                columns: table => new
                {
                    VisitId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    FailureReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompensatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitBillingSagas", x => x.VisitId);
                });

            migrationBuilder.CreateIndex(
                name: "UX_Transactions_Source_SourceId_NotNull",
                table: "Transactions",
                columns: new[] { "Source", "SourceId" },
                unique: true,
                filter: "\"SourceId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VisitBillingSagas_Status",
                table: "VisitBillingSagas",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_VisitBillingSagas_UpdatedAt",
                table: "VisitBillingSagas",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_VisitBillingSagas_UserId",
                table: "VisitBillingSagas",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VisitBillingSagas");

            migrationBuilder.DropIndex(
                name: "UX_Transactions_Source_SourceId_NotNull",
                table: "Transactions");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Source_SourceId",
                table: "Transactions",
                columns: new[] { "Source", "SourceId" });
        }
    }
}
