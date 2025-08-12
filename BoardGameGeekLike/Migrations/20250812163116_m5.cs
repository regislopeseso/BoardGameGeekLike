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
            migrationBuilder.DropForeignKey(
                name: "FK_mabCampaigns_mabDecks_ActiveMabPlayerDeckId",
                table: "mabCampaigns");

            migrationBuilder.DropIndex(
                name: "IX_mabCampaigns_ActiveMabPlayerDeckId",
                table: "mabCampaigns");

            migrationBuilder.DropColumn(
                name: "ActiveMabPlayerDeckId",
                table: "mabCampaigns");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "mabDecks",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "mabDecks",
                type: "tinyint(1)",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "mabDecks",
                type: "tinyint(1)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "mabDecks");

            migrationBuilder.UpdateData(
                table: "mabDecks",
                keyColumn: "Name",
                keyValue: null,
                column: "Name",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "mabDecks",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "mabDecks",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldNullable: true);

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
    }
}
