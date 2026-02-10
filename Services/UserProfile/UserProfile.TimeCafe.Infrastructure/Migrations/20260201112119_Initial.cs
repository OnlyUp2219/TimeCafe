using Microsoft.EntityFrameworkCore.Migrations;

using System;

#nullable disable

namespace UserProfile.TimeCafe.Infrastructure.Migrations;

/// <inheritdoc />
public partial class Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Profiles",
            columns: table => new
            {
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                MiddleName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                PhotoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                BirthDate = table.Column<DateOnly>(type: "date", nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                Gender = table.Column<byte>(type: "smallint", nullable: false),
                ProfileStatus = table.Column<byte>(type: "smallint", nullable: false),
                BanReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Profiles", x => x.UserId);
            });

        migrationBuilder.CreateTable(
            name: "AdditionalInfo",
            columns: table => new
            {
                InfoId = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                InfoText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                CreatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AdditionalInfo", x => x.InfoId);
                table.ForeignKey(
                    name: "FK_AdditionalInfo_Profiles_UserId",
                    column: x => x.UserId,
                    principalTable: "Profiles",
                    principalColumn: "UserId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AdditionalInfo_UserId",
            table: "AdditionalInfo",
            column: "UserId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AdditionalInfo");

        migrationBuilder.DropTable(
            name: "Profiles");
    }
}
