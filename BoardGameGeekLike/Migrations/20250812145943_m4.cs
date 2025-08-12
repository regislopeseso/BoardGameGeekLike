using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoardGameGeekLike.Migrations
{
    /// <inheritdoc />
    public partial class m4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_mabDecks_mabCampaigns_MabCampaignId1",
                table: "mabDecks");

            migrationBuilder.DropIndex(
                name: "IX_mabDecks_MabCampaignId1",
                table: "mabDecks");

            migrationBuilder.DropColumn(
                name: "MabCampaignId1",
                table: "mabDecks");

            migrationBuilder.AddColumn<int>(
                name: "ActiveMabPlayerDeckId",
                table: "mabCampaigns",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_mabCampaigns_ActiveMabPlayerDeckId",
                table: "mabCampaigns",
                column: "ActiveMabPlayerDeckId");

            migrationBuilder.AddForeignKey(
                name: "FK_mabCampaigns_mabDecks_ActiveMabPlayerDeckId",
                table: "mabCampaigns",
                column: "ActiveMabPlayerDeckId",
                principalTable: "mabDecks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_mabCampaigns_mabDecks_ActiveMabPlayerDeckId",
                table: "mabCampaigns");

            migrationBuilder.DropIndex(
                name: "IX_mabCampaigns_ActiveMabPlayerDeckId",
                table: "mabCampaigns");

            migrationBuilder.DropColumn(
                name: "ActiveMabPlayerDeckId",
                table: "mabCampaigns");

            migrationBuilder.AddColumn<int>(
                name: "MabCampaignId1",
                table: "mabDecks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_mabDecks_MabCampaignId1",
                table: "mabDecks",
                column: "MabCampaignId1",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_mabDecks_mabCampaigns_MabCampaignId1",
                table: "mabDecks",
                column: "MabCampaignId1",
                principalTable: "mabCampaigns",
                principalColumn: "Id");
        }
    }
}
