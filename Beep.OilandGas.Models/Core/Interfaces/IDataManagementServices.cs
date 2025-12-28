using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.DTOs;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Interface for data validation service
    /// Validates entities against business rules and data quality rules
    /// </summary>
    public interface IPPDMDataValidationService
    {
        /// <summary>
        /// Validates an entity against business rules
        /// </summary>
        Task<ValidationResult> ValidateAsync(object entity, string tableName);

        /// <summary>
        /// Validates multiple entities in batch
        /// </summary>
        Task<List<ValidationResult>> ValidateBatchAsync(IEnumerable<object> entities, string tableName);

        /// <summary>
        /// Gets validation rules for a table
        /// </summary>
        Task<List<ValidationRule>> GetValidationRulesAsync(string tableName);
    }

    /// <summary>
    /// Interface for data quality service
    /// Measures and reports on data quality metrics
    /// </summary>
    public interface IPPDMDataQualityService
    {
        /// <summary>
        /// Calculates data quality score for an entity
        /// </summary>
        Task<DataQualityScore> CalculateQualityScoreAsync(object entity, string tableName);

        /// <summary>
        /// Calculates data quality metrics for a table
        /// </summary>
        Task<DataQualityMetrics> CalculateTableQualityMetricsAsync(string tableName);

        /// <summary>
        /// Finds data quality issues in a table
        /// </summary>
        Task<List<DataQualityIssue>> FindQualityIssuesAsync(string tableName, List<string> fieldNames = null);
    }

    /// <summary>
    /// Interface for data reconciliation service
    /// Compares and reconciles data across different sources
    /// </summary>
    public interface IPPDMDataReconciliationService
    {
        /// <summary>
        /// Reconciles data between two data sources
        /// </summary>
        Task<ReconciliationResult> ReconcileAsync(
            string tableName,
            string sourceConnection1,
            string sourceConnection2,
            List<string> keyFields,
            List<string> compareFields = null);

        /// <summary>
        /// Finds differences between two entities
        /// </summary>
        Task<List<FieldDifference>> FindDifferencesAsync(object entity1, object entity2, string tableName);
    }

    /// <summary>
    /// Interface for data deduplication service
    /// Finds and merges duplicate records
    /// </summary>
    public interface IPPDMDataDeduplicationService
    {
        /// <summary>
        /// Finds duplicate records in a table
        /// </summary>
        Task<List<DuplicateGroup>> FindDuplicatesAsync(
            string tableName,
            List<string> matchFields,
            double similarityThreshold = 0.8);

        /// <summary>
        /// Merges duplicate records into a single record
        /// </summary>
        Task<MergeResult> MergeDuplicatesAsync(DuplicateGroup duplicateGroup, string userId);
    }

    /// <summary>
    /// Interface for data archiving service
    /// Archives old or inactive data
    /// </summary>
    public interface IPPDMDataArchivingService
    {
        /// <summary>
        /// Archives records based on criteria
        /// </summary>
        Task<ArchiveResult> ArchiveAsync(
            string tableName,
            ArchiveCriteria criteria,
            string archiveConnectionName,
            string userId);

        /// <summary>
        /// Restores archived records
        /// </summary>
        Task<RestoreResult> RestoreAsync(
            string archiveConnectionName,
            string tableName,
            List<string> recordIds,
            string targetConnectionName,
            string userId);
    }

    /// <summary>
    /// Interface for bulk operations service
    /// Efficient bulk data operations
    /// </summary>
    public interface IPPDMBulkOperationsService
    {
        /// <summary>
        /// Bulk updates entities
        /// </summary>
        Task<BulkOperationResult> BulkUpdateAsync(
            string tableName,
            List<object> entities,
            string userId);

        /// <summary>
        /// Bulk deletes entities by criteria
        /// </summary>
        Task<BulkOperationResult> BulkDeleteAsync(
            string tableName,
            List<AppFilter> criteria,
            string userId);

        /// <summary>
        /// Bulk inserts entities with validation
        /// </summary>
        Task<BulkOperationResult> BulkInsertAsync(
            string tableName,
            List<object> entities,
            string userId,
            bool validate = true);
    }

    /// <summary>
    /// Interface for data export/import service
    /// Handles data export and import in standard formats
    /// </summary>
    public interface IPPDMDataExportImportService
    {
        /// <summary>
        /// Exports data to CSV
        /// </summary>
        Task<string> ExportToCsvAsync(
            string tableName,
            List<AppFilter> filters = null,
            string outputPath = null);

        /// <summary>
        /// Exports data to JSON
        /// </summary>
        Task<string> ExportToJsonAsync(
            string tableName,
            List<AppFilter> filters = null,
            string outputPath = null);

        /// <summary>
        /// Imports data from CSV
        /// </summary>
        Task<ImportResult> ImportFromCsvAsync(
            string csvPath,
            string tableName,
            string userId,
            bool validate = true);

        /// <summary>
        /// Imports data from JSON
        /// </summary>
        Task<ImportResult> ImportFromJsonAsync(
            string jsonPath,
            string tableName,
            string userId,
            bool validate = true);
    }

    /// <summary>
    /// Interface for data lineage tracking service
    /// Tracks data sources, transformations, and data flow
    /// </summary>
    public interface IPPDMDataLineageService
    {
        /// <summary>
        /// Records data lineage for an entity
        /// </summary>
        Task RecordLineageAsync(
            string tableName,
            object entityId,
            DataLineageInfo lineageInfo);

        /// <summary>
        /// Gets lineage information for an entity
        /// </summary>
        Task<DataLineageInfo> GetLineageAsync(string tableName, object entityId);

        /// <summary>
        /// Gets all entities that depend on this entity (downstream)
        /// </summary>
        Task<List<DataLineageNode>> GetDownstreamDependenciesAsync(string tableName, object entityId);

        /// <summary>
        /// Gets all entities this entity depends on (upstream)
        /// </summary>
        Task<List<DataLineageNode>> GetUpstreamDependenciesAsync(string tableName, object entityId);
    }

    /// <summary>
    /// Interface for data versioning service
    /// Tracks entity changes over time and provides versioning capabilities
    /// </summary>
    public interface IPPDMDataVersioningService
    {
        /// <summary>
        /// Creates a version snapshot of an entity
        /// </summary>
        Task<VersionSnapshot> CreateVersionAsync(string tableName, object entity, string userId, string versionLabel = null);

        /// <summary>
        /// Gets all versions of an entity
        /// </summary>
        Task<List<VersionSnapshot>> GetVersionsAsync(string tableName, object entityId);

        /// <summary>
        /// Gets a specific version of an entity
        /// </summary>
        Task<VersionSnapshot> GetVersionAsync(string tableName, object entityId, int versionNumber);

        /// <summary>
        /// Compares two versions of an entity
        /// </summary>
        Task<VersionComparison> CompareVersionsAsync(string tableName, object entityId, int version1, int version2);

        /// <summary>
        /// Restores an entity to a specific version
        /// </summary>
        Task<RestoreVersionResult> RestoreToVersionAsync(string tableName, object entityId, int versionNumber, string userId);
    }

    /// <summary>
    /// Interface for data access audit service
    /// Tracks who accessed what data and when
    /// </summary>
    public interface IPPDMDataAccessAuditService
    {
        /// <summary>
        /// Records data access event
        /// </summary>
        Task RecordAccessAsync(DataAccessEvent accessEvent);

        /// <summary>
        /// Gets access history for an entity
        /// </summary>
        Task<List<DataAccessEvent>> GetAccessHistoryAsync(string tableName, object entityId);

        /// <summary>
        /// Gets access history for a user
        /// </summary>
        Task<List<DataAccessEvent>> GetUserAccessHistoryAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null);

        /// <summary>
        /// Gets access statistics for compliance reporting
        /// </summary>
        Task<AccessStatistics> GetAccessStatisticsAsync(string tableName = null, DateTime? fromDate = null, DateTime? toDate = null);
    }

    /// <summary>
    /// Interface for data masking/anonymization service
    /// Masks or anonymizes sensitive data for testing/sharing
    /// </summary>
    public interface IPPDMDataMaskingService
    {
        /// <summary>
        /// Masks sensitive data in an entity
        /// </summary>
        Task<object> MaskEntityAsync(object entity, string tableName, MaskingStrategy strategy);

        /// <summary>
        /// Anonymizes an entity (removes identifying information)
        /// </summary>
        Task<object> AnonymizeEntityAsync(object entity, string tableName);

        /// <summary>
        /// Masks data in a query result
        /// </summary>
        Task<List<object>> MaskQueryResultsAsync(List<object> entities, string tableName, MaskingStrategy strategy);
    }

    /// <summary>
    /// Interface for data synchronization service
    /// Syncs data between different systems/connections
    /// </summary>
    public interface IPPDMDataSynchronizationService
    {
        /// <summary>
        /// Synchronizes data from source to target
        /// </summary>
        Task<SyncResult> SynchronizeAsync(
            string tableName,
            string sourceConnection,
            string targetConnection,
            SyncOptions options);

        /// <summary>
        /// Gets synchronization status
        /// </summary>
        Task<SyncStatus> GetSyncStatusAsync(string syncId);

        /// <summary>
        /// Resolves conflicts during synchronization
        /// </summary>
        Task<ConflictResolutionResult> ResolveConflictAsync(
            string syncId,
            SyncConflict conflict,
            ConflictResolutionStrategy strategy);
    }

    /// <summary>
    /// Interface for data transformation service
    /// Transforms data between formats and structures
    /// </summary>
    public interface IPPDMDataTransformationService
    {
        /// <summary>
        /// Transforms entity from one structure to another
        /// </summary>
        Task<object> TransformEntityAsync(object sourceEntity, TransformationMapping mapping);

        /// <summary>
        /// Transforms a list of entities
        /// </summary>
        Task<List<object>> TransformEntitiesAsync(List<object> sourceEntities, TransformationMapping mapping);

        /// <summary>
        /// Maps fields from source to target
        /// </summary>
        Task<object> MapFieldsAsync(object sourceEntity, Dictionary<string, string> fieldMapping, Type targetType);
    }

    /// <summary>
    /// Interface for data quality dashboard service
    /// Provides real-time quality metrics and dashboards
    /// </summary>
    public interface IPPDMDataQualityDashboardService
    {
        /// <summary>
        /// Gets quality dashboard data for a table
        /// </summary>
        Task<QualityDashboardData> GetDashboardDataAsync(string tableName);

        /// <summary>
        /// Gets quality trends over time
        /// </summary>
        Task<List<QualityTrend>> GetQualityTrendsAsync(string tableName, DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Gets quality alerts (issues that need attention)
        /// </summary>
        Task<List<QualityAlert>> GetQualityAlertsAsync(string tableName = null, QualityAlertSeverity? severity = null);
    }
}

