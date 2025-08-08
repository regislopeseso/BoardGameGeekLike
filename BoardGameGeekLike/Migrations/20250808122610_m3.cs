using Microsoft.EntityFrameworkCore.Metadata;
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
            migrationBuilder.DropForeignKey(
                name: "FK_battles_mabCampains_SaveId",
                table: "battles");

            migrationBuilder.DropForeignKey(
                name: "FK_decks_mabCampains_MabCampainId",
                table: "decks");

            migrationBuilder.DropForeignKey(
                name: "FK_playerCardEntries_mabCampains_MabCampainId",
                table: "playerCardEntries");

            migrationBuilder.DropTable(
                name: "mabCampains");

            migrationBuilder.RenameColumn(
                name: "MabCampainId",
                table: "playerCardEntries",
                newName: "MabCampaignId");

            migrationBuilder.RenameIndex(
                name: "IX_playerCardEntries_MabCampainId",
                table: "playerCardEntries",
                newName: "IX_playerCardEntries_MabCampaignId");

            migrationBuilder.RenameColumn(
                name: "MabCampainId",
                table: "decks",
                newName: "MabCampaignId");

            migrationBuilder.RenameIndex(
                name: "IX_decks_MabCampainId",
                table: "decks",
                newName: "IX_decks_MabCampaignId");

            migrationBuilder.RenameColumn(
                name: "SaveId",
                table: "battles",
                newName: "MabCampaignId");

            migrationBuilder.RenameIndex(
                name: "IX_battles_SaveId",
                table: "battles",
                newName: "IX_battles_MabCampaignId");

            migrationBuilder.CreateTable(
                name: "mabCampaigns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PlayerLevel = table.Column<int>(type: "int", nullable: false),
                    Gold = table.Column<int>(type: "int", nullable: false),
                    CountMatches = table.Column<int>(type: "int", nullable: false),
                    CountVictories = table.Column<int>(type: "int", nullable: false),
                    CountDefeats = table.Column<int>(type: "int", nullable: false),
                    CountBoosters = table.Column<int>(type: "int", nullable: false),
                    AllCardsCollectedTrophy = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AllNpcsDefeatedTrophy = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mabCampaigns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_mabCampaigns_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_mabCampaigns_UserId",
                table: "mabCampaigns",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_battles_mabCampaigns_MabCampaignId",
                table: "battles",
                column: "MabCampaignId",
                principalTable: "mabCampaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_decks_mabCampaigns_MabCampaignId",
                table: "decks",
                column: "MabCampaignId",
                principalTable: "mabCampaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_playerCardEntries_mabCampaigns_MabCampaignId",
                table: "playerCardEntries",
                column: "MabCampaignId",
                principalTable: "mabCampaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_battles_mabCampaigns_MabCampaignId",
                table: "battles");

            migrationBuilder.DropForeignKey(
                name: "FK_decks_mabCampaigns_MabCampaignId",
                table: "decks");

            migrationBuilder.DropForeignKey(
                name: "FK_playerCardEntries_mabCampaigns_MabCampaignId",
                table: "playerCardEntries");

            migrationBuilder.DropTable(
                name: "mabCampaigns");

            migrationBuilder.RenameColumn(
                name: "MabCampaignId",
                table: "playerCardEntries",
                newName: "MabCampainId");

            migrationBuilder.RenameIndex(
                name: "IX_playerCardEntries_MabCampaignId",
                table: "playerCardEntries",
                newName: "IX_playerCardEntries_MabCampainId");

            migrationBuilder.RenameColumn(
                name: "MabCampaignId",
                table: "decks",
                newName: "MabCampainId");

            migrationBuilder.RenameIndex(
                name: "IX_decks_MabCampaignId",
                table: "decks",
                newName: "IX_decks_MabCampainId");

            migrationBuilder.RenameColumn(
                name: "MabCampaignId",
                table: "battles",
                newName: "SaveId");

            migrationBuilder.RenameIndex(
                name: "IX_battles_MabCampaignId",
                table: "battles",
                newName: "IX_battles_SaveId");

            migrationBuilder.CreateTable(
                name: "mabCampains",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AllCardsCollectedTrophy = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AllNpcsDefeatedTrophy = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CountBoosters = table.Column<int>(type: "int", nullable: false),
                    CountDefeats = table.Column<int>(type: "int", nullable: false),
                    CountMatches = table.Column<int>(type: "int", nullable: false),
                    CountVictories = table.Column<int>(type: "int", nullable: false),
                    Gold = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PlayerLevel = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mabCampains", x => x.Id);
                    table.ForeignKey(
                        name: "FK_mabCampains_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_mabCampains_UserId",
                table: "mabCampains",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_battles_mabCampains_SaveId",
                table: "battles",
                column: "SaveId",
                principalTable: "mabCampains",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_decks_mabCampains_MabCampainId",
                table: "decks",
                column: "MabCampainId",
                principalTable: "mabCampains",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_playerCardEntries_mabCampains_MabCampainId",
                table: "playerCardEntries",
                column: "MabCampainId",
                principalTable: "mabCampains",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
