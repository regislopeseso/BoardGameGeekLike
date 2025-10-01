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
            migrationBuilder.AddColumn<int>(
                name: "Mab_AdamantiumInflation",
                table: "MabCampaigns",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Mab_BrassInflation",
                table: "MabCampaigns",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Mab_CopperInflation",
                table: "MabCampaigns",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Mab_DiamondInflation",
                table: "MabCampaigns",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Mab_GoldInflation",
                table: "MabCampaigns",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Mab_IronInflation",
                table: "MabCampaigns",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Mab_SilverInflation",
                table: "MabCampaigns",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Mab_SteelInflation",
                table: "MabCampaigns",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Mab_TitaniumInflation",
                table: "MabCampaigns",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Mab_AdamantiumInflation",
                table: "MabCampaigns");

            migrationBuilder.DropColumn(
                name: "Mab_BrassInflation",
                table: "MabCampaigns");

            migrationBuilder.DropColumn(
                name: "Mab_CopperInflation",
                table: "MabCampaigns");

            migrationBuilder.DropColumn(
                name: "Mab_DiamondInflation",
                table: "MabCampaigns");

            migrationBuilder.DropColumn(
                name: "Mab_GoldInflation",
                table: "MabCampaigns");

            migrationBuilder.DropColumn(
                name: "Mab_IronInflation",
                table: "MabCampaigns");

            migrationBuilder.DropColumn(
                name: "Mab_SilverInflation",
                table: "MabCampaigns");

            migrationBuilder.DropColumn(
                name: "Mab_SteelInflation",
                table: "MabCampaigns");

            migrationBuilder.DropColumn(
                name: "Mab_TitaniumInflation",
                table: "MabCampaigns");
        }
    }
}
