using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Venue.TimeCafe.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFinishRequestedAtToVisit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FinishRequestedAt",
                table: "Visits",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinishRequestedAt",
                table: "Visits");
        }
    }
}
