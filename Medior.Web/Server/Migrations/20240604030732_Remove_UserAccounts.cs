using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medior.Web.Server.Migrations
{
    /// <inheritdoc />
    public partial class Remove_UserAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClipboardSaves_UserAccounts_UserAccountId",
                table: "ClipboardSaves");

            migrationBuilder.DropForeignKey(
                name: "FK_UploadedFiles_UserAccounts_UserAccountId",
                table: "UploadedFiles");

            migrationBuilder.DropTable(
                name: "UserAccounts");

            migrationBuilder.DropIndex(
                name: "IX_UploadedFiles_UserAccountId",
                table: "UploadedFiles");

            migrationBuilder.DropIndex(
                name: "IX_ClipboardSaves_UserAccountId",
                table: "ClipboardSaves");

            migrationBuilder.DropColumn(
                name: "UserAccountId",
                table: "UploadedFiles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserAccountId",
                table: "UploadedFiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PublicKey = table.Column<string>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccounts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFiles_UserAccountId",
                table: "UploadedFiles",
                column: "UserAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ClipboardSaves_UserAccountId",
                table: "ClipboardSaves",
                column: "UserAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAccounts_Username",
                table: "UserAccounts",
                column: "Username");

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
    }
}
