using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Beep.OilandGas.Client.App.Services.DataManagement
{
    /// <summary>
    /// Service interface for Data Management (PPDM39) operations
    /// Includes CRUD, Validation, Quality, Versioning, Audit, LOV, and Setup
    /// </summary>
    public interface IDataManagementService
    {
        #region CRUD Operations

        Task<object> GetEntitiesAsync(string tableName, object request, CancellationToken cancellationToken = default);
        Task<object> GetEntityAsync(string tableName, string id, CancellationToken cancellationToken = default);
        Task<object> InsertEntityAsync(string tableName, object request, string userId = "SYSTEM", CancellationToken cancellationToken = default);
        Task<object> UpdateEntityAsync(string tableName, string id, object request, string userId = "SYSTEM", CancellationToken cancellationToken = default);
        Task<object> DeleteEntityAsync(string tableName, string id, string userId = "SYSTEM", CancellationToken cancellationToken = default);

        #endregion

        #region Validation

        Task<object> ValidateEntityAsync(string tableName, object entity, CancellationToken cancellationToken = default);
        Task<List<object>> ValidateBatchAsync(string tableName, List<object> entities, CancellationToken cancellationToken = default);
        Task<List<object>> GetValidationRulesAsync(string tableName, CancellationToken cancellationToken = default);
        Task<object> SaveValidationRuleAsync(string tableName, object rule, CancellationToken cancellationToken = default);

        #endregion

        #region Data Quality

        Task<object> CalculateQualityScoreAsync(string tableName, object entity, CancellationToken cancellationToken = default);
        Task<object> CalculateTableQualityMetricsAsync(string tableName, CancellationToken cancellationToken = default);
        Task<List<object>> FindQualityIssuesAsync(string tableName, List<string>? fieldNames = null, CancellationToken cancellationToken = default);
        Task<object> GetQualityDashboardAsync(string? tableName = null, CancellationToken cancellationToken = default);
        Task<List<object>> GetQualityTrendsAsync(string tableName, int days = 30, CancellationToken cancellationToken = default);

        #endregion

        #region Versioning

        Task<object> CreateVersionAsync(string tableName, object entity, string userId, string? versionLabel = null, CancellationToken cancellationToken = default);
        Task<List<object>> GetVersionsAsync(string tableName, object entityId, CancellationToken cancellationToken = default);
        Task<object> GetVersionAsync(string tableName, object entityId, int versionNumber, CancellationToken cancellationToken = default);
        Task<object> CompareVersionsAsync(string tableName, object entityId, int version1, int version2, CancellationToken cancellationToken = default);
        Task<object> RollbackToVersionAsync(string tableName, object entityId, int versionNumber, string userId, CancellationToken cancellationToken = default);

        #endregion

        #region Access Audit

        Task<object> RecordAccessAsync(object accessEvent, CancellationToken cancellationToken = default);
        Task<List<object>> GetAccessHistoryAsync(string tableName, object entityId, CancellationToken cancellationToken = default);
        Task<List<object>> GetUserAccessHistoryAsync(string userId, int? limit = null, CancellationToken cancellationToken = default);
        Task<object> GetAccessStatisticsAsync(string tableName, CancellationToken cancellationToken = default);

        #endregion

        #region List of Values (LOV)

        Task<List<object>> GetLOVAsync(string lovType, CancellationToken cancellationToken = default);
        Task<List<object>> GetLOVsByTypeAsync(string lovType, CancellationToken cancellationToken = default);
        Task<object> GetLOVByCodeAsync(string lovType, string code, CancellationToken cancellationToken = default);
        Task<object> CreateLOVAsync(object lovEntry, string userId = "SYSTEM", CancellationToken cancellationToken = default);
        Task<object> UpdateLOVAsync(string lovId, object lovEntry, string userId = "SYSTEM", CancellationToken cancellationToken = default);
        Task<object> DeleteLOVAsync(string lovId, string userId = "SYSTEM", CancellationToken cancellationToken = default);
        Task<List<object>> GetReferenceTableDataAsync(string tableName, CancellationToken cancellationToken = default);

        #endregion

        #region Field Mapping

        Task<object> GetFieldMappingsAsync(string sourceTable, string targetTable, CancellationToken cancellationToken = default);
        Task<object> SaveFieldMappingAsync(object mapping, string userId = "SYSTEM", CancellationToken cancellationToken = default);
        Task<object> ApplyFieldMappingAsync(string mappingId, object sourceEntity, CancellationToken cancellationToken = default);

        #endregion

        #region Jurisdiction

        Task<List<object>> GetJurisdictionsAsync(CancellationToken cancellationToken = default);
        Task<object> GetJurisdictionAsync(string jurisdictionId, CancellationToken cancellationToken = default);
        Task<object> GetJurisdictionRequirementsAsync(string jurisdictionId, string entityType, CancellationToken cancellationToken = default);

        #endregion

        #region Setup & Configuration

        Task<object> InitializeDatabaseAsync(object setupOptions, CancellationToken cancellationToken = default);
        Task<object> GetDatabaseStatusAsync(CancellationToken cancellationToken = default);
        Task<object> RunMigrationsAsync(CancellationToken cancellationToken = default);
        Task<object> SeedDataAsync(string seedType, CancellationToken cancellationToken = default);

        #endregion

        #region Demo Database

        Task<object> CreateDemoDatabaseAsync(object options, CancellationToken cancellationToken = default);
        Task<object> CleanupDemoDatabaseAsync(string demoId, CancellationToken cancellationToken = default);
        Task<object> GetDemoDatabaseStatusAsync(string demoId, CancellationToken cancellationToken = default);

        #endregion

        #region Workflow

        Task<object> StartWorkflowAsync(string workflowType, object request, string userId, CancellationToken cancellationToken = default);
        Task<object> GetWorkflowStatusAsync(string workflowId, CancellationToken cancellationToken = default);
        Task<object> AdvanceWorkflowAsync(string workflowId, object action, string userId, CancellationToken cancellationToken = default);
        Task<List<object>> GetPendingWorkflowsAsync(string? userId = null, CancellationToken cancellationToken = default);

        #endregion

        #region Metadata

        Task<object> GetTableMetadataAsync(string tableName, CancellationToken cancellationToken = default);
        Task<List<object>> GetAllTablesMetadataAsync(CancellationToken cancellationToken = default);
        Task<object> GetColumnMetadataAsync(string tableName, string columnName, CancellationToken cancellationToken = default);

        #endregion

        #region Defaults Management (Per Data Source)

        /// <summary>
        /// Gets a default value for a specific data source (database)
        /// </summary>
        Task<string?> GetDefaultValueAsync(string key, string databaseId, string? userId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets or updates a default value for a specific data source
        /// </summary>
        Task SetDefaultValueAsync(string key, string value, string databaseId, string? userId = null, string category = "System", string valueType = "String", string? description = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all default values for a category in a specific data source
        /// </summary>
        Task<Dictionary<string, string>> GetDefaultsByCategoryAsync(string category, string databaseId, string? userId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all default values for a data source
        /// </summary>
        Task<Dictionary<string, string>> GetDefaultsForDatabaseAsync(string databaseId, string? userId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Initializes system defaults for a data source
        /// </summary>
        Task InitializeSystemDefaultsAsync(string databaseId, string userId = "SYSTEM", CancellationToken cancellationToken = default);

        /// <summary>
        /// Resets user overrides to system defaults for a data source
        /// </summary>
        Task ResetToSystemDefaultsAsync(string databaseId, string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets standard PPDM default values (hardcoded constants)
        /// </summary>
        Task<Dictionary<string, string>> GetStandardDefaultsAsync(CancellationToken cancellationToken = default);

        #endregion

        #region Generic Repository Operations

        #region Batch Operations

        /// <summary>
        /// Inserts multiple entities in a batch
        /// </summary>
        Task<List<object>> InsertBatchAsync(string tableName, List<object> entities, string userId, int batchSize = 100, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates multiple entities in a batch
        /// </summary>
        Task<List<object>> UpdateBatchAsync(string tableName, List<object> entities, string userId, int batchSize = 100, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes multiple entities by IDs in a batch
        /// </summary>
        Task<int> DeleteBatchAsync(string tableName, List<string> ids, string userId, bool softDelete = true, int batchSize = 100, CancellationToken cancellationToken = default);

        /// <summary>
        /// Performs bulk upsert (insert or update) operation
        /// </summary>
        Task<List<object>> UpsertBatchAsync(string tableName, List<object> entities, string userId, int batchSize = 100, CancellationToken cancellationToken = default);

        #endregion

        #region Import/Export

        /// <summary>
        /// Imports data from a CSV file
        /// </summary>
        Task<object> ImportFromCsvAsync(string tableName, string csvFilePath, string userId, Dictionary<string, string>? columnMapping = null, bool skipHeaderRow = true, bool validateForeignKeys = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// Exports data to CSV file
        /// </summary>
        Task<int> ExportToCsvAsync(string tableName, string csvFilePath, List<object>? filters = null, bool includeHeaders = true, CancellationToken cancellationToken = default);

        #endregion

        #region Pagination & Aggregation

        /// <summary>
        /// Gets entities with pagination
        /// </summary>
        Task<object> GetPaginatedAsync(string tableName, List<object>? filters = null, int pageNumber = 1, int pageSize = 50, string? sortField = null, string sortDirection = "ASC", CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets count of entities matching filters
        /// </summary>
        Task<long> GetCountAsync(string tableName, List<object>? filters = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets aggregated value for a numeric field
        /// </summary>
        Task<decimal?> GetAggregateAsync(string tableName, string fieldName, string aggregationType, List<object>? filters = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets grouped aggregated results
        /// </summary>
        Task<Dictionary<string, decimal?>> GetGroupedAggregateAsync(string tableName, string groupByField, string aggregateField, string aggregationType, List<object>? filters = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets distinct values for a specific field
        /// </summary>
        Task<List<object?>> GetDistinctAsync(string tableName, string fieldName, List<object>? filters = null, CancellationToken cancellationToken = default);

        #endregion

        #region Relationship Navigation

        /// <summary>
        /// Gets related entities through a foreign key relationship
        /// </summary>
        Task<List<object>> GetRelatedEntitiesAsync(string tableName, string entityId, string relatedTableName, string? foreignKeyColumn = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets parent entity through a foreign key relationship
        /// </summary>
        Task<object?> GetParentEntityAsync(string tableName, string entityId, string parentTableName, string? foreignKeyColumn = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all relationships for an entity (both parent and child relationships)
        /// </summary>
        Task<Dictionary<string, List<object>>> GetEntityRelationshipsAsync(string tableName, string entityId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets child entities by parent key(s)
        /// </summary>
        Task<List<object>> GetChildrenByParentKeyAsync(string tableName, string parentTableName, object parentKey, CancellationToken cancellationToken = default);

        #endregion

        #endregion
    }
}
