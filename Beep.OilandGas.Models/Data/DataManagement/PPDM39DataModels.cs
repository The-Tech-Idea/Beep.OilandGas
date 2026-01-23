using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.DataManagement
{
    /// <summary>
    /// Request for generic entity operations
    /// </summary>
    public class GenericEntityRequest : ModelEntityBase
    {
        private string TableNameValue = string.Empty;

        public string TableName

        {

            get { return this.TableNameValue; }

            set { SetProperty(ref TableNameValue, value); }

        }
        public Dictionary<string, object> EntityData { get; set; } = new Dictionary<string, object>();
        private string? ConnectionNameValue;

        public string? ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
    }

    /// <summary>
    /// Response for generic entity operations
    /// </summary>
    public class GenericEntityResponse : ModelEntityBase
    {
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private string MessageValue = string.Empty;

        public string Message

        {

            get { return this.MessageValue; }

            set { SetProperty(ref MessageValue, value); }

        }
        public Dictionary<string, object>? EntityData { get; set; }
        private string? ErrorMessageValue;

        public string? ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }
    }

    /// <summary>
    /// Request to get entities with filters
    /// </summary>
    public class GetEntitiesRequest : ModelEntityBase
    {
        private string TableNameValue = string.Empty;

        public string TableName

        {

            get { return this.TableNameValue; }

            set { SetProperty(ref TableNameValue, value); }

        }
        private List<AppFilter> FiltersValue = new List<AppFilter>();

        public List<AppFilter> Filters

        {

            get { return this.FiltersValue; }

            set { SetProperty(ref FiltersValue, value); }

        }
        private string? ConnectionNameValue;

        public string? ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
    }

    /// <summary>
    /// Response with list of entities
    /// </summary>
    public class GetEntitiesResponse : ModelEntityBase
    {
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        public List<Dictionary<string, object>> Entities { get; set; } = new List<Dictionary<string, object>>();
        private int CountValue;

        public int Count

        {

            get { return this.CountValue; }

            set { SetProperty(ref CountValue, value); }

        }
        private string? ErrorMessageValue;

        public string? ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }
    }

    /// <summary>
    /// Import request for CSV/JSON
    /// </summary>
    public class ImportRequest : ModelEntityBase
    {
        private string TableNameValue = string.Empty;

        public string TableName

        {

            get { return this.TableNameValue; }

            set { SetProperty(ref TableNameValue, value); }

        }
        private string FileNameValue = string.Empty;

        public string FileName

        {

            get { return this.FileNameValue; }

            set { SetProperty(ref FileNameValue, value); }

        }
        private string ContentTypeValue = "text/csv";

        public string ContentType

        {

            get { return this.ContentTypeValue; }

            set { SetProperty(ref ContentTypeValue, value); }

        } // "text/csv" or "application/json"
        public Dictionary<string, string>? ColumnMapping { get; set; }
        private bool SkipHeaderRowValue = true;

        public bool SkipHeaderRow

        {

            get { return this.SkipHeaderRowValue; }

            set { SetProperty(ref SkipHeaderRowValue, value); }

        }
        private bool ValidateForeignKeysValue = true;

        public bool ValidateForeignKeys

        {

            get { return this.ValidateForeignKeysValue; }

            set { SetProperty(ref ValidateForeignKeysValue, value); }

        }
        private string? ConnectionNameValue;

        public string? ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
        private string UserIdValue = string.Empty;

        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }

    /// <summary>
    /// File import result
    /// </summary>
    public class FileImportResult : ModelEntityBase
    {
        private string FilePathValue = string.Empty;

        public string FilePath

        {

            get { return this.FilePathValue; }

            set { SetProperty(ref FilePathValue, value); }

        }
        private int TotalRowsValue;

        public int TotalRows

        {

            get { return this.TotalRowsValue; }

            set { SetProperty(ref TotalRowsValue, value); }

        }
        private int SuccessCountValue;

        public int SuccessCount

        {

            get { return this.SuccessCountValue; }

            set { SetProperty(ref SuccessCountValue, value); }

        }
        private int ErrorCountValue;

        public int ErrorCount

        {

            get { return this.ErrorCountValue; }

            set { SetProperty(ref ErrorCountValue, value); }

        }
        private List<FileImportError> ErrorsValue = new List<FileImportError>();

        public List<FileImportError> Errors

        {

            get { return this.ErrorsValue; }

            set { SetProperty(ref ErrorsValue, value); }

        }
    }

    /// <summary>
    /// File import error
    /// </summary>
    public class FileImportError : ModelEntityBase
    {
        private int RowNumberValue;

        public int RowNumber

        {

            get { return this.RowNumberValue; }

            set { SetProperty(ref RowNumberValue, value); }

        }
        private string MessageValue = string.Empty;

        public string Message

        {

            get { return this.MessageValue; }

            set { SetProperty(ref MessageValue, value); }

        }
        private string? ColumnNameValue;

        public string? ColumnName

        {

            get { return this.ColumnNameValue; }

            set { SetProperty(ref ColumnNameValue, value); }

        }
    }

    /// <summary>
    /// Export request
    /// </summary>
    public class ExportRequest : ModelEntityBase
    {
        private string TableNameValue = string.Empty;

        public string TableName

        {

            get { return this.TableNameValue; }

            set { SetProperty(ref TableNameValue, value); }

        }
        private List<AppFilter>? FiltersValue;

        public List<AppFilter>? Filters

        {

            get { return this.FiltersValue; }

            set { SetProperty(ref FiltersValue, value); }

        }
        private string FormatValue = "csv";

        public string Format

        {

            get { return this.FormatValue; }

            set { SetProperty(ref FormatValue, value); }

        } // "csv" or "json"
        private bool IncludeHeadersValue = true;

        public bool IncludeHeaders

        {

            get { return this.IncludeHeadersValue; }

            set { SetProperty(ref IncludeHeadersValue, value); }

        }
        private string? ConnectionNameValue;

        public string? ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
    }

    /// <summary>
    /// Validation request
    /// </summary>
    public class ValidationRequest : ModelEntityBase
    {
        private string TableNameValue = string.Empty;

        public string TableName

        {

            get { return this.TableNameValue; }

            set { SetProperty(ref TableNameValue, value); }

        }
        public Dictionary<string, object> EntityData { get; set; } = new Dictionary<string, object>();
        private string? ConnectionNameValue;

        public string? ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
    }

    /// <summary>
    /// Batch validation request
    /// </summary>
    public class BatchValidationRequest : ModelEntityBase
    {
        private string TableNameValue = string.Empty;

        public string TableName

        {

            get { return this.TableNameValue; }

            set { SetProperty(ref TableNameValue, value); }

        }
        public List<Dictionary<string, object>> Entities { get; set; } = new List<Dictionary<string, object>>();
        private string? ConnectionNameValue;

        public string? ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
    }

    /// <summary>
    /// Validation result
    /// </summary>
    public class ValidationResult : ModelEntityBase
    {
        private bool IsValidValue;

        public bool IsValid

        {

            get { return this.IsValidValue; }

            set { SetProperty(ref IsValidValue, value); }

        }
        private List<ValidationError> ErrorsValue = new List<ValidationError>();

        public List<ValidationError> Errors

        {

            get { return this.ErrorsValue; }

            set { SetProperty(ref ErrorsValue, value); }

        }
        public int ErrorCount => Errors.Count;
    }

    /// <summary>
    /// Validation error
    /// </summary>
    public class ValidationError : ModelEntityBase
    {
        private string FieldNameValue = string.Empty;

        public string FieldName

        {

            get { return this.FieldNameValue; }

            set { SetProperty(ref FieldNameValue, value); }

        }
        private string ErrorMessageValue = string.Empty;

        public string ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }
        private string? ErrorCodeValue;

        public string? ErrorCode

        {

            get { return this.ErrorCodeValue; }

            set { SetProperty(ref ErrorCodeValue, value); }

        }
    }

    /// <summary>
    /// Data quality request
    /// </summary>
    public class DataQualityRequest : ModelEntityBase
    {
        private string TableNameValue = string.Empty;

        public string TableName

        {

            get { return this.TableNameValue; }

            set { SetProperty(ref TableNameValue, value); }

        }
        private string? ConnectionNameValue;

        public string? ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
    }

    /// <summary>
    /// Data quality result
    /// </summary>
    public class DataQualityResult : ModelEntityBase
    {
        private string TableNameValue = string.Empty;

        public string TableName

        {

            get { return this.TableNameValue; }

            set { SetProperty(ref TableNameValue, value); }

        }
        private double OverallQualityScoreValue;

        public double OverallQualityScore

        {

            get { return this.OverallQualityScoreValue; }

            set { SetProperty(ref OverallQualityScoreValue, value); }

        }
        public Dictionary<string, double> FieldQualityScores { get; set; } = new Dictionary<string, double>();
        private int TotalRowsValue;

        public int TotalRows

        {

            get { return this.TotalRowsValue; }

            set { SetProperty(ref TotalRowsValue, value); }

        }
        private int CompleteRowsValue;

        public int CompleteRows

        {

            get { return this.CompleteRowsValue; }

            set { SetProperty(ref CompleteRowsValue, value); }

        }
        private List<string> QualityIssuesValue = new List<string>();

        public List<string> QualityIssues

        {

            get { return this.QualityIssuesValue; }

            set { SetProperty(ref QualityIssuesValue, value); }

        }
    }

    /// <summary>
    /// Data quality dashboard result
    /// </summary>
    public class DataQualityDashboardResult : ModelEntityBase
    {
        public Dictionary<string, DataQualityResult> TableQualityResults { get; set; } = new Dictionary<string, DataQualityResult>();
        private double OverallQualityScoreValue;

        public double OverallQualityScore

        {

            get { return this.OverallQualityScoreValue; }

            set { SetProperty(ref OverallQualityScoreValue, value); }

        }
        private int TotalTablesValue;

        public int TotalTables

        {

            get { return this.TotalTablesValue; }

            set { SetProperty(ref TotalTablesValue, value); }

        }
        private int TablesWithIssuesValue;

        public int TablesWithIssues

        {

            get { return this.TablesWithIssuesValue; }

            set { SetProperty(ref TablesWithIssuesValue, value); }

        }
    }

    /// <summary>
    /// Versioning request
    /// </summary>
    public class VersioningRequest : ModelEntityBase
    {
        private string TableNameValue = string.Empty;

        public string TableName

        {

            get { return this.TableNameValue; }

            set { SetProperty(ref TableNameValue, value); }

        }
        private string EntityIdValue = string.Empty;

        public string EntityId

        {

            get { return this.EntityIdValue; }

            set { SetProperty(ref EntityIdValue, value); }

        }
        private string? ConnectionNameValue;

        public string? ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
        private string UserIdValue = string.Empty;

        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }

    /// <summary>
    /// Versioning result
    /// </summary>
    public class VersioningResult : ModelEntityBase
    {
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private string VersionIdValue = string.Empty;

        public string VersionId

        {

            get { return this.VersionIdValue; }

            set { SetProperty(ref VersionIdValue, value); }

        }
        private string MessageValue = string.Empty;

        public string Message

        {

            get { return this.MessageValue; }

            set { SetProperty(ref MessageValue, value); }

        }
        private string? ErrorMessageValue;

        public string? ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }
    }

    /// <summary>
    /// Version info
    /// </summary>
    public class VersionInfo : ModelEntityBase
    {
        private string VersionIdValue = string.Empty;

        public string VersionId

        {

            get { return this.VersionIdValue; }

            set { SetProperty(ref VersionIdValue, value); }

        }
        private DateTime CreatedAtValue;

        public DateTime CreatedAt

        {

            get { return this.CreatedAtValue; }

            set { SetProperty(ref CreatedAtValue, value); }

        }
        private string CreatedByValue = string.Empty;

        public string CreatedBy

        {

            get { return this.CreatedByValue; }

            set { SetProperty(ref CreatedByValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        public Dictionary<string, object>? EntityData { get; set; }
    }

    /// <summary>
    /// Get version history request
    /// </summary>
    public class GetVersionHistoryRequest : ModelEntityBase
    {
        private string TableNameValue = string.Empty;

        public string TableName

        {

            get { return this.TableNameValue; }

            set { SetProperty(ref TableNameValue, value); }

        }
        private string EntityIdValue = string.Empty;

        public string EntityId

        {

            get { return this.EntityIdValue; }

            set { SetProperty(ref EntityIdValue, value); }

        }
        private string? ConnectionNameValue;

        public string? ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
    }

    /// <summary>
    /// Restore version request
    /// </summary>
    public class RestoreVersionRequest : ModelEntityBase
    {
        private string TableNameValue = string.Empty;

        public string TableName

        {

            get { return this.TableNameValue; }

            set { SetProperty(ref TableNameValue, value); }

        }
        private string EntityIdValue = string.Empty;

        public string EntityId

        {

            get { return this.EntityIdValue; }

            set { SetProperty(ref EntityIdValue, value); }

        }
        private string VersionIdValue = string.Empty;

        public string VersionId

        {

            get { return this.VersionIdValue; }

            set { SetProperty(ref VersionIdValue, value); }

        }
        private string? ConnectionNameValue;

        public string? ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
        private string UserIdValue = string.Empty;

        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }

    /// <summary>
    /// Get defaults request
    /// </summary>
    public class GetDefaultsRequest : ModelEntityBase
    {
        private string EntityTypeValue = string.Empty;

        public string EntityType

        {

            get { return this.EntityTypeValue; }

            set { SetProperty(ref EntityTypeValue, value); }

        }
        private string? ConnectionNameValue;

        public string? ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
    }

    /// <summary>
    /// Get well status facets request
    /// </summary>
    public class GetWellStatusFacetsRequest : ModelEntityBase
    {
        private string StatusIdValue = string.Empty;

        public string StatusId

        {

            get { return this.StatusIdValue; }

            set { SetProperty(ref StatusIdValue, value); }

        }
        private string? ConnectionNameValue;

        public string? ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
    }
}








