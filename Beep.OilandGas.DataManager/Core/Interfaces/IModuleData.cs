using Beep.OilandGas.DataManager.Core.Models;

namespace Beep.OilandGas.DataManager.Core.Interfaces
{
    /// <summary>
    /// Interface for module data providers that specify scripts and related files for a module
    /// </summary>
    public interface IModuleData
    {
        /// <summary>
        /// Module name (e.g., "ProductionAccounting", "EconomicAnalysis")
        /// </summary>
        string ModuleName { get; }
        
        /// <summary>
        /// Module description
        /// </summary>
        string Description { get; }
        
        /// <summary>
        /// Base path to module scripts (relative to Scripts/{DatabaseType}/)
        /// </summary>
        string ScriptBasePath { get; }
        
        /// <summary>
        /// Get all scripts for a specific database type
        /// </summary>
        Task<IEnumerable<ModuleScriptInfo>> GetScriptsAsync(string databaseType);
        
        /// <summary>
        /// Get script dependencies (other modules that must be executed first)
        /// </summary>
        IEnumerable<string> GetDependencies();
        
        /// <summary>
        /// Get execution order (lower numbers execute first)
        /// </summary>
        int ExecutionOrder { get; }
        
        /// <summary>
        /// Whether this module is required for basic functionality
        /// </summary>
        bool IsRequired { get; }
    }
}
