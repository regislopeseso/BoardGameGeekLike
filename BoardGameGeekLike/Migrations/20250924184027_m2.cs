using Microsoft.EntityFrameworkCore.Metadata;
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
            migrationBuilder.DropTable(
                name: "MabDefeatedNpcs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MabDefeatedNpcs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Mab_CampaignId = table.Column<int>(type: "int", nullable: true),
                    Mab_NpcId = table.Column<int>(type: "int", nullable: true),
                    Mab_QuestId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MabDefeatedNpcs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MabDefeatedNpcs_MabCampaigns_Mab_CampaignId",
                        column: x => x.Mab_CampaignId,
                        principalTable: "MabCampaigns",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MabDefeatedNpcs_MabNpcs_Mab_NpcId",
                        column: x => x.Mab_NpcId,
                        principalTable: "MabNpcs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MabDefeatedNpcs_MabQuests_Mab_QuestId",
                        column: x => x.Mab_QuestId,
                        principalTable: "MabQuests",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_MabDefeatedNpcs_Mab_CampaignId",
                table: "MabDefeatedNpcs",
                column: "Mab_CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_MabDefeatedNpcs_Mab_NpcId",
                table: "MabDefeatedNpcs",
                column: "Mab_NpcId");

            migrationBuilder.CreateIndex(
                name: "IX_MabDefeatedNpcs_Mab_QuestId",
                table: "MabDefeatedNpcs",
                column: "Mab_QuestId");
        }
    }
}
