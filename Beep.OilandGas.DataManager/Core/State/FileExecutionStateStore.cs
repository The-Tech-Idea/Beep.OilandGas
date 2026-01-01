using System.Text.Json;
using Beep.OilandGas.DataManager.Core.Models;
using Beep.OilandGas.DataManager.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.DataManager.Core.State
{
    /// <summary>
    /// File-based implementation of execution state store
    /// </summary>
    public class FileExecutionStateStore : IExecutionStateStore
    {
        private readonly string _stateDirectory;
        private readonly ILogger<FileExecutionStateStore>? _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public FileExecutionStateStore(string? stateDirectory = null, ILogger<FileExecutionStateStore>? logger = null)
        {
            _stateDirectory = stateDirectory ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Beep.OilandGas", "DataManager", "ExecutionStates");
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            // Ensure directory exists
            if (!Directory.Exists(_stateDirectory))
            {
                Directory.CreateDirectory(_stateDirectory);
            }
        }

        public async Task SaveStateAsync(ExecutionState state)
        {
            try
            {
                var filePath = GetStateFilePath(state.ExecutionId);
                var json = JsonSerializer.Serialize(state, _jsonOptions);
                await File.WriteAllTextAsync(filePath, json);
                _logger?.LogDebug("Saved execution state: {ExecutionId} to {FilePath}", state.ExecutionId, filePath);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to save execution state: {ExecutionId}", state.ExecutionId);
                throw new DataManagerException($"Failed to save execution state: {ex.Message}", ex);
            }
        }

        public async Task<ExecutionState?> LoadStateAsync(string executionId)
        {
            try
            {
                var filePath = GetStateFilePath(executionId);
                if (!File.Exists(filePath))
                {
                    _logger?.LogDebug("Execution state not found: {ExecutionId}", executionId);
                    return null;
                }

                var json = await File.ReadAllTextAsync(filePath);
                var state = JsonSerializer.Deserialize<ExecutionState>(json, _jsonOptions);
                _logger?.LogDebug("Loaded execution state: {ExecutionId}", executionId);
                return state;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to load execution state: {ExecutionId}", executionId);
                throw new DataManagerException($"Failed to load execution state: {ex.Message}", ex);
            }
        }

        public async Task DeleteStateAsync(string executionId)
        {
            try
            {
                var filePath = GetStateFilePath(executionId);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger?.LogDebug("Deleted execution state: {ExecutionId}", executionId);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to delete execution state: {ExecutionId}", executionId);
                throw new DataManagerException($"Failed to delete execution state: {ex.Message}", ex);
            }
        }

        public async Task<List<ExecutionState>> GetIncompleteExecutionsAsync()
        {
            var incompleteStates = new List<ExecutionState>();

            try
            {
                if (!Directory.Exists(_stateDirectory))
                {
                    return incompleteStates;
                }

                var stateFiles = Directory.GetFiles(_stateDirectory, "*.json");
                foreach (var filePath in stateFiles)
                {
                    try
                    {
                        var json = await File.ReadAllTextAsync(filePath);
                        var state = JsonSerializer.Deserialize<ExecutionState>(json, _jsonOptions);
                        if (state != null && !state.IsCompleted)
                        {
                            incompleteStates.Add(state);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Failed to read state file: {FilePath}", filePath);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to get incomplete executions");
                throw new DataManagerException($"Failed to get incomplete executions: {ex.Message}", ex);
            }

            return incompleteStates;
        }

        private string GetStateFilePath(string executionId)
        {
            var fileName = $"{executionId}.json";
            return Path.Combine(_stateDirectory, fileName);
        }
    }
}
