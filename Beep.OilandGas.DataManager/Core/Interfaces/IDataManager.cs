using Beep.OilandGas.DataManager.Core.Models;
using Beep.OilandGas.DataManager.Core.State;
using Beep.OilandGas.PPDM39.DataManagement.Core.Models.DatabaseCreation;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.DataManager.Core.Interfaces
{
    /// <summary>
    /// Interface for managing and executing database scripts for modules
    /// </summary>
    public interface IDataManager
    {
        /// <summary>
        /// Execute all scripts for a specific module
        /// </summary>
        Task<ModuleExecutionResult> ExecuteModuleAsync(
            IModuleData moduleData,
            IDataSource dataSource,
            ScriptExecutionOptions? options = null,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Execute scripts for multiple modules in dependency order
        /// </summary>
        Task<Dictionary<string, ModuleExecutionResult>> ExecuteModulesAsync(
            IEnumerable<IModuleData> modules,
            IDataSource dataSource,
            ScriptExecutionOptions? options = null,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Execute a specific script
        /// </summary>
        Task<ScriptExecutionResult> ExecuteScriptAsync(
            ModuleScriptInfo scriptInfo,
            IDataSource dataSource,
            ScriptExecutionOptions? options = null,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Validate that all required scripts exist for a module
        /// </summary>
        Task<ValidationResult> ValidateModuleAsync(
            IModuleData moduleData,
            string databaseType);
        
        /// <summary>
        /// Resume execution from a previous checkpoint
        /// </summary>
        Task<ModuleExecutionResult> ResumeModuleExecutionAsync(
            string executionId,
            IDataSource dataSource,
            ScriptExecutionOptions? options = null,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Get execution state/checkpoint for resuming
        /// </summary>
        Task<ExecutionState?> GetExecutionStateAsync(string executionId);
        
        /// <summary>
        /// Validate scripts for syntax errors before execution
        /// </summary>
        Task<ValidationResult> ValidateScriptsAsync(
            IModuleData moduleData,
            string databaseType,
            IDataSource? dataSource = null);
        
        /// <summary>
        /// Check for errors in executed scripts (verify objects were created)
        /// </summary>
        Task<ErrorCheckResult> CheckForErrorsAsync(
            string executionId,
            IDataSource dataSource);
    }
}
