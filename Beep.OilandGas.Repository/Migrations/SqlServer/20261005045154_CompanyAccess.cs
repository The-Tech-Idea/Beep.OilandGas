using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beep.OilandGas.Repository.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class CompanyAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BUSINESS_ASSOCIATE_ID",
                table: "APP_USER_ROLE",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TENANT_ID",
                table: "APP_USER_ROLE",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "APP_TENANT",
                columns: table => new
                {
                    TenantId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APP_TENANT", x => x.TenantId);
                });

            migrationBuilder.CreateTable(
                name: "APP_COMPANY",
                columns: table => new
                {
                    TenantId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    BusinessAssociateId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APP_COMPANY", x => new { x.TenantId, x.BusinessAssociateId });
                    table.ForeignKey(
                        name: "FK_APP_COMPANY_APP_TENANT_Ten~",
                        column: x => x.TenantId,
                        principalTable: "APP_TENANT",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "APP_COMPANY_MEMBERSHIP",
                columns: table => new
                {
                    TenantId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    BusinessAssociateId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    EffectiveFromUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveToUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GrantedByUserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ChangedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APP_COMPANY_MEMBERSHIP", x => new { x.TenantId, x.BusinessAssociateId, x.UserId });
                    table.ForeignKey(
                        name: "FK_APP_COMPANY_MEMBERSHIP_APP~",
                        columns: x => new { x.TenantId, x.BusinessAssociateId },
                        principalTable: "APP_COMPANY",
                        principalColumns: new[] { "TenantId", "BusinessAssociateId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_APP_COMPANY_MEMBERSHIP_Asp~",
                        column: x => x.GrantedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_APP_COMPANY_MEMBERSHIP_As~1",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_APP_USER_ROLE_TENANT_ID_BU~",
                table: "APP_USER_ROLE",
                columns: new[] { "TENANT_ID", "BUSINESS_ASSOCIATE_ID", "USER_ID" });

            migrationBuilder.CreateIndex(
                name: "IX_APP_COMPANY_MEMBERSHIP_Gra~",
                table: "APP_COMPANY_MEMBERSHIP",
                column: "GrantedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_APP_COMPANY_MEMBERSHIP_Use~",
                table: "APP_COMPANY_MEMBERSHIP",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_APP_USER_ROLE_APP_COMPANY_~",
                table: "APP_USER_ROLE",
                columns: new[] { "TENANT_ID", "BUSINESS_ASSOCIATE_ID", "USER_ID" },
                principalTable: "APP_COMPANY_MEMBERSHIP",
                principalColumns: new[] { "TenantId", "BusinessAssociateId", "UserId" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_APP_USER_ROLE_APP_COMPANY_~",
                table: "APP_USER_ROLE");

            migrationBuilder.DropTable(
                name: "APP_COMPANY_MEMBERSHIP");

            migrationBuilder.DropTable(
                name: "APP_COMPANY");

            migrationBuilder.DropTable(
                name: "APP_TENANT");

            migrationBuilder.DropIndex(
                name: "IX_APP_USER_ROLE_TENANT_ID_BU~",
                table: "APP_USER_ROLE");

            migrationBuilder.DropColumn(
                name: "BUSINESS_ASSOCIATE_ID",
                table: "APP_USER_ROLE");

            migrationBuilder.DropColumn(
                name: "TENANT_ID",
                table: "APP_USER_ROLE");
        }
    }
}
