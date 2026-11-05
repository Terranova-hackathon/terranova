using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lune.Favorites.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameImageUrlToBackgroundImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                schema: "favorites",
                table: "Favorites",
                newName: "BackgroundImage");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BackgroundImage",
                schema: "favorites",
                table: "Favorites",
                newName: "ImageUrl");
        }
    }
}
