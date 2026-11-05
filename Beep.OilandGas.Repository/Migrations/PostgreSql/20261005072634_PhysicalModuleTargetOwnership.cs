using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beep.OilandGas.Repository.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class PhysicalModuleTargetOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhysicalTargetKey",
                table: "APP_MODULE_CONNECTION",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetVerifiedByUserId",
                table: "APP_MODULE_CONNECTION",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TargetVerifiedUtc",
                table: "APP_MODULE_CONNECTION",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_APP_MOD_CONN_TARGET",
                table: "APP_MODULE_CONNECTION",
                column: "PhysicalTargetKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_APP_MOD_CONN_TARGET",
                table: "APP_MODULE_CONNECTION");

            migrationBuilder.DropColumn(
                name: "PhysicalTargetKey",
                table: "APP_MODULE_CONNECTION");

            migrationBuilder.DropColumn(
                name: "TargetVerifiedByUserId",
                table: "APP_MODULE_CONNECTION");

            migrationBuilder.DropColumn(
                name: "TargetVerifiedUtc",
                table: "APP_MODULE_CONNECTION");
        }
    }
}
