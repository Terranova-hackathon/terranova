using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lune.Auth.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPrivacyConsent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PrivacyConsentedAtUtc",
                schema: "auth",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrivacyConsentedAtUtc",
                schema: "auth",
                table: "AspNetUsers");
        }
    }
}
