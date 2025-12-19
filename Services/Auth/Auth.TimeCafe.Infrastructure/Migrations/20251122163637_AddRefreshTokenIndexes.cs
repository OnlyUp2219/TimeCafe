using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.TimeCafe.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddRefreshTokenIndexes : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_RefreshTokens_UserId",
            table: "RefreshTokens");

        migrationBuilder.CreateIndex(
            name: "IX_RefreshTokens_Expires",
            table: "RefreshTokens",
            column: "Expires");

        migrationBuilder.CreateIndex(
            name: "IX_RefreshTokens_Token",
            table: "RefreshTokens",
            column: "Token",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_RefreshTokens_UserId_IsRevoked",
            table: "RefreshTokens",
            columns: new[] { "UserId", "IsRevoked" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_RefreshTokens_Expires",
            table: "RefreshTokens");

        migrationBuilder.DropIndex(
            name: "IX_RefreshTokens_Token",
            table: "RefreshTokens");

        migrationBuilder.DropIndex(
            name: "IX_RefreshTokens_UserId_IsRevoked",
            table: "RefreshTokens");

        migrationBuilder.CreateIndex(
            name: "IX_RefreshTokens_UserId",
            table: "RefreshTokens",
            column: "UserId");
    }
}
