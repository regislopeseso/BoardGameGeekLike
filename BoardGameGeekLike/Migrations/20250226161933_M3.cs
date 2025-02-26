using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoardGameGeekLike.Migrations
{
    /// <inheritdoc />
    public partial class M3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MechanicId",
                table: "boardgames",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "mechanics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mechanics", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_boardgames_mechanics_MechanicId",
                table: "boardgames");

            migrationBuilder.DropTable(
                name: "mechanics");

            migrationBuilder.DropIndex(
                name: "IX_boardgames_MechanicId",
                table: "boardgames");

            migrationBuilder.DropColumn(
                name: "MechanicId",
                table: "boardgames");
        }
    }
}
