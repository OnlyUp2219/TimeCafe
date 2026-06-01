using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Venue.TimeCafe.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddResourceGroupsAndResources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Visits_UserId",
                table: "Visits");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Visits",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<bool>(
                name: "IsFinishRequested",
                table: "Visits",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ResourceId",
                table: "Visits",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ResourceGroups",
                columns: table => new
                {
                    ResourceGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceGroups", x => x.ResourceGroupId);
                });

            migrationBuilder.CreateTable(
                name: "Resources",
                columns: table => new
                {
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resources", x => x.ResourceId);
                    table.ForeignKey(
                        name: "FK_Resources_ResourceGroups_ResourceGroupId",
                        column: x => x.ResourceGroupId,
                        principalTable: "ResourceGroups",
                        principalColumn: "ResourceGroupId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Visits_ResourceId",
                table: "Visits",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_UserId",
                table: "Visits",
                column: "UserId",
                unique: true,
                filter: "\"Status\" = 3 AND \"UserId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_ResourceGroupId",
                table: "Resources",
                column: "ResourceGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Visits_Resources_ResourceId",
                table: "Visits",
                column: "ResourceId",
                principalTable: "Resources",
                principalColumn: "ResourceId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Visits_Resources_ResourceId",
                table: "Visits");

            migrationBuilder.DropTable(
                name: "Resources");

            migrationBuilder.DropTable(
                name: "ResourceGroups");

            migrationBuilder.DropIndex(
                name: "IX_Visits_ResourceId",
                table: "Visits");

            migrationBuilder.DropIndex(
                name: "IX_Visits_UserId",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "IsFinishRequested",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "ResourceId",
                table: "Visits");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Visits",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Visits_UserId",
                table: "Visits",
                column: "UserId",
                unique: true,
                filter: "\"Status\" = 3");
        }
    }
}
