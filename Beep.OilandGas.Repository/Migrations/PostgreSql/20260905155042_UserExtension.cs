using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beep.OilandGas.Repository.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class UserExtension : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "APP_USER",
                columns: table => new
                {
                    USER_ID = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    FULL_NAME = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TENANT_ID = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    BUSINESS_ASSOCIATE_ID = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CHANGED_BY = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_UTC = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CHANGED_UTC = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APP_USER", x => x.USER_ID);
                    table.ForeignKey(
                        name: "FK_APP_USER_AspNetUsers_USER_~",
                        column: x => x.USER_ID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "APP_USER");
        }
    }
}
