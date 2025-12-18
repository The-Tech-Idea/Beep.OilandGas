using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ApiService.Models
{
    /// <summary>
    /// Request for generic entity operations
    /// </summary>
    public class GenericEntityRequest
    {
        public string TableName { get; set; } = string.Empty;
        public Dictionary<string, object> EntityData { get; set; } = new Dictionary<string, object>();
        public string? ConnectionName { get; set; }
    }

    /// <summary>
    /// Response for generic entity operations
    /// </summary>
    public class GenericEntityResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, object>? EntityData { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Request to get entities with filters
    /// </summary>
    public class GetEntitiesRequest
    {
        public string TableName { get; set; } = string.Empty;
        public List<AppFilter> Filters { get; set; } = new List<AppFilter>();
        public string? ConnectionName { get; set; }
    }

    /// <summary>
    /// Response with list of entities
    /// </summary>
    public class GetEntitiesResponse
    {
        public bool Success { get; set; }
        public List<Dictionary<string, object>> Entities { get; set; } = new List<Dictionary<string, object>>();
        public int Count { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Import request for CSV/JSON
    /// </summary>
    public class ImportRequest
    {
        public string TableName { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = "text/csv"; // "text/csv" or "application/json"
        public Dictionary<string, string>? ColumnMapping { get; set; }
        public bool SkipHeaderRow { get; set; } = true;
        public bool ValidateForeignKeys { get; set; } = true;
        public string? ConnectionName { get; set; }
        public string UserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// File import result
    /// </summary>
    public class FileImportResult
    {
        public string FilePath { get; set; } = string.Empty;
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<FileImportError> Errors { get; set; } = new List<FileImportError>();
    }

    /// <summary>
    /// File import error
    /// </summary>
    public class FileImportError
    {
        public int RowNumber { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ColumnName { get; set; }
    }

    /// <summary>
    /// Export request
    /// </summary>
    public class ExportRequest
    {
        public string TableName { get; set; } = string.Empty;
        public List<AppFilter>? Filters { get; set; }
        public string Format { get; set; } = "csv"; // "csv" or "json"
        public bool IncludeHeaders { get; set; } = true;
        public string? ConnectionName { get; set; }
    }

    /// <summary>
    /// Validation request
    /// </summary>
    public class ValidationRequest
    {
        public string TableName { get; set; } = string.Empty;
        public Dictionary<string, object> EntityData { get; set; } = new Dictionary<string, object>();
        public string? ConnectionName { get; set; }
    }

    /// <summary>
    /// Batch validation request
    /// </summary>
    public class BatchValidationRequest
    {
        public string TableName { get; set; } = string.Empty;
        public List<Dictionary<string, object>> Entities { get; set; } = new List<Dictionary<string, object>>();
        public string? ConnectionName { get; set; }
    }

    /// <summary>
    /// Validation result
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<ValidationError> Errors { get; set; } = new List<ValidationError>();
        public int ErrorCount => Errors.Count;
    }

    /// <summary>
    /// Validation error
    /// </summary>
    public class ValidationError
    {
        public string FieldName { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string? ErrorCode { get; set; }
    }

    /// <summary>
    /// Data quality request
    /// </summary>
    public class DataQualityRequest
    {
        public string TableName { get; set; } = string.Empty;
        public string? ConnectionName { get; set; }
    }

    /// <summary>
    /// Data quality result
    /// </summary>
    public class DataQualityResult
    {
        public string TableName { get; set; } = string.Empty;
        public double OverallQualityScore { get; set; }
        public Dictionary<string, double> FieldQualityScores { get; set; } = new Dictionary<string, double>();
        public int TotalRows { get; set; }
        public int CompleteRows { get; set; }
        public List<string> QualityIssues { get; set; } = new List<string>();
    }

    /// <summary>
    /// Data quality dashboard result
    /// </summary>
    public class DataQualityDashboardResult
    {
        public Dictionary<string, DataQualityResult> TableQualityResults { get; set; } = new Dictionary<string, DataQualityResult>();
        public double OverallQualityScore { get; set; }
        public int TotalTables { get; set; }
        public int TablesWithIssues { get; set; }
    }

    /// <summary>
    /// Versioning request
    /// </summary>
    public class VersioningRequest
    {
        public string TableName { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string? ConnectionName { get; set; }
        public string UserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Versioning result
    /// </summary>
    public class VersioningResult
    {
        public bool Success { get; set; }
        public string VersionId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Version info
    /// </summary>
    public class VersionInfo
    {
        public string VersionId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Dictionary<string, object>? EntityData { get; set; }
    }

    /// <summary>
    /// Get version history request
    /// </summary>
    public class GetVersionHistoryRequest
    {
        public string TableName { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string? ConnectionName { get; set; }
    }

    /// <summary>
    /// Restore version request
    /// </summary>
    public class RestoreVersionRequest
    {
        public string TableName { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string VersionId { get; set; } = string.Empty;
        public string? ConnectionName { get; set; }
        public string UserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Get defaults request
    /// </summary>
    public class GetDefaultsRequest
    {
        public string EntityType { get; set; } = string.Empty;
        public string? ConnectionName { get; set; }
    }

    /// <summary>
    /// Get well status facets request
    /// </summary>
    public class GetWellStatusFacetsRequest
    {
        public string StatusId { get; set; } = string.Empty;
        public string? ConnectionName { get; set; }
    }
}
