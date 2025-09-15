using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoardGameGeekLike.Migrations
{
    /// <inheritdoc />
    public partial class m13 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Mab_HasPlayerWon",
                table: "MabDuels",
                newName: "IsFinished");

            migrationBuilder.AddColumn<int>(
                name: "Mab_NpcCardLevel",
                table: "MabDuels",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Mab_NpcCardName",
                table: "MabDuels",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "Mab_NpcCardPower",
                table: "MabDuels",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Mab_NpcCardType",
                table: "MabDuels",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Mab_NpcCardUpperHand",
                table: "MabDuels",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Mab_PlayerCardLevel",
                table: "MabDuels",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Mab_PlayerCardName",
                table: "MabDuels",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "Mab_PlayerCardPower",
                table: "MabDuels",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Mab_PlayerCardType",
                table: "MabDuels",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Mab_PlayerCardUpperHand",
                table: "MabDuels",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Mab_NpcCardLevel",
                table: "MabDuels");

            migrationBuilder.DropColumn(
                name: "Mab_NpcCardName",
                table: "MabDuels");

            migrationBuilder.DropColumn(
                name: "Mab_NpcCardPower",
                table: "MabDuels");

            migrationBuilder.DropColumn(
                name: "Mab_NpcCardType",
                table: "MabDuels");

            migrationBuilder.DropColumn(
                name: "Mab_NpcCardUpperHand",
                table: "MabDuels");

            migrationBuilder.DropColumn(
                name: "Mab_PlayerCardLevel",
                table: "MabDuels");

            migrationBuilder.DropColumn(
                name: "Mab_PlayerCardName",
                table: "MabDuels");

            migrationBuilder.DropColumn(
                name: "Mab_PlayerCardPower",
                table: "MabDuels");

            migrationBuilder.DropColumn(
                name: "Mab_PlayerCardType",
                table: "MabDuels");

            migrationBuilder.DropColumn(
                name: "Mab_PlayerCardUpperHand",
                table: "MabDuels");

            migrationBuilder.RenameColumn(
                name: "IsFinished",
                table: "MabDuels",
                newName: "Mab_HasPlayerWon");
        }
    }
}
