using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lune.Favorites.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialFavorites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "favorites");

            migrationBuilder.CreateTable(
                name: "Favorites",
                schema: "favorites",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    RawgId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Released = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Rating = table.Column<double>(type: "float", nullable: false),
                    Metacritic = table.Column<int>(type: "int", nullable: true),
                    Genres = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Platforms = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SnapshotUpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favorites", x => new { x.UserId, x.RawgId });
                });

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_RawgId",
                schema: "favorites",
                table: "Favorites",
                column: "RawgId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Favorites",
                schema: "favorites");
        }
    }
}
