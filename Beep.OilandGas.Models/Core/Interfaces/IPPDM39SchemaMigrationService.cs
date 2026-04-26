using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.DataManagement;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Governs the schema create/update lifecycle through MigrationManager:
    /// privilege check → plan → preflight → approve → execute/resume → progress → artifacts.
    /// All destructive schema operations (drop, recreate, copy) route through this service.
    /// </summary>
    public interface IPPDM39SchemaMigrationService
    {
        // ── Privilege and provider readiness ──────────────────────────────
        Task<SchemaPrivilegeCheckResult> CheckSchemaPrivilegesAsync(ConnectionConfig config, string? schemaName = null);

        // ── Schema bootstrap operations ───────────────────────────────────
        Task<CreateSqliteResult> CreateSqliteAsync(CreateSqliteRequest request);
        Task<CreateSchemaResult> CreateSchemaAsync(ConnectionConfig config, string schemaName);
        Task<DropDatabaseResult> DropDatabaseAsync(string connectionName, string? schemaName, bool dropIfExists = true);
        Task<RecreateDatabaseResult> RecreateDatabaseAsync(string connectionName, string? schemaName, bool backupFirst = false, string? backupPath = null);
        Task<CopyDatabaseResult> CopyDatabaseAsync(CopyDatabaseRequest request, string? operationId = null);

        // ── Migration plan pipeline ────────────────────────────────────────
        Task<SchemaMigrationPlanResult> PlanSchemaMigrationAsync(SchemaMigrationPlanRequest request);
        Task<SchemaMigrationApprovalResult> ApproveSchemaMigrationPlanAsync(SchemaMigrationApprovalRequest request);
        Task<SchemaMigrationCiValidationResult> ValidateMigrationPlanForCiAsync(string planId);
        Task<SchemaMigrationExecuteResult> ExecuteSchemaMigrationPlanAsync(SchemaMigrationExecuteRequest request);
        Task<OperationStartResponse> StartSchemaMigrationExecutionAsync(SchemaMigrationExecuteRequest request);
        Task<SchemaMigrationProgressResult> GetSchemaMigrationProgressAsync(string executionToken);
        Task<SchemaMigrationArtifactsResult> GetSchemaMigrationArtifactsAsync(string planId);

        // ── Setup status ───────────────────────────────────────────────────
        Task<SetupStatusResult> GetSetupStatusAsync();
    }
}
