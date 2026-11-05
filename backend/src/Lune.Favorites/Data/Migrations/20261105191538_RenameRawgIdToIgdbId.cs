using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lune.Favorites.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameRawgIdToIgdbId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RawgId",
                schema: "favorites",
                table: "Favorites",
                newName: "IgdbId");

            migrationBuilder.RenameIndex(
                name: "IX_Favorites_RawgId",
                schema: "favorites",
                table: "Favorites",
                newName: "IX_Favorites_IgdbId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IgdbId",
                schema: "favorites",
                table: "Favorites",
                newName: "RawgId");

            migrationBuilder.RenameIndex(
                name: "IX_Favorites_IgdbId",
                schema: "favorites",
                table: "Favorites",
                newName: "IX_Favorites_RawgId");
        }
    }
}
