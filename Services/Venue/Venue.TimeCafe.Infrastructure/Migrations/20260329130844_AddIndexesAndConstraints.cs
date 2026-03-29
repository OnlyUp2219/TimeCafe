using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Venue.TimeCafe.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesAndConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Visits_UserId",
                table: "Visits");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_Status_EntryTime",
                table: "Visits",
                columns: new[] { "Status", "EntryTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Visits_UserId",
                table: "Visits",
                column: "UserId",
                unique: true,
                filter: "\"Status\" = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_UserId_EntryTime",
                table: "Visits",
                columns: new[] { "UserId", "EntryTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Themes_Name",
                table: "Themes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tariffs_BillingType_IsActive_PricePerMinute",
                table: "Tariffs",
                columns: new[] { "BillingType", "IsActive", "PricePerMinute" });

            migrationBuilder.CreateIndex(
                name: "IX_Tariffs_CreatedAt",
                table: "Tariffs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tariffs_IsActive_Name",
                table: "Tariffs",
                columns: new[] { "IsActive", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_CreatedAt",
                table: "Promotions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_IsActive_CreatedAt",
                table: "Promotions",
                columns: new[] { "IsActive", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_IsActive_ValidFrom_ValidTo_DiscountPercent",
                table: "Promotions",
                columns: new[] { "IsActive", "ValidFrom", "ValidTo", "DiscountPercent" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Visits_Status_EntryTime",
                table: "Visits");

            migrationBuilder.DropIndex(
                name: "IX_Visits_UserId",
                table: "Visits");

            migrationBuilder.DropIndex(
                name: "IX_Visits_UserId_EntryTime",
                table: "Visits");

            migrationBuilder.DropIndex(
                name: "IX_Themes_Name",
                table: "Themes");

            migrationBuilder.DropIndex(
                name: "IX_Tariffs_BillingType_IsActive_PricePerMinute",
                table: "Tariffs");

            migrationBuilder.DropIndex(
                name: "IX_Tariffs_CreatedAt",
                table: "Tariffs");

            migrationBuilder.DropIndex(
                name: "IX_Tariffs_IsActive_Name",
                table: "Tariffs");

            migrationBuilder.DropIndex(
                name: "IX_Promotions_CreatedAt",
                table: "Promotions");

            migrationBuilder.DropIndex(
                name: "IX_Promotions_IsActive_CreatedAt",
                table: "Promotions");

            migrationBuilder.DropIndex(
                name: "IX_Promotions_IsActive_ValidFrom_ValidTo_DiscountPercent",
                table: "Promotions");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_UserId",
                table: "Visits",
                column: "UserId");
        }
    }
}
