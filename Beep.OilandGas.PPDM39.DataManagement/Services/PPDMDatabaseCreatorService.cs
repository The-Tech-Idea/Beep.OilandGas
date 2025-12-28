using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.DataManagement.Core.Models.DatabaseCreation;
using TheTechIdea.Beep.Editor;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.PPDM39.DataManagement.Services
{
    /// <summary>
    /// Main service for creating PPDM39 databases by executing scripts
    /// </summary>
    public class PPDMDatabaseCreatorService : IPPDMDatabaseCreatorService
    {
        private readonly IDMEEditor _editor;
        private readonly ILogger<PPDMDatabaseCreatorService>? _logger;
        private readonly PPDMScriptDiscoveryService _discoveryService;
        private readonly PPDMScriptCategorizer _categorizer;
        private readonly PPDMScriptExecutionOrderManager _orderManager;
        private readonly Dictionary<string, ScriptExecutionProgress> _progressStore = new Dictionary<string, ScriptExecutionProgress>();
        private readonly object _progressLock = new object();

        public PPDMDatabaseCreatorService(
            IDMEEditor editor,
            ILogger<PPDMDatabaseCreatorService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _logger = logger;
            _discoveryService = new PPDMScriptDiscoveryService(null);
            _categorizer = new PPDMScriptCategorizer(null);
            _orderManager = new PPDMScriptExecutionOrderManager(null);
        }

        /// <summary>
        /// Creates database by executing all scripts
        /// </summary>
        public async Task<DatabaseCreationResult> CreateDatabaseAsync(
            DatabaseCreationOptions options,
            CancellationToken cancellationToken = default)
        {
            var executionId = options.ExecutionId ?? Guid.NewGuid().ToString();
            var result = new DatabaseCreationResult
            {
                ExecutionId = executionId,
                StartTime = DateTime.UtcNow,
                LogFilePath = options.LogFilePath
            };

            var scriptLogger = new PPDMDatabaseScriptLogger(_logger, options.LogFilePath);

            try
            {
                scriptLogger.LogInfo($"Starting database creation for {options.DatabaseType}");
                scriptLogger.LogInfo($"Database: {options.DatabaseName}");
                scriptLogger.LogInfo($"Scripts Path: {options.ScriptsPath}");

                // Discover scripts
                var allScripts = await DiscoverScriptsAsync(options.DatabaseType.ToString(), options.ScriptsPath);
                scriptLogger.LogInfo($"Discovered {allScripts.Count} scripts");

                // Filter scripts based on options
                var scriptsToExecute = FilterScripts(allScripts, options);

                // Categorize scripts
                var categories = _categorizer.CategorizeScripts(scriptsToExecute);
                scriptLogger.LogInfo($"Categorized into {categories.Count} categories");

                // Order scripts for execution
                var orderedScripts = _orderManager.OrderScriptsForExecution(scriptsToExecute);

                // Validate dependencies if requested
                if (options.ValidateDependencies)
                {
                    var isValid = _orderManager.ValidateDependencies(orderedScripts, out var errors);
                    if (!isValid)
                    {
                        foreach (var error in errors)
                        {
                            scriptLogger.LogWarning(error);
                        }
                        if (!options.ContinueOnError)
                        {
                            throw new InvalidOperationException($"Dependency validation failed: {string.Join("; ", errors)}");
                        }
                    }
                }

                // Initialize progress tracking
                var progress = new ScriptExecutionProgress
                {
                    ExecutionId = executionId,
                    TotalScripts = orderedScripts.Count,
                    StartTime = DateTime.UtcNow,
                    Status = "In Progress"
                };
                StoreProgress(executionId, progress);

                // Create execution engine
                // Note: Connection name should be the configured connection name, not connection string
                // For now, using a default connection name - this should be configurable
                var connectionName = options.DatabaseName ?? "PPDM39";
                var executionEngine = new PPDMScriptExecutionEngine(
                    _editor,
                    connectionName,
                    scriptLogger,
                    null);

                // Execute scripts
                int completed = 0;
                int failed = 0;
                int skipped = 0;

                foreach (var script in orderedScripts)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        progress.Status = "Cancelled";
                        scriptLogger.LogWarning("Database creation cancelled by user");
                        break;
                    }

                    progress.CurrentScript = script.FileName;
                    progress.CompletedScripts = completed;
                    progress.FailedScripts = failed;
                    progress.SkippedScripts = skipped;

                    scriptLogger.LogProgress(completed + failed + skipped + 1, orderedScripts.Count, script.FileName);

                    try
                    {
                        var scriptResult = await executionEngine.ExecuteScriptAsync(script, cancellationToken);
                        result.ScriptResults.Add(scriptResult);

                        if (scriptResult.Success)
                        {
                            completed++;
                        }
                        else
                        {
                            failed++;
                            if (!options.ContinueOnError)
                            {
                                throw new InvalidOperationException($"Script execution failed: {script.FileName} - {scriptResult.ErrorMessage}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        result.ScriptResults.Add(new ScriptExecutionResult
                        {
                            ScriptFileName = script.FileName,
                            Success = false,
                            ErrorMessage = ex.Message,
                            Exception = ex,
                            StartTime = DateTime.UtcNow,
                            EndTime = DateTime.UtcNow
                        });

                        if (!options.ContinueOnError)
                        {
                            throw;
                        }
                    }
                }

                // Finalize results
                result.EndTime = DateTime.UtcNow;
                result.TotalScripts = orderedScripts.Count;
                result.SuccessfulScripts = completed;
                result.FailedScripts = failed;
                result.SkippedScripts = skipped;
                result.Success = failed == 0;

                progress.Status = result.Success ? "Completed" : "Failed";
                progress.CompletedScripts = completed;
                progress.FailedScripts = failed;
                progress.SkippedScripts = skipped;

                scriptLogger.LogInfo($"Database creation completed: {completed} successful, {failed} failed, {skipped} skipped");
                scriptLogger.LogInfo($"Total duration: {result.TotalDuration.TotalMinutes:F2} minutes");

                result.Summary = new Dictionary<string, object>
                {
                    ["TotalDuration"] = result.TotalDuration.TotalMinutes,
                    ["ScriptsPerMinute"] = result.TotalScripts / Math.Max(result.TotalDuration.TotalMinutes, 0.1),
                    ["SuccessRate"] = result.TotalScripts > 0 ? (decimal)completed * 100 / result.TotalScripts : 0
                };
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.EndTime = DateTime.UtcNow;
                scriptLogger.LogError($"Database creation failed: {ex.Message}", ex);
                _logger?.LogError(ex, "Error creating database");
            }
            finally
            {
                scriptLogger.Dispose();
            }

            return result;
        }

        /// <summary>
        /// Executes scripts by category and script type
        /// </summary>
        public async Task<ScriptExecutionResult> ExecuteScriptsByCategoryAsync(
            string databaseType,
            string category,
            ScriptType scriptType,
            CancellationToken cancellationToken = default)
        {
            // This is a simplified version - in full implementation, would filter and execute specific category
            throw new NotImplementedException("ExecuteScriptsByCategoryAsync will be implemented in next iteration");
        }

        /// <summary>
        /// Discovers all scripts for a database type
        /// </summary>
        public async Task<List<ScriptInfo>> DiscoverScriptsAsync(string databaseType, string scriptsBasePath)
        {
            return await _discoveryService.DiscoverScriptsAsync(databaseType, scriptsBasePath);
        }

        /// <summary>
        /// Gets execution progress
        /// </summary>
        public async Task<ScriptExecutionProgress> GetProgressAsync(string executionId)
        {
            return await Task.FromResult(GetStoredProgress(executionId));
        }

        /// <summary>
        /// Filters scripts based on options
        /// </summary>
        private List<ScriptInfo> FilterScripts(List<ScriptInfo> scripts, DatabaseCreationOptions options)
        {
            var filtered = scripts.AsEnumerable();

            // Filter by consolidated vs individual
            if (!options.ExecuteConsolidatedScripts)
            {
                filtered = filtered.Where(s => !s.IsConsolidated);
            }

            if (!options.ExecuteIndividualScripts)
            {
                filtered = filtered.Where(s => s.IsConsolidated);
            }

            // Filter optional scripts
            if (!options.ExecuteOptionalScripts)
            {
                filtered = filtered.Where(s => !s.IsOptional);
            }

            // Filter by categories
            if (options.Categories != null && options.Categories.Any())
            {
                filtered = filtered.Where(s =>
                    (s.Module != null && options.Categories.Contains(s.Module, StringComparer.OrdinalIgnoreCase)) ||
                    (s.SubjectArea != null && options.Categories.Contains(s.SubjectArea, StringComparer.OrdinalIgnoreCase)));
            }

            // Filter by script types
            if (options.ScriptTypes != null && options.ScriptTypes.Any())
            {
                filtered = filtered.Where(s => options.ScriptTypes.Contains(s.ScriptType));
            }

            return filtered.ToList();
        }

        /// <summary>
        /// Stores progress
        /// </summary>
        private void StoreProgress(string executionId, ScriptExecutionProgress progress)
        {
            lock (_progressLock)
            {
                _progressStore[executionId] = progress;
            }
        }

        /// <summary>
        /// Gets stored progress
        /// </summary>
        private ScriptExecutionProgress GetStoredProgress(string executionId)
        {
            lock (_progressLock)
            {
                return _progressStore.TryGetValue(executionId, out var progress) 
                    ? progress 
                    : new ScriptExecutionProgress { ExecutionId = executionId, Status = "Not Found" };
            }
        }
    }
}

