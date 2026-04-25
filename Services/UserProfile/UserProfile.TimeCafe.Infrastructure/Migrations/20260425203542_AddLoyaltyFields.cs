using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserProfile.TimeCafe.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLoyaltyFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PersonalDiscountPercent",
                table: "Profiles",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VisitCount",
                table: "Profiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PersonalDiscountPercent",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "VisitCount",
                table: "Profiles");
        }
    }
}
