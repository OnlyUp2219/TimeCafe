using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Audit.TimeCafe.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Action = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    UserName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    MachineName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DomainName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Exception = table.Column<string>(type: "text", nullable: true),
                    Duration = table.Column<long>(type: "bigint", nullable: false),
                    old_data = table.Column<string>(type: "jsonb", nullable: true),
                    new_data = table.Column<string>(type: "jsonb", nullable: true),
                    environment_json = table.Column<string>(type: "jsonb", nullable: true),
                    custom_fields_json = table.Column<string>(type: "jsonb", nullable: true),
                    Comments = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_CorrelationId",
                table: "audit_logs",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_CreatedAt",
                table: "audit_logs",
                column: "CreatedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_EventType",
                table: "audit_logs",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_UserName",
                table: "audit_logs",
                column: "UserName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs");
        }
    }
}
