using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoardGameGeekLike.Migrations
{
    /// <inheritdoc />
    public partial class M5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_boardgames_mechanics_MechanicId",
                table: "boardgames");

            migrationBuilder.DropIndex(
                name: "IX_boardgames_MechanicId",
                table: "boardgames");

            migrationBuilder.DropColumn(
                name: "MechanicId",
                table: "boardgames");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MechanicId",
                table: "boardgames",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_boardgames_MechanicId",
                table: "boardgames",
                column: "MechanicId");

            migrationBuilder.AddForeignKey(
                name: "FK_boardgames_mechanics_MechanicId",
                table: "boardgames",
                column: "MechanicId",
                principalTable: "mechanics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
