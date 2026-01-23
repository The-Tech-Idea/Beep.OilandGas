using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    /// <summary>
    /// Request for seeding data
    /// </summary>
    public class SeedDataRequest : ModelEntityBase
    {
        private string? ConnectionNameValue;

        public string? ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
        private List<string>? TableNamesValue;

        public List<string>? TableNames

        {

            get { return this.TableNamesValue; }

            set { SetProperty(ref TableNamesValue, value); }

        }
        private bool ValidateOnlyValue;

        public bool ValidateOnly

        {

            get { return this.ValidateOnlyValue; }

            set { SetProperty(ref ValidateOnlyValue, value); }

        }
        private bool SkipExistingValue = true;

        public bool SkipExisting

        {

            get { return this.SkipExistingValue; }

            set { SetProperty(ref SkipExistingValue, value); }

        }
        private string? UserIdValue = "SYSTEM";

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }

    /// <summary>
    /// Request for validating seed data
    /// </summary>
    public class SeedDataValidationRequest : ModelEntityBase
    {
        private string CategoryValue = string.Empty;

        public string Category

        {

            get { return this.CategoryValue; }

            set { SetProperty(ref CategoryValue, value); }

        }
        private string? ConnectionNameValue;

        public string? ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
        private List<string>? TableNamesValue;

        public List<string>? TableNames

        {

            get { return this.TableNamesValue; }

            set { SetProperty(ref TableNamesValue, value); }

        }
    }

    /// <summary>
    /// Response from seed data operation
    /// </summary>
    public class SeedDataResponse : ModelEntityBase
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
        private int TablesSeededValue;

        public int TablesSeeded

        {

            get { return this.TablesSeededValue; }

            set { SetProperty(ref TablesSeededValue, value); }

        }
        private int RecordsInsertedValue;

        public int RecordsInserted

        {

            get { return this.RecordsInsertedValue; }

            set { SetProperty(ref RecordsInsertedValue, value); }

        }
        private int RecordsSkippedValue;

        public int RecordsSkipped

        {

            get { return this.RecordsSkippedValue; }

            set { SetProperty(ref RecordsSkippedValue, value); }

        }
        private List<TableSeedResult> TableResultsValue = new List<TableSeedResult>();

        public List<TableSeedResult> TableResults

        {

            get { return this.TableResultsValue; }

            set { SetProperty(ref TableResultsValue, value); }

        }
        private List<string> ErrorsValue = new List<string>();

        public List<string> Errors

        {

            get { return this.ErrorsValue; }

            set { SetProperty(ref ErrorsValue, value); }

        }
    }

    /// <summary>
    /// Result for a single table seed operation
    /// </summary>
    public class TableSeedResult : ModelEntityBase
    {
        private string TableNameValue = string.Empty;

        public string TableName

        {

            get { return this.TableNameValue; }

            set { SetProperty(ref TableNameValue, value); }

        }
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private int RecordsInsertedValue;

        public int RecordsInserted

        {

            get { return this.RecordsInsertedValue; }

            set { SetProperty(ref RecordsInsertedValue, value); }

        }
        private int RecordsSkippedValue;

        public int RecordsSkipped

        {

            get { return this.RecordsSkippedValue; }

            set { SetProperty(ref RecordsSkippedValue, value); }

        }
        private string? ErrorMessageValue;

        public string? ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }
    }

    /// <summary>
    /// Seed data category information
    /// </summary>
    public class SeedDataCategory : ModelEntityBase
    {
        private string CategoryNameValue = string.Empty;

        public string CategoryName

        {

            get { return this.CategoryNameValue; }

            set { SetProperty(ref CategoryNameValue, value); }

        }
        private string DescriptionValue = string.Empty;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private List<string> TableNamesValue = new List<string>();

        public List<string> TableNames

        {

            get { return this.TableNamesValue; }

            set { SetProperty(ref TableNamesValue, value); }

        }
        private int EstimatedRecordsValue;

        public int EstimatedRecords

        {

            get { return this.EstimatedRecordsValue; }

            set { SetProperty(ref EstimatedRecordsValue, value); }

        }
    }

    /// <summary>
    /// Workflow seed data requirement
    /// </summary>
    public class WorkflowSeedDataRequirement : ModelEntityBase
    {
        private string WorkflowNameValue = string.Empty;

        public string WorkflowName

        {

            get { return this.WorkflowNameValue; }

            set { SetProperty(ref WorkflowNameValue, value); }

        }
        private string WorkflowCategoryValue = string.Empty;

        public string WorkflowCategory

        {

            get { return this.WorkflowCategoryValue; }

            set { SetProperty(ref WorkflowCategoryValue, value); }

        }
        private List<string> RequiredTablesValue = new List<string>();

        public List<string> RequiredTables

        {

            get { return this.RequiredTablesValue; }

            set { SetProperty(ref RequiredTablesValue, value); }

        }
        private string DescriptionValue = string.Empty;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
    }

    /// <summary>
    /// Request for generating SQL scripts
    /// </summary>
    public class ScriptGenerationRequest : ModelEntityBase
    {
        private List<string>? EntityNamesValue;

        public List<string>? EntityNames

        {

            get { return this.EntityNamesValue; }

            set { SetProperty(ref EntityNamesValue, value); }

        }
        private List<string>? DatabaseTypesValue;

        public List<string>? DatabaseTypes

        {

            get { return this.DatabaseTypesValue; }

            set { SetProperty(ref DatabaseTypesValue, value); }

        }
        private string? OutputPathValue;

        public string? OutputPath

        {

            get { return this.OutputPathValue; }

            set { SetProperty(ref OutputPathValue, value); }

        }
        private List<string>? ScriptTypesValue;

        public List<string>? ScriptTypes

        {

            get { return this.ScriptTypesValue; }

            set { SetProperty(ref ScriptTypesValue, value); }

        } // TAB, PK, FK, IX, etc.
        private bool SaveToFileValue = true;

        public bool SaveToFile

        {

            get { return this.SaveToFileValue; }

            set { SetProperty(ref SaveToFileValue, value); }

        }
    }

    /// <summary>
    /// Response from script generation operation
    /// </summary>
    public class ScriptGenerationResponse : ModelEntityBase
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
        private int ScriptsGeneratedValue;

        public int ScriptsGenerated

        {

            get { return this.ScriptsGeneratedValue; }

            set { SetProperty(ref ScriptsGeneratedValue, value); }

        }
        private int EntitiesProcessedValue;

        public int EntitiesProcessed

        {

            get { return this.EntitiesProcessedValue; }

            set { SetProperty(ref EntitiesProcessedValue, value); }

        }
        private int ErrorsValue;

        public int Errors

        {

            get { return this.ErrorsValue; }

            set { SetProperty(ref ErrorsValue, value); }

        }
        private string? OutputPathValue;

        public string? OutputPath

        {

            get { return this.OutputPathValue; }

            set { SetProperty(ref OutputPathValue, value); }

        }
        private List<EntityScriptResult> EntityResultsValue = new List<EntityScriptResult>();

        public List<EntityScriptResult> EntityResults

        {

            get { return this.EntityResultsValue; }

            set { SetProperty(ref EntityResultsValue, value); }

        }
    }

    /// <summary>
    /// Result for a single entity script generation
    /// </summary>
    public class EntityScriptResult : ModelEntityBase
    {
        private string EntityNameValue = string.Empty;

        public string EntityName

        {

            get { return this.EntityNameValue; }

            set { SetProperty(ref EntityNameValue, value); }

        }
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private int ScriptsGeneratedValue;

        public int ScriptsGenerated

        {

            get { return this.ScriptsGeneratedValue; }

            set { SetProperty(ref ScriptsGeneratedValue, value); }

        }
        private List<ScriptResult> ScriptsValue = new List<ScriptResult>();

        public List<ScriptResult> Scripts

        {

            get { return this.ScriptsValue; }

            set { SetProperty(ref ScriptsValue, value); }

        }
        private string? ErrorMessageValue;

        public string? ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }
    }

    /// <summary>
    /// Result for a single script
    /// </summary>
    public class ScriptResult : ModelEntityBase
    {
        private string ScriptTypeValue = string.Empty;

        public string ScriptType

        {

            get { return this.ScriptTypeValue; }

            set { SetProperty(ref ScriptTypeValue, value); }

        } // TAB, PK, FK, IX
        private string DatabaseTypeValue = string.Empty;

        public string DatabaseType

        {

            get { return this.DatabaseTypeValue; }

            set { SetProperty(ref DatabaseTypeValue, value); }

        }
        private string? FilePathValue;

        public string? FilePath

        {

            get { return this.FilePathValue; }

            set { SetProperty(ref FilePathValue, value); }

        }
        private int ScriptLengthValue;

        public int ScriptLength

        {

            get { return this.ScriptLengthValue; }

            set { SetProperty(ref ScriptLengthValue, value); }

        }
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private string? ErrorMessageValue;

        public string? ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }
    }

    /// <summary>
    /// Request for LOV operations
    /// </summary>
    public class LOVRequest : ModelEntityBase
    {
        private string? ConnectionNameValue;

        public string? ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
        private string? ValueTypeValue;

        public string? ValueType

        {

            get { return this.ValueTypeValue; }

            set { SetProperty(ref ValueTypeValue, value); }

        }
        private string? CategoryValue;

        public string? Category

        {

            get { return this.CategoryValue; }

            set { SetProperty(ref CategoryValue, value); }

        }
        private string? ModuleValue;

        public string? Module

        {

            get { return this.ModuleValue; }

            set { SetProperty(ref ModuleValue, value); }

        }

    }

    /// <summary>
    /// Response from LOV operations
    /// </summary>
    public class LOVResponse : ModelEntityBase
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
        private List<ListOfValue> LOVsValue = new List<ListOfValue>();

        public List<ListOfValue> LOVs

        {

            get { return this.LOVsValue; }

            set { SetProperty(ref LOVsValue, value); }

        }
        private int CountValue;

        public int Count

        {

            get { return this.CountValue; }

            set { SetProperty(ref CountValue, value); }

        }
    }

    /// <summary>
    /// DTO for List of Value
    /// </summary>
    public class ListOfValue : ModelEntityBase
    {
        private string ListOfValueIdValue = string.Empty;

        public string ListOfValueId

        {

            get { return this.ListOfValueIdValue; }

            set { SetProperty(ref ListOfValueIdValue, value); }

        }
        private string ValueTypeValue = string.Empty;

        public string ValueType

        {

            get { return this.ValueTypeValue; }

            set { SetProperty(ref ValueTypeValue, value); }

        }
        private string ValueCodeValue = string.Empty;

        public string ValueCode

        {

            get { return this.ValueCodeValue; }

            set { SetProperty(ref ValueCodeValue, value); }

        }
        private string ValueNameValue = string.Empty;

        public string ValueName

        {

            get { return this.ValueNameValue; }

            set { SetProperty(ref ValueNameValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private string? CategoryValue;

        public string? Category

        {

            get { return this.CategoryValue; }

            set { SetProperty(ref CategoryValue, value); }

        }
        private string? ModuleValue;

        public string? Module

        {

            get { return this.ModuleValue; }

            set { SetProperty(ref ModuleValue, value); }

        }
        private int? SortOrderValue;

        public int? SortOrder

        {

            get { return this.SortOrderValue; }

            set { SetProperty(ref SortOrderValue, value); }

        }
        private string? ParentValueIdValue;

        public string? ParentValueId

        {

            get { return this.ParentValueIdValue; }

            set { SetProperty(ref ParentValueIdValue, value); }

        }
        private string? IsDefaultValue;

        public string? IsDefault

        {

            get { return this.IsDefaultValue; }

            set { SetProperty(ref IsDefaultValue, value); }

        }
        private string ActiveIndValue = string.Empty;

        public string ActiveInd

        {

            get { return this.ActiveIndValue; }

            set { SetProperty(ref ActiveIndValue, value); }

        }

        private List<ListOfValue>? ChildrenValue;

        public List<ListOfValue>? Children

        {

            get { return this.ChildrenValue; }

            set { SetProperty(ref ChildrenValue, value); }

        }
    }

    /// <summary>
    /// Response from RA_* tables extraction operation
    /// </summary>
    public class RATablesExtractResponse : ModelEntityBase
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
        private int TotalTablesValue;

        public int TotalTables

        {

            get { return this.TotalTablesValue; }

            set { SetProperty(ref TotalTablesValue, value); }

        }
        private DateTime ExtractionDateValue;

        public DateTime ExtractionDate

        {

            get { return this.ExtractionDateValue; }

            set { SetProperty(ref ExtractionDateValue, value); }

        }
        private List<string> TableNamesValue = new List<string>();

        public List<string> TableNames

        {

            get { return this.TableNamesValue; }

            set { SetProperty(ref TableNamesValue, value); }

        }
        private List<CategoryInfo> CategoriesValue = new List<CategoryInfo>();

        public List<CategoryInfo> Categories

        {

            get { return this.CategoriesValue; }

            set { SetProperty(ref CategoriesValue, value); }

        }
    }

    /// <summary>
    /// Category information for RA_* tables
    /// </summary>
    public class CategoryInfo : ModelEntityBase
    {
        private string CategoryValue = string.Empty;

        public string Category

        {

            get { return this.CategoryValue; }

            set { SetProperty(ref CategoryValue, value); }

        }
        private int CountValue;

        public int Count

        {

            get { return this.CountValue; }

            set { SetProperty(ref CountValue, value); }

        }
        private List<string> TablesValue = new List<string>();

        public List<string> Tables

        {

            get { return this.TablesValue; }

            set { SetProperty(ref TablesValue, value); }

        }
    }

    /// <summary>
    /// Request for exporting RA_* tables to JSON
    /// </summary>
    public class RATablesExportRequest : ModelEntityBase
    {
        private string? OutputPathValue;

        public string? OutputPath

        {

            get { return this.OutputPathValue; }

            set { SetProperty(ref OutputPathValue, value); }

        }
    }
}


