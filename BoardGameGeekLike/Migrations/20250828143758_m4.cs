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
            migrationBuilder.RenameColumn(
                name: "Mab_OpenedBoostersOpened",
                table: "MabCampaigns",
                newName: "Mab_OpenedBoostersCount");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Mab_OpenedBoostersCount",
                table: "MabCampaigns",
                newName: "Mab_OpenedBoostersOpened");
        }
    }
}
