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
            migrationBuilder.AddColumn<bool>(
                name: "Mab_BlacksmithTrophy",
                table: "MabCampaigns",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Mab_CountForgings",
                table: "MabCampaigns",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Mab_ForgingsCount",
                table: "MabCampaigns",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Mab_MeltCount",
                table: "MabCampaigns",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Mab_SharpenCount",
                table: "MabCampaigns",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Mab_BlacksmithTrophy",
                table: "MabCampaigns");

            migrationBuilder.DropColumn(
                name: "Mab_CountForgings",
                table: "MabCampaigns");

            migrationBuilder.DropColumn(
                name: "Mab_ForgingsCount",
                table: "MabCampaigns");

            migrationBuilder.DropColumn(
                name: "Mab_MeltCount",
                table: "MabCampaigns");

            migrationBuilder.DropColumn(
                name: "Mab_SharpenCount",
                table: "MabCampaigns");
        }
    }
}
