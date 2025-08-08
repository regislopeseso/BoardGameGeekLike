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
                name: "FK_decks_mabCampains_SaveId",
                table: "decks");

            migrationBuilder.DropForeignKey(
                name: "FK_mabCampains_AspNetUsers_UserId",
                table: "mabCampains");

            migrationBuilder.DropForeignKey(
                name: "FK_playerCardEntries_mabCampains_SaveId",
                table: "playerCardEntries");

            migrationBuilder.RenameColumn(
                name: "SaveId",
                table: "playerCardEntries",
                newName: "MabCampainId");

            migrationBuilder.RenameIndex(
                name: "IX_playerCardEntries_SaveId",
                table: "playerCardEntries",
                newName: "IX_playerCardEntries_MabCampainId");

            migrationBuilder.RenameColumn(
                name: "SaveId",
                table: "decks",
                newName: "MabCampainId");

            migrationBuilder.RenameIndex(
                name: "IX_decks_SaveId",
                table: "decks",
                newName: "IX_decks_MabCampainId");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "mabCampains",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_decks_mabCampains_MabCampainId",
                table: "decks",
                column: "MabCampainId",
                principalTable: "mabCampains",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_mabCampains_AspNetUsers_UserId",
                table: "mabCampains",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_playerCardEntries_mabCampains_MabCampainId",
                table: "playerCardEntries",
                column: "MabCampainId",
                principalTable: "mabCampains",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_decks_mabCampains_MabCampainId",
                table: "decks");

            migrationBuilder.DropForeignKey(
                name: "FK_mabCampains_AspNetUsers_UserId",
                table: "mabCampains");

            migrationBuilder.DropForeignKey(
                name: "FK_playerCardEntries_mabCampains_MabCampainId",
                table: "playerCardEntries");

            migrationBuilder.RenameColumn(
                name: "MabCampainId",
                table: "playerCardEntries",
                newName: "SaveId");

            migrationBuilder.RenameIndex(
                name: "IX_playerCardEntries_MabCampainId",
                table: "playerCardEntries",
                newName: "IX_playerCardEntries_SaveId");

            migrationBuilder.RenameColumn(
                name: "MabCampainId",
                table: "decks",
                newName: "SaveId");

            migrationBuilder.RenameIndex(
                name: "IX_decks_MabCampainId",
                table: "decks",
                newName: "IX_decks_SaveId");

            migrationBuilder.UpdateData(
                table: "mabCampains",
                keyColumn: "UserId",
                keyValue: null,
                column: "UserId",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "mabCampains",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_decks_mabCampains_SaveId",
                table: "decks",
                column: "SaveId",
                principalTable: "mabCampains",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_mabCampains_AspNetUsers_UserId",
                table: "mabCampains",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_playerCardEntries_mabCampains_SaveId",
                table: "playerCardEntries",
                column: "SaveId",
                principalTable: "mabCampains",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
