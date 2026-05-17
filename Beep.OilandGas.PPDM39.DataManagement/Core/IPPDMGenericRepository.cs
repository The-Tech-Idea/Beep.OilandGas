using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.PPDM39.DataManagement.Core
{
    public interface IPPDMGenericRepository
    {
        ICommonColumnHandler CommonColumnHandler { get; }
        IPPDM39DefaultsRepository Defaults { get; }
        Type EntityType { get; }
        IPPDMMetadataRepository Metadata { get; }
        string TableName { get; }
        IUnitofWork UnitOfWork { get; }

        List<AppFilter> BuildAppFiltersFromSql(string sql);
        Task<int> CountAsync(List<AppFilter> filters = null);
        Task<bool> DeleteAsync(object entity);
        Task<int> DeleteBatchAsync(IEnumerable<string> ids, string userId, bool softDelete = true, int batchSize = 100);
        Task<bool> DeleteByIdAsync(object id, string userId);
        Task<bool> ExistsAsync(object id);
        Task<int> ExportToCsvAsync(string csvFilePath, List<AppFilter> filters = null, bool includeHeaders = true, PPDMGenericRepository.ProgressReportDelegate? onProgress = null, string? operationId = null);
        Task<IEnumerable<object>> GetActiveAsync();
        Task<decimal?> GetAggregateAsync(string fieldName, string aggregationType, List<AppFilter>? filters = null);
        Task<IEnumerable<object>> GetAsync(List<AppFilter> filters);
        Task<object> GetByIdAsync(object id);
        Task<IEnumerable<object>> GetChildrenByParentEntityAsync(object parentEntity, string parentTableName = null);
        Task<IEnumerable<object>> GetChildrenByParentKeyAsync(string parentTableName, object parentKey);
        Task<IEnumerable<object>> GetChildrenByParentKeysAsync(string parentTableName, Dictionary<string, object> parentKeys);
        Task<long> GetCountAsync(List<AppFilter>? filters = null);
        Task<List<object?>> GetDistinctAsync(string fieldName, List<AppFilter>? filters = null);
        Task<IEnumerable<object>> GetEntitiesWithFiltersAsync(Type entityType, string tableName, List<AppFilter> filters);
        Task<Dictionary<string, List<object>>> GetEntityRelationshipsAsync(string entityId);
        Task<Dictionary<string, decimal?>> GetGroupedAggregateAsync(string groupByField, string aggregateField, string aggregationType, List<AppFilter>? filters = null);
        Task<PPDMGenericRepository.PaginatedResult> GetPaginatedAsync(List<AppFilter>? filters = null, int pageNumber = 1, int pageSize = 50, string? sortField = null, string? sortDirection = "ASC");
        Task<object?> GetParentEntityAsync(string entityId, string parentTableName, string? foreignKeyColumn = null);
        Task<Dictionary<string, PPDMGenericRepository.ParentKeyInfo>> GetParentKeyInfoAsync();
        Task<List<object>> GetRelatedEntitiesAsync(string entityId, string relatedTableName, string? foreignKeyColumn = null);
        Task<FileImportResult> ImportFromCsvAsync(string csvFilePath, string userId, Dictionary<string, string>? columnMapping = null, bool skipHeaderRow = true, bool validateForeignKeys = true, PPDMGenericRepository.ProgressReportDelegate? onProgress = null, string? operationId = null);
        Task InsertAsync(NodalAnalysisResult result, object value);
        Task<object> InsertAsync(object entity, string userId);
        Task<IEnumerable<object>> InsertBatchAsync(IEnumerable<object> entities, string userId);
        Task<IEnumerable<object>> InsertBatchWithParentKeysAsync(IEnumerable<object> entities, string userId, Dictionary<string, object> parentKeys = null);
        Task<List<object>> InsertBatchWithSizeAsync(IEnumerable<object> entities, string userId, int batchSize = 100);
        Task<object> InsertWithParentKeysAsync(object entity, string userId, Dictionary<string, object> parentKeys = null);
        Task<bool> SoftDeleteAsync(object id, string userId);
        Task<object> UpdateAsync(object entity, string userId);
        Task<IEnumerable<object>> UpdateBatchAsync(IEnumerable<object> entities, string userId);
        Task<List<object>> UpdateBatchAsync(IEnumerable<object> entities, string userId, int batchSize = 100);
        Task<List<object>> UpsertBatchAsync(IEnumerable<object> entities, string userId, int batchSize = 100);
        Task<List<ForeignKeyValidationError>> ValidateForeignKeyValuesAsync(Dictionary<string, string> columnValues, int rowNumber = 0);
        Task<List<ForeignKeyValidationError>> ValidateForeignKeyValuesBatchAsync(List<Dictionary<string, string>> rows);
    }
}