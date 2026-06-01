using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Venue.TimeCafe.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlannedMinutesAndGuestsToVisit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GuestsCount",
                table: "Visits",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "PlannedMinutes",
                table: "Visits",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GuestsCount",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "PlannedMinutes",
                table: "Visits");
        }
    }
}
