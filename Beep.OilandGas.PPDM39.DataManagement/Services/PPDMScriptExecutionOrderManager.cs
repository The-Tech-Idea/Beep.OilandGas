using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.PPDM39.DataManagement.Core.Models.DatabaseCreation;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.PPDM39.DataManagement.Services
{
    /// <summary>
    /// Manages execution order and dependencies for scripts
    /// </summary>
    public class PPDMScriptExecutionOrderManager
    {
        private readonly ILogger<PPDMScriptExecutionOrderManager>? _logger;

        public PPDMScriptExecutionOrderManager(ILogger<PPDMScriptExecutionOrderManager>? logger = null)
        {
            _logger = logger;
        }

        /// <summary>
        /// Orders scripts for execution based on dependencies and script type
        /// </summary>
        public List<ScriptInfo> OrderScriptsForExecution(List<ScriptInfo> scripts)
        {
            try
            {
                // First, order by script type (execution order)
                var ordered = scripts.OrderBy(s => s.ExecutionOrder).ToList();

                // Within each script type, order consolidated scripts first, then individual scripts
                var result = new List<ScriptInfo>();
                var grouped = ordered.GroupBy(s => s.ScriptType);

                foreach (var group in grouped)
                {
                    // Consolidated scripts first
                    var consolidated = group.Where(s => s.IsConsolidated).ToList();
                    result.AddRange(consolidated);

                    // Then individual scripts, ordered by table name
                    var individual = group.Where(s => !s.IsConsolidated)
                        .OrderBy(s => s.TableName ?? s.FileName)
                        .ToList();
                    result.AddRange(individual);
                }

                _logger?.LogInformation($"Ordered {scripts.Count} scripts for execution");
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error ordering scripts for execution");
                throw;
            }
        }

        /// <summary>
        /// Validates script dependencies
        /// </summary>
        public bool ValidateDependencies(List<ScriptInfo> scripts, out List<string> errors)
        {
            errors = new List<string>();

            try
            {
                var scriptNames = scripts.Select(s => s.FileName).ToHashSet(StringComparer.OrdinalIgnoreCase);

                foreach (var script in scripts)
                {
                    foreach (var dependency in script.Dependencies)
                    {
                        if (!scriptNames.Contains(dependency))
                        {
                            errors.Add($"Script {script.FileName} depends on {dependency}, but dependency not found");
                        }
                    }
                }

                if (errors.Any())
                {
                    _logger?.LogWarning($"Found {errors.Count} dependency validation errors");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error validating dependencies");
                errors.Add($"Error validating dependencies: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets scripts that can be executed in parallel (independent scripts)
        /// </summary>
        public List<List<ScriptInfo>> GetParallelExecutionGroups(List<ScriptInfo> scripts)
        {
            try
            {
                var groups = new List<List<ScriptInfo>>();
                var processed = new HashSet<string>();

                foreach (var script in scripts)
                {
                    if (processed.Contains(script.FileName)) continue;

                    // Find all scripts that can run in parallel with this one
                    var parallelGroup = new List<ScriptInfo> { script };
                    processed.Add(script.FileName);

                    foreach (var otherScript in scripts)
                    {
                        if (processed.Contains(otherScript.FileName)) continue;
                        if (otherScript.ScriptType != script.ScriptType) continue;

                        // Can run in parallel if no dependencies between them
                        if (!HasDependency(script, otherScript) && !HasDependency(otherScript, script))
                        {
                            parallelGroup.Add(otherScript);
                            processed.Add(otherScript.FileName);
                        }
                    }

                    if (parallelGroup.Count > 1)
                    {
                        groups.Add(parallelGroup);
                    }
                }

                return groups;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error determining parallel execution groups");
                return new List<List<ScriptInfo>>();
            }
        }

        /// <summary>
        /// Checks if script1 depends on script2
        /// </summary>
        private bool HasDependency(ScriptInfo script1, ScriptInfo script2)
        {
            return script1.Dependencies.Contains(script2.FileName, StringComparer.OrdinalIgnoreCase);
        }
    }
}









