using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.DataManagement;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Governs the schema create/update lifecycle through MigrationManager:
    /// plan → preflight → approve → execute/resume → progress → artifacts.
    /// </summary>
    public interface IPPDM39SchemaMigrationService
    {
        // ── Schema bootstrap operations ───────────────────────────────────
        Task<CreateSqliteResult> CreateSqliteAsync(CreateSqliteRequest request);
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
