using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

public partial class AddOwnership : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>("UserId", "Documents", nullable: true);
        migrationBuilder.AddColumn<Guid>("UserId", "AIJobs", nullable: true);
        migrationBuilder.AddColumn<Guid>("DocumentId", "AIJobs", nullable: true);

        migrationBuilder.Sql("UPDATE \"Documents\" SET \"UserId\" = (SELECT \"Id\" FROM \"Users\" ORDER BY \"CreatedAt\" LIMIT 1) WHERE \"UserId\" IS NULL");
        migrationBuilder.Sql("UPDATE \"AIJobs\" SET \"UserId\" = (SELECT \"Id\" FROM \"Users\" ORDER BY \"CreatedAt\" LIMIT 1) WHERE \"UserId\" IS NULL");

        migrationBuilder.AlterColumn<Guid>("UserId", "Documents", nullable: false, oldClrType: typeof(Guid), oldNullable: true);
        migrationBuilder.AlterColumn<Guid>("UserId", "AIJobs", nullable: false, oldClrType: typeof(Guid), oldNullable: true);

        migrationBuilder.CreateIndex("IX_Documents_UserId", "Documents", "UserId");
        migrationBuilder.CreateIndex("IX_AIJobs_UserId_CreatedAt", "AIJobs", new[] { "UserId", "CreatedAt" });
        migrationBuilder.CreateIndex("IX_AIJobs_DocumentId", "AIJobs", "DocumentId");

        migrationBuilder.AddForeignKey("FK_Documents_Users_UserId", "Documents", "UserId", "Users", "Id", onDelete: ReferentialAction.Cascade);
        migrationBuilder.AddForeignKey("FK_AIJobs_Users_UserId", "AIJobs", "UserId", "Users", "Id", onDelete: ReferentialAction.Cascade);
        migrationBuilder.AddForeignKey("FK_AIJobs_Documents_DocumentId", "AIJobs", "DocumentId", "Documents", "Id", onDelete: ReferentialAction.SetNull);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey("FK_AIJobs_Documents_DocumentId", "AIJobs");
        migrationBuilder.DropForeignKey("FK_AIJobs_Users_UserId", "AIJobs");
        migrationBuilder.DropForeignKey("FK_Documents_Users_UserId", "Documents");
        migrationBuilder.DropIndex("IX_AIJobs_DocumentId", "AIJobs");
        migrationBuilder.DropIndex("IX_AIJobs_UserId_CreatedAt", "AIJobs");
        migrationBuilder.DropIndex("IX_Documents_UserId", "Documents");
        migrationBuilder.DropColumn("DocumentId", "AIJobs");
        migrationBuilder.DropColumn("UserId", "AIJobs");
        migrationBuilder.DropColumn("UserId", "Documents");
    }
}