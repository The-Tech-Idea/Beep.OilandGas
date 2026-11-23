using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beep.OilandGas.Diagnostics.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddBackchannelLogoutRevocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BackchannelLogoutRevocations",
                columns: table => new
                {
                    Subject = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    RevokedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackchannelLogoutRevocatio~", x => x.Subject);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BackchannelLogoutRevocatio~",
                table: "BackchannelLogoutRevocations",
                column: "ExpiresAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BackchannelLogoutRevocations");
        }
    }
}
