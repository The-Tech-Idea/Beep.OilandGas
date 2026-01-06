using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.DTOs
{
    #region Validation DTOs

    /// <summary>
    /// Result of entity validation
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string FieldName { get; set; } = string.Empty;
          public Dictionary<string, object> RuleParameters { get; set; } = new Dictionary<string, object>();
        public List<ValidationError> Errors { get; set; } = new List<ValidationError>();
        public List<ValidationWarning> Warnings { get; set; } = new List<ValidationWarning>();
        public object Entity { get; set; }
        public string TableName { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;

        public string ValidationId { get; set; } = string.Empty;

        public DateTime ValidatedDate { get; set; } = DateTime.UtcNow;

           public string RuleId { get; set; } = string.Empty;
            public string RuleName { get; set; } = string.Empty;
            public string RuleType { get; set; } = string.Empty; // REQUIRED, RANGE, FORMAT, BUSINESS_RULE
    }

    /// <summary>
    /// Validation error
    /// </summary>
    public class ValidationError
    {
        public string FieldName { get; set; }
        public string ErrorMessage { get; set; }
        public string RuleName { get; set; }
        public ValidationSeverity Severity { get; set; }
    }

    /// <summary>
    /// Validation warning
    /// </summary>
    public class ValidationWarning
    {
        public string FieldName { get; set; }
        public string WarningMessage { get; set; }
        public string RuleName { get; set; }
    }

    /// <summary>
    /// Validation rule definition
    /// </summary>
    public class ValidationRule
    {
        public string RuleName { get; set; }
        public string FieldName { get; set; }
        public ValidationRuleType RuleType { get; set; }
        public string RuleValue { get; set; }
        public string ErrorMessage { get; set; }
        public ValidationSeverity Severity { get; set; }
        public bool IsActive { get; set; }
      
        public Dictionary<string, object> RuleParameters { get; set; } = new Dictionary<string, object>();
        public string RuleId { get; set; } = string.Empty;
       
    }

    public enum ValidationRuleType
    {
        Required,
        MaxLength,
        MinLength,
        Range,
        Pattern,
        Custom,
        ForeignKey,
        Unique,
        DateRange,
        Format,
        BusinessRule
    }

    public enum ValidationSeverity
    {
        Error,
        Warning,
        Info
    }

    #endregion

    #region Data Quality DTOs

    /// <summary>
    /// Data quality score for an entity
    /// </summary>
    public class DataQualityScore
    {
        public object Entity { get; set; }
        public string TableName { get; set; }
        public double OverallScore { get; set; } // 0-100
        public Dictionary<string, double> FieldScores { get; set; } = new Dictionary<string, double>();
        public List<DataQualityIssue> Issues { get; set; } = new List<DataQualityIssue>();
    }

    /// <summary>
    /// Data quality metrics for a table
    /// </summary>
    public class DataQualityMetrics
    {
        public string TableName { get; set; }
        public int TotalRecords { get; set; }
        public int CompleteRecords { get; set; }
        public int IncompleteRecords { get; set; }
        public double CompletenessScore { get; set; } // 0-100
        public double AccuracyScore { get; set; } // 0-100
        public double ConsistencyScore { get; set; } // 0-100
        public double OverallQualityScore { get; set; } // 0-100
        public Dictionary<string, FieldQualityMetrics> FieldMetrics { get; set; } = new Dictionary<string, FieldQualityMetrics>();
    }

    /// <summary>
    /// Quality metrics for a specific field
    /// </summary>
    public class FieldQualityMetrics
    {
        public string FieldName { get; set; }
        public int TotalValues { get; set; }
        public int NullValues { get; set; }
        public int EmptyValues { get; set; }
        public int ValidValues { get; set; }
        public double Completeness { get; set; } // 0-100
        public List<string> InvalidValues { get; set; } = new List<string>();
    }

    /// <summary>
    /// Data quality issue
    /// </summary>
    public class DataQualityIssue
    {
        public string TableName { get; set; }
        public string FieldName { get; set; }
        public string IssueType { get; set; }
        public string IssueDescription { get; set; }
        public object RecordId { get; set; }
        public string Severity { get; set; }
    }

    #endregion

    #region Reconciliation DTOs

    /// <summary>
    /// Result of data reconciliation
    /// </summary>
    public class ReconciliationResult
    {
        public string TableName { get; set; }
        public int Source1RecordCount { get; set; }
        public int Source2RecordCount { get; set; }
        public int MatchingRecords { get; set; }
        public int Source1OnlyRecords { get; set; }
        public int Source2OnlyRecords { get; set; }
        public int DifferingRecords { get; set; }
        public List<ReconciliationDifference> Differences { get; set; } = new List<ReconciliationDifference>();
    }

    /// <summary>
    /// Difference found during reconciliation
    /// </summary>
    public class ReconciliationDifference
    {
        public object RecordId { get; set; }
        public List<FieldDifference> FieldDifferences { get; set; } = new List<FieldDifference>();
        public string Source1Value { get; set; }
        public string Source2Value { get; set; }
    }

    /// <summary>
    /// Difference in a specific field
    /// </summary>
    public class FieldDifference
    {
        public string FieldName { get; set; }
        public object Source1Value { get; set; }
        public object Source2Value { get; set; }
        public string DifferenceType { get; set; } // Missing, Different, Extra
    }

    #endregion

    #region Deduplication DTOs

    /// <summary>
    /// Group of duplicate records
    /// </summary>
    public class DuplicateGroup
    {
        public string TableName { get; set; }
        public List<DuplicateRecord> Records { get; set; } = new List<DuplicateRecord>();
        public double SimilarityScore { get; set; }
        public List<string> MatchFields { get; set; } = new List<string>();
    }

    /// <summary>
    /// Duplicate record
    /// </summary>
    public class DuplicateRecord
    {
        public object RecordId { get; set; }
        public object Entity { get; set; }
        public Dictionary<string, object> FieldValues { get; set; } = new Dictionary<string, object>();
        public bool IsMaster { get; set; }
    }

    /// <summary>
    /// Result of merging duplicates
    /// </summary>
    public class MergeResult
    {
        public bool Success { get; set; }
        public object MergedEntity { get; set; }
        public List<object> MergedRecordIds { get; set; } = new List<object>();
        public List<string> Messages { get; set; } = new List<string>();
    }

    #endregion

    #region Archiving DTOs

    /// <summary>
    /// Criteria for archiving records
    /// </summary>
    public class ArchiveCriteria
    {
        public List<AppFilter> Filters { get; set; } = new List<AppFilter>();
        public DateTime? OlderThanDate { get; set; }
        public string StatusField { get; set; }
        public string StatusValue { get; set; }
        public bool ArchiveInactiveOnly { get; set; }
    }

    /// <summary>
    /// Result of archiving operation
    /// </summary>
    public class ArchiveResult
    {
        public bool Success { get; set; }
        public int ArchivedRecordCount { get; set; }
        public List<object> ArchivedRecordIds { get; set; } = new List<object>();
        public string ArchiveLocation { get; set; }
        public List<string> Messages { get; set; } = new List<string>();
    }

    /// <summary>
    /// Result of restore operation
    /// </summary>
    public class RestoreResult
    {
        public bool Success { get; set; }
        public int RestoredRecordCount { get; set; }
        public List<object> RestoredRecordIds { get; set; } = new List<object>();
        public List<string> Messages { get; set; } = new List<string>();
    }

    #endregion

    #region Bulk Operations DTOs

    /// <summary>
    /// Result of bulk operation
    /// </summary>
    public class BulkOperationResult
    {
        public bool Success { get; set; }
        public int ProcessedCount { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<BulkOperationError> Errors { get; set; } = new List<BulkOperationError>();
        public TimeSpan Duration { get; set; }
    }

    /// <summary>
    /// Error in bulk operation
    /// </summary>
    public class BulkOperationError
    {
        public int RecordIndex { get; set; }
        public object RecordId { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorType { get; set; }
    }

    #endregion

    #region Export/Import DTOs

    /// <summary>
    /// Result of import operation
    /// </summary>
    public class ImportResult
    {
        public bool Success { get; set; }
        public int TotalRecords { get; set; }
        public int ImportedCount { get; set; }
        public int SkippedCount { get; set; }
        public int ErrorCount { get; set; }
        public List<ImportError> Errors { get; set; } = new List<ImportError>();
        public string OutputPath { get; set; }
    }

    /// <summary>
    /// Error during import
    /// </summary>
    public class ImportError
    {
        public int RowNumber { get; set; }
        public string ErrorMessage { get; set; }
        public Dictionary<string, object> RowData { get; set; } = new Dictionary<string, object>();
    }

    #endregion

    #region Data Lineage DTOs

    /// <summary>
    /// Data lineage information
    /// </summary>
    public class DataLineageInfo
    {
        public string TableName { get; set; }
        public object EntityId { get; set; }
        public string SourceSystem { get; set; }
        public string SourceTable { get; set; }
        public object SourceEntityId { get; set; }
        public string TransformationType { get; set; }
        public string TransformationDetails { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Data lineage node (dependency)
    /// </summary>
    public class DataLineageNode
    {
        public string TableName { get; set; }
        public object EntityId { get; set; }
        public string RelationshipType { get; set; }
        public string Description { get; set; }
    }

    #endregion

    #region Data Versioning DTOs

    /// <summary>
    /// Version snapshot of an entity
    /// </summary>
    public class VersionSnapshot
    {
        public int VersionNumber { get; set; }
        public string TableName { get; set; }
        public object EntityId { get; set; }
        public object EntityData { get; set; }
        public string VersionLabel { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string ChangeDescription { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Comparison between two versions
    /// </summary>
    public class VersionComparison
    {
        public string TableName { get; set; }
        public object EntityId { get; set; }
        public int Version1 { get; set; }
        public int Version2 { get; set; }
        public List<FieldDifference> Differences { get; set; } = new List<FieldDifference>();
        public bool HasDifferences { get; set; }
    }

    /// <summary>
    /// Result of restoring to a version
    /// </summary>
    public class RestoreVersionResult
    {
        public bool Success { get; set; }
        public object RestoredEntity { get; set; }
        public int RestoredVersionNumber { get; set; }
        public string Message { get; set; }
    }

    #endregion

    #region Data Access Audit DTOs

    /// <summary>
    /// Data access event
    /// </summary>
    public class DataAccessEvent
    {
        public string EventId { get; set; }
        public string TableName { get; set; }
        public object EntityId { get; set; }
        public string UserId { get; set; }
        public string AccessType { get; set; } // Read, Write, Delete, Export
        public DateTime AccessDate { get; set; }
        public string IpAddress { get; set; }
        public string ApplicationName { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Access statistics for compliance reporting
    /// </summary>
    public class AccessStatistics
    {
        public string TableName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalAccessEvents { get; set; }
        public int UniqueUsers { get; set; }
        public int ReadOperations { get; set; }
        public int WriteOperations { get; set; }
        public int DeleteOperations { get; set; }
        public Dictionary<string, int> AccessByUser { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> AccessByType { get; set; } = new Dictionary<string, int>();
    }

    #endregion

    #region Data Masking DTOs

    /// <summary>
    /// Masking strategy
    /// </summary>
    public class MaskingStrategy
    {
        public Dictionary<string, MaskingRule> FieldRules { get; set; } = new Dictionary<string, MaskingRule>();
        public bool PreserveFormat { get; set; } = true;
        public string DefaultMaskingValue { get; set; } = "***";
    }

    /// <summary>
    /// Masking rule for a specific field
    /// </summary>
    public class MaskingRule
    {
        public MaskingType Type { get; set; }
        public string MaskingValue { get; set; }
        public bool PreserveFormat { get; set; } = true;
        public int? PreserveLength { get; set; }
    }

    public enum MaskingType
    {
        FullMask,           // Replace with ***
        PartialMask,        // Show first/last N characters
        Hash,               // Hash the value
        Randomize,          // Random value of same type
        Nullify,            // Set to null
        Custom              // Custom masking logic
    }

    #endregion

    #region Data Synchronization DTOs

    /// <summary>
    /// Synchronization options
    /// </summary>
    public class SyncOptions
    {
        public SyncDirection Direction { get; set; } = SyncDirection.Both;
        public List<string> KeyFields { get; set; } = new List<string>();
        public List<string> SyncFields { get; set; } = new List<string>();
        public ConflictResolutionStrategy DefaultConflictStrategy { get; set; } = ConflictResolutionStrategy.SourceWins;
        public bool ValidateBeforeSync { get; set; } = true;
        public bool CreateMissingRecords { get; set; } = true;
        public bool UpdateExistingRecords { get; set; } = true;
    }

    /// <summary>
    /// Synchronization result
    /// </summary>
    public class SyncResult
    {
        public string SyncId { get; set; }
        public bool Success { get; set; }
        public int RecordsProcessed { get; set; }
        public int RecordsCreated { get; set; }
        public int RecordsUpdated { get; set; }
        public int RecordsSkipped { get; set; }
        public int ConflictsFound { get; set; }
        public List<SyncConflict> Conflicts { get; set; } = new List<SyncConflict>();
        public TimeSpan Duration { get; set; }
        public List<string> Messages { get; set; } = new List<string>();
    }

    /// <summary>
    /// Synchronization status
    /// </summary>
    public class SyncStatus
    {
        public string SyncId { get; set; }
        public SyncStatusType Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int ProgressPercentage { get; set; }
        public string CurrentOperation { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }

    /// <summary>
    /// Synchronization conflict
    /// </summary>
    public class SyncConflict
    {
        public string ConflictId { get; set; }
        public string TableName { get; set; }
        public object EntityId { get; set; }
        public object SourceValue { get; set; }
        public object TargetValue { get; set; }
        public List<FieldDifference> FieldDifferences { get; set; } = new List<FieldDifference>();
        public string ConflictReason { get; set; }
    }

    /// <summary>
    /// Conflict resolution result
    /// </summary>
    public class ConflictResolutionResult
    {
        public bool Success { get; set; }
        public object ResolvedEntity { get; set; }
        public ConflictResolutionStrategy StrategyUsed { get; set; }
        public string Message { get; set; }
    }

    public enum SyncDirection
    {
        SourceToTarget,
        TargetToSource,
        Both
    }

    public enum SyncStatusType
    {
        Pending,
        InProgress,
        Completed,
        Failed,
        Cancelled
    }

    public enum ConflictResolutionStrategy
    {
        SourceWins,
        TargetWins,
        Manual,
        Merge,
        Skip
    }

    #endregion

    #region Data Transformation DTOs

    /// <summary>
    /// Transformation mapping
    /// </summary>
    public class TransformationMapping
    {
        public Type SourceType { get; set; }
        public Type TargetType { get; set; }
        public Dictionary<string, string> FieldMapping { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, Func<object, object>> CustomTransformations { get; set; } = new Dictionary<string, Func<object, object>>();
        public List<string> FieldsToInclude { get; set; } = new List<string>();
        public List<string> FieldsToExclude { get; set; } = new List<string>();
    }

    #endregion

    #region Data Quality Dashboard DTOs

    /// <summary>
    /// Quality dashboard data
    /// </summary>
    public class QualityDashboardData
    {
        public string TableName { get; set; }
        public DateTime LastUpdated { get; set; }
        public double OverallQualityScore { get; set; }
        public DataQualityMetrics CurrentMetrics { get; set; }
        public List<QualityTrend> RecentTrends { get; set; } = new List<QualityTrend>();
        public List<QualityAlert> ActiveAlerts { get; set; } = new List<QualityAlert>();
        public Dictionary<string, double> FieldQualityScores { get; set; } = new Dictionary<string, double>();
    }

    /// <summary>
    /// Quality trend over time
    /// </summary>
    public class QualityTrend
    {
        public DateTime Date { get; set; }
        public double QualityScore { get; set; }
        public int RecordCount { get; set; }
        public int IssueCount { get; set; }
    }

    /// <summary>
    /// Quality alert
    /// </summary>
    public class QualityAlert
    {
        public string AlertId { get; set; }
        public string TableName { get; set; }
        public string FieldName { get; set; }
        public QualityAlertSeverity Severity { get; set; }
        public string AlertMessage { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsResolved { get; set; }
        public DateTime? ResolvedDate { get; set; }
    }

    public enum QualityAlertSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }

    #endregion
}




