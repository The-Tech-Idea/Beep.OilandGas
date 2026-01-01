using Beep.OilandGas.PPDM39.DataManagement.Core.Models.DatabaseCreation;

namespace Beep.OilandGas.DataManager.Core.Models
{
    /// <summary>
    /// Information about a script file for a module
    /// </summary>
    public class ModuleScriptInfo
    {
        public string FileName { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public string RelativePath { get; set; } = string.Empty;
        public ScriptType ScriptType { get; set; }
        
        /// <summary>
        /// Single table name (for individual table scripts like TABLE_TAB.sql)
        /// </summary>
        public string? TableName { get; set; }
        
        /// <summary>
        /// Multiple table names (for consolidated scripts like TAB.sql, PK.sql, FK.sql)
        /// </summary>
        public List<string> TableNames { get; set; } = new List<string>();
        
        /// <summary>
        /// Whether this script contains multiple tables (consolidated script)
        /// </summary>
        public bool IsConsolidated { get; set; }
        
        public int ExecutionOrder { get; set; }
        public List<string> Dependencies { get; set; } = new List<string>();
        public bool IsRequired { get; set; } = true;
        public bool IsOptional { get; set; } = false;
        public long FileSize { get; set; }
        public DateTime LastModified { get; set; }
    }
}
