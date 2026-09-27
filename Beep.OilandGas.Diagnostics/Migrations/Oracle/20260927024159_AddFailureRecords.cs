using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beep.OilandGas.Diagnostics.Migrations.Oracle
{
    /// <inheritdoc />
    public partial class AddFailureRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FailureRecords",
                columns: table => new
                {
                    Id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Reference = table.Column<string>(type: "NVARCHAR2(32)", maxLength: 32, nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    Severity = table.Column<string>(type: "NVARCHAR2(16)", maxLength: 16, nullable: false),
                    Operation = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    Consequence = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: false),
                    ExceptionType = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    Detail = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    RequestPath = table.Column<string>(type: "NCLOB", maxLength: 2048, nullable: true),
                    TraceId = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: true),
                    ResolvedAtUtc = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    ResolvedByUserId = table.Column<string>(type: "NVARCHAR2(450)", maxLength: 450, nullable: true),
                    Resolution = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FailureRecords", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FailureRecords_OccurredAtU~",
                table: "FailureRecords",
                column: "OccurredAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_FailureRecords_Reference",
                table: "FailureRecords",
                column: "Reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FailureRecords_ResolvedAtU~",
                table: "FailureRecords",
                column: "ResolvedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FailureRecords");
        }
    }
}
