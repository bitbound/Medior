using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medior.Web.Server.Migrations;

public partial class UploadedFileViewEditAccess : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "AccessToken",
            table: "UploadedFiles",
            newName: "AccessTokenView");

        migrationBuilder.AddColumn<string>(
            name: "AccessTokenEdit",
            table: "UploadedFiles",
            type: "TEXT",
            nullable: false,
            defaultValue: "");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "AccessTokenEdit",
            table: "UploadedFiles");

        migrationBuilder.RenameColumn(
            name: "AccessTokenView",
            table: "UploadedFiles",
            newName: "AccessToken");
    }
}
