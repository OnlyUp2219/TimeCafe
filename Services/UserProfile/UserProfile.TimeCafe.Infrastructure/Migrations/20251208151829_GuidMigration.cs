using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserProfile.TimeCafe.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GuidMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop foreign key constraint first to allow column type changes
            migrationBuilder.DropForeignKey(
                name: "FK_AdditionalInfo_Profiles_UserId",
                table: "AdditionalInfo");

            // Convert Profile.UserId from varchar(450) to uuid using explicit cast
            migrationBuilder.Sql(
                @"ALTER TABLE ""Profiles"" 
                  ALTER COLUMN ""UserId"" TYPE uuid USING ""UserId""::uuid;");

            // Convert AdditionalInfo.UserId from varchar(450) to uuid using explicit cast
            migrationBuilder.Sql(
                @"ALTER TABLE ""AdditionalInfo"" 
                  ALTER COLUMN ""UserId"" TYPE uuid USING ""UserId""::uuid;");

            // Convert AdditionalInfo.InfoId from integer to uuid with random UUIDs
            migrationBuilder.Sql(
                @"ALTER TABLE ""AdditionalInfo"" 
                  ALTER COLUMN ""InfoId"" DROP IDENTITY;
                  ALTER TABLE ""AdditionalInfo"" 
                  ALTER COLUMN ""InfoId"" TYPE uuid USING gen_random_uuid();");

            // Recreate foreign key constraint with new types
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
            // Drop foreign key constraint first
            migrationBuilder.DropForeignKey(
                name: "FK_AdditionalInfo_Profiles_UserId",
                table: "AdditionalInfo");

            // Convert Profile.UserId back from uuid to varchar(450)
            migrationBuilder.Sql(
                @"ALTER TABLE ""Profiles"" 
                  ALTER COLUMN ""UserId"" TYPE character varying(450) USING ""UserId""::text;");

            // Convert AdditionalInfo.UserId back from uuid to varchar(450)
            migrationBuilder.Sql(
                @"ALTER TABLE ""AdditionalInfo"" 
                  ALTER COLUMN ""UserId"" TYPE character varying(450) USING ""UserId""::text;");

            // Convert AdditionalInfo.InfoId back to integer
            migrationBuilder.Sql(
                @"ALTER TABLE ""AdditionalInfo"" 
                  ALTER COLUMN ""InfoId"" TYPE integer;");

            // Recreate foreign key constraint with original types
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
