using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Venue.TimeCafe.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExtendedTariffFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "AudienceTags",
                table: "Tariffs",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "CancellationPolicy",
                table: "Tariffs",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "Features",
                table: "Tariffs",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRecommended",
                table: "Tariffs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxGuests",
                table: "Tariffs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinSessionMinutes",
                table: "Tariffs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RoundingRule",
                table: "Tariffs",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "Tariffs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "Tariffs",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AudienceTags",
                table: "Tariffs");

            migrationBuilder.DropColumn(
                name: "CancellationPolicy",
                table: "Tariffs");

            migrationBuilder.DropColumn(
                name: "Features",
                table: "Tariffs");

            migrationBuilder.DropColumn(
                name: "IsRecommended",
                table: "Tariffs");

            migrationBuilder.DropColumn(
                name: "MaxGuests",
                table: "Tariffs");

            migrationBuilder.DropColumn(
                name: "MinSessionMinutes",
                table: "Tariffs");

            migrationBuilder.DropColumn(
                name: "RoundingRule",
                table: "Tariffs");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "Tariffs");

            migrationBuilder.DropColumn(
                name: "Summary",
                table: "Tariffs");
        }
    }
}
