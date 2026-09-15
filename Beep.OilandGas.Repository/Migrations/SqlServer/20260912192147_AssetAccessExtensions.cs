using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beep.OilandGas.Repository.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AssetAccessExtensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "APP_USER_ASSET_ACCESS",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    DatabaseScope = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AssetType = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    AssetId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    OrganizationId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    AccessLevel = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    Inherit = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APP_USER_ASSET_ACCESS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_APP_USER_ASSET_ACCESS_AspN~",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_APP_USER_ASSET_ACCESS_User~",
                table: "APP_USER_ASSET_ACCESS",
                columns: new[] { "UserId", "DatabaseScope" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "APP_USER_ASSET_ACCESS");
        }
    }
}
