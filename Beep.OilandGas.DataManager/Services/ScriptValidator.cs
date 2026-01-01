using System.Text.RegularExpressions;
using Beep.OilandGas.DataManager.Core.Interfaces;
using Beep.OilandGas.DataManager.Core.Models;
using Beep.OilandGas.DataManager.Core.Exceptions;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.DataManager.Services
{
    /// <summary>
    /// Service for validating scripts before execution and checking for errors after execution
    /// </summary>
    public class ScriptValidator
    {
        private readonly ILogger<ScriptValidator>? _logger;

        public ScriptValidator(ILogger<ScriptValidator>? logger = null)
        {
            _logger = logger;
        }

        /// <summary>
        /// Validate scripts for a module
        /// </summary>
        public async Task<ValidationResult> ValidateScriptsAsync(
            IModuleData moduleData,
            string databaseType,
            IDataSource? dataSource = null)
        {
            var result = new ValidationResult
            {
                IsValid = true
            };

            try
            {
                var scripts = await moduleData.GetScriptsAsync(databaseType);
                result.TotalScriptsChecked = scripts.Count();

                foreach (var script in scripts)
                {
                    var scriptValidation = await ValidateScriptAsync(script, dataSource);
                    
                    if (scriptValidation.IsValid)
                    {
                        result.ValidScripts++;
                    }
                    else
                    {
                        result.InvalidScripts++;
                        result.IsValid = false;
                        result.Errors.AddRange(scriptValidation.Errors);
                        result.Warnings.AddRange(scriptValidation.Warnings);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error validating scripts for module: {ModuleName}", moduleData.ModuleName);
                result.IsValid = false;
                result.Errors.Add(new ValidationError
                {
                    ScriptFileName = "Module Validation",
                    ErrorType = "Exception",
                    Message = $"Failed to validate module: {ex.Message}",
                    Details = ex.ToString()
                });
            }

            return result;
        }

        /// <summary>
        /// Validate a single script
        /// </summary>
        public async Task<ValidationResult> ValidateScriptAsync(
            ModuleScriptInfo scriptInfo,
            IDataSource? dataSource = null)
        {
            var result = new ValidationResult
            {
                IsValid = true,
                TotalScriptsChecked = 1
            };

            try
            {
                // Check if file exists
                if (!File.Exists(scriptInfo.FullPath))
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationError
                    {
                        ScriptFileName = scriptInfo.FileName,
                        ErrorType = "Missing",
                        Message = $"Script file not found: {scriptInfo.FullPath}"
                    });
                    return result;
                }

                // Read script content
                var scriptContent = await File.ReadAllTextAsync(scriptInfo.FullPath);

                // Basic syntax validation
                var syntaxErrors = ValidateSyntax(scriptContent, scriptInfo.FileName);
                result.Errors.AddRange(syntaxErrors);

                // Validate dependencies if dataSource is provided
                if (dataSource != null && scriptInfo.Dependencies.Any())
                {
                    var dependencyErrors = await ValidateDependenciesAsync(scriptInfo, dataSource);
                    result.Errors.AddRange(dependencyErrors);
                }

                if (result.Errors.Any())
                {
                    result.IsValid = false;
                    result.InvalidScripts = 1;
                }
                else
                {
                    result.ValidScripts = 1;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error validating script: {FileName}", scriptInfo.FileName);
                result.IsValid = false;
                result.Errors.Add(new ValidationError
                {
                    ScriptFileName = scriptInfo.FileName,
                    ErrorType = "Exception",
                    Message = $"Validation error: {ex.Message}",
                    Details = ex.ToString()
                });
            }

            return result;
        }

        /// <summary>
        /// Check for errors after script execution
        /// </summary>
        public async Task<ErrorCheckResult> CheckForErrorsAsync(
            ExecutionState executionState,
            IDataSource dataSource)
        {
            var result = new ErrorCheckResult
            {
                ExecutionId = executionState.ExecutionId,
                HasErrors = false
            };

            try
            {
                // Check failed scripts
                foreach (var failedScript in executionState.FailedScripts)
                {
                    result.HasErrors = true;
                    result.ScriptErrors.Add(new ScriptError
                    {
                        ScriptFileName = failedScript,
                        ErrorMessage = executionState.ErrorMessage ?? "Script execution failed",
                        ErrorTime = executionState.LastCheckpoint ?? executionState.StartTime
                    });
                }

                // Verify objects were created (if VerifyObjectsCreated is enabled)
                if (executionState.ScriptResults.Any())
                {
                    var verificationErrors = await VerifyObjectsCreatedAsync(executionState, dataSource);
                    result.ObjectErrors.AddRange(verificationErrors);
                    result.TotalObjectsChecked = verificationErrors.Count + (verificationErrors.Count - verificationErrors.Count(e => !string.IsNullOrEmpty(e.ErrorMessage)));
                    result.ValidObjects = verificationErrors.Count(e => string.IsNullOrEmpty(e.ErrorMessage));
                    result.MissingObjects = verificationErrors.Count(e => !string.IsNullOrEmpty(e.ErrorMessage));

                    if (verificationErrors.Any(e => !string.IsNullOrEmpty(e.ErrorMessage)))
                    {
                        result.HasErrors = true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error checking for errors: {ExecutionId}", executionState.ExecutionId);
                result.HasErrors = true;
                result.ScriptErrors.Add(new ScriptError
                {
                    ScriptFileName = "Error Check",
                    ErrorMessage = $"Failed to check for errors: {ex.Message}",
                    Exception = ex,
                    ErrorTime = DateTime.UtcNow
                });
            }

            return result;
        }

        private List<ValidationError> ValidateSyntax(string scriptContent, string fileName)
        {
            var errors = new List<ValidationError>();

            try
            {
                // Basic syntax checks
                var lines = scriptContent.Split('\n');
                var openParens = 0;
                var openBrackets = 0;

                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    var lineNumber = i + 1;

                    // Check for unmatched parentheses (basic check)
                    openParens += line.Count(c => c == '(') - line.Count(c => c == ')');
                    openBrackets += line.Count(c => c == '[') - line.Count(c => c == ']');

                    // Check for common SQL syntax issues
                    if (line.Trim().StartsWith("CREATE TABLE", StringComparison.OrdinalIgnoreCase) ||
                        line.Trim().StartsWith("ALTER TABLE", StringComparison.OrdinalIgnoreCase))
                    {
                        // Basic validation - check if statement seems complete
                        if (i == lines.Length - 1 && !line.TrimEnd().EndsWith(';'))
                        {
                            // This is a warning, not an error
                            // Could be part of a multi-line statement
                        }
                    }
                }

                // Check for unmatched parentheses at end
                if (openParens != 0)
                {
                    errors.Add(new ValidationError
                    {
                        ScriptFileName = fileName,
                        ErrorType = "Syntax",
                        Message = $"Unmatched parentheses detected (difference: {openParens})",
                        LineNumber = lines.Length
                    });
                }

                if (openBrackets != 0)
                {
                    errors.Add(new ValidationError
                    {
                        ScriptFileName = fileName,
                        ErrorType = "Syntax",
                        Message = $"Unmatched brackets detected (difference: {openBrackets})",
                        LineNumber = lines.Length
                    });
                }
            }
            catch (Exception ex)
            {
                errors.Add(new ValidationError
                {
                    ScriptFileName = fileName,
                    ErrorType = "Syntax",
                    Message = $"Error during syntax validation: {ex.Message}",
                    Details = ex.ToString()
                });
            }

            return errors;
        }

        private async Task<List<ValidationError>> ValidateDependenciesAsync(
            ModuleScriptInfo scriptInfo,
            IDataSource dataSource)
        {
            var errors = new List<ValidationError>();

            try
            {
                // Check if dependent tables exist
                foreach (var dependency in scriptInfo.Dependencies)
                {
                    var tableExists = await CheckTableExistsAsync(dependency, dataSource);
                    if (!tableExists)
                    {
                        errors.Add(new ValidationError
                        {
                            ScriptFileName = scriptInfo.FileName,
                            ErrorType = "Dependency",
                            Message = $"Dependent table '{dependency}' does not exist",
                            Details = $"Script requires table '{dependency}' to exist before execution"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error validating dependencies for script: {FileName}", scriptInfo.FileName);
                errors.Add(new ValidationError
                {
                    ScriptFileName = scriptInfo.FileName,
                    ErrorType = "Dependency",
                    Message = $"Error checking dependencies: {ex.Message}"
                });
            }

            return errors;
        }

        private async Task<List<ObjectVerificationError>> VerifyObjectsCreatedAsync(
            ExecutionState executionState,
            IDataSource dataSource)
        {
            var errors = new List<ObjectVerificationError>();

            try
            {
                // Extract table names from completed scripts
                var tableNames = new HashSet<string>();

                foreach (var scriptResult in executionState.ScriptResults.Values)
                {
                    // Try to extract table names from script results
                    // This would need to be populated during script execution
                    // For now, we'll check based on script type
                    if (scriptResult.ScriptFileName.Contains("_TAB", StringComparison.OrdinalIgnoreCase))
                    {
                        // Extract table name from script filename
                        var tableName = ExtractTableNameFromFileName(scriptResult.ScriptFileName);
                        if (!string.IsNullOrEmpty(tableName))
                        {
                            tableNames.Add(tableName);
                        }
                    }
                }

                // Verify tables exist
                foreach (var tableName in tableNames)
                {
                    var exists = await CheckTableExistsAsync(tableName, dataSource);
                    if (!exists)
                    {
                        errors.Add(new ObjectVerificationError
                        {
                            ObjectName = tableName,
                            ObjectType = "Table",
                            ExpectedScript = $"{tableName}_TAB.sql",
                            ErrorMessage = $"Table '{tableName}' was not created successfully"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error verifying objects created: {ExecutionId}", executionState.ExecutionId);
                errors.Add(new ObjectVerificationError
                {
                    ObjectName = "Verification",
                    ObjectType = "System",
                    ExpectedScript = "N/A",
                    ErrorMessage = $"Error during verification: {ex.Message}"
                });
            }

            return errors;
        }

        private async Task<bool> CheckTableExistsAsync(string tableName, IDataSource dataSource)
        {
            try
            {
                var dbType = dataSource.DatasourceEntity?.DatabaseType?.ToLower() ?? "sqlserver";
                var paramDelim = dataSource.ParameterDelimiter;
                string sql;

                switch (dbType)
                {
                    case "sqlserver":
                        sql = $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = {paramDelim}tableName";
                        break;
                    case "postgresql":
                        sql = $"SELECT COUNT(*) FROM information_schema.tables WHERE table_name = {paramDelim}tableName";
                        break;
                    case "oracle":
                        sql = $"SELECT COUNT(*) FROM user_tables WHERE table_name = UPPER({paramDelim}tableName)";
                        break;
                    case "mysql":
                    case "mariadb":
                        sql = $"SELECT COUNT(*) FROM information_schema.tables WHERE table_name = {paramDelim}tableName";
                        break;
                    case "sqlite":
                        sql = $"SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name = {paramDelim}tableName";
                        break;
                    default:
                        sql = $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = {paramDelim}tableName";
                        break;
                }

                var count = dataSource.GetScalar(sql, new Dictionary<string, object> { { "tableName", tableName } });
                return count > 0;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error checking if table exists: {TableName}", tableName);
                return false;
            }
        }

        private string? ExtractTableNameFromFileName(string fileName)
        {
            // Extract table name from patterns like "TABLE_NAME_TAB.sql" or "TABLE_NAME_PK.sql"
            var match = Regex.Match(fileName, @"^([A-Z_]+)_(TAB|PK|FK|IX|CK)\.sql$", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return null;
        }
    }
}
