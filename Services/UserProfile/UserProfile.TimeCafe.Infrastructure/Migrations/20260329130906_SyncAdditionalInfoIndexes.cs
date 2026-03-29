using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserProfile.TimeCafe.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncAdditionalInfoIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Profiles_CreatedAt",
                table: "Profiles",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalInfo_UserId_CreatedAt",
                table: "AdditionalInfo",
                columns: new[] { "UserId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Profiles_CreatedAt",
                table: "Profiles");

            migrationBuilder.DropIndex(
                name: "IX_AdditionalInfo_UserId_CreatedAt",
                table: "AdditionalInfo");
        }
    }
}
