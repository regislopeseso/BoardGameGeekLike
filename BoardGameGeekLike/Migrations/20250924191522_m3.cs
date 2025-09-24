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
                name: "FK_MabBattles_MabQuests_Mab_QuestId",
                table: "MabBattles");

            migrationBuilder.AlterColumn<int>(
                name: "Mab_QuestId",
                table: "MabBattles",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_MabBattles_MabQuests_Mab_QuestId",
                table: "MabBattles",
                column: "Mab_QuestId",
                principalTable: "MabQuests",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MabBattles_MabQuests_Mab_QuestId",
                table: "MabBattles");

            migrationBuilder.AlterColumn<int>(
                name: "Mab_QuestId",
                table: "MabBattles",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MabBattles_MabQuests_Mab_QuestId",
                table: "MabBattles",
                column: "Mab_QuestId",
                principalTable: "MabQuests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
