using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace web_project.Data.Migrations
{
    public partial class ReviewFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Auctions_AuctionId",
                table: "Transactions");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Auctions_AuctionId",
                table: "Transactions",
                column: "AuctionId",
                principalTable: "Auctions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Auctions_AuctionId",
                table: "Transactions");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Auctions_AuctionId",
                table: "Transactions",
                column: "AuctionId",
                principalTable: "Auctions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
