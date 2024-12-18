using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GokApp_PRA.Migrations
{
    /// <inheritdoc />
    public partial class Betings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Amount",
                table: "Bets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsBetPlaced",
                table: "Bets",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsWon",
                table: "Bets",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amount",
                table: "Bets");

            migrationBuilder.DropColumn(
                name: "IsBetPlaced",
                table: "Bets");

            migrationBuilder.DropColumn(
                name: "IsWon",
                table: "Bets");
        }
    }
}
