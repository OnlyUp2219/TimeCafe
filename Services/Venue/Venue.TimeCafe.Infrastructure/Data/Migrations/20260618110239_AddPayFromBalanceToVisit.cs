using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Venue.TimeCafe.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPayFromBalanceToVisit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PayFromBalance",
                table: "Visits",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PayFromBalance",
                table: "Visits");
        }
    }
}
