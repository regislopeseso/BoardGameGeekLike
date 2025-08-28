using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoardGameGeekLike.Migrations
{
    /// <inheritdoc />
    public partial class m5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MabPlayerAssignedCards");

            migrationBuilder.DropTable(
                name: "MabPlayerDecks");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "MabNpcs",
                newName: "Mab_NpcName");

            migrationBuilder.RenameColumn(
                name: "Level",
                table: "MabNpcs",
                newName: "Mab_NpcLevel");

            migrationBuilder.RenameColumn(
                name: "IsDummy",
                table: "MabNpcs",
                newName: "Mab_IsNpcDummy");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "MabNpcs",
                newName: "Mab_IsNpcDeleted");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "MabNpcs",
                newName: "Mab_NpcDescription");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "MabNpcCards",
                newName: "Mab_IsNpcCardDeleted");

            migrationBuilder.RenameColumn(
                name: "PlayerCardId",
                table: "MabDuels",
                newName: "Mab_PlayerCardId");

            migrationBuilder.RenameColumn(
                name: "NpcCardId",
                table: "MabDuels",
                newName: "Mab_NpcCardId");

            migrationBuilder.RenameColumn(
                name: "HasPlayerWon",
                table: "MabDuels",
                newName: "Mab_HasPlayerWon");

            migrationBuilder.RenameColumn(
                name: "DuelPoints",
                table: "MabDuels",
                newName: "Mab_DuelPoints");

            migrationBuilder.RenameColumn(
                name: "UpperHand",
                table: "MabCards",
                newName: "Mab_CardUpperHand");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "MabCards",
                newName: "Mab_CardType");

            migrationBuilder.RenameColumn(
                name: "Power",
                table: "MabCards",
                newName: "Mab_CardPower");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "MabCards",
                newName: "Mab_CardName");

            migrationBuilder.RenameColumn(
                name: "Level",
                table: "MabCards",
                newName: "Mab_CardLevel");

            migrationBuilder.RenameColumn(
                name: "IsDummy",
                table: "MabCards",
                newName: "Mab_IsCardDummy");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "MabCards",
                newName: "Mab_IsCardDeleted");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "MabCampaigns",
                newName: "Mab_IsCampaignDeleted");

            migrationBuilder.RenameColumn(
                name: "Results",
                table: "MabBattles",
                newName: "Mab_BattleResults");

            migrationBuilder.RenameColumn(
                name: "IsPlayerTurn",
                table: "MabBattles",
                newName: "Mab_IsPlayerTurn");

            migrationBuilder.RenameColumn(
                name: "IsFinished",
                table: "MabBattles",
                newName: "Mab_IsBattleFinished");

            migrationBuilder.RenameColumn(
                name: "HasPlayerWon",
                table: "MabBattles",
                newName: "Mab_HasPlayerWon");

            migrationBuilder.RenameColumn(
                name: "BattlePoints",
                table: "MabBattles",
                newName: "Mab_BattlePoints");

            migrationBuilder.CreateTable(
                name: "MabDecks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Mab_DeckName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Mab_IsDeckActive = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    Mab_CampaignId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MabDecks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MabDecks_MabCampaigns_Mab_CampaignId",
                        column: x => x.Mab_CampaignId,
                        principalTable: "MabCampaigns",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MabAssignedCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Mab_PlayerCardId = table.Column<int>(type: "int", nullable: true),
                    Mab_DeckId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MabAssignedCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MabAssignedCards_MabDecks_Mab_DeckId",
                        column: x => x.Mab_DeckId,
                        principalTable: "MabDecks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MabAssignedCards_MabPlayerCards_Mab_PlayerCardId",
                        column: x => x.Mab_PlayerCardId,
                        principalTable: "MabPlayerCards",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_MabAssignedCards_Mab_DeckId",
                table: "MabAssignedCards",
                column: "Mab_DeckId");

            migrationBuilder.CreateIndex(
                name: "IX_MabAssignedCards_Mab_PlayerCardId",
                table: "MabAssignedCards",
                column: "Mab_PlayerCardId");

            migrationBuilder.CreateIndex(
                name: "IX_MabDecks_Mab_CampaignId",
                table: "MabDecks",
                column: "Mab_CampaignId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MabAssignedCards");

            migrationBuilder.DropTable(
                name: "MabDecks");

            migrationBuilder.RenameColumn(
                name: "Mab_NpcName",
                table: "MabNpcs",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "Mab_NpcLevel",
                table: "MabNpcs",
                newName: "Level");

            migrationBuilder.RenameColumn(
                name: "Mab_NpcDescription",
                table: "MabNpcs",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "Mab_IsNpcDummy",
                table: "MabNpcs",
                newName: "IsDummy");

            migrationBuilder.RenameColumn(
                name: "Mab_IsNpcDeleted",
                table: "MabNpcs",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "Mab_IsNpcCardDeleted",
                table: "MabNpcCards",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "Mab_PlayerCardId",
                table: "MabDuels",
                newName: "PlayerCardId");

            migrationBuilder.RenameColumn(
                name: "Mab_NpcCardId",
                table: "MabDuels",
                newName: "NpcCardId");

            migrationBuilder.RenameColumn(
                name: "Mab_HasPlayerWon",
                table: "MabDuels",
                newName: "HasPlayerWon");

            migrationBuilder.RenameColumn(
                name: "Mab_DuelPoints",
                table: "MabDuels",
                newName: "DuelPoints");

            migrationBuilder.RenameColumn(
                name: "Mab_IsCardDummy",
                table: "MabCards",
                newName: "IsDummy");

            migrationBuilder.RenameColumn(
                name: "Mab_IsCardDeleted",
                table: "MabCards",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "Mab_CardUpperHand",
                table: "MabCards",
                newName: "UpperHand");

            migrationBuilder.RenameColumn(
                name: "Mab_CardType",
                table: "MabCards",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "Mab_CardPower",
                table: "MabCards",
                newName: "Power");

            migrationBuilder.RenameColumn(
                name: "Mab_CardName",
                table: "MabCards",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "Mab_CardLevel",
                table: "MabCards",
                newName: "Level");

            migrationBuilder.RenameColumn(
                name: "Mab_IsCampaignDeleted",
                table: "MabCampaigns",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "Mab_IsPlayerTurn",
                table: "MabBattles",
                newName: "IsPlayerTurn");

            migrationBuilder.RenameColumn(
                name: "Mab_IsBattleFinished",
                table: "MabBattles",
                newName: "IsFinished");

            migrationBuilder.RenameColumn(
                name: "Mab_HasPlayerWon",
                table: "MabBattles",
                newName: "HasPlayerWon");

            migrationBuilder.RenameColumn(
                name: "Mab_BattleResults",
                table: "MabBattles",
                newName: "Results");

            migrationBuilder.RenameColumn(
                name: "Mab_BattlePoints",
                table: "MabBattles",
                newName: "BattlePoints");

            migrationBuilder.CreateTable(
                name: "MabPlayerDecks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Mab_CampaignId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MabPlayerDecks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MabPlayerDecks_MabCampaigns_Mab_CampaignId",
                        column: x => x.Mab_CampaignId,
                        principalTable: "MabCampaigns",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MabPlayerAssignedCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Mab_PlayerCardId = table.Column<int>(type: "int", nullable: true),
                    Mab_PlayerDeckId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MabPlayerAssignedCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MabPlayerAssignedCards_MabPlayerCards_Mab_PlayerCardId",
                        column: x => x.Mab_PlayerCardId,
                        principalTable: "MabPlayerCards",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MabPlayerAssignedCards_MabPlayerDecks_Mab_PlayerDeckId",
                        column: x => x.Mab_PlayerDeckId,
                        principalTable: "MabPlayerDecks",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_MabPlayerAssignedCards_Mab_PlayerCardId",
                table: "MabPlayerAssignedCards",
                column: "Mab_PlayerCardId");

            migrationBuilder.CreateIndex(
                name: "IX_MabPlayerAssignedCards_Mab_PlayerDeckId",
                table: "MabPlayerAssignedCards",
                column: "Mab_PlayerDeckId");

            migrationBuilder.CreateIndex(
                name: "IX_MabPlayerDecks_Mab_CampaignId",
                table: "MabPlayerDecks",
                column: "Mab_CampaignId");
        }
    }
}
