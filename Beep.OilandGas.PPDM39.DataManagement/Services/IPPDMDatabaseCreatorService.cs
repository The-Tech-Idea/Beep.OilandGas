using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.DataManagement.Core.Models.DatabaseCreation;

namespace Beep.OilandGas.PPDM39.DataManagement.Services
{
    /// <summary>
    /// Interface for database creation service
    /// </summary>
    public interface IPPDMDatabaseCreatorService
    {
        /// <summary>
        /// Creates database by executing all scripts
        /// </summary>
        Task<DatabaseCreationResult> CreateDatabaseAsync(
            DatabaseCreationOptions options,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes scripts by category and script type
        /// </summary>
        Task<ScriptExecutionResult> ExecuteScriptsByCategoryAsync(
            string databaseType,
            string category,
            ScriptType scriptType,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Discovers all scripts for a database type
        /// </summary>
        Task<List<ScriptInfo>> DiscoverScriptsAsync(string databaseType, string scriptsBasePath);

        /// <summary>
        /// Gets execution progress
        /// </summary>
        Task<ScriptExecutionProgress> GetProgressAsync(string executionId);
    }
}









