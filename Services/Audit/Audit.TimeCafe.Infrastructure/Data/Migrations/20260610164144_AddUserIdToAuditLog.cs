using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Audit.TimeCafe.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "audit_logs",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_UserId",
                table: "audit_logs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_audit_logs_UserId",
                table: "audit_logs");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "audit_logs");
        }
    }
}
