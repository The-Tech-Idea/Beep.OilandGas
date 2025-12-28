using System;
using System.Collections.Generic;

namespace Beep.OilandGas.PPDM39.DataManagement.Core.Models.DatabaseCreation
{
    /// <summary>
    /// Information about a database script file
    /// </summary>
    public class ScriptInfo
    {
        public string FileName { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public string RelativePath { get; set; } = string.Empty;
        public ScriptType ScriptType { get; set; }
        public string? TableName { get; set; } // For individual table scripts (e.g., FIELD_PHASE_TAB.sql)
        public string? Module { get; set; } // PPDM module this script belongs to
        public string? SubjectArea { get; set; } // PPDM subject area
        public bool IsConsolidated { get; set; } // True for TAB.sql, PK.sql, etc.
        public bool IsMandatory { get; set; } // True for mandatory scripts
        public bool IsOptional { get; set; } // True for optional scripts (TCM, CCM, SYN, GUID)
        public long FileSize { get; set; }
        public DateTime LastModified { get; set; }
        public int ExecutionOrder { get; set; } // Order in which script should be executed
        public List<string> Dependencies { get; set; } = new List<string>(); // Scripts that must execute before this one
        public string? Category { get; set; } // Custom category for grouping
    }
}








