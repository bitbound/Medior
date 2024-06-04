using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medior.Web.Server.Migrations;

public partial class Clipboardsaves : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsEncrypted",
            table: "UploadedFiles",
            type: "INTEGER",
            nullable: false,
            defaultValue: false);

        migrationBuilder.CreateTable(
            name: "ClipboardSaves",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                AccessTokenEdit = table.Column<string>(type: "TEXT", nullable: false),
                AccessTokenView = table.Column<string>(type: "TEXT", nullable: false),
                Content = table.Column<byte[]>(type: "BLOB", nullable: false),
                ContentSize = table.Column<long>(type: "INTEGER", nullable: false),
                ContentType = table.Column<int>(type: "INTEGER", nullable: false),
                CreatedAt = table.Column<string>(type: "TEXT", nullable: false),
                IsEncrypted = table.Column<bool>(type: "INTEGER", nullable: false),
                LastAccessed = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ClipboardSaves", x => x.Id);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ClipboardSaves");

        migrationBuilder.DropColumn(
            name: "IsEncrypted",
            table: "UploadedFiles");
    }
}
