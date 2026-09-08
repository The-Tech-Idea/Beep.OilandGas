using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beep.OilandGas.Repository.Migrations.Oracle
{
    /// <inheritdoc />
    public partial class IdentityExtensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "APP_PERMISSION",
                columns: table => new
                {
                    PERMISSION_ID = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: false),
                    PERMISSION_KEY = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: false),
                    RESOURCE_KEY = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    ACTION_KEY = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    SCOPE_KEY = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    DOMAIN_KEY = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    POLICY_MAPPING_KEY = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    DESCRIPTION = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    RISK_LEVEL = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APP_PERMISSION", x => x.PERMISSION_ID);
                });

            migrationBuilder.CreateTable(
                name: "APP_ROLE",
                columns: table => new
                {
                    ROLE_ID = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    ROLE_TYPE = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    ROLE_CATEGORY = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    SYSTEM_ROLE_IND = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: false),
                    SENSITIVE_ROLE_IND = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: false),
                    SOD_FLAG = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: false),
                    DISPLAY_SORT_ORDER = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    CREATED_UTC = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    VALID_FIELD_SCOPE = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APP_ROLE", x => x.ROLE_ID);
                    table.ForeignKey(
                        name: "FK_APP_ROLE_AspNetRoles_ROLE_~",
                        column: x => x.ROLE_ID,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "APP_USER_ROLE",
                columns: table => new
                {
                    USER_ROLE_ID = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: false),
                    USER_ID = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: false),
                    ROLE_ID = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: false),
                    GRANTED_BY_USER_ID = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    ASSIGNMENT_REASON = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    EFFECTIVE_FROM_UTC = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    EFFECTIVE_TO_UTC = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    APPROVAL_STATUS = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: false),
                    APPROVAL_REFERENCE = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APP_USER_ROLE", x => x.USER_ROLE_ID);
                    table.ForeignKey(
                        name: "FK_APP_USER_ROLE_AspNetRoles_~",
                        column: x => x.ROLE_ID,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_APP_USER_ROLE_AspNetUsers_~",
                        column: x => x.USER_ID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "APP_ROLE_PERMISSION",
                columns: table => new
                {
                    ROLE_PERMISSION_ID = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: false),
                    ROLE_ID = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: false),
                    PERMISSION_ID = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: false),
                    ROLE_CLAIM_ID = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    EFFECTIVE_FROM_UTC = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    EFFECTIVE_TO_UTC = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    SOURCE_SYSTEM = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    APPROVED_BY_USER_ID = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    APPROVED_AT_UTC = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APP_ROLE_PERMISSION", x => x.ROLE_PERMISSION_ID);
                    table.ForeignKey(
                        name: "FK_APP_ROLE_PERMISSION_APP_PE~",
                        column: x => x.PERMISSION_ID,
                        principalTable: "APP_PERMISSION",
                        principalColumn: "PERMISSION_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_APP_ROLE_PERMISSION_AspNet~",
                        column: x => x.ROLE_CLAIM_ID,
                        principalTable: "AspNetRoleClaims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_APP_ROLE_PERMISSION_AspNe~1",
                        column: x => x.ROLE_ID,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_APP_PERMISSION_PERMISSION_~",
                table: "APP_PERMISSION",
                column: "PERMISSION_KEY",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_APP_ROLE_PERMISSION_PERMIS~",
                table: "APP_ROLE_PERMISSION",
                column: "PERMISSION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_APP_ROLE_PERMISSION_ROLE_C~",
                table: "APP_ROLE_PERMISSION",
                column: "ROLE_CLAIM_ID");

            migrationBuilder.CreateIndex(
                name: "IX_APP_ROLE_PERMISSION_ROLE_I~",
                table: "APP_ROLE_PERMISSION",
                columns: new[] { "ROLE_ID", "PERMISSION_ID" });

            migrationBuilder.CreateIndex(
                name: "IX_APP_USER_ROLE_ROLE_ID",
                table: "APP_USER_ROLE",
                column: "ROLE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_APP_USER_ROLE_USER_ID_ROLE~",
                table: "APP_USER_ROLE",
                columns: new[] { "USER_ID", "ROLE_ID" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "APP_ROLE");

            migrationBuilder.DropTable(
                name: "APP_ROLE_PERMISSION");

            migrationBuilder.DropTable(
                name: "APP_USER_ROLE");

            migrationBuilder.DropTable(
                name: "APP_PERMISSION");
        }
    }
}
