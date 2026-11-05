using Beep.OilandGas.Models.Data.AccessControl;
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beep.OilandGas.Repository.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AssetGrantRetirement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "APP_ASSET_GRANT_RETIREMENT",
                columns: table => new
                {
                    Revision = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    GrantId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    OrganizationId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    DatabaseScope = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    PreviousRevision = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    SnapshotJson = table.Column<string>(type: "nvarchar(max)", maxLength: 16000, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ActorUserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ChangedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APP_ASSET_GRANT_RETIREMENT", x => x.Revision);
                    table.ForeignKey(
                        name: "FK_APP_ASSET_GRANT_RETIREMENT~",
                        column: x => x.ActorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_APP_ASSET_GRANT_RETIREMEN~1",
                        column: x => x.GrantId,
                        principalTable: "APP_USER_ASSET_ACCESS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_APP_ASSET_GRANT_RETIREMEN~2",
                        columns: x => new { x.TenantId, x.OrganizationId },
                        principalTable: "APP_COMPANY",
                        principalColumns: new[] { "TenantId", "BusinessAssociateId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_APP_ASSET_GRANT_RETIREMEN~1",
                table: "APP_ASSET_GRANT_RETIREMENT",
                columns: new[] { "GrantId", "ChangedUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_APP_ASSET_GRANT_RETIREMEN~2",
                table: "APP_ASSET_GRANT_RETIREMENT",
                columns: new[] { "TenantId", "OrganizationId" });

            migrationBuilder.CreateIndex(
                name: "IX_APP_ASSET_GRANT_RETIREMENT~",
                table: "APP_ASSET_GRANT_RETIREMENT",
                column: "ActorUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "APP_ASSET_GRANT_RETIREMENT");
        }
    }
}
