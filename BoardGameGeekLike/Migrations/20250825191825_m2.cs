using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoardGameGeekLike.Migrations
{
    /// <inheritdoc />
    public partial class m2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Winner",
                table: "mabBattles");

            migrationBuilder.RenameColumn(
                name: "DoesPlayerPlaysFirst",
                table: "mabBattles",
                newName: "IsPlayersTurn");

            migrationBuilder.AddColumn<bool>(
                name: "HasPlayerWon",
                table: "mabBattles",
                type: "tinyint(1)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasPlayerWon",
                table: "mabBattles");

            migrationBuilder.RenameColumn(
                name: "IsPlayersTurn",
                table: "mabBattles",
                newName: "DoesPlayerPlaysFirst");

            migrationBuilder.AddColumn<string>(
                name: "Winner",
                table: "mabBattles",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
