using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    /// <summary>
    /// Request for seeding data
    /// </summary>
    public class SeedDataRequest : ModelEntityBase
    {
        public string? ConnectionName { get; set; }
        public List<string>? TableNames { get; set; }
        public bool ValidateOnly { get; set; }
        public bool SkipExisting { get; set; } = true;
        public string? UserId { get; set; } = "SYSTEM";
    }

    /// <summary>
    /// Request for validating seed data
    /// </summary>
    public class SeedDataValidationRequest : ModelEntityBase
    {
        public string Category { get; set; } = string.Empty;
        public string? ConnectionName { get; set; }
        public List<string>? TableNames { get; set; }
    }

    /// <summary>
    /// Response from seed data operation
    /// </summary>
    public class SeedDataResponse : ModelEntityBase
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TablesSeeded { get; set; }
        public int RecordsInserted { get; set; }
        public int RecordsSkipped { get; set; }
        public List<TableSeedResult> TableResults { get; set; } = new List<TableSeedResult>();
        public List<string> Errors { get; set; } = new List<string>();
    }

    /// <summary>
    /// Result for a single table seed operation
    /// </summary>
    public class TableSeedResult : ModelEntityBase
    {
        public string TableName { get; set; } = string.Empty;
        public bool Success { get; set; }
        public int RecordsInserted { get; set; }
        public int RecordsSkipped { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Seed data category information
    /// </summary>
    public class SeedDataCategory : ModelEntityBase
    {
        public string CategoryName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> TableNames { get; set; } = new List<string>();
        public int EstimatedRecords { get; set; }
    }

    /// <summary>
    /// Workflow seed data requirement
    /// </summary>
    public class WorkflowSeedDataRequirement : ModelEntityBase
    {
        public string WorkflowName { get; set; } = string.Empty;
        public string WorkflowCategory { get; set; } = string.Empty;
        public List<string> RequiredTables { get; set; } = new List<string>();
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request for generating SQL scripts
    /// </summary>
    public class ScriptGenerationRequest : ModelEntityBase
    {
        public List<string>? EntityNames { get; set; }
        public List<string>? DatabaseTypes { get; set; }
        public string? OutputPath { get; set; }
        public List<string>? ScriptTypes { get; set; } // TAB, PK, FK, IX, etc.
        public bool SaveToFile { get; set; } = true;
    }

    /// <summary>
    /// Response from script generation operation
    /// </summary>
    public class ScriptGenerationResponse : ModelEntityBase
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int ScriptsGenerated { get; set; }
        public int EntitiesProcessed { get; set; }
        public int Errors { get; set; }
        public string? OutputPath { get; set; }
        public List<EntityScriptResult> EntityResults { get; set; } = new List<EntityScriptResult>();
    }

    /// <summary>
    /// Result for a single entity script generation
    /// </summary>
    public class EntityScriptResult : ModelEntityBase
    {
        public string EntityName { get; set; } = string.Empty;
        public bool Success { get; set; }
        public int ScriptsGenerated { get; set; }
        public List<ScriptResult> Scripts { get; set; } = new List<ScriptResult>();
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Result for a single script
    /// </summary>
    public class ScriptResult : ModelEntityBase
    {
        public string ScriptType { get; set; } = string.Empty; // TAB, PK, FK, IX
        public string DatabaseType { get; set; } = string.Empty;
        public string? FilePath { get; set; }
        public int ScriptLength { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Request for LOV operations
    /// </summary>
    public class LOVRequest : ModelEntityBase
    {
        public string? ConnectionName { get; set; }
        public string? ValueType { get; set; }
        public string? Category { get; set; }
        public string? Module { get; set; }
        public string? Source { get; set; }
    }

    /// <summary>
    /// Response from LOV operations
    /// </summary>
    public class LOVResponse : ModelEntityBase
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<ListOfValue> LOVs { get; set; } = new List<ListOfValue>();
        public int Count { get; set; }
    }

    /// <summary>
    /// DTO for List of Value
    /// </summary>
    public class ListOfValue : ModelEntityBase
    {
        public string ListOfValueId { get; set; } = string.Empty;
        public string ValueType { get; set; } = string.Empty;
        public string ValueCode { get; set; } = string.Empty;
        public string ValueName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? Module { get; set; }
        public int? SortOrder { get; set; }
        public string? ParentValueId { get; set; }
        public string? IsDefault { get; set; }
        public string ActiveInd { get; set; } = string.Empty;
        public string? Source { get; set; }
        public List<ListOfValue>? Children { get; set; }
    }

    /// <summary>
    /// Response from RA_* tables extraction operation
    /// </summary>
    public class RATablesExtractResponse : ModelEntityBase
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TotalTables { get; set; }
        public DateTime ExtractionDate { get; set; }
        public List<string> TableNames { get; set; } = new List<string>();
        public List<CategoryInfo> Categories { get; set; } = new List<CategoryInfo>();
    }

    /// <summary>
    /// Category information for RA_* tables
    /// </summary>
    public class CategoryInfo : ModelEntityBase
    {
        public string Category { get; set; } = string.Empty;
        public int Count { get; set; }
        public List<string> Tables { get; set; } = new List<string>();
    }

    /// <summary>
    /// Request for exporting RA_* tables to JSON
    /// </summary>
    public class RATablesExportRequest : ModelEntityBase
    {
        public string? OutputPath { get; set; }
    }
}





