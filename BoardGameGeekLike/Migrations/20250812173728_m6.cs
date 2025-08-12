using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoardGameGeekLike.Migrations
{
    /// <inheritdoc />
    public partial class m6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                newName: "mabPlayerDecks");

            migrationBuilder.RenameIndex(
                name: "IX_mabDecks_MabCampaignId",
                table: "mabPlayerDecks",
                newName: "IX_mabPlayerDecks_MabCampaignId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_mabPlayerDecks",
                table: "mabPlayerDecks",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_mabPlayerCardCopy_mabPlayerDecks_MabPlayerDeckId",
                table: "mabPlayerCardCopy",
                column: "MabPlayerDeckId",
                principalTable: "mabPlayerDecks",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_mabPlayerDecks_mabCampaigns_MabCampaignId",
                table: "mabPlayerDecks",
                column: "MabCampaignId",
                principalTable: "mabCampaigns",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_mabPlayerCardCopy_mabPlayerDecks_MabPlayerDeckId",
                table: "mabPlayerCardCopy");

            migrationBuilder.DropForeignKey(
                name: "FK_mabPlayerDecks_mabCampaigns_MabCampaignId",
                table: "mabPlayerDecks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_mabPlayerDecks",
                table: "mabPlayerDecks");

            migrationBuilder.RenameTable(
                name: "mabPlayerDecks",
                newName: "mabDecks");

            migrationBuilder.RenameIndex(
                name: "IX_mabPlayerDecks_MabCampaignId",
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
    }
}
