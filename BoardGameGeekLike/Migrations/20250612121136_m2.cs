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
            migrationBuilder.RenameColumn(
                name: "MaxLifePoints",
                table: "LifeCounters",
                newName: "PlayersMaxLifePoints");

            migrationBuilder.RenameColumn(
                name: "FixedMaxLife",
                table: "LifeCounters",
                newName: "FixedMaxLifePointsMode");

            migrationBuilder.RenameColumn(
                name: "AutoEndMatch",
                table: "LifeCounters",
                newName: "AutoEndMode");

            migrationBuilder.RenameColumn(
                name: "StartingLife",
                table: "LifeCounterPlayers",
                newName: "StartingLifePoints");

            migrationBuilder.RenameColumn(
                name: "MaxLife",
                table: "LifeCounterPlayers",
                newName: "MaxLifePoints");

            migrationBuilder.RenameColumn(
                name: "FixedMaxLife",
                table: "LifeCounterPlayers",
                newName: "FixedMaxLifePointsMode");

            migrationBuilder.RenameColumn(
                name: "CurrentLife",
                table: "LifeCounterPlayers",
                newName: "CurrentLifePoints");

            migrationBuilder.AddColumn<bool>(
                name: "FixedMaxLifePointsMode",
                table: "LifeCounterManagers",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlayerMaxLifePoints",
                table: "LifeCounterManagers",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FixedMaxLifePointsMode",
                table: "LifeCounterManagers");

            migrationBuilder.DropColumn(
                name: "PlayerMaxLifePoints",
                table: "LifeCounterManagers");

            migrationBuilder.RenameColumn(
                name: "PlayersMaxLifePoints",
                table: "LifeCounters",
                newName: "MaxLifePoints");

            migrationBuilder.RenameColumn(
                name: "FixedMaxLifePointsMode",
                table: "LifeCounters",
                newName: "FixedMaxLife");

            migrationBuilder.RenameColumn(
                name: "AutoEndMode",
                table: "LifeCounters",
                newName: "AutoEndMatch");

            migrationBuilder.RenameColumn(
                name: "StartingLifePoints",
                table: "LifeCounterPlayers",
                newName: "StartingLife");

            migrationBuilder.RenameColumn(
                name: "MaxLifePoints",
                table: "LifeCounterPlayers",
                newName: "MaxLife");

            migrationBuilder.RenameColumn(
                name: "FixedMaxLifePointsMode",
                table: "LifeCounterPlayers",
                newName: "FixedMaxLife");

            migrationBuilder.RenameColumn(
                name: "CurrentLifePoints",
                table: "LifeCounterPlayers",
                newName: "CurrentLife");
        }
    }
}
