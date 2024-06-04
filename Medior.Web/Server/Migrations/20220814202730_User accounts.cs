using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medior.Web.Server.Migrations;

public partial class Useraccounts : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "UserAccounts",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Username = table.Column<string>(type: "TEXT", nullable: false),
                PublicKey = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserAccounts", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_UserAccounts_Username",
            table: "UserAccounts",
            column: "Username");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "UserAccounts");
    }
}
