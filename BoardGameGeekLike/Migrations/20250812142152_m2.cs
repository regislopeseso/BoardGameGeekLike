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
            migrationBuilder.DropForeignKey(
                name: "FK_decks_mabCampaigns_MabCampaignId",
                table: "decks");

            migrationBuilder.DropForeignKey(
                name: "FK_mabPlayerCardCopy_decks_MabPlayerDeckId",
                table: "mabPlayerCardCopy");

            migrationBuilder.DropPrimaryKey(
                name: "PK_decks",
                table: "decks");

            migrationBuilder.RenameTable(
                name: "decks",
                newName: "mabDecks");

            migrationBuilder.RenameIndex(
                name: "IX_decks_MabCampaignId",
                table: "mabDecks",
                newName: "IX_mabDecks_MabCampaignId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_mabDecks",
                table: "mabDecks",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_mabDecks_mabCampaigns_MabCampaignId",
                table: "mabDecks",
                column: "MabCampaignId",
                principalTable: "mabCampaigns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_mabPlayerCardCopy_mabDecks_MabPlayerDeckId",
                table: "mabPlayerCardCopy",
                column: "MabPlayerDeckId",
                principalTable: "mabDecks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_mabDecks_mabCampaigns_MabCampaignId",
                table: "mabDecks");

            migrationBuilder.DropForeignKey(
                name: "FK_mabPlayerCardCopy_mabDecks_MabPlayerDeckId",
                table: "mabPlayerCardCopy");

            migrationBuilder.DropPrimaryKey(
                name: "PK_mabDecks",
                table: "mabDecks");

            migrationBuilder.RenameTable(
                name: "mabDecks",
                newName: "decks");

            migrationBuilder.RenameIndex(
                name: "IX_mabDecks_MabCampaignId",
                table: "decks",
                newName: "IX_decks_MabCampaignId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_decks",
                table: "decks",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_decks_mabCampaigns_MabCampaignId",
                table: "decks",
                column: "MabCampaignId",
                principalTable: "mabCampaigns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_mabPlayerCardCopy_decks_MabPlayerDeckId",
                table: "mabPlayerCardCopy",
                column: "MabPlayerDeckId",
                principalTable: "decks",
                principalColumn: "Id");
        }
    }
}
