using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beep.OilandGas.Repository.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class PersonaExtensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "APP_PERSONA",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DefaultRoute = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APP_PERSONA", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "APP_PERSONA_AUDIT",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ActorUserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    BeforeJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AfterJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChangedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APP_PERSONA_AUDIT", x => x.Id);
                    table.ForeignKey(
                        name: "FK_APP_PERSONA_AUDIT_AspNetUs~",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "APP_PERSONA_PREFERENCE",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    PersonaCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ViewKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ChangedBy = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ChangedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APP_PERSONA_PREFERENCE", x => new { x.UserId, x.PersonaCode, x.ViewKey });
                    table.ForeignKey(
                        name: "FK_APP_PERSONA_PREFERENCE_APP~",
                        column: x => x.PersonaCode,
                        principalTable: "APP_PERSONA",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_APP_PERSONA_PREFERENCE_Asp~",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "APP_USER_PERSONA",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    PersonaCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Locale = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    TimeZone = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    UnitSystem = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    DefaultFieldId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ChangedBy = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ChangedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APP_USER_PERSONA", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_APP_USER_PERSONA_APP_PERSO~",
                        column: x => x.PersonaCode,
                        principalTable: "APP_PERSONA",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_APP_USER_PERSONA_AspNetUse~",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_APP_PERSONA_AUDIT_UserId_C~",
                table: "APP_PERSONA_AUDIT",
                columns: new[] { "UserId", "ChangedUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_APP_PERSONA_PREFERENCE_Per~",
                table: "APP_PERSONA_PREFERENCE",
                column: "PersonaCode");

            migrationBuilder.CreateIndex(
                name: "IX_APP_USER_PERSONA_PersonaCo~",
                table: "APP_USER_PERSONA",
                column: "PersonaCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "APP_PERSONA_AUDIT");

            migrationBuilder.DropTable(
                name: "APP_PERSONA_PREFERENCE");

            migrationBuilder.DropTable(
                name: "APP_USER_PERSONA");

            migrationBuilder.DropTable(
                name: "APP_PERSONA");
        }
    }
}
