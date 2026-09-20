using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Documents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DocumentId",
                table: "AIJobs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "AIJobs",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql("""
        UPDATE "Documents"
        SET "UserId" = (
            SELECT "Id"
            FROM "Users"
            WHERE LOWER(TRIM("Email")) = LOWER(TRIM('admin@smartAIdashboard.com'))
            LIMIT 1
        )
        WHERE "UserId" IS NULL;
        """);

            migrationBuilder.Sql("""
        UPDATE "AIJobs"
        SET "UserId" = (
            SELECT "Id"
            FROM "Users"
            WHERE LOWER(TRIM("Email")) = LOWER(TRIM('admin@smartAIdashboard.com'))
            LIMIT 1
        )
        WHERE "UserId" IS NULL;
        """);

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Documents",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "AIJobs",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_UserId",
                table: "Documents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AIJobs_UserId_CreatedAt",
                table: "AIJobs",
                columns: new[] { "UserId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Documents_UserId",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_AIJobs_UserId_CreatedAt",
                table: "AIJobs");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "DocumentId",
                table: "AIJobs");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "AIJobs");
        }
    }
}
