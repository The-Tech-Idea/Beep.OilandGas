using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Core.DTOs
{
    /// <summary>
    /// Request to seed selected IModuleSetup implementations.
    /// </summary>
    public class ModuleSeedingRequest
    {
        /// <summary>
        /// Module IDs to execute. If empty/null, runs all registered modules.
        /// Modules run in their declared Order regardless of list order.
        /// </summary>
        public List<string> ModuleIds { get; set; } = new();

        /// <summary>
        /// Database connection name. If empty, uses the default connection.
        /// </summary>
        public string ConnectionName { get; set; } = "PPDM39";

        /// <summary>
        /// User ID for audit trail. Defaults to "SYSTEM".
        /// </summary>
        public string UserId { get; set; } = "SYSTEM";

        /// <summary>
        /// Optional operation ID for progress tracking.
        /// </summary>
        public string? OperationId { get; set; }

        /// <summary>
        /// If true, execute without waiting for completion (async/fire-and-forget).
        /// Returns operationId for polling progress.
        /// </summary>
        public bool RunAsync { get; set; }
    }

    /// <summary>
    /// Response from selective module seeding operation.
    /// </summary>
    public class ModuleSeedingResponse
    {
        /// <summary>
        /// Overall success — true if all selected modules succeeded.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Human-readable summary message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Total records inserted across all executed modules.
        /// </summary>
        public int TotalRecordsInserted { get; set; }

        /// <summary>
        /// Number of modules that were executed.
        /// </summary>
        public int ModulesRun { get; set; }

        /// <summary>
        /// Number of modules that completed successfully.
        /// </summary>
        public int ModulesSucceeded { get; set; }

        /// <summary>
        /// Per-module execution details (module ID, records, status).
        /// </summary>
        public List<ModuleExecutionDetail> ModuleDetails { get; set; } = new();

        /// <summary>
        /// All error messages across all modules.
        /// </summary>
        public List<string> Errors { get; set; } = new();

        /// <summary>
        /// If RunAsync=true, the operation ID for polling progress.
        /// </summary>
        public string? OperationId { get; set; }
    }

    /// <summary>
    /// Detail for one executed module within ModuleSeedingResponse.
    /// </summary>
    public class ModuleExecutionDetail
    {
        /// <summary>
        /// Module's stable ID.
        /// </summary>
        public string ModuleId { get; set; } = string.Empty;

        /// <summary>
        /// Module's display name.
        /// </summary>
        public string ModuleName { get; set; } = string.Empty;

        /// <summary>
        /// Execution order number.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// True if the module completed successfully.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Records inserted by this module.
        /// </summary>
        public int RecordsInserted { get; set; }

        /// <summary>
        /// Tables seeded by this module.
        /// </summary>
        public int TablesSeeded { get; set; }

        /// <summary>
        /// Non-null if the module was intentionally skipped.
        /// </summary>
        public string? SkipReason { get; set; }

        /// <summary>
        /// Error messages from this module, if any.
        /// </summary>
        public List<string> Errors { get; set; } = new();
    }

    /// <summary>
    /// Response listing available modules for seeding.
    /// </summary>
    public class AvailableModulesResponse
    {
        /// <summary>
        /// Total registered modules.
        /// </summary>
        public int TotalModules { get; set; }

        /// <summary>
        /// Modules in execution order.
        /// </summary>
        public List<ModuleInfo> Modules { get; set; } = new();
    }

    /// <summary>
    /// Info about one available module.
    /// </summary>
    public class ModuleInfo
    {
        /// <summary>
        /// Stable module ID.
        /// </summary>
        public string ModuleId { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable display name.
        /// </summary>
        public string ModuleName { get; set; } = string.Empty;

        /// <summary>
        /// Execution order (lower runs first).
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Entity types owned by this module.
        /// </summary>
        public List<string> EntityTypes { get; set; } = new();
    }
}
