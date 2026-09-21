using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beep.OilandGas.Repository.Migrations.Oracle
{
    /// <inheritdoc />
    public partial class FinancialOperationClaims : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FINANCIAL_OPERATION_CLAIM",
                columns: table => new
                {
                    OperationKey = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: false),
                    Version = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    OwnerId = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: true),
                    Token = table.Column<string>(type: "NVARCHAR2(36)", maxLength: 36, nullable: true),
                    ChangedBy = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: false),
                    ChangedUtc = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FINANCIAL_OPERATION_CLAIM", x => x.OperationKey);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FINANCIAL_OPERATION_CLAIM");
        }
    }
}
