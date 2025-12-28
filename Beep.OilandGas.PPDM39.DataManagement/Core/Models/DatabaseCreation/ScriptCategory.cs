using System.Collections.Generic;

namespace Beep.OilandGas.PPDM39.DataManagement.Core.Models.DatabaseCreation
{
    /// <summary>
    /// Represents a category of scripts (e.g., by module or subject area)
    /// </summary>
    public class ScriptCategory
    {
        public string CategoryName { get; set; } = string.Empty;
        public string? Module { get; set; }
        public string? SubjectArea { get; set; }
        public ScriptType ScriptType { get; set; }
        public List<ScriptInfo> Scripts { get; set; } = new List<ScriptInfo>();
        public int ExecutionOrder { get; set; }
        public bool IsMandatory { get; set; }
    }
}








