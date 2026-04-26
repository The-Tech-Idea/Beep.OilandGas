using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.DataManagement;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Manages PPDM SQL script discovery and execution. Responsible for resolving
    /// available scripts for a database type, running individual scripts, and
    /// running a sequenced set of all scripts with progress reporting.
    /// </summary>
    public interface IPPDM39SetupExecutionService
    {
        // ── Script discovery ───────────────────────────────────────────────
        List<ScriptInfo> GetAvailableScripts(string databaseType);
        Task<List<ScriptInfo>> GetAvailableScriptsAsync(string databaseType);
        Task<List<ScriptInfo>> DiscoverScriptsAsync(string databaseType);

        // ── Script execution ───────────────────────────────────────────────
        Task<ScriptExecutionResult> ExecuteScriptAsync(ConnectionConfig config, string scriptName, string? operationId = null);
        Task<AllScriptsExecutionResult> ExecuteAllScriptsAsync(ConnectionConfig config, List<string> scriptNames, string? operationId = null);
        Task<ScriptGenerationResult> GenerateScriptsAsync(string connectionName, string? schemaName);

        // ── Progress tracking ──────────────────────────────────────────────
        Task<OperationProgressResult> GetOperationProgressAsync(string operationId);
        Task<CreationProgressResult> GetCreationProgressAsync(string executionId);
    }
}
