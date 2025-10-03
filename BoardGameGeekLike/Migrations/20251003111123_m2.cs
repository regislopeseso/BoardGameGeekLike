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
                name: "Mab_MinerTrophy",
                table: "MabCampaigns",
                newName: "Mab_TheMinerTrophy");

            migrationBuilder.RenameColumn(
                name: "Mab_BourgeoisTrophy",
                table: "MabCampaigns",
                newName: "Mab_TheCollectorTrophy");

            migrationBuilder.RenameColumn(
                name: "Mab_BlacksmithTrophy",
                table: "MabCampaigns",
                newName: "Mab_TheBraveTrophy");

            migrationBuilder.RenameColumn(
                name: "Mab_AllNpcsDefeatedTrophy",
                table: "MabCampaigns",
                newName: "Mab_TheBourgeoisTrophy");

            migrationBuilder.RenameColumn(
                name: "Mab_AllCardsCollectedTrophy",
                table: "MabCampaigns",
                newName: "Mab_TheBlacksmithTrophy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Mab_TheMinerTrophy",
                table: "MabCampaigns",
                newName: "Mab_MinerTrophy");

            migrationBuilder.RenameColumn(
                name: "Mab_TheCollectorTrophy",
                table: "MabCampaigns",
                newName: "Mab_BourgeoisTrophy");

            migrationBuilder.RenameColumn(
                name: "Mab_TheBraveTrophy",
                table: "MabCampaigns",
                newName: "Mab_BlacksmithTrophy");

            migrationBuilder.RenameColumn(
                name: "Mab_TheBourgeoisTrophy",
                table: "MabCampaigns",
                newName: "Mab_AllNpcsDefeatedTrophy");

            migrationBuilder.RenameColumn(
                name: "Mab_TheBlacksmithTrophy",
                table: "MabCampaigns",
                newName: "Mab_AllCardsCollectedTrophy");
        }
    }
}
