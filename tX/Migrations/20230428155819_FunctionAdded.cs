using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tX.Migrations
{
    public partial class FunctionAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FunctionName",
                table: "Transactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FunctionSignature",
                table: "Transactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Hash",
                table: "Transactions",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FunctionName",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "FunctionSignature",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Hash",
                table: "Transactions");
        }
    }
}
