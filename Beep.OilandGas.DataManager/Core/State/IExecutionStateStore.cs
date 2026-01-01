using Beep.OilandGas.DataManager.Core.Models;

namespace Beep.OilandGas.DataManager.Core.State
{
    /// <summary>
    /// Interface for persisting and retrieving execution state for checkpoint/resume functionality
    /// </summary>
    public interface IExecutionStateStore
    {
        /// <summary>
        /// Save execution state
        /// </summary>
        Task SaveStateAsync(ExecutionState state);

        /// <summary>
        /// Load execution state by execution ID
        /// </summary>
        Task<ExecutionState?> LoadStateAsync(string executionId);

        /// <summary>
        /// Delete execution state
        /// </summary>
        Task DeleteStateAsync(string executionId);

        /// <summary>
        /// Get all incomplete executions
        /// </summary>
        Task<List<ExecutionState>> GetIncompleteExecutionsAsync();
    }
}
