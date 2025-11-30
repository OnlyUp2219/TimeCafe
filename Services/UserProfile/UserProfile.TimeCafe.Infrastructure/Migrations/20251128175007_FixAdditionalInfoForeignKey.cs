using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserProfile.TimeCafe.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixAdditionalInfoForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdditionalInfo_Profiles_ProfileUserId",
                table: "AdditionalInfo");

            migrationBuilder.DropIndex(
                name: "IX_AdditionalInfo_ProfileUserId",
                table: "AdditionalInfo");

            migrationBuilder.DropColumn(
                name: "ProfileUserId",
                table: "AdditionalInfo");

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalInfo_UserId",
                table: "AdditionalInfo",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AdditionalInfo_Profiles_UserId",
                table: "AdditionalInfo",
                column: "UserId",
                principalTable: "Profiles",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdditionalInfo_Profiles_UserId",
                table: "AdditionalInfo");

            migrationBuilder.DropIndex(
                name: "IX_AdditionalInfo_UserId",
                table: "AdditionalInfo");

            migrationBuilder.AddColumn<string>(
                name: "ProfileUserId",
                table: "AdditionalInfo",
                type: "character varying(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalInfo_ProfileUserId",
                table: "AdditionalInfo",
                column: "ProfileUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AdditionalInfo_Profiles_ProfileUserId",
                table: "AdditionalInfo",
                column: "ProfileUserId",
                principalTable: "Profiles",
                principalColumn: "UserId");
        }
    }
}
