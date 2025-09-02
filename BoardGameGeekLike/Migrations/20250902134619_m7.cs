using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoardGameGeekLike.Migrations
{
    /// <inheritdoc />
    public partial class m7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Mab_BattleResults",
                table: "MabBattles");

            migrationBuilder.AddColumn<int>(
                name: "Mab_NpcCardFullPower",
                table: "MabDuels",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Mab_PlayerCardFullPower",
                table: "MabDuels",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DoesPlayerGoesFirst",
                table: "MabBattles",
                type: "tinyint(1)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Mab_NpcCardFullPower",
                table: "MabDuels");

            migrationBuilder.DropColumn(
                name: "Mab_PlayerCardFullPower",
                table: "MabDuels");

            migrationBuilder.DropColumn(
                name: "DoesPlayerGoesFirst",
                table: "MabBattles");

            migrationBuilder.AddColumn<string>(
                name: "Mab_BattleResults",
                table: "MabBattles",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
