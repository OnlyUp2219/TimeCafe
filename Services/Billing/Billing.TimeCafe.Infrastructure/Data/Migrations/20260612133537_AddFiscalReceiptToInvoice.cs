using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Billing.TimeCafe.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFiscalReceiptToInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FiscalReceiptNumber",
                table: "Invoices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FiscalReceiptUrl",
                table: "Invoices",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FiscalReceiptNumber",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "FiscalReceiptUrl",
                table: "Invoices");
        }
    }
}
