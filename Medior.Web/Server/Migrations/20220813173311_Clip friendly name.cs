using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medior.Web.Server.Migrations;

public partial class Clipfriendlyname : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "FriendlyName",
            table: "ClipboardSaves",
            type: "TEXT",
            nullable: false,
            defaultValue: "");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "FriendlyName",
            table: "ClipboardSaves");
    }
}
