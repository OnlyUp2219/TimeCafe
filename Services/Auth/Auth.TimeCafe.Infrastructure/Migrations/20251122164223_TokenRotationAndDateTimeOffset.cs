using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.TimeCafe.Infrastructure.Migrations;

/// <inheritdoc />
public partial class TokenRotationAndDateTimeOffset : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ReplacedByToken",
            table: "RefreshTokens",
            type: "text",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ReplacedByToken",
            table: "RefreshTokens");
    }
}
