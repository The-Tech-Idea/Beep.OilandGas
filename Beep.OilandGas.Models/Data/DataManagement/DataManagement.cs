using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Data
{
    #region Validation DTOs

    /// <summary>
    /// Result of entity validation
    /// </summary>
    public class ValidationResult : ModelEntityBase
    {
        private bool IsValidValue;

        public bool IsValid

        {

            get { return this.IsValidValue; }

            set { SetProperty(ref IsValidValue, value); }

        }
        private string FieldNameValue = string.Empty;

        public string FieldName

        {

            get { return this.FieldNameValue; }

            set { SetProperty(ref FieldNameValue, value); }

        }
          public Dictionary<string, object> RuleParameters { get; set; } = new Dictionary<string, object>();
        private List<ValidationError> ErrorsValue = new List<ValidationError>();

        public List<ValidationError> Errors

        {

            get { return this.ErrorsValue; }

            set { SetProperty(ref ErrorsValue, value); }

        }
        private List<ValidationWarning> WarningsValue = new List<ValidationWarning>();

        public List<ValidationWarning> Warnings

        {

            get { return this.WarningsValue; }

            set { SetProperty(ref WarningsValue, value); }

        }
        private object EntityValue;

        public object Entity

        {

            get { return this.EntityValue; }

            set { SetProperty(ref EntityValue, value); }

        }
        private string TableNameValue;

        public string TableName

        {

            get { return this.TableNameValue; }

            set { SetProperty(ref TableNameValue, value); }

        }
        private string ErrorMessageValue = string.Empty;

        public string ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }

        private string ValidationIdValue = string.Empty;


        public string ValidationId


        {


            get { return this.ValidationIdValue; }


            set { SetProperty(ref ValidationIdValue, value); }


        }

        private DateTime ValidatedDateValue = DateTime.UtcNow;


        public DateTime ValidatedDate


        {


            get { return this.ValidatedDateValue; }


            set { SetProperty(ref ValidatedDateValue, value); }


        }

           private string RuleIdValue = string.Empty;


           public string RuleId


           {


               get { return this.RuleIdValue; }


               set { SetProperty(ref RuleIdValue, value); }


           }
            private string RuleNameValue = string.Empty;

            public string RuleName

            {

                get { return this.RuleNameValue; }

                set { SetProperty(ref RuleNameValue, value); }

            }
            private string RuleTypeValue = string.Empty;

            public string RuleType

            {

                get { return this.RuleTypeValue; }

                set { SetProperty(ref RuleTypeValue, value); }

            } // REQUIRED, RANGE, FORMAT, BUSINESS_RULE
    }

    /// <summary>
    /// Validation error
    /// </summary>
    public class ValidationError : ModelEntityBase
    {
        private string FieldNameValue;

        public string FieldName

        {

            get { return this.FieldNameValue; }

            set { SetProperty(ref FieldNameValue, value); }

        }
        private string ErrorMessageValue;

        public string ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }
        private string RuleNameValue;

        public string RuleName

        {

            get { return this.RuleNameValue; }

            set { SetProperty(ref RuleNameValue, value); }

        }
        private ValidationSeverity SeverityValue;

        public ValidationSeverity Severity

        {

            get { return this.SeverityValue; }

            set { SetProperty(ref SeverityValue, value); }

        }
    }

    /// <summary>
    /// Validation warning
    /// </summary>
    public class ValidationWarning : ModelEntityBase
    {
        private string FieldNameValue;

        public string FieldName

        {

            get { return this.FieldNameValue; }

            set { SetProperty(ref FieldNameValue, value); }

        }
        private string WarningMessageValue;

        public string WarningMessage

        {

            get { return this.WarningMessageValue; }

            set { SetProperty(ref WarningMessageValue, value); }

        }
        private string RuleNameValue;

        public string RuleName

        {

            get { return this.RuleNameValue; }

            set { SetProperty(ref RuleNameValue, value); }

        }
    }

    /// <summary>
    /// Validation rule definition
    /// </summary>
    public class ValidationRule : ModelEntityBase
    {
        private string RuleNameValue;

        public string RuleName

        {

            get { return this.RuleNameValue; }

            set { SetProperty(ref RuleNameValue, value); }

        }
        private string FieldNameValue;

        public string FieldName

        {

            get { return this.FieldNameValue; }

            set { SetProperty(ref FieldNameValue, value); }

        }
        private ValidationRuleType RuleTypeValue;

        public ValidationRuleType RuleType

        {

            get { return this.RuleTypeValue; }

            set { SetProperty(ref RuleTypeValue, value); }

        }
        private string RuleValueValue;

        public string RuleValue

        {

            get { return this.RuleValueValue; }

            set { SetProperty(ref RuleValueValue, value); }

        }
        private string ErrorMessageValue;

        public string ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }
        private ValidationSeverity SeverityValue;

        public ValidationSeverity Severity

        {

            get { return this.SeverityValue; }

            set { SetProperty(ref SeverityValue, value); }

        }
        private bool IsActiveValue;

        public bool IsActive

        {

            get { return this.IsActiveValue; }

            set { SetProperty(ref IsActiveValue, value); }

        }
      
        public Dictionary<string, object> RuleParameters { get; set; } = new Dictionary<string, object>();
        private string RuleIdValue = string.Empty;

        public string RuleId

        {

            get { return this.RuleIdValue; }

            set { SetProperty(ref RuleIdValue, value); }

        }
       
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
    public class DataQualityScore : ModelEntityBase
    {
        private object EntityValue;

        public object Entity

        {

            get { return this.EntityValue; }

            set { SetProperty(ref EntityValue, value); }

        }
        private string TableNameValue;

        public string TableName

        {

            get { return this.TableNameValue; }

            set { SetProperty(ref TableNameValue, value); }

        }
        private double OverallScoreValue;

        public double OverallScore

        {

            get { return this.OverallScoreValue; }

            set { SetProperty(ref OverallScoreValue, value); }

        } // 0-100
        public Dictionary<string, double> FieldScores { get; set; } = new Dictionary<string, double>();
        private List<DataQualityIssue> IssuesValue = new List<DataQualityIssue>();

        public List<DataQualityIssue> Issues

        {

            get { return this.IssuesValue; }

            set { SetProperty(ref IssuesValue, value); }

        }
    }

    /// <summary>
    /// Data quality metrics for a table
    /// </summary>
    public class DataQualityMetrics : ModelEntityBase
    {
        private string TableNameValue;

        public string TableName

        {

            get { return this.TableNameValue; }

            set { SetProperty(ref TableNameValue, value); }

        }
        private int TotalRecordsValue;

        public int TotalRecords

        {

            get { return this.TotalRecordsValue; }

            set { SetProperty(ref TotalRecordsValue, value); }

        }
        private int CompleteRecordsValue;

        public int CompleteRecords

        {

            get { return this.CompleteRecordsValue; }

            set { SetProperty(ref CompleteRecordsValue, value); }

        }
        private int IncompleteRecordsValue;

        public int IncompleteRecords

        {

            get { return this.IncompleteRecordsValue; }

            set { SetProperty(ref IncompleteRecordsValue, value); }

        }
        private double CompletenessScoreValue;

        public double CompletenessScore

        {

            get { return this.CompletenessScoreValue; }

            set { SetProperty(ref CompletenessScoreValue, value); }

        } // 0-100
        private double AccuracyScoreValue;

        public double AccuracyScore

        {

            get { return this.AccuracyScoreValue; }

            set { SetProperty(ref AccuracyScoreValue, value); }

        } // 0-100
        private double ConsistencyScoreValue;

        public double ConsistencyScore

        {

            get { return this.ConsistencyScoreValue; }

            set { SetProperty(ref ConsistencyScoreValue, value); }

        } // 0-100
        private double OverallQualityScoreValue;

        public double OverallQualityScore

        {

            get { return this.OverallQualityScoreValue; }

            set { SetProperty(ref OverallQualityScoreValue, value); }

        } // 0-100
        public Dictionary<string, FieldQualityMetrics> FieldMetrics { get; set; } = new Dictionary<string, FieldQualityMetrics>();
    }

    /// <summary>
    /// Quality metrics for a specific field
    /// </summary>
    public class FieldQualityMetrics : ModelEntityBase
    {
        private string FieldNameValue;

        public string FieldName

        {

            get { return this.FieldNameValue; }

            set { SetProperty(ref FieldNameValue, value); }

        }
        private int TotalValuesValue;

        public int TotalValues

        {

            get { return this.TotalValuesValue; }

            set { SetProperty(ref TotalValuesValue, value); }

        }
        private int NullValuesValue;

        public int NullValues

        {

            get { return this.NullValuesValue; }

            set { SetProperty(ref NullValuesValue, value); }

        }
        private int EmptyValuesValue;

        public int EmptyValues

        {

            get { return this.EmptyValuesValue; }

            set { SetProperty(ref EmptyValuesValue, value); }

        }
        private int ValidValuesValue;

        public int ValidValues

        {

            get { return this.ValidValuesValue; }

            set { SetProperty(ref ValidValuesValue, value); }

        }
        private double CompletenessValue;

        public double Completeness

        {

            get { return this.CompletenessValue; }

            set { SetProperty(ref CompletenessValue, value); }

        } // 0-100
        private List<string> InvalidValuesValue = new List<string>();

        public List<string> InvalidValues

        {

            get { return this.InvalidValuesValue; }

            set { SetProperty(ref InvalidValuesValue, value); }

        }
    }

    /// <summary>
    /// Data quality issue
    /// </summary>
    public class DataQualityIssue : ModelEntityBase
    {
        private string TableNameValue;

        public string TableName

        {

            get { return this.TableNameValue; }

            set { SetProperty(ref TableNameValue, value); }

        }
        private string FieldNameValue;

        public string FieldName

        {

            get { return this.FieldNameValue; }

            set { SetProperty(ref FieldNameValue, value); }

        }
        private string IssueTypeValue;

        public string IssueType

        {

            get { return this.IssueTypeValue; }

            set { SetProperty(ref IssueTypeValue, value); }

        }
        private string IssueDescriptionValue;

        public string IssueDescription

        {

            get { return this.IssueDescriptionValue; }

            set { SetProperty(ref IssueDescriptionValue, value); }

        }
        private object RecordIdValue;

        public object RecordId

        {

            get { return this.RecordIdValue; }

            set { SetProperty(ref RecordIdValue, value); }

        }
        private string SeverityValue;

        public string Severity

        {

            get { return this.SeverityValue; }

            set { SetProperty(ref SeverityValue, value); }

        }
    }

    #endregion

    #region Reconciliation DTOs

    /// <summary>
    /// Result of data reconciliation
    /// </summary>
    public class ReconciliationResult : ModelEntityBase
    {
        private string TableNameValue;

        public string TableName

        {

            get { return this.TableNameValue; }

            set { SetProperty(ref TableNameValue, value); }

        }
        private int Source1RecordCountValue;

        public int Source1RecordCount

        {

            get { return this.Source1RecordCountValue; }

            set { SetProperty(ref Source1RecordCountValue, value); }

        }
        private int Source2RecordCountValue;

        public int Source2RecordCount

        {

            get { return this.Source2RecordCountValue; }

            set { SetProperty(ref Source2RecordCountValue, value); }

        }
        private int MatchingRecordsValue;

        public int MatchingRecords

        {

            get { return this.MatchingRecordsValue; }

            set { SetProperty(ref MatchingRecordsValue, value); }

        }
        private int Source1OnlyRecordsValue;

        public int Source1OnlyRecords

        {

            get { return this.Source1OnlyRecordsValue; }

            set { SetProperty(ref Source1OnlyRecordsValue, value); }

        }
        private int Source2OnlyRecordsValue;

        public int Source2OnlyRecords

        {

            get { return this.Source2OnlyRecordsValue; }

            set { SetProperty(ref Source2OnlyRecordsValue, value); }

        }
        private int DifferingRecordsValue;

        public int DifferingRecords

        {

            get { return this.DifferingRecordsValue; }

            set { SetProperty(ref DifferingRecordsValue, value); }

        }
        private List<ReconciliationDifference> DifferencesValue = new List<ReconciliationDifference>();

        public List<ReconciliationDifference> Differences

        {

            get { return this.DifferencesValue; }

            set { SetProperty(ref DifferencesValue, value); }

        }
    }

    /// <summary>
    /// Difference found during reconciliation
    /// </summary>
    public class ReconciliationDifference : ModelEntityBase
    {
        private object RecordIdValue;

        public object RecordId

        {

            get { return this.RecordIdValue; }

            set { SetProperty(ref RecordIdValue, value); }

        }
        private List<FieldDifference> FieldDifferencesValue = new List<FieldDifference>();

        public List<FieldDifference> FieldDifferences

        {

            get { return this.FieldDifferencesValue; }

            set { SetProperty(ref FieldDifferencesValue, value); }

        }
        private string Source1ValueValue;

        public string Source1Value

        {

            get { return this.Source1ValueValue; }

            set { SetProperty(ref Source1ValueValue, value); }

        }
        private string Source2ValueValue;

        public string Source2Value

        {

            get { return this.Source2ValueValue; }

            set { SetProperty(ref Source2ValueValue, value); }

        }
    }

    /// <summary>
    /// Difference in a specific field
    /// </summary>
    public class FieldDifference : ModelEntityBase
    {
        private string FieldNameValue;

        public string FieldName

        {

            get { return this.FieldNameValue; }

            set { SetProperty(ref FieldNameValue, value); }

        }
        private object Source1ValueValue;

        public object Source1Value

        {

            get { return this.Source1ValueValue; }

            set { SetProperty(ref Source1ValueValue, value); }

        }
        private object Source2ValueValue;

        public object Source2Value

        {

            get { return this.Source2ValueValue; }

            set { SetProperty(ref Source2ValueValue, value); }

        }
        private string DifferenceTypeValue;

        public string DifferenceType

        {

            get { return this.DifferenceTypeValue; }

            set { SetProperty(ref DifferenceTypeValue, value); }

        } // Missing, Different, Extra
    }

    #endregion

    #region Deduplication DTOs

    /// <summary>
    /// Group of duplicate records
    /// </summary>
    public class DuplicateGroup : ModelEntityBase
    {
        private string TableNameValue;

        public string TableName

        {

            get { return this.TableNameValue; }

            set { SetProperty(ref TableNameValue, value); }

        }
        private List<DuplicateRecord> RecordsValue = new List<DuplicateRecord>();

        public List<DuplicateRecord> Records

        {

            get { return this.RecordsValue; }

            set { SetProperty(ref RecordsValue, value); }

        }
        private double SimilarityScoreValue;

        public double SimilarityScore

        {

            get { return this.SimilarityScoreValue; }

            set { SetProperty(ref SimilarityScoreValue, value); }

        }
        private List<string> MatchFieldsValue = new List<string>();

        public List<string> MatchFields

        {

            get { return this.MatchFieldsValue; }

            set { SetProperty(ref MatchFieldsValue, value); }

        }
    }

    /// <summary>
    /// Duplicate class
    /// </summary>
    public class DuplicateRecord : ModelEntityBase
    {
        private object RecordIdValue;

        public object RecordId

        {

            get { return this.RecordIdValue; }

            set { SetProperty(ref RecordIdValue, value); }

        }
        private object EntityValue;

        public object Entity

        {

            get { return this.EntityValue; }

            set { SetProperty(ref EntityValue, value); }

        }
        public Dictionary<string, object> FieldValues { get; set; } = new Dictionary<string, object>();
        private bool IsMasterValue;

        public bool IsMaster

        {

            get { return this.IsMasterValue; }

            set { SetProperty(ref IsMasterValue, value); }

        }
    }

    /// <summary>
    /// Result of merging duplicates
    /// </summary>
    public class MergeResult : ModelEntityBase
    {
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private object MergedEntityValue;

        public object MergedEntity

        {

            get { return this.MergedEntityValue; }

            set { SetProperty(ref MergedEntityValue, value); }

        }
        private List<object> MergedRecordIdsValue = new List<object>();

        public List<object> MergedRecordIds

        {

            get { return this.MergedRecordIdsValue; }

            set { SetProperty(ref MergedRecordIdsValue, value); }

        }
        private List<string> MessagesValue = new List<string>();

        public List<string> Messages

        {

            get { return this.MessagesValue; }

            set { SetProperty(ref MessagesValue, value); }

        }
    }

    #endregion

    #region Archiving DTOs

    /// <summary>
    /// Criteria for archiving records
    /// </summary>
    public class ArchiveCriteria : ModelEntityBase
    {
        private List<AppFilter> FiltersValue = new List<AppFilter>();

        public List<AppFilter> Filters

        {

            get { return this.FiltersValue; }

            set { SetProperty(ref FiltersValue, value); }

        }
        private DateTime? OlderThanDateValue;

        public DateTime? OlderThanDate

        {

            get { return this.OlderThanDateValue; }

            set { SetProperty(ref OlderThanDateValue, value); }

        }
        private string StatusFieldValue;

        public string StatusField

        {

            get { return this.StatusFieldValue; }

            set { SetProperty(ref StatusFieldValue, value); }

        }
        private string StatusValueValue;

        public string StatusValue

        {

            get { return this.StatusValueValue; }

            set { SetProperty(ref StatusValueValue, value); }

        }
        private bool ArchiveInactiveOnlyValue;

        public bool ArchiveInactiveOnly

        {

            get { return this.ArchiveInactiveOnlyValue; }

            set { SetProperty(ref ArchiveInactiveOnlyValue, value); }

        }
    }

    /// <summary>
    /// Result of archiving operation
    /// </summary>
    public class ArchiveResult : ModelEntityBase
    {
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private int ArchivedRecordCountValue;

        public int ArchivedRecordCount

        {

            get { return this.ArchivedRecordCountValue; }

            set { SetProperty(ref ArchivedRecordCountValue, value); }

        }
        private List<object> ArchivedRecordIdsValue = new List<object>();

        public List<object> ArchivedRecordIds

        {

            get { return this.ArchivedRecordIdsValue; }

            set { SetProperty(ref ArchivedRecordIdsValue, value); }

        }
        private string ArchiveLocationValue;

        public string ArchiveLocation

        {

            get { return this.ArchiveLocationValue; }

            set { SetProperty(ref ArchiveLocationValue, value); }

        }
        private List<string> MessagesValue = new List<string>();

        public List<string> Messages

        {

            get { return this.MessagesValue; }

            set { SetProperty(ref MessagesValue, value); }

        }
    }

    /// <summary>
    /// Result of restore operation
    /// </summary>
    public class RestoreResult : ModelEntityBase
    {
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private int RestoredRecordCountValue;

        public int RestoredRecordCount

        {

            get { return this.RestoredRecordCountValue; }

            set { SetProperty(ref RestoredRecordCountValue, value); }

        }
        private List<object> RestoredRecordIdsValue = new List<object>();

        public List<object> RestoredRecordIds

        {

            get { return this.RestoredRecordIdsValue; }

            set { SetProperty(ref RestoredRecordIdsValue, value); }

        }
        private List<string> MessagesValue = new List<string>();

        public List<string> Messages

        {

            get { return this.MessagesValue; }

            set { SetProperty(ref MessagesValue, value); }

        }
    }

    #endregion

    #region Bulk Operations DTOs

    /// <summary>
    /// Result of bulk operation
    /// </summary>
    public class BulkOperationResult : ModelEntityBase
    {
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private int ProcessedCountValue;

        public int ProcessedCount

        {

            get { return this.ProcessedCountValue; }

            set { SetProperty(ref ProcessedCountValue, value); }

        }
        private int SuccessCountValue;

        public int SuccessCount

        {

            get { return this.SuccessCountValue; }

            set { SetProperty(ref SuccessCountValue, value); }

        }
        private int FailureCountValue;

        public int FailureCount

        {

            get { return this.FailureCountValue; }

            set { SetProperty(ref FailureCountValue, value); }

        }
        private List<BulkOperationError> ErrorsValue = new List<BulkOperationError>();

        public List<BulkOperationError> Errors

        {

            get { return this.ErrorsValue; }

            set { SetProperty(ref ErrorsValue, value); }

        }
        private TimeSpan DurationValue;

        public TimeSpan Duration

        {

            get { return this.DurationValue; }

            set { SetProperty(ref DurationValue, value); }

        }
    }

    /// <summary>
    /// Error in bulk operation
    /// </summary>
    public class BulkOperationError : ModelEntityBase
    {
        private int RecordIndexValue;

        public int RecordIndex

        {

            get { return this.RecordIndexValue; }

            set { SetProperty(ref RecordIndexValue, value); }

        }
        private object RecordIdValue;

        public object RecordId

        {

            get { return this.RecordIdValue; }

            set { SetProperty(ref RecordIdValue, value); }

        }
        private string ErrorMessageValue;

        public string ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }
        private string ErrorTypeValue;

        public string ErrorType

        {

            get { return this.ErrorTypeValue; }

            set { SetProperty(ref ErrorTypeValue, value); }

        }
    }

    #endregion

    #region Export/Import DTOs

    /// <summary>
    /// Result of import operation
    /// </summary>
    public class ImportResult : ModelEntityBase
    {
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private int TotalRecordsValue;

        public int TotalRecords

        {

            get { return this.TotalRecordsValue; }

            set { SetProperty(ref TotalRecordsValue, value); }

        }
        private int ImportedCountValue;

        public int ImportedCount

        {

            get { return this.ImportedCountValue; }

            set { SetProperty(ref ImportedCountValue, value); }

        }
        private int SkippedCountValue;

        public int SkippedCount

        {

            get { return this.SkippedCountValue; }

            set { SetProperty(ref SkippedCountValue, value); }

        }
        private int ErrorCountValue;

        public int ErrorCount

        {

            get { return this.ErrorCountValue; }

            set { SetProperty(ref ErrorCountValue, value); }

        }
        private List<ImportError> ErrorsValue = new List<ImportError>();

        public List<ImportError> Errors

        {

            get { return this.ErrorsValue; }

            set { SetProperty(ref ErrorsValue, value); }

        }
        private string OutputPathValue;

        public string OutputPath

        {

            get { return this.OutputPathValue; }

            set { SetProperty(ref OutputPathValue, value); }

        }
    }

    /// <summary>
    /// Error during import
    /// </summary>
    public class ImportError : ModelEntityBase
    {
        private int RowNumberValue;

        public int RowNumber

        {

            get { return this.RowNumberValue; }

            set { SetProperty(ref RowNumberValue, value); }

        }
        private string ErrorMessageValue;

        public string ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }
        public Dictionary<string, object> RowData { get; set; } = new Dictionary<string, object>();
    }

    #endregion

    #region Data Lineage DTOs

    /// <summary>
    /// Data lineage information
    /// </summary>
    public class DataLineageInfo : ModelEntityBase
    {
        private string TableNameValue;

        public string TableName

        {

            get { return this.TableNameValue; }

            set { SetProperty(ref TableNameValue, value); }

        }
        private object EntityIdValue;

        public object EntityId

        {

            get { return this.EntityIdValue; }

            set { SetProperty(ref EntityIdValue, value); }

        }
        private string SourceSystemValue;

        public string SourceSystem

        {

            get { return this.SourceSystemValue; }

            set { SetProperty(ref SourceSystemValue, value); }

        }
        private string SourceTableValue;

        public string SourceTable

        {

            get { return this.SourceTableValue; }

            set { SetProperty(ref SourceTableValue, value); }

        }
        private object SourceEntityIdValue;

        public object SourceEntityId

        {

            get { return this.SourceEntityIdValue; }

            set { SetProperty(ref SourceEntityIdValue, value); }

        }
        private string TransformationTypeValue;

        public string TransformationType

        {

            get { return this.TransformationTypeValue; }

            set { SetProperty(ref TransformationTypeValue, value); }

        }
        private string TransformationDetailsValue;

        public string TransformationDetails

        {

            get { return this.TransformationDetailsValue; }

            set { SetProperty(ref TransformationDetailsValue, value); }

        }
        private DateTime CreatedDateValue;

        public DateTime CreatedDate

        {

            get { return this.CreatedDateValue; }

            set { SetProperty(ref CreatedDateValue, value); }

        }
        private string CreatedByValue;

        public string CreatedBy

        {

            get { return this.CreatedByValue; }

            set { SetProperty(ref CreatedByValue, value); }

        }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Data lineage node (dependency)
    /// </summary>
    public class DataLineageNode : ModelEntityBase
    {
        private string TableNameValue;

        public string TableName

        {

            get { return this.TableNameValue; }

            set { SetProperty(ref TableNameValue, value); }

        }
        private object EntityIdValue;

        public object EntityId

        {

            get { return this.EntityIdValue; }

            set { SetProperty(ref EntityIdValue, value); }

        }
        private string RelationshipTypeValue;

        public string RelationshipType

        {

            get { return this.RelationshipTypeValue; }

            set { SetProperty(ref RelationshipTypeValue, value); }

        }
        private string DescriptionValue;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
    }

    #endregion

    #region Data Versioning DTOs

    /// <summary>
    /// Version snapshot of an entity
    /// </summary>
    public class VersionSnapshot : ModelEntityBase
    {
        private int VersionNumberValue;

        public int VersionNumber

        {

            get { return this.VersionNumberValue; }

            set { SetProperty(ref VersionNumberValue, value); }

        }
        private string TableNameValue;

        public string TableName

        {

            get { return this.TableNameValue; }

            set { SetProperty(ref TableNameValue, value); }

        }
        private object EntityIdValue;

        public object EntityId

        {

            get { return this.EntityIdValue; }

            set { SetProperty(ref EntityIdValue, value); }

        }
        private object EntityDataValue;

        public object EntityData

        {

            get { return this.EntityDataValue; }

            set { SetProperty(ref EntityDataValue, value); }

        }
        private string VersionLabelValue;

        public string VersionLabel

        {

            get { return this.VersionLabelValue; }

            set { SetProperty(ref VersionLabelValue, value); }

        }
        private DateTime CreatedDateValue;

        public DateTime CreatedDate

        {

            get { return this.CreatedDateValue; }

            set { SetProperty(ref CreatedDateValue, value); }

        }
        private string CreatedByValue;

        public string CreatedBy

        {

            get { return this.CreatedByValue; }

            set { SetProperty(ref CreatedByValue, value); }

        }
        private string ChangeDescriptionValue;

        public string ChangeDescription

        {

            get { return this.ChangeDescriptionValue; }

            set { SetProperty(ref ChangeDescriptionValue, value); }

        }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Comparison between two versions
    /// </summary>
    public class VersionComparison : ModelEntityBase
    {
        private string TableNameValue;

        public string TableName

        {

            get { return this.TableNameValue; }

            set { SetProperty(ref TableNameValue, value); }

        }
        private object EntityIdValue;

        public object EntityId

        {

            get { return this.EntityIdValue; }

            set { SetProperty(ref EntityIdValue, value); }

        }
        private int Version1Value;

        public int Version1

        {

            get { return this.Version1Value; }

            set { SetProperty(ref Version1Value, value); }

        }
        private int Version2Value;

        public int Version2

        {

            get { return this.Version2Value; }

            set { SetProperty(ref Version2Value, value); }

        }
        private List<FieldDifference> DifferencesValue = new List<FieldDifference>();

        public List<FieldDifference> Differences

        {

            get { return this.DifferencesValue; }

            set { SetProperty(ref DifferencesValue, value); }

        }
        private bool HasDifferencesValue;

        public bool HasDifferences

        {

            get { return this.HasDifferencesValue; }

            set { SetProperty(ref HasDifferencesValue, value); }

        }
    }

    /// <summary>
    /// Result of restoring to a version
    /// </summary>
    public class RestoreVersionResult : ModelEntityBase
    {
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private object RestoredEntityValue;

        public object RestoredEntity

        {

            get { return this.RestoredEntityValue; }

            set { SetProperty(ref RestoredEntityValue, value); }

        }
        private int RestoredVersionNumberValue;

        public int RestoredVersionNumber

        {

            get { return this.RestoredVersionNumberValue; }

            set { SetProperty(ref RestoredVersionNumberValue, value); }

        }
        private string MessageValue;

        public string Message

        {

            get { return this.MessageValue; }

            set { SetProperty(ref MessageValue, value); }

        }
    }

    #endregion

    #region Data Access Audit DTOs

    /// <summary>
    /// Data access event
    /// </summary>
    public class DataAccessEvent : ModelEntityBase
    {
        private string EventIdValue;

        public string EventId

        {

            get { return this.EventIdValue; }

            set { SetProperty(ref EventIdValue, value); }

        }
        private string TableNameValue;

        public string TableName

        {

            get { return this.TableNameValue; }

            set { SetProperty(ref TableNameValue, value); }

        }
        private object EntityIdValue;

        public object EntityId

        {

            get { return this.EntityIdValue; }

            set { SetProperty(ref EntityIdValue, value); }

        }
        private string UserIdValue;

        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
        private string AccessTypeValue;

        public string AccessType

        {

            get { return this.AccessTypeValue; }

            set { SetProperty(ref AccessTypeValue, value); }

        } // Read, Write, Delete, Export
        private DateTime AccessDateValue;

        public DateTime AccessDate

        {

            get { return this.AccessDateValue; }

            set { SetProperty(ref AccessDateValue, value); }

        }
        private string IpAddressValue;

        public string IpAddress

        {

            get { return this.IpAddressValue; }

            set { SetProperty(ref IpAddressValue, value); }

        }
        private string ApplicationNameValue;

        public string ApplicationName

        {

            get { return this.ApplicationNameValue; }

            set { SetProperty(ref ApplicationNameValue, value); }

        }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Access statistics for compliance reporting
    /// </summary>
    public class AccessStatistics : ModelEntityBase
    {
        private string TableNameValue;

        public string TableName

        {

            get { return this.TableNameValue; }

            set { SetProperty(ref TableNameValue, value); }

        }
        private DateTime FromDateValue;

        public DateTime FromDate

        {

            get { return this.FromDateValue; }

            set { SetProperty(ref FromDateValue, value); }

        }
        private DateTime ToDateValue;

        public DateTime ToDate

        {

            get { return this.ToDateValue; }

            set { SetProperty(ref ToDateValue, value); }

        }
        private int TotalAccessEventsValue;

        public int TotalAccessEvents

        {

            get { return this.TotalAccessEventsValue; }

            set { SetProperty(ref TotalAccessEventsValue, value); }

        }
        private int UniqueUsersValue;

        public int UniqueUsers

        {

            get { return this.UniqueUsersValue; }

            set { SetProperty(ref UniqueUsersValue, value); }

        }
        private int ReadOperationsValue;

        public int ReadOperations

        {

            get { return this.ReadOperationsValue; }

            set { SetProperty(ref ReadOperationsValue, value); }

        }
        private int WriteOperationsValue;

        public int WriteOperations

        {

            get { return this.WriteOperationsValue; }

            set { SetProperty(ref WriteOperationsValue, value); }

        }
        private int DeleteOperationsValue;

        public int DeleteOperations

        {

            get { return this.DeleteOperationsValue; }

            set { SetProperty(ref DeleteOperationsValue, value); }

        }
        public Dictionary<string, int> AccessByUser { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> AccessByType { get; set; } = new Dictionary<string, int>();
    }

    #endregion

    #region Data Masking DTOs

    /// <summary>
    /// Masking strategy
    /// </summary>
    public class MaskingStrategy : ModelEntityBase
    {
        public Dictionary<string, MaskingRule> FieldRules { get; set; } = new Dictionary<string, MaskingRule>();
        private bool PreserveFormatValue = true;

        public bool PreserveFormat

        {

            get { return this.PreserveFormatValue; }

            set { SetProperty(ref PreserveFormatValue, value); }

        }
        private string DefaultMaskingValueValue = "***";

        public string DefaultMaskingValue

        {

            get { return this.DefaultMaskingValueValue; }

            set { SetProperty(ref DefaultMaskingValueValue, value); }

        }
    }

    /// <summary>
    /// Masking rule for a specific field
    /// </summary>
    public class MaskingRule : ModelEntityBase
    {
        private MaskingType TypeValue;

        public MaskingType Type

        {

            get { return this.TypeValue; }

            set { SetProperty(ref TypeValue, value); }

        }
        private string MaskingValueValue;

        public string MaskingValue

        {

            get { return this.MaskingValueValue; }

            set { SetProperty(ref MaskingValueValue, value); }

        }
        private bool PreserveFormatValue = true;

        public bool PreserveFormat

        {

            get { return this.PreserveFormatValue; }

            set { SetProperty(ref PreserveFormatValue, value); }

        }
        private int? PreserveLengthValue;

        public int? PreserveLength

        {

            get { return this.PreserveLengthValue; }

            set { SetProperty(ref PreserveLengthValue, value); }

        }
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
    public class SyncOptions : ModelEntityBase
    {
        private SyncDirection DirectionValue = SyncDirection.Both;

        public SyncDirection Direction

        {

            get { return this.DirectionValue; }

            set { SetProperty(ref DirectionValue, value); }

        }
        private List<string> KeyFieldsValue = new List<string>();

        public List<string> KeyFields

        {

            get { return this.KeyFieldsValue; }

            set { SetProperty(ref KeyFieldsValue, value); }

        }
        private List<string> SyncFieldsValue = new List<string>();

        public List<string> SyncFields

        {

            get { return this.SyncFieldsValue; }

            set { SetProperty(ref SyncFieldsValue, value); }

        }
        private ConflictResolutionStrategy DefaultConflictStrategyValue = ConflictResolutionStrategy.SourceWins;

        public ConflictResolutionStrategy DefaultConflictStrategy

        {

            get { return this.DefaultConflictStrategyValue; }

            set { SetProperty(ref DefaultConflictStrategyValue, value); }

        }
        private bool ValidateBeforeSyncValue = true;

        public bool ValidateBeforeSync

        {

            get { return this.ValidateBeforeSyncValue; }

            set { SetProperty(ref ValidateBeforeSyncValue, value); }

        }
        private bool CreateMissingRecordsValue = true;

        public bool CreateMissingRecords

        {

            get { return this.CreateMissingRecordsValue; }

            set { SetProperty(ref CreateMissingRecordsValue, value); }

        }
        private bool UpdateExistingRecordsValue = true;

        public bool UpdateExistingRecords

        {

            get { return this.UpdateExistingRecordsValue; }

            set { SetProperty(ref UpdateExistingRecordsValue, value); }

        }
    }

    /// <summary>
    /// Synchronization result
    /// </summary>
    public class SyncResult : ModelEntityBase
    {
        private string SyncIdValue;

        public string SyncId

        {

            get { return this.SyncIdValue; }

            set { SetProperty(ref SyncIdValue, value); }

        }
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private int RecordsProcessedValue;

        public int RecordsProcessed

        {

            get { return this.RecordsProcessedValue; }

            set { SetProperty(ref RecordsProcessedValue, value); }

        }
        private int RecordsCreatedValue;

        public int RecordsCreated

        {

            get { return this.RecordsCreatedValue; }

            set { SetProperty(ref RecordsCreatedValue, value); }

        }
        private int RecordsUpdatedValue;

        public int RecordsUpdated

        {

            get { return this.RecordsUpdatedValue; }

            set { SetProperty(ref RecordsUpdatedValue, value); }

        }
        private int RecordsSkippedValue;

        public int RecordsSkipped

        {

            get { return this.RecordsSkippedValue; }

            set { SetProperty(ref RecordsSkippedValue, value); }

        }
        private int ConflictsFoundValue;

        public int ConflictsFound

        {

            get { return this.ConflictsFoundValue; }

            set { SetProperty(ref ConflictsFoundValue, value); }

        }
        private List<SyncConflict> ConflictsValue = new List<SyncConflict>();

        public List<SyncConflict> Conflicts

        {

            get { return this.ConflictsValue; }

            set { SetProperty(ref ConflictsValue, value); }

        }
        private TimeSpan DurationValue;

        public TimeSpan Duration

        {

            get { return this.DurationValue; }

            set { SetProperty(ref DurationValue, value); }

        }
        private List<string> MessagesValue = new List<string>();

        public List<string> Messages

        {

            get { return this.MessagesValue; }

            set { SetProperty(ref MessagesValue, value); }

        }
    }

    /// <summary>
    /// Synchronization status
    /// </summary>
    public class SyncStatus : ModelEntityBase
    {
        private string SyncIdValue;

        public string SyncId

        {

            get { return this.SyncIdValue; }

            set { SetProperty(ref SyncIdValue, value); }

        }
        private SyncStatusType StatusValue;

        public SyncStatusType Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private DateTime StartTimeValue;

        public DateTime StartTime

        {

            get { return this.StartTimeValue; }

            set { SetProperty(ref StartTimeValue, value); }

        }
        private DateTime? EndTimeValue;

        public DateTime? EndTime

        {

            get { return this.EndTimeValue; }

            set { SetProperty(ref EndTimeValue, value); }

        }
        private int ProgressPercentageValue;

        public int ProgressPercentage

        {

            get { return this.ProgressPercentageValue; }

            set { SetProperty(ref ProgressPercentageValue, value); }

        }
        private string CurrentOperationValue;

        public string CurrentOperation

        {

            get { return this.CurrentOperationValue; }

            set { SetProperty(ref CurrentOperationValue, value); }

        }
        private List<string> ErrorsValue = new List<string>();

        public List<string> Errors

        {

            get { return this.ErrorsValue; }

            set { SetProperty(ref ErrorsValue, value); }

        }
    }

    /// <summary>
    /// Synchronization conflict
    /// </summary>
    public class SyncConflict : ModelEntityBase
    {
        private string ConflictIdValue;

        public string ConflictId

        {

            get { return this.ConflictIdValue; }

            set { SetProperty(ref ConflictIdValue, value); }

        }
        private string TableNameValue;

        public string TableName

        {

            get { return this.TableNameValue; }

            set { SetProperty(ref TableNameValue, value); }

        }
        private object EntityIdValue;

        public object EntityId

        {

            get { return this.EntityIdValue; }

            set { SetProperty(ref EntityIdValue, value); }

        }
        private object SourceValueValue;

        public object SourceValue

        {

            get { return this.SourceValueValue; }

            set { SetProperty(ref SourceValueValue, value); }

        }
        private object TargetValueValue;

        public object TargetValue

        {

            get { return this.TargetValueValue; }

            set { SetProperty(ref TargetValueValue, value); }

        }
        private List<FieldDifference> FieldDifferencesValue = new List<FieldDifference>();

        public List<FieldDifference> FieldDifferences

        {

            get { return this.FieldDifferencesValue; }

            set { SetProperty(ref FieldDifferencesValue, value); }

        }
        private string ConflictReasonValue;

        public string ConflictReason

        {

            get { return this.ConflictReasonValue; }

            set { SetProperty(ref ConflictReasonValue, value); }

        }
    }

    /// <summary>
    /// Conflict resolution result
    /// </summary>
    public class ConflictResolutionResult : ModelEntityBase
    {
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private object ResolvedEntityValue;

        public object ResolvedEntity

        {

            get { return this.ResolvedEntityValue; }

            set { SetProperty(ref ResolvedEntityValue, value); }

        }
        private ConflictResolutionStrategy StrategyUsedValue;

        public ConflictResolutionStrategy StrategyUsed

        {

            get { return this.StrategyUsedValue; }

            set { SetProperty(ref StrategyUsedValue, value); }

        }
        private string MessageValue;

        public string Message

        {

            get { return this.MessageValue; }

            set { SetProperty(ref MessageValue, value); }

        }
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
    public class TransformationMapping : ModelEntityBase
    {
        private Type SourceTypeValue;

        public Type SourceType

        {

            get { return this.SourceTypeValue; }

            set { SetProperty(ref SourceTypeValue, value); }

        }
        private Type TargetTypeValue;

        public Type TargetType

        {

            get { return this.TargetTypeValue; }

            set { SetProperty(ref TargetTypeValue, value); }

        }
        public Dictionary<string, string> FieldMapping { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, Func<object, object>> CustomTransformations { get; set; } = new Dictionary<string, Func<object, object>>();
        private List<string> FieldsToIncludeValue = new List<string>();

        public List<string> FieldsToInclude

        {

            get { return this.FieldsToIncludeValue; }

            set { SetProperty(ref FieldsToIncludeValue, value); }

        }
        private List<string> FieldsToExcludeValue = new List<string>();

        public List<string> FieldsToExclude

        {

            get { return this.FieldsToExcludeValue; }

            set { SetProperty(ref FieldsToExcludeValue, value); }

        }
    }

    #endregion

    #region Data Quality Dashboard DTOs

    /// <summary>
    /// Quality dashboard data
    /// </summary>
    public class QualityDashboardData : ModelEntityBase
    {
        private string TableNameValue;

        public string TableName

        {

            get { return this.TableNameValue; }

            set { SetProperty(ref TableNameValue, value); }

        }
        private DateTime LastUpdatedValue;

        public DateTime LastUpdated

        {

            get { return this.LastUpdatedValue; }

            set { SetProperty(ref LastUpdatedValue, value); }

        }
        private double OverallQualityScoreValue;

        public double OverallQualityScore

        {

            get { return this.OverallQualityScoreValue; }

            set { SetProperty(ref OverallQualityScoreValue, value); }

        }
        private DataQualityMetrics CurrentMetricsValue;

        public DataQualityMetrics CurrentMetrics

        {

            get { return this.CurrentMetricsValue; }

            set { SetProperty(ref CurrentMetricsValue, value); }

        }
        private List<QualityTrend> RecentTrendsValue = new List<QualityTrend>();

        public List<QualityTrend> RecentTrends

        {

            get { return this.RecentTrendsValue; }

            set { SetProperty(ref RecentTrendsValue, value); }

        }
        private List<QualityAlert> ActiveAlertsValue = new List<QualityAlert>();

        public List<QualityAlert> ActiveAlerts

        {

            get { return this.ActiveAlertsValue; }

            set { SetProperty(ref ActiveAlertsValue, value); }

        }
        public Dictionary<string, double> FieldQualityScores { get; set; } = new Dictionary<string, double>();
    }

    /// <summary>
    /// Quality trend over time
    /// </summary>
    public class QualityTrend : ModelEntityBase
    {
        private DateTime DateValue;

        public DateTime Date

        {

            get { return this.DateValue; }

            set { SetProperty(ref DateValue, value); }

        }
        private double QualityScoreValue;

        public double QualityScore

        {

            get { return this.QualityScoreValue; }

            set { SetProperty(ref QualityScoreValue, value); }

        }
        private int RecordCountValue;

        public int RecordCount

        {

            get { return this.RecordCountValue; }

            set { SetProperty(ref RecordCountValue, value); }

        }
        private int IssueCountValue;

        public int IssueCount

        {

            get { return this.IssueCountValue; }

            set { SetProperty(ref IssueCountValue, value); }

        }
    }

    /// <summary>
    /// Quality alert
    /// </summary>
    public class QualityAlert : ModelEntityBase
    {
        private string AlertIdValue;

        public string AlertId

        {

            get { return this.AlertIdValue; }

            set { SetProperty(ref AlertIdValue, value); }

        }
        private string TableNameValue;

        public string TableName

        {

            get { return this.TableNameValue; }

            set { SetProperty(ref TableNameValue, value); }

        }
        private string FieldNameValue;

        public string FieldName

        {

            get { return this.FieldNameValue; }

            set { SetProperty(ref FieldNameValue, value); }

        }
        private QualityAlertSeverity SeverityValue;

        public QualityAlertSeverity Severity

        {

            get { return this.SeverityValue; }

            set { SetProperty(ref SeverityValue, value); }

        }
        private string AlertMessageValue;

        public string AlertMessage

        {

            get { return this.AlertMessageValue; }

            set { SetProperty(ref AlertMessageValue, value); }

        }
        private DateTime CreatedDateValue;

        public DateTime CreatedDate

        {

            get { return this.CreatedDateValue; }

            set { SetProperty(ref CreatedDateValue, value); }

        }
        private bool IsResolvedValue;

        public bool IsResolved

        {

            get { return this.IsResolvedValue; }

            set { SetProperty(ref IsResolvedValue, value); }

        }
        private DateTime? ResolvedDateValue;

        public DateTime? ResolvedDate

        {

            get { return this.ResolvedDateValue; }

            set { SetProperty(ref ResolvedDateValue, value); }

        }
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








