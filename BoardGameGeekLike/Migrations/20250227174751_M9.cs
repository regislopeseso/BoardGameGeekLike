using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoardGameGeekLike.Migrations
{
    /// <inheritdoc />
    public partial class M9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AverageRating",
                table: "boardgames",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RatingsCount",
                table: "boardgames",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "boardGameRatings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Rate = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    BoardGameId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_boardGameRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_boardGameRatings_boardgames_BoardGameId",
                        column: x => x.BoardGameId,
                        principalTable: "boardgames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_boardGameRatings_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_boardGameRatings_BoardGameId",
                table: "boardGameRatings",
                column: "BoardGameId");

            migrationBuilder.CreateIndex(
                name: "IX_boardGameRatings_UserId",
                table: "boardGameRatings",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "boardGameRatings");

            migrationBuilder.DropColumn(
                name: "AverageRating",
                table: "boardgames");

            migrationBuilder.DropColumn(
                name: "RatingsCount",
                table: "boardgames");
        }
    }
}
