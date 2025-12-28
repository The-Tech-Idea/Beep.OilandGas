using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.DataManagement.Core.Models.DatabaseCreation;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.ConfigUtil;

namespace Beep.OilandGas.PPDM39.DataManagement.Services
{
    /// <summary>
    /// Engine for executing database scripts
    /// </summary>
    public class PPDMScriptExecutionEngine
    {
        private readonly IDMEEditor _editor;
        private readonly string _connectionName;
        private readonly ILogger<PPDMScriptExecutionEngine>? _logger;
        private readonly PPDMDatabaseScriptLogger _scriptLogger;

        public PPDMScriptExecutionEngine(
            IDMEEditor editor,
            string connectionName,
            PPDMDatabaseScriptLogger scriptLogger,
            ILogger<PPDMScriptExecutionEngine>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
            _scriptLogger = scriptLogger ?? throw new ArgumentNullException(nameof(scriptLogger));
            _logger = logger;
        }

        /// <summary>
        /// Executes a single script file
        /// </summary>
        public async Task<ScriptExecutionResult> ExecuteScriptAsync(
            ScriptInfo scriptInfo,
            CancellationToken cancellationToken = default)
        {
            var result = new ScriptExecutionResult
            {
                ScriptFileName = scriptInfo.FileName,
                StartTime = DateTime.UtcNow
            };

            try
            {
                _scriptLogger.LogScriptStart(scriptInfo.FileName);

                if (!File.Exists(scriptInfo.FullPath))
                {
                    throw new FileNotFoundException($"Script file not found: {scriptInfo.FullPath}");
                }

                var scriptContent = await File.ReadAllTextAsync(scriptInfo.FullPath, cancellationToken);
                
                if (string.IsNullOrWhiteSpace(scriptContent))
                {
                    _scriptLogger.LogWarning($"Script file is empty: {scriptInfo.FileName}");
                    result.Success = true;
                    result.EndTime = DateTime.UtcNow;
                    return result;
                }

                // Execute script based on database type
                var executionResult = await ExecuteScriptContentAsync(scriptContent, scriptInfo, cancellationToken);

                result.Success = true;
                result.RowsAffected = executionResult.RowsAffected;
                result.ExecutionLog = executionResult.Log;
                result.EndTime = DateTime.UtcNow;

                _scriptLogger.LogScriptComplete(scriptInfo.FileName, result.Duration, result.RowsAffected);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.Exception = ex;
                result.EndTime = DateTime.UtcNow;

                _scriptLogger.LogScriptFailure(scriptInfo.FileName, ex.Message, ex);
                _logger?.LogError(ex, $"Error executing script: {scriptInfo.FileName}");
            }

            return result;
        }

        /// <summary>
        /// Executes script content using IDataSource.ExecuteSql
        /// </summary>
        private async Task<(int? RowsAffected, string? Log)> ExecuteScriptContentAsync(
            string scriptContent,
            ScriptInfo scriptInfo,
            CancellationToken cancellationToken)
        {
            try
            {
                // Get data source from editor
                var dataSource = _editor.GetDataSource(_connectionName);
                
                if (dataSource == null)
                {
                    throw new InvalidOperationException($"Connection not found: {_connectionName}");
                }

                // Ensure data source connection is open
                if (dataSource.ConnectionStatus != ConnectionState.Open)
                {
                    var connectionState = dataSource.Openconnection();
                    if (connectionState != ConnectionState.Open)
                    {
                        throw new InvalidOperationException($"Failed to open connection for: {_connectionName}. Status: {connectionState}");
                    }
                }

                // Split script into batches (by GO statements for SQL Server, or semicolons for others)
                var batches = SplitScriptIntoBatches(scriptContent, scriptInfo);

                int totalBatchesExecuted = 0;
                int totalBatchesFailed = 0;
                var logBuilder = new StringBuilder();

                foreach (var batch in batches)
                {
                    if (string.IsNullOrWhiteSpace(batch)) continue;
                    if (cancellationToken.IsCancellationRequested) break;

                    try
                    {
                        // Execute batch using IDataSource.ExecuteSql
                        var errorInfo = dataSource.ExecuteSql(batch);
                        
                        if (errorInfo != null && errorInfo.Flag != Errors.Ok)
                        {
                            var errorMessage = !string.IsNullOrEmpty(errorInfo.Message) 
                                ? errorInfo.Message 
                                : "Unknown error";
                            
                            logBuilder.AppendLine($"Batch failed: {errorMessage}");
                            totalBatchesFailed++;
                            
                            // Log the error
                            _logger?.LogError($"Error executing batch: {errorMessage}");
                            
                            // Re-throw to stop execution on error
                            throw new InvalidOperationException($"Batch execution failed: {errorMessage}");
                        }
                        else
                        {
                            totalBatchesExecuted++;
                            logBuilder.AppendLine($"Batch executed successfully");
                        }
                    }
                    catch (Exception ex)
                    {
                        logBuilder.AppendLine($"Batch failed: {ex.Message}");
                        totalBatchesFailed++;
                        _logger?.LogError(ex, $"Error executing batch in script: {scriptInfo.FileName}");
                        throw; // Re-throw to stop execution
                    }
                }

                var logMessage = $"Executed {totalBatchesExecuted} batches successfully";
                if (totalBatchesFailed > 0)
                {
                    logMessage += $", {totalBatchesFailed} batches failed";
                }

                return (totalBatchesExecuted > 0 ? totalBatchesExecuted : null, logBuilder.ToString());
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error executing script content: {scriptInfo.FileName}");
                throw;
            }
        }

        /// <summary>
        /// Splits script into executable batches
        /// </summary>
        private List<string> SplitScriptIntoBatches(string scriptContent, ScriptInfo scriptInfo)
        {
            var batches = new List<string>();

            // For SQL Server, split by GO statements
            if (scriptInfo.FileName.Contains("Sqlserver", StringComparison.OrdinalIgnoreCase) ||
                scriptInfo.FileName.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
            {
                var lines = scriptContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                var currentBatch = new StringBuilder();

                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();
                    
                    // SQL Server GO statement
                    if (trimmedLine.Equals("GO", StringComparison.OrdinalIgnoreCase) ||
                        trimmedLine.Equals("go", StringComparison.OrdinalIgnoreCase))
                    {
                        if (currentBatch.Length > 0)
                        {
                            batches.Add(currentBatch.ToString());
                            currentBatch.Clear();
                        }
                    }
                    else
                    {
                        currentBatch.AppendLine(line);
                    }
                }

                if (currentBatch.Length > 0)
                {
                    batches.Add(currentBatch.ToString());
                }
            }
            else
            {
                // For other databases, split by semicolons (simple approach)
                var statements = scriptContent.Split(';', StringSplitOptions.RemoveEmptyEntries);
                batches.AddRange(statements.Where(s => !string.IsNullOrWhiteSpace(s)));
            }

            return batches;
        }

    }
}

