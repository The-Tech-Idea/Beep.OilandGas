using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beep.OilandGas.Repository.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class CompanyPersonaScope : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_APP_USER_PERSONA",
                table: "APP_USER_PERSONA");

            migrationBuilder.DropPrimaryKey(
                name: "PK_APP_PERSONA_PREFERENCE",
                table: "APP_PERSONA_PREFERENCE");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "APP_USER_PERSONA",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BusinessAssociateId",
                table: "APP_USER_PERSONA",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "APP_PERSONA_PREFERENCE",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BusinessAssociateId",
                table: "APP_PERSONA_PREFERENCE",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_APP_USER_PERSONA",
                table: "APP_USER_PERSONA",
                columns: new[] { "UserId", "TenantId", "BusinessAssociateId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_APP_PERSONA_PREFERENCE",
                table: "APP_PERSONA_PREFERENCE",
                columns: new[] { "UserId", "TenantId", "BusinessAssociateId", "PersonaCode", "ViewKey" })
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_APP_USER_PERSONA_TenantId_~",
                table: "APP_USER_PERSONA",
                columns: new[] { "TenantId", "BusinessAssociateId" });

            migrationBuilder.CreateIndex(
                name: "IX_APP_PERSONA_PREFERENCE_Ten~",
                table: "APP_PERSONA_PREFERENCE",
                columns: new[] { "TenantId", "BusinessAssociateId" });

            migrationBuilder.AddForeignKey(
                name: "FK_APP_PERSONA_PREFERENCE_AP~1",
                table: "APP_PERSONA_PREFERENCE",
                columns: new[] { "TenantId", "BusinessAssociateId" },
                principalTable: "APP_COMPANY",
                principalColumns: new[] { "TenantId", "BusinessAssociateId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_APP_USER_PERSONA_APP_COMPA~",
                table: "APP_USER_PERSONA",
                columns: new[] { "TenantId", "BusinessAssociateId" },
                principalTable: "APP_COMPANY",
                principalColumns: new[] { "TenantId", "BusinessAssociateId" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new System.NotSupportedException("Company-scoped persona settings cannot be reduced to one account-wide record without losing data.");
        }
    }
}
