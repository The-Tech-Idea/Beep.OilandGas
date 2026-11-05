using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beep.OilandGas.Repository.Migrations.Oracle
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
                type: "NVARCHAR2(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetVerifiedByUserId",
                table: "APP_MODULE_CONNECTION",
                type: "NVARCHAR2(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TargetVerifiedUtc",
                table: "APP_MODULE_CONNECTION",
                type: "TIMESTAMP(7)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_APP_MOD_CONN_TARGET",
                table: "APP_MODULE_CONNECTION",
                column: "PhysicalTargetKey",
                unique: true,
                filter: "\"PhysicalTargetKey\" IS NOT NULL");
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
