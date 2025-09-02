using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoardGameGeekLike.Migrations
{
    /// <inheritdoc />
    public partial class m8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DoesPlayerGoesFirst",
                table: "MabBattles",
                newName: "Mab_DoesPlayerGoesFirst");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Mab_DoesPlayerGoesFirst",
                table: "MabBattles",
                newName: "DoesPlayerGoesFirst");
        }
    }
}
