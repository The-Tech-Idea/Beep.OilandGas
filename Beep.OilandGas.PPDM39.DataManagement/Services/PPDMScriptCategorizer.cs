using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.PPDM39.DataManagement.Core.Models.DatabaseCreation;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.PPDM39.DataManagement.Services
{
    /// <summary>
    /// Categorizes scripts by module and script type
    /// </summary>
    public class PPDMScriptCategorizer
    {
        private readonly ILogger<PPDMScriptCategorizer>? _logger;

        public PPDMScriptCategorizer(ILogger<PPDMScriptCategorizer>? logger = null)
        {
            _logger = logger;
        }

        /// <summary>
        /// Categorizes scripts by module and script type
        /// </summary>
        public List<ScriptCategory> CategorizeScripts(List<ScriptInfo> scripts)
        {
            try
            {
                var categories = new Dictionary<string, ScriptCategory>();

                foreach (var script in scripts)
                {
                    var categoryKey = GetCategoryKey(script);
                    
                    if (!categories.ContainsKey(categoryKey))
                    {
                        categories[categoryKey] = new ScriptCategory
                        {
                            CategoryName = GetCategoryName(script),
                            Module = script.Module,
                            SubjectArea = script.SubjectArea,
                            ScriptType = script.ScriptType,
                            ExecutionOrder = script.ExecutionOrder,
                            IsMandatory = script.IsMandatory
                        };
                    }

                    categories[categoryKey].Scripts.Add(script);
                }

                var result = categories.Values.OrderBy(c => c.ExecutionOrder).ThenBy(c => c.CategoryName).ToList();
                _logger?.LogInformation($"Categorized {scripts.Count} scripts into {result.Count} categories");
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error categorizing scripts");
                throw;
            }
        }

        /// <summary>
        /// Gets category key for grouping
        /// </summary>
        private string GetCategoryKey(ScriptInfo script)
        {
            if (script.IsConsolidated)
            {
                return $"CONSOLIDATED_{script.ScriptType}";
            }

            var module = script.Module ?? "Unknown";
            return $"{module}_{script.ScriptType}";
        }

        /// <summary>
        /// Gets display name for category
        /// </summary>
        private string GetCategoryName(ScriptInfo script)
        {
            if (script.IsConsolidated)
            {
                return $"Consolidated {script.ScriptType} Scripts";
            }

            var module = script.Module ?? "Unknown";
            return $"{module} - {script.ScriptType}";
        }

        /// <summary>
        /// Filters scripts by category and script type
        /// </summary>
        public List<ScriptInfo> FilterScripts(
            List<ScriptInfo> scripts,
            List<string>? categories = null,
            List<ScriptType>? scriptTypes = null)
        {
            var filtered = scripts.AsEnumerable();

            if (categories != null && categories.Any())
            {
                filtered = filtered.Where(s => 
                    (s.Module != null && categories.Contains(s.Module, StringComparer.OrdinalIgnoreCase)) ||
                    (s.SubjectArea != null && categories.Contains(s.SubjectArea, StringComparer.OrdinalIgnoreCase)) ||
                    (s.Category != null && categories.Contains(s.Category, StringComparer.OrdinalIgnoreCase)));
            }

            if (scriptTypes != null && scriptTypes.Any())
            {
                filtered = filtered.Where(s => scriptTypes.Contains(s.ScriptType));
            }

            return filtered.ToList();
        }
    }
}









