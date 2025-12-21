using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.PPDM39.Scripts.DataClasses
{
    /// <summary>
    /// Data class for script execution metadata and tracking
    /// Tracks which scripts have been executed for a connection/organization
    /// Maps to SETUP_WIZARD_LOG table
    /// </summary>
    public class ScriptExecutionSetupData
    {
        [Required]
        public string SetupLogId { get; set; } = string.Empty;

        [Required]
        public string ScriptName { get; set; } = string.Empty;

        [Required]
        public string DatabaseType { get; set; } = string.Empty;

        [Required]
        public int ExecutionOrder { get; set; }

        [Required]
        public bool Required { get; set; } = true;

        public string? OrganizationId { get; set; }

        public string? ConnectionName { get; set; }

        public string? ExecutedBy { get; set; }

        public DateTime? ExecutionDate { get; set; }

        public string Status { get; set; } = "PENDING"; // PENDING, SUCCESS, FAILED

        public string? ErrorMessage { get; set; }

        public TimeSpan? ExecutionTime { get; set; }

        public string? SetupData { get; set; } // JSON or structured setup data
    }

    /// <summary>
    /// Collection of script execution records for tracking setup progress
    /// </summary>
    public class ScriptExecutionSetupCollection
    {
        [Required]
        public string OrganizationId { get; set; } = string.Empty;

        [Required]
        public string ConnectionName { get; set; } = string.Empty;

        public List<ScriptExecutionSetupData> ScriptExecutions { get; set; } = new List<ScriptExecutionSetupData>();

        public DateTime SetupStartDate { get; set; } = DateTime.UtcNow;

        public DateTime? SetupCompletionDate { get; set; }

        public string OverallStatus { get; set; } = "IN_PROGRESS"; // IN_PROGRESS, COMPLETED, FAILED
    }
}
