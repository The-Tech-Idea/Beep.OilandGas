using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beep.OilandGas.Repository.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class UserProfileMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LAST_LOGIN_UTC",
                table: "APP_USER",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PREFERENCES_JSON",
                table: "APP_USER",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PREFERRED_LAYOUT",
                table: "APP_USER",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PRIMARY_ROLE_ID",
                table: "APP_USER",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_APP_USER_PRIMARY_ROLE_ID",
                table: "APP_USER",
                column: "PRIMARY_ROLE_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_APP_USER_AspNetRoles_PRIMA~",
                table: "APP_USER",
                column: "PRIMARY_ROLE_ID",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_APP_USER_AspNetRoles_PRIMA~",
                table: "APP_USER");

            migrationBuilder.DropIndex(
                name: "IX_APP_USER_PRIMARY_ROLE_ID",
                table: "APP_USER");

            migrationBuilder.DropColumn(
                name: "LAST_LOGIN_UTC",
                table: "APP_USER");

            migrationBuilder.DropColumn(
                name: "PREFERENCES_JSON",
                table: "APP_USER");

            migrationBuilder.DropColumn(
                name: "PREFERRED_LAYOUT",
                table: "APP_USER");

            migrationBuilder.DropColumn(
                name: "PRIMARY_ROLE_ID",
                table: "APP_USER");
        }
    }
}
