using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UserProfile.TimeCafe.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GuidMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop foreign key constraint before altering column type
            migrationBuilder.DropForeignKey(
                name: "FK_AdditionalInfo_Profiles_UserId",
                table: "AdditionalInfo");

            // Convert Profile.UserId from varchar(450) to uuid
            migrationBuilder.Sql(
                @"ALTER TABLE ""Profiles"" 
                  ALTER COLUMN ""UserId"" TYPE uuid USING ""UserId""::uuid;");

            // Convert AdditionalInfo.UserId from varchar(450) to uuid
            migrationBuilder.Sql(
                @"ALTER TABLE ""AdditionalInfo"" 
                  ALTER COLUMN ""UserId"" TYPE uuid USING ""UserId""::uuid;");

            // Convert AdditionalInfo.InfoId from integer to uuid
            migrationBuilder.Sql(
                @"ALTER TABLE ""AdditionalInfo"" 
                  ALTER COLUMN ""InfoId"" TYPE uuid USING gen_random_uuid();");

            // Recreate foreign key constraint with proper type
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
            // Drop foreign key constraint before altering column type
            migrationBuilder.DropForeignKey(
                name: "FK_AdditionalInfo_Profiles_UserId",
                table: "AdditionalInfo");

            // Convert Profile.UserId from uuid back to varchar(450)
            migrationBuilder.Sql(
                @"ALTER TABLE ""Profiles"" 
                  ALTER COLUMN ""UserId"" TYPE character varying(450) USING ""UserId""::text;");

            // Convert AdditionalInfo.UserId from uuid back to varchar(450)
            migrationBuilder.Sql(
                @"ALTER TABLE ""AdditionalInfo"" 
                  ALTER COLUMN ""UserId"" TYPE character varying(450) USING ""UserId""::text;");

            // Convert AdditionalInfo.InfoId from uuid back to integer (will lose data)
            migrationBuilder.Sql(
                @"ALTER TABLE ""AdditionalInfo"" 
                  ALTER COLUMN ""InfoId"" TYPE integer USING 1;"); // Default to 1 for all rows

            // Recreate foreign key constraint with original type
            migrationBuilder.AddForeignKey(
                name: "FK_AdditionalInfo_Profiles_UserId",
                table: "AdditionalInfo",
                column: "UserId",
                principalTable: "Profiles",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
