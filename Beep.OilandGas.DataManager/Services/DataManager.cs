using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using Beep.OilandGas.DataManager.Core.Interfaces;
using Beep.OilandGas.DataManager.Core.Models;
using Beep.OilandGas.DataManager.Core.State;
using Beep.OilandGas.DataManager.Core.Exceptions;
using Beep.OilandGas.PPDM39.DataManagement.Core.Models.DatabaseCreation;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;

namespace Beep.OilandGas.DataManager.Services
{
    /// <summary>
    /// Implementation of IDataManager for executing database scripts
    /// </summary>
    public class DataManager : IDataManager
    {
        private readonly ILogger<DataManager>? _logger;
        private readonly ScriptValidator _scriptValidator;
        private readonly DataManagerLogger? _dataManagerLogger;

        public DataManager(
            ILogger<DataManager>? logger = null,
            ScriptValidator? scriptValidator = null,
            DataManagerLogger? dataManagerLogger = null)
        {
            _logger = logger;
            _scriptValidator = scriptValidator ?? new ScriptValidator(logger);
            _dataManagerLogger = dataManagerLogger;
        }

        public async Task<ModuleExecutionResult> ExecuteModuleAsync(
            IModuleData moduleData,
            IDataSource dataSource,
            ScriptExecutionOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            options ??= new ScriptExecutionOptions();
            var result = new ModuleExecutionResult
            {
                ModuleName = moduleData.ModuleName,
                StartTime = DateTime.UtcNow,
                IsCompleted = false
            };

            try
            {
                // Generate execution ID if not provided
                result.ExecutionId = options.ExecutionId ?? Guid.NewGuid().ToString();

                _logger?.LogInformation("Starting execution of module: {ModuleName} (ExecutionId: {ExecutionId})", 
                    moduleData.ModuleName, result.ExecutionId);
                _dataManagerLogger?.LogInfo($"Starting execution of module: {moduleData.ModuleName}");

                // Pre-execution validation
                if (options.ValidateBeforeExecution)
                {
                    _logger?.LogInformation("Validating scripts before execution...");
                    var validation = await ValidateScriptsAsync(moduleData, GetDatabaseType(dataSource), dataSource);
                    if (!validation.IsValid && !options.ContinueOnError)
                    {
                        result.Success = false;
                        result.ErrorMessage = $"Validation failed: {string.Join("; ", validation.Errors.Select(e => e.Message))}";
                        result.EndTime = DateTime.UtcNow;
                        return result;
                    }
                }

                // Get scripts
                var scripts = await moduleData.GetScriptsAsync(GetDatabaseType(dataSource));
                result.TotalScripts = scripts.Count();

                // Filter scripts based on options
                scripts = FilterScripts(scripts, options);

                // Create execution state
                var executionState = new ExecutionState
                {
                    ExecutionId = result.ExecutionId,
                    ModuleName = moduleData.ModuleName,
                    DatabaseType = GetDatabaseType(dataSource),
                    ConnectionName = dataSource.DatasourceName ?? "Unknown",
                    StartTime = result.StartTime,
                    PendingScripts = scripts.Select(s => s.FileName).ToList()
                };

                // Ensure connection is open
                EnsureConnectionOpen(dataSource);

                // Execute scripts
                var scriptResults = await ExecuteScriptsAsync(
                    scripts,
                    dataSource,
                    executionState,
                    options,
                    cancellationToken);

                result.ScriptResults = scriptResults;
                result.SuccessfulScripts = scriptResults.Count(r => r.Success);
                result.FailedScripts = scriptResults.Count(r => !r.Success);
                result.SkippedScripts = result.TotalScripts - scriptResults.Count;

                // Update execution state
                executionState.CompletedScripts = scriptResults.Where(r => r.Success).Select(r => r.ScriptFileName).ToList();
                executionState.FailedScripts = scriptResults.Where(r => !r.Success).Select(r => r.ScriptFileName).ToList();
                executionState.PendingScripts = executionState.PendingScripts
                    .Except(executionState.CompletedScripts)
                    .Except(executionState.FailedScripts)
                    .ToList();
                executionState.IsCompleted = !cancellationToken.IsCancellationRequested && result.FailedScripts == 0;
                executionState.LastCheckpoint = DateTime.UtcNow;

                // Save checkpoint if enabled
                if (options.EnableCheckpointing && options.StateStore != null)
                {
                    await options.StateStore.SaveStateAsync(executionState);
                }

                result.Checkpoint = executionState;
                result.IsCompleted = executionState.IsCompleted;
                result.Success = result.FailedScripts == 0;

                // Post-execution error checking
                if (options.CheckErrorsAfterExecution && result.Success)
                {
                    _logger?.LogInformation("Checking for errors after execution...");
                    var errorCheck = await CheckForErrorsAsync(result.ExecutionId!, dataSource);
                    if (errorCheck.HasErrors)
                    {
                        result.Success = false;
                        result.ErrorMessage = $"Errors detected: {errorCheck.ScriptErrors.Count} script errors, {errorCheck.ObjectErrors.Count} object errors";
                    }
                }

                result.EndTime = DateTime.UtcNow;
                _logger?.LogInformation("Completed execution of module: {ModuleName} - Success: {Success}, Duration: {Duration}s", 
                    moduleData.ModuleName, result.Success, result.Duration.TotalSeconds);
                _dataManagerLogger?.LogInfo($"Completed execution of module: {moduleData.ModuleName} - Success: {result.Success}");

                return result;
            }
            catch (OperationCanceledException)
            {
                result.Success = false;
                result.IsCompleted = false;
                result.ErrorMessage = "Execution was cancelled";
                result.EndTime = DateTime.UtcNow;
                _logger?.LogWarning("Execution cancelled for module: {ModuleName}", moduleData.ModuleName);
                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.Exception = ex;
                result.EndTime = DateTime.UtcNow;
                _logger?.LogError(ex, "Error executing module: {ModuleName}", moduleData.ModuleName);
                _dataManagerLogger?.LogError($"Error executing module: {moduleData.ModuleName}", ex);
                return result;
            }
        }

        public async Task<Dictionary<string, ModuleExecutionResult>> ExecuteModulesAsync(
            IEnumerable<IModuleData> modules,
            IDataSource dataSource,
            ScriptExecutionOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var results = new Dictionary<string, ModuleExecutionResult>();
            options ??= new ScriptExecutionOptions();

            // Sort modules by execution order and resolve dependencies
            var sortedModules = SortModulesByDependencies(modules);

            foreach (var module in sortedModules)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var result = await ExecuteModuleAsync(module, dataSource, options, cancellationToken);
                results[module.ModuleName] = result;

                // Stop on error if ContinueOnError is false
                if (!result.Success && !options.ContinueOnError)
                {
                    break;
                }
            }

            return results;
        }

        public async Task<ScriptExecutionResult> ExecuteScriptAsync(
            ModuleScriptInfo scriptInfo,
            IDataSource dataSource,
            ScriptExecutionOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var result = new ScriptExecutionResult
            {
                ScriptFileName = scriptInfo.FileName,
                ScriptName = scriptInfo.FileName,
                StartTime = DateTime.UtcNow
            };

            try
            {
                _dataManagerLogger?.LogScriptStart(scriptInfo.FileName);

                if (!File.Exists(scriptInfo.FullPath))
                {
                    throw new FileNotFoundException($"Script file not found: {scriptInfo.FullPath}");
                }

                var scriptContent = await File.ReadAllTextAsync(scriptInfo.FullPath, cancellationToken);

                if (string.IsNullOrWhiteSpace(scriptContent))
                {
                    _dataManagerLogger?.LogWarning($"Script file is empty: {scriptInfo.FileName}");
                    result.Success = true;
                    result.EndTime = DateTime.UtcNow;
                    return result;
                }

                // Execute script
                var executionResult = await ExecuteScriptContentAsync(scriptContent, scriptInfo, dataSource, cancellationToken);

                result.Success = true;
                result.RowsAffected = executionResult.RowsAffected;
                result.ExecutionLog = executionResult.Log;
                result.Message = "Script executed successfully";
                result.EndTime = DateTime.UtcNow;

                _dataManagerLogger?.LogScriptComplete(scriptInfo.FileName, result.Duration, result.RowsAffected);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.Exception = ex;
                result.Message = $"Script execution failed: {ex.Message}";
                result.EndTime = DateTime.UtcNow;

                _dataManagerLogger?.LogScriptFailure(scriptInfo.FileName, ex.Message, ex);
                _logger?.LogError(ex, $"Error executing script: {scriptInfo.FileName}");
            }

            return result;
        }

        public async Task<ValidationResult> ValidateModuleAsync(
            IModuleData moduleData,
            string databaseType)
        {
            return await _scriptValidator.ValidateScriptsAsync(moduleData, databaseType);
        }

        public async Task<ModuleExecutionResult> ResumeModuleExecutionAsync(
            string executionId,
            IDataSource dataSource,
            ScriptExecutionOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            options ??= new ScriptExecutionOptions();
            options.ExecutionId = executionId;

            var state = await GetExecutionStateAsync(executionId);
            if (state == null)
            {
                throw new DataManagerException($"Execution state not found: {executionId}");
            }

            // Create a module data implementation that uses the saved state
            // For now, we'll need to reconstruct the module data
            // This is a simplified approach - in practice, you'd want to store module info in state
            throw new NotImplementedException("Resume functionality requires module data reconstruction. This will be implemented based on specific module implementations.");
        }

        public async Task<ExecutionState?> GetExecutionStateAsync(string executionId)
        {
            // This requires a state store to be configured
            // For now, return null - actual implementation will use IExecutionStateStore
            return await Task.FromResult<ExecutionState?>(null);
        }

        public async Task<ValidationResult> ValidateScriptsAsync(
            IModuleData moduleData,
            string databaseType,
            IDataSource? dataSource = null)
        {
            return await _scriptValidator.ValidateScriptsAsync(moduleData, databaseType, dataSource);
        }

        public async Task<ErrorCheckResult> CheckForErrorsAsync(
            string executionId,
            IDataSource dataSource)
        {
            var state = await GetExecutionStateAsync(executionId);
            if (state == null)
            {
                throw new DataManagerException($"Execution state not found: {executionId}");
            }

            return await _scriptValidator.CheckForErrorsAsync(state, dataSource);
        }

        private async Task<List<ScriptExecutionResult>> ExecuteScriptsAsync(
            IEnumerable<ModuleScriptInfo> scripts,
            IDataSource dataSource,
            ExecutionState executionState,
            ScriptExecutionOptions options,
            CancellationToken cancellationToken)
        {
            var results = new List<ScriptExecutionResult>();
            var orderedScripts = scripts.OrderBy(s => s.ExecutionOrder).ToList();

            foreach (var script in orderedScripts)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                // Skip if already completed
                if (executionState.CompletedScripts.Contains(script.FileName))
                {
                    continue;
                }

                var scriptResult = await ExecuteScriptAsync(script, dataSource, options, cancellationToken);
                results.Add(scriptResult);

                // Update execution state
                if (scriptResult.Success)
                {
                    executionState.CompletedScripts.Add(script.FileName);
                    executionState.ScriptResults[script.FileName] = scriptResult;
                }
                else
                {
                    executionState.FailedScripts.Add(script.FileName);
                    executionState.ScriptResults[script.FileName] = scriptResult;

                    if (!options.ContinueOnError)
                    {
                        break;
                    }
                }

                // Save checkpoint periodically
                if (options.EnableCheckpointing && options.StateStore != null)
                {
                    executionState.LastCheckpoint = DateTime.UtcNow;
                    await options.StateStore.SaveStateAsync(executionState);
                }
            }

            return results;
        }

        private async Task<(int? RowsAffected, string? Log)> ExecuteScriptContentAsync(
            string scriptContent,
            ModuleScriptInfo scriptInfo,
            IDataSource dataSource,
            CancellationToken cancellationToken)
        {
            try
            {
                // Ensure connection is open
                if (dataSource.ConnectionStatus != ConnectionState.Open)
                {
                    var connectionState = dataSource.Openconnection();
                    if (connectionState != ConnectionState.Open)
                    {
                        throw new InvalidOperationException($"Failed to open connection. Status: {connectionState}");
                    }
                }

                // Split script into batches
                var batches = SplitScriptIntoBatches(scriptContent, GetDatabaseType(dataSource));

                int totalBatchesExecuted = 0;
                int totalBatchesFailed = 0;
                var logBuilder = new StringBuilder();

                foreach (var batch in batches)
                {
                    if (string.IsNullOrWhiteSpace(batch)) continue;
                    if (cancellationToken.IsCancellationRequested) break;

                    try
                    {
                        var errorInfo = dataSource.ExecuteSql(batch);

                        if (errorInfo != null && errorInfo.Flag != Errors.Ok)
                        {
                            var errorMessage = !string.IsNullOrEmpty(errorInfo.Message)
                                ? errorInfo.Message
                                : "Unknown error";

                            logBuilder.AppendLine($"Batch failed: {errorMessage}");
                            totalBatchesFailed++;

                            if (!string.IsNullOrEmpty(errorInfo.Message))
                            {
                                throw new ScriptExecutionException(scriptInfo.FileName, errorInfo.Message);
                            }
                        }
                        else
                        {
                            totalBatchesExecuted++;
                        }
                    }
                    catch (Exception ex)
                    {
                        totalBatchesFailed++;
                        logBuilder.AppendLine($"Batch exception: {ex.Message}");
                        throw new ScriptExecutionException(scriptInfo.FileName, ex.Message, ex);
                    }
                }

                var log = logBuilder.ToString();
                if (string.IsNullOrEmpty(log))
                {
                    log = $"Executed {totalBatchesExecuted} batch(es) successfully";
                }

                return (totalBatchesExecuted, log);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error executing script content: {FileName}", scriptInfo.FileName);
                throw;
            }
        }

        private List<string> SplitScriptIntoBatches(string scriptContent, string databaseType)
        {
            var batches = new List<string>();

            if (databaseType.Equals("sqlserver", StringComparison.OrdinalIgnoreCase))
            {
                // Split by GO statements (case-insensitive, can be on its own line or with other text)
                var goPattern = new Regex(@"^\s*GO\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                var parts = goPattern.Split(scriptContent);
                batches.AddRange(parts.Where(p => !string.IsNullOrWhiteSpace(p)));
            }
            else
            {
                // For other databases, split by semicolons (but be careful with string literals)
                // Simple approach: split by semicolon followed by newline
                var parts = scriptContent.Split(new[] { ";\n", ";\r\n", ";\r" }, StringSplitOptions.RemoveEmptyEntries);
                batches.AddRange(parts.Where(p => !string.IsNullOrWhiteSpace(p)));
            }

            // If no batches found, treat entire script as one batch
            if (batches.Count == 0)
            {
                batches.Add(scriptContent);
            }

            return batches;
        }

        private void EnsureConnectionOpen(IDataSource dataSource)
        {
            if (dataSource.ConnectionStatus != ConnectionState.Open)
            {
                var connectionState = dataSource.Openconnection();
                if (connectionState != ConnectionState.Open)
                {
                    throw new InvalidOperationException($"Failed to open connection. Status: {connectionState}");
                }
            }
        }

        private string GetDatabaseType(IDataSource dataSource)
        {
            return dataSource.DatasourceType.ToString() ?? "sqlserver";
        }

        private IEnumerable<ModuleScriptInfo> FilterScripts(
            IEnumerable<ModuleScriptInfo> scripts,
            ScriptExecutionOptions options)
        {
            var filtered = scripts.AsEnumerable();

            // Filter by script type
            if (options.IncludedScriptTypes != null && options.IncludedScriptTypes.Any())
            {
                filtered = filtered.Where(s => options.IncludedScriptTypes.Contains(s.ScriptType));
            }

            if (options.ExcludedScriptTypes != null && options.ExcludedScriptTypes.Any())
            {
                filtered = filtered.Where(s => !options.ExcludedScriptTypes.Contains(s.ScriptType));
            }

            // Filter optional scripts
            if (!options.ExecuteOptionalScripts)
            {
                filtered = filtered.Where(s => !s.IsOptional);
            }

            return filtered.ToList();
        }

        private List<IModuleData> SortModulesByDependencies(IEnumerable<IModuleData> modules)
        {
            var moduleDict = modules.ToDictionary(m => m.ModuleName);
            var sorted = new List<IModuleData>();
            var visited = new HashSet<string>();
            var visiting = new HashSet<string>();

            void Visit(IModuleData module)
            {
                if (visiting.Contains(module.ModuleName))
                {
                    throw new DataManagerException($"Circular dependency detected involving module: {module.ModuleName}");
                }

                if (visited.Contains(module.ModuleName))
                {
                    return;
                }

                visiting.Add(module.ModuleName);

                foreach (var depName in module.GetDependencies())
                {
                    if (moduleDict.TryGetValue(depName, out var depModule))
                    {
                        Visit(depModule);
                    }
                }

                visiting.Remove(module.ModuleName);
                visited.Add(module.ModuleName);
                sorted.Add(module);
            }

            foreach (var module in modules.OrderBy(m => m.ExecutionOrder))
            {
                if (!visited.Contains(module.ModuleName))
                {
                    Visit(module);
                }
            }

            return sorted;
        }

        public Task<ModuleExecutionResult> ExecuteModuleAsync(IModuleData moduleData, IDataSource dataSource, ScriptExecutionOptions? options = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<string, ModuleExecutionResult>> ExecuteModulesAsync(IEnumerable<IModuleData> modules, IDataSource dataSource, ScriptExecutionOptions? options = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<ScriptExecutionResult> ExecuteScriptAsync(ModuleScriptInfo scriptInfo, IDataSource dataSource, ScriptExecutionOptions? options = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<ModuleExecutionResult> ResumeModuleExecutionAsync(string executionId, IDataSource dataSource, ScriptExecutionOptions? options = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<ValidationResult> ValidateScriptsAsync(IModuleData moduleData, string databaseType, IDataSource? dataSource = null)
        {
            throw new NotImplementedException();
        }

        public Task<ErrorCheckResult> CheckForErrorsAsync(string executionId, IDataSource dataSource)
        {
            throw new NotImplementedException();
        }
    }
}
