using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beep.OilandGas.Repository.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class CompanyHierarchyConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ORGANIZATION_HIERARCHY_CONFIG",
                columns: table => new
                {
                    CONFIG_ID = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TENANT_ID = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ORGANIZATION_ID = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    LEVELS_JSON = table.Column<string>(type: "character varying(16000)", maxLength: 16000, nullable: false),
                    REVISION = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CREATED_UTC = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CHANGED_BY = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CHANGED_UTC = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ORGANIZATION_HIERARCHY_CON~", x => x.CONFIG_ID);
                    table.ForeignKey(
                        name: "FK_ORGANIZATION_HIERARCHY_CON~",
                        column: x => x.CHANGED_BY,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ORGANIZATION_HIERARCHY_CO~1",
                        column: x => x.CREATED_BY,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ORGANIZATION_HIERARCHY_CO~2",
                        columns: x => new { x.TENANT_ID, x.ORGANIZATION_ID },
                        principalTable: "APP_COMPANY",
                        principalColumns: new[] { "TenantId", "BusinessAssociateId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "APP_HIERARCHY_REVISION",
                columns: table => new
                {
                    Revision = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    ConfigId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PreviousRevision = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    LevelsJson = table.Column<string>(type: "character varying(16000)", maxLength: 16000, nullable: false),
                    ActorUserId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ChangedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APP_HIERARCHY_REVISION", x => x.Revision);
                    table.ForeignKey(
                        name: "FK_APP_HIERARCHY_REVISION_Asp~",
                        column: x => x.ActorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_APP_HIERARCHY_REVISION_ORG~",
                        column: x => x.ConfigId,
                        principalTable: "ORGANIZATION_HIERARCHY_CONFIG",
                        principalColumn: "CONFIG_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_APP_HIERARCHY_REVISION_Act~",
                table: "APP_HIERARCHY_REVISION",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_APP_HIERARCHY_REVISION_Con~",
                table: "APP_HIERARCHY_REVISION",
                column: "ConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_HIERARCHY_COMPANY",
                table: "ORGANIZATION_HIERARCHY_CONFIG",
                columns: new[] { "TENANT_ID", "ORGANIZATION_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ORGANIZATION_HIERARCHY_CO~1",
                table: "ORGANIZATION_HIERARCHY_CONFIG",
                column: "CREATED_BY");

            migrationBuilder.CreateIndex(
                name: "IX_ORGANIZATION_HIERARCHY_CON~",
                table: "ORGANIZATION_HIERARCHY_CONFIG",
                column: "CHANGED_BY");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "APP_HIERARCHY_REVISION");

            migrationBuilder.DropTable(
                name: "ORGANIZATION_HIERARCHY_CONFIG");
        }
    }
}
