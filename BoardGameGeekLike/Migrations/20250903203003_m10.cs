using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoardGameGeekLike.Migrations
{
    /// <inheritdoc />
    public partial class m10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Mab_BattlePoints",
                table: "MabBattles",
                newName: "Mab_FinalPlayerState");

            migrationBuilder.AddColumn<int>(
                name: "Mab_BonusXp",
                table: "MabDuels",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Mab_EarnedXp",
                table: "MabDuels",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Mab_PlayerState",
                table: "MabDuels",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Mab_PlayerExperience",
                table: "MabCampaigns",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Mab_BonusXp",
                table: "MabBattles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Mab_EarnedGold",
                table: "MabBattles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Mab_EarnedXp",
                table: "MabBattles",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Mab_BonusXp",
                table: "MabDuels");

            migrationBuilder.DropColumn(
                name: "Mab_EarnedXp",
                table: "MabDuels");

            migrationBuilder.DropColumn(
                name: "Mab_PlayerState",
                table: "MabDuels");

            migrationBuilder.DropColumn(
                name: "Mab_PlayerExperience",
                table: "MabCampaigns");

            migrationBuilder.DropColumn(
                name: "Mab_BonusXp",
                table: "MabBattles");

            migrationBuilder.DropColumn(
                name: "Mab_EarnedGold",
                table: "MabBattles");

            migrationBuilder.DropColumn(
                name: "Mab_EarnedXp",
                table: "MabBattles");

            migrationBuilder.RenameColumn(
                name: "Mab_FinalPlayerState",
                table: "MabBattles",
                newName: "Mab_BattlePoints");
        }
    }
}
