using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beep.OilandGas.Repository.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class CompanyMembershipRetirement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "APP_COMPANY_MEMBER_RETIREMENT",
                columns: table => new
                {
                    IntentId = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TenantId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    BusinessAssociateId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    UserId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ActorUserId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    PreviousRevision = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    ResultRevision = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    EffectiveFromUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PreviousEffectiveToUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EffectiveToUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RetiredUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APP_COMPANY_MEMBER_RETIREM~", x => x.IntentId);
                    table.ForeignKey(
                        name: "FK_APP_COMPANY_MEMBER_RETIREM~",
                        column: x => x.ActorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_APP_COMPANY_MEMBER_RETIRE~1",
                        columns: x => new { x.TenantId, x.BusinessAssociateId, x.UserId },
                        principalTable: "APP_COMPANY_MEMBERSHIP",
                        principalColumns: new[] { "TenantId", "BusinessAssociateId", "UserId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_APP_COMPANY_MEMBER_RETIRE~1",
                table: "APP_COMPANY_MEMBER_RETIREMENT",
                columns: new[] { "TenantId", "BusinessAssociateId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_APP_COMPANY_MEMBER_RETIREM~",
                table: "APP_COMPANY_MEMBER_RETIREMENT",
                column: "ActorUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "APP_COMPANY_MEMBER_RETIREMENT");
        }
    }
}
