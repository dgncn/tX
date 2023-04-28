using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tX.Migrations
{
    public partial class ProcessingAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TxStatus",
                table: "Transactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TxStatus",
                table: "Transactions");
        }
    }
}
