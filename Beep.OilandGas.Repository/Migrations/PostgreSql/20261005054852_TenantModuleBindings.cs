using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Beep.OilandGas.Repository.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class TenantModuleBindings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ModuleDatabaseBindings",
                table: "ModuleDatabaseBindings");

            migrationBuilder.AddColumn<long>(
                name: "BindingId",
                table: "ModuleDatabaseBindings",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "ConnectionKey",
                table: "ModuleDatabaseBindings",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnershipAssignedByUserId",
                table: "ModuleDatabaseBindings",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OwnershipAssignedUtc",
                table: "ModuleDatabaseBindings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnershipEvidence",
                table: "ModuleDatabaseBindings",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "ModuleDatabaseBindings",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ModuleDatabaseBindings",
                table: "ModuleDatabaseBindings",
                column: "BindingId");

            migrationBuilder.CreateTable(
                name: "APP_MODULE_CONNECTION",
                columns: table => new
                {
                    ConnectionKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TenantId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AssignedByUserId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    AssignedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APP_MODULE_CONNECTION", x => x.ConnectionKey);
                    table.UniqueConstraint("AK_APP_MODULE_CONNECTION_Tena~", x => new { x.TenantId, x.ConnectionKey });
                    table.ForeignKey(
                        name: "FK_APP_MODULE_CONNECTION_APP_~",
                        column: x => x.TenantId,
                        principalTable: "APP_TENANT",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModuleDatabaseBindings_Te~1",
                table: "ModuleDatabaseBindings",
                columns: new[] { "TenantId", "ModuleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModuleDatabaseBindings_Ten~",
                table: "ModuleDatabaseBindings",
                columns: new[] { "TenantId", "ConnectionKey" });

            migrationBuilder.AddForeignKey(
                name: "FK_ModuleDatabaseBindings_APP~",
                table: "ModuleDatabaseBindings",
                column: "TenantId",
                principalTable: "APP_TENANT",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ModuleDatabaseBindings_AP~1",
                table: "ModuleDatabaseBindings",
                columns: new[] { "TenantId", "ConnectionKey" },
                principalTable: "APP_MODULE_CONNECTION",
                principalColumns: new[] { "TenantId", "ConnectionKey" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ModuleDatabaseBindings_APP~",
                table: "ModuleDatabaseBindings");

            migrationBuilder.DropForeignKey(
                name: "FK_ModuleDatabaseBindings_AP~1",
                table: "ModuleDatabaseBindings");

            migrationBuilder.DropTable(
                name: "APP_MODULE_CONNECTION");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ModuleDatabaseBindings",
                table: "ModuleDatabaseBindings");

            migrationBuilder.DropIndex(
                name: "IX_ModuleDatabaseBindings_Te~1",
                table: "ModuleDatabaseBindings");

            migrationBuilder.DropIndex(
                name: "IX_ModuleDatabaseBindings_Ten~",
                table: "ModuleDatabaseBindings");

            migrationBuilder.DropColumn(
                name: "BindingId",
                table: "ModuleDatabaseBindings");

            migrationBuilder.DropColumn(
                name: "ConnectionKey",
                table: "ModuleDatabaseBindings");

            migrationBuilder.DropColumn(
                name: "OwnershipAssignedByUserId",
                table: "ModuleDatabaseBindings");

            migrationBuilder.DropColumn(
                name: "OwnershipAssignedUtc",
                table: "ModuleDatabaseBindings");

            migrationBuilder.DropColumn(
                name: "OwnershipEvidence",
                table: "ModuleDatabaseBindings");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ModuleDatabaseBindings");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ModuleDatabaseBindings",
                table: "ModuleDatabaseBindings",
                column: "ModuleId");
        }
    }
}
