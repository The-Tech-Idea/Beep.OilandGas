using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Beep.OilandGas.PPDM39.DataManagement.Core.Models.DatabaseCreation;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.PPDM39.DataManagement.Services
{
    /// <summary>
    /// Comprehensive logging service for database script execution
    /// </summary>
    public class PPDMDatabaseScriptLogger
    {
        private readonly ILogger? _logger;
        private readonly string? _logFilePath;
        private readonly StreamWriter? _fileWriter;
        private readonly object _lockObject = new object();
        private readonly List<string> _inMemoryLog = new List<string>();

        public PPDMDatabaseScriptLogger(ILogger? logger = null, string? logFilePath = null)
        {
            _logger = logger;
            _logFilePath = logFilePath;

            if (!string.IsNullOrEmpty(_logFilePath))
            {
                try
                {
                    var directory = Path.GetDirectoryName(_logFilePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    _fileWriter = new StreamWriter(_logFilePath, append: true, Encoding.UTF8)
                    {
                        AutoFlush = true
                    };
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, $"Failed to create log file: {_logFilePath}");
                }
            }
        }

        /// <summary>
        /// Logs an information message
        /// </summary>
        public void LogInfo(string message)
        {
            var logMessage = FormatLogMessage("INFO", message);
            WriteLog(logMessage);
            _logger?.LogInformation(message);
        }

        /// <summary>
        /// Logs a success message
        /// </summary>
        public void LogSuccess(string message)
        {
            var logMessage = FormatLogMessage("SUCCESS", message);
            WriteLog(logMessage);
            _logger?.LogInformation(message);
        }

        /// <summary>
        /// Logs a warning message
        /// </summary>
        public void LogWarning(string message)
        {
            var logMessage = FormatLogMessage("WARNING", message);
            WriteLog(logMessage);
            _logger?.LogWarning(message);
        }

        /// <summary>
        /// Logs an error message
        /// </summary>
        public void LogError(string message, Exception? exception = null)
        {
            var logMessage = FormatLogMessage("ERROR", message);
            if (exception != null)
            {
                logMessage += $"\nException: {exception}\nStack Trace: {exception.StackTrace}";
            }
            WriteLog(logMessage);
            _logger?.LogError(exception, message);
        }

        /// <summary>
        /// Logs script execution start
        /// </summary>
        public void LogScriptStart(string scriptFileName)
        {
            LogInfo($"Executing: {scriptFileName}");
        }

        /// <summary>
        /// Logs script execution completion
        /// </summary>
        public void LogScriptComplete(string scriptFileName, TimeSpan duration, int? rowsAffected = null)
        {
            var message = $"{scriptFileName} completed in {duration.TotalSeconds:F2}s";
            if (rowsAffected.HasValue)
            {
                message += $" ({rowsAffected} rows affected)";
            }
            LogSuccess(message);
        }

        /// <summary>
        /// Logs script execution failure
        /// </summary>
        public void LogScriptFailure(string scriptFileName, string errorMessage, Exception? exception = null)
        {
            LogError($"{scriptFileName} failed: {errorMessage}", exception);
        }

        /// <summary>
        /// Logs progress update
        /// </summary>
        public void LogProgress(int current, int total, string currentScript)
        {
            var percentage = total > 0 ? (decimal)current * 100 / total : 0;
            var message = $"Progress: {current}/{total} ({percentage:F1}%) - {currentScript}";
            LogInfo(message);
        }

        /// <summary>
        /// Gets in-memory log entries
        /// </summary>
        public List<string> GetInMemoryLog()
        {
            lock (_lockObject)
            {
                return new List<string>(_inMemoryLog);
            }
        }

        /// <summary>
        /// Formats a log message with timestamp
        /// </summary>
        private string FormatLogMessage(string level, string message)
        {
            return $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
        }

        /// <summary>
        /// Writes log message to all targets
        /// </summary>
        private void WriteLog(string logMessage)
        {
            lock (_lockObject)
            {
                _inMemoryLog.Add(logMessage);
                
                // Keep only last 1000 entries in memory
                if (_inMemoryLog.Count > 1000)
                {
                    _inMemoryLog.RemoveAt(0);
                }

                _fileWriter?.WriteLine(logMessage);
            }
        }

        /// <summary>
        /// Disposes resources
        /// </summary>
        public void Dispose()
        {
            _fileWriter?.Dispose();
        }
    }
}








