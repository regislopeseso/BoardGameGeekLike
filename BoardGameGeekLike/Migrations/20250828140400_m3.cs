using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoardGameGeekLike.Migrations
{
    /// <inheritdoc />
    public partial class m3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PlayerNickname",
                table: "MabCampaigns",
                newName: "Mab_PlayerNickname");

            migrationBuilder.RenameColumn(
                name: "PlayerLevel",
                table: "MabCampaigns",
                newName: "Mab_PlayerLevel");

            migrationBuilder.RenameColumn(
                name: "GoldValue",
                table: "MabCampaigns",
                newName: "Mab_GoldValue");

            migrationBuilder.RenameColumn(
                name: "GoldStash",
                table: "MabCampaigns",
                newName: "Mab_OpenedBoostersOpened");

            migrationBuilder.RenameColumn(
                name: "Difficulty",
                table: "MabCampaigns",
                newName: "Mab_Difficulty");

            migrationBuilder.RenameColumn(
                name: "CountVictories",
                table: "MabCampaigns",
                newName: "Mab_GoldStash");

            migrationBuilder.RenameColumn(
                name: "CountMatches",
                table: "MabCampaigns",
                newName: "Mab_BattlesCount");

            migrationBuilder.RenameColumn(
                name: "CountDefeats",
                table: "MabCampaigns",
                newName: "Mab_BattleVictoriesCount");

            migrationBuilder.RenameColumn(
                name: "CountBoosters",
                table: "MabCampaigns",
                newName: "Mab_BattleDefeatsCount");

            migrationBuilder.RenameColumn(
                name: "AllNpcsDefeatedTrophy",
                table: "MabCampaigns",
                newName: "Mab_AllNpcsDefeatedTrophy");

            migrationBuilder.RenameColumn(
                name: "AllCardsCollectedTrophy",
                table: "MabCampaigns",
                newName: "Mab_AllCardsCollectedTrophy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Mab_PlayerNickname",
                table: "MabCampaigns",
                newName: "PlayerNickname");

            migrationBuilder.RenameColumn(
                name: "Mab_PlayerLevel",
                table: "MabCampaigns",
                newName: "PlayerLevel");

            migrationBuilder.RenameColumn(
                name: "Mab_OpenedBoostersOpened",
                table: "MabCampaigns",
                newName: "GoldStash");

            migrationBuilder.RenameColumn(
                name: "Mab_GoldValue",
                table: "MabCampaigns",
                newName: "GoldValue");

            migrationBuilder.RenameColumn(
                name: "Mab_GoldStash",
                table: "MabCampaigns",
                newName: "CountVictories");

            migrationBuilder.RenameColumn(
                name: "Mab_Difficulty",
                table: "MabCampaigns",
                newName: "Difficulty");

            migrationBuilder.RenameColumn(
                name: "Mab_BattlesCount",
                table: "MabCampaigns",
                newName: "CountMatches");

            migrationBuilder.RenameColumn(
                name: "Mab_BattleVictoriesCount",
                table: "MabCampaigns",
                newName: "CountDefeats");

            migrationBuilder.RenameColumn(
                name: "Mab_BattleDefeatsCount",
                table: "MabCampaigns",
                newName: "CountBoosters");

            migrationBuilder.RenameColumn(
                name: "Mab_AllNpcsDefeatedTrophy",
                table: "MabCampaigns",
                newName: "AllNpcsDefeatedTrophy");

            migrationBuilder.RenameColumn(
                name: "Mab_AllCardsCollectedTrophy",
                table: "MabCampaigns",
                newName: "AllCardsCollectedTrophy");
        }
    }
}
