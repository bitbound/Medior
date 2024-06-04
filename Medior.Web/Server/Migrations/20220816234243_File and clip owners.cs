using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medior.Web.Server.Migrations;

public partial class Fileandclipowners : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "IsEncrypted",
            table: "UploadedFiles");

        migrationBuilder.DropColumn(
            name: "IsEncrypted",
            table: "ClipboardSaves");

        migrationBuilder.AddColumn<Guid>(
            name: "UserAccountId",
            table: "UploadedFiles",
            type: "TEXT",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "UserAccountId",
            table: "ClipboardSaves",
            type: "TEXT",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_UploadedFiles_UserAccountId",
            table: "UploadedFiles",
            column: "UserAccountId");

        migrationBuilder.CreateIndex(
            name: "IX_ClipboardSaves_UserAccountId",
            table: "ClipboardSaves",
            column: "UserAccountId");

        migrationBuilder.AddForeignKey(
            name: "FK_ClipboardSaves_UserAccounts_UserAccountId",
            table: "ClipboardSaves",
            column: "UserAccountId",
            principalTable: "UserAccounts",
            principalColumn: "Id");

        migrationBuilder.AddForeignKey(
            name: "FK_UploadedFiles_UserAccounts_UserAccountId",
            table: "UploadedFiles",
            column: "UserAccountId",
            principalTable: "UserAccounts",
            principalColumn: "Id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_ClipboardSaves_UserAccounts_UserAccountId",
            table: "ClipboardSaves");

        migrationBuilder.DropForeignKey(
            name: "FK_UploadedFiles_UserAccounts_UserAccountId",
            table: "UploadedFiles");

        migrationBuilder.DropIndex(
            name: "IX_UploadedFiles_UserAccountId",
            table: "UploadedFiles");

        migrationBuilder.DropIndex(
            name: "IX_ClipboardSaves_UserAccountId",
            table: "ClipboardSaves");

        migrationBuilder.DropColumn(
            name: "UserAccountId",
            table: "UploadedFiles");

        migrationBuilder.DropColumn(
            name: "UserAccountId",
            table: "ClipboardSaves");

        migrationBuilder.AddColumn<bool>(
            name: "IsEncrypted",
            table: "UploadedFiles",
            type: "INTEGER",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsEncrypted",
            table: "ClipboardSaves",
            type: "INTEGER",
            nullable: false,
            defaultValue: false);
    }
}
