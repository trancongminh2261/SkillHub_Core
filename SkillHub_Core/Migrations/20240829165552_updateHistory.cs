using Microsoft.EntityFrameworkCore.Migrations;

namespace LMSCore.Migrations
{
    public partial class updateHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TaskOrder",
                table: "tbl_HistoryCheckWriting",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TaskOrder",
                table: "tbl_HistoryCheckWriting");
        }
    }
}
