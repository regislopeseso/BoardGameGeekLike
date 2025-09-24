using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoardGameGeekLike.Migrations
{
    /// <inheritdoc />
    public partial class m4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Mab_GoldValue",
                table: "MabCampaigns");

            migrationBuilder.AddColumn<int>(
                name: "Mab_Horse",
                table: "MabCampaigns",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Mab_IronOre",
                table: "MabCampaigns",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Mab_WoodLog",
                table: "MabCampaigns",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Mab_Horse",
                table: "MabCampaigns");

            migrationBuilder.DropColumn(
                name: "Mab_IronOre",
                table: "MabCampaigns");

            migrationBuilder.DropColumn(
                name: "Mab_WoodLog",
                table: "MabCampaigns");

            migrationBuilder.AddColumn<double>(
                name: "Mab_GoldValue",
                table: "MabCampaigns",
                type: "double",
                nullable: true);
        }
    }
}
