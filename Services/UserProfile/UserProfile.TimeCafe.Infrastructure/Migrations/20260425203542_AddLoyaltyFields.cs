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
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN 
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Profiles' AND column_name='PersonalDiscountPercent') THEN
                        ALTER TABLE ""Profiles"" ADD COLUMN ""PersonalDiscountPercent"" numeric NULL;
                    END IF;

                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Profiles' AND column_name='VisitCount') THEN
                        ALTER TABLE ""Profiles"" ADD COLUMN ""VisitCount"" integer NOT NULL DEFAULT 0;
                    END IF;
                END $$;");
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
