using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class SchemaMigrationPlanRequest : ModelEntityBase
    {
        public string ConnectionName { get; set; } = string.Empty;
        public string? SchemaName { get; set; }
        public string? TargetAssemblyName { get; set; }
        public string? TargetModelNamespace { get; set; }
        public string EnvironmentTier { get; set; } = "Development";
        public bool BackupConfirmed { get; set; }
        public bool RestoreTestEvidenceProvided { get; set; }
        public string? RestoreTestEvidence { get; set; }
    }

    public class SchemaMigrationPlanResult : ModelEntityBase
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ConnectionName { get; set; } = string.Empty;
        public string PlanId { get; set; } = string.Empty;
        public string PlanHash { get; set; } = string.Empty;
        public string PolicyDecision { get; set; } = string.Empty;
        public bool RequiresManualApproval { get; set; }
        public bool CanApply { get; set; }
        public bool IsApproved { get; set; }
        public int TotalEntities { get; set; }
        public int PendingOperationCount { get; set; }
        public int TablesToCreate { get; set; }
        public int ColumnsToAdd { get; set; }
        public int HighRiskOperationCount { get; set; }
        public List<SchemaMigrationCiGateResult> CiGates { get; set; } = new();
        public List<SchemaMigrationPreflightCheckResult> PreflightChecks { get; set; } = new();
        public List<SchemaMigrationPolicyFindingResult> PolicyFindings { get; set; } = new();
        public List<SchemaMigrationDryRunOperationResult> DryRunOperations { get; set; } = new();
        public List<string> DryRunDiagnostics { get; set; } = new();
        public List<string> CompensationActions { get; set; } = new();
        public List<string> RollbackChecks { get; set; } = new();
    }

    public class SchemaMigrationApprovalRequest : ModelEntityBase
    {
        public string PlanId { get; set; } = string.Empty;
        public string ApprovedBy { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class SchemaMigrationApprovalResult : ModelEntityBase
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string PlanId { get; set; } = string.Empty;
        public string PlanHash { get; set; } = string.Empty;
        public string ApprovedBy { get; set; } = string.Empty;
        public DateTime? ApprovedOnUtc { get; set; }
    }

    public class SchemaMigrationExecuteRequest : ModelEntityBase
    {
        public string PlanId { get; set; } = string.Empty;
        public string ExecutedBy { get; set; } = string.Empty;
        public bool ResumeIfCheckpointExists { get; set; }
    }

    public class SchemaMigrationExecuteResult : ModelEntityBase
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string PlanId { get; set; } = string.Empty;
        public string PlanHash { get; set; } = string.Empty;
        public string ExecutionToken { get; set; } = string.Empty;
        public bool ResumedFromCheckpoint { get; set; }
        public bool RequiresOperatorIntervention { get; set; }
        public string RollbackOutcome { get; set; } = string.Empty;
        public string CompensationOutcome { get; set; } = string.Empty;
        public int TotalSteps { get; set; }
        public int CompletedSteps { get; set; }
    }

    public class SchemaMigrationProgressResult : ModelEntityBase
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string PlanId { get; set; } = string.Empty;
        public string PlanHash { get; set; } = string.Empty;
        public string ExecutionToken { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public bool HasFailed { get; set; }
        public string FailureCategory { get; set; } = string.Empty;
        public string FailureReason { get; set; } = string.Empty;
        public int LastCompletedStep { get; set; }
        public int TotalSteps { get; set; }
        public int CompletedSteps { get; set; }
        public List<SchemaMigrationStepResult> Steps { get; set; } = new();
    }

    public class SchemaMigrationArtifactsResult : ModelEntityBase
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string PlanId { get; set; } = string.Empty;
        public string PlanHash { get; set; } = string.Empty;
        public string ConnectionName { get; set; } = string.Empty;
        public bool IsApproved { get; set; }
        public string ApprovedBy { get; set; } = string.Empty;
        public DateTime? ApprovedOnUtc { get; set; }
        public string ApprovalNotes { get; set; } = string.Empty;
        public string PlanJson { get; set; } = string.Empty;
        public string DryRunJson { get; set; } = string.Empty;
        public string PreflightJson { get; set; } = string.Empty;
        public string CiValidationJson { get; set; } = string.Empty;
        public string PolicyJson { get; set; } = string.Empty;
        public string RollbackReadinessJson { get; set; } = string.Empty;
        public string CompensationJson { get; set; } = string.Empty;
        public string RuntimeEntityMetadataJson { get; set; } = string.Empty;
        public string ApprovalSummaryMarkdown { get; set; } = string.Empty;
    }

    public class SchemaMigrationEntityMetadata : ModelEntityBase
    {
        public string EntityName { get; set; } = string.Empty;
        public string EntityTypeName { get; set; } = string.Empty;
        public string ResolvedTableName { get; set; } = string.Empty;
        public string ConventionTableName { get; set; } = string.Empty;
        public List<string> PrimaryKeyColumns { get; set; } = new();
        public List<string> JsonColumns { get; set; } = new();
        public List<string> IndicatorColumns { get; set; } = new();
        public List<SchemaMigrationEntityColumnMetadata> Columns { get; set; } = new();
    }

    public class SchemaMigrationEntityColumnMetadata : ModelEntityBase
    {
        public string PropertyName { get; set; } = string.Empty;
        public string ColumnName { get; set; } = string.Empty;
        public string ClrType { get; set; } = string.Empty;
        public bool IsNullable { get; set; }
        public bool IsPrimaryKeyCandidate { get; set; }
        public bool IsJsonPayload { get; set; }
        public bool IsIndicatorFlag { get; set; }
    }

    public class SchemaMigrationStepResult : ModelEntityBase
    {
        public int Sequence { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public string OperationKind { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int AttemptCount { get; set; }
    }

    public class SchemaMigrationCiGateResult : ModelEntityBase
    {
        public string Gate { get; set; } = string.Empty;
        public string Decision { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class SchemaMigrationPreflightCheckResult : ModelEntityBase
    {
        public string Code { get; set; } = string.Empty;
        public string Decision { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
    }

    public class SchemaMigrationPolicyFindingResult : ModelEntityBase
    {
        public string RuleId { get; set; } = string.Empty;
        public string Decision { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string OperationKind { get; set; } = string.Empty;
        public string RiskLevel { get; set; } = string.Empty;
    }

    public class SchemaMigrationDryRunOperationResult : ModelEntityBase
    {
        public string EntityName { get; set; } = string.Empty;
        public string Kind { get; set; } = string.Empty;
        public string RiskLevel { get; set; } = string.Empty;
        public List<string> DdlPreview { get; set; } = new();
        public List<string> RiskTags { get; set; } = new();
        public List<string> Diagnostics { get; set; } = new();
    }
}