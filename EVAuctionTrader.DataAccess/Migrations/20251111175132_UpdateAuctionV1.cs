using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EVAuctionTrader.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAuctionV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CurrentPrice",
                table: "Auctions",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DepositRate",
                table: "Auctions",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "WinnerId",
                table: "Auctions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Auctions_WinnerId",
                table: "Auctions",
                column: "WinnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Auctions_Users_WinnerId",
                table: "Auctions",
                column: "WinnerId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Auctions_Users_WinnerId",
                table: "Auctions");

            migrationBuilder.DropIndex(
                name: "IX_Auctions_WinnerId",
                table: "Auctions");

            migrationBuilder.DropColumn(
                name: "CurrentPrice",
                table: "Auctions");

            migrationBuilder.DropColumn(
                name: "DepositRate",
                table: "Auctions");

            migrationBuilder.DropColumn(
                name: "WinnerId",
                table: "Auctions");
        }
    }
}
