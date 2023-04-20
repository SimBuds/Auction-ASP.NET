using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace web_project.Data.Migrations
{
    public partial class SecondAdjustment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Auction",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<decimal>(
                name: "StartingPrice",
                table: "Auction",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");

            migrationBuilder.CreateIndex(
                name: "IX_Auction_UserId",
                table: "Auction",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Auction_AspNetUsers_UserId",
                table: "Auction",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Auction_AspNetUsers_UserId",
                table: "Auction");

            migrationBuilder.DropIndex(
                name: "IX_Auction_UserId",
                table: "Auction");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Auction",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<decimal>(
                name: "StartingPrice",
                table: "Auction",
                type: "decimal(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }
    }
}
