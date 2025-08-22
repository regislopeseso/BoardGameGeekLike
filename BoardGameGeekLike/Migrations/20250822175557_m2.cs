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
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "mabPlayerDecks");

            migrationBuilder.AddColumn<double>(
                name: "GoldValue",
                table: "mabCampaigns",
                type: "double",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GoldValue",
                table: "mabCampaigns");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "mabPlayerDecks",
                type: "tinyint(1)",
                nullable: true);
        }
    }
}
