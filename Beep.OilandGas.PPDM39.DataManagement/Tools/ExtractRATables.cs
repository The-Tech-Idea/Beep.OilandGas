using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Beep.OilandGas.PPDM39.DataManagement.Tools
{
    /// <summary>
    /// Extracts all RA_* table names from PPDM scripts and categorizes them by domain
    /// </summary>
    public class ExtractRATables
    {
        private readonly string _scriptsPath;

        public ExtractRATables(string scriptsPath = null)
        {
            _scriptsPath = scriptsPath ?? Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "Beep.OilandGas.PPDM39", "Scripts", "Sqlserver");
        }

        /// <summary>
        /// Extracts all RA_* table names from TAB.sql
        /// </summary>
        public async Task<List<string>> ExtractRATableNamesAsync()
        {
            var tableNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var tabSqlPath = Path.Combine(_scriptsPath, "TAB.sql");

            if (!File.Exists(tabSqlPath))
            {
                throw new FileNotFoundException($"TAB.sql not found at: {tabSqlPath}");
            }

            var content = await File.ReadAllTextAsync(tabSqlPath);
            
            // Pattern to match CREATE TABLE statements for RA_* tables
            var pattern = @"CREATE\s+TABLE\s+(ra_\w+)";
            var matches = Regex.Matches(content, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);

            foreach (Match match in matches)
            {
                if (match.Groups.Count > 1)
                {
                    var tableName = match.Groups[1].Value.Trim();
                    tableNames.Add(tableName);
                }
            }

            return tableNames.OrderBy(t => t).ToList();
        }

        /// <summary>
        /// Categorizes RA_* tables by domain
        /// </summary>
        public Dictionary<string, List<string>> CategorizeRATables(List<string> tableNames)
        {
            var categories = new Dictionary<string, List<string>>
            {
                ["Well"] = new List<string>(),
                ["Field"] = new List<string>(),
                ["Cost"] = new List<string>(),
                ["Accounting"] = new List<string>(),
                ["Analysis"] = new List<string>(),
                ["Facility"] = new List<string>(),
                ["Pipeline"] = new List<string>(),
                ["Property"] = new List<string>(),
                ["Production"] = new List<string>(),
                ["Equipment"] = new List<string>(),
                ["Geology"] = new List<string>(),
                ["Reservoir"] = new List<string>(),
                ["Drilling"] = new List<string>(),
                ["Completion"] = new List<string>(),
                ["Measurement"] = new List<string>(),
                ["Unit"] = new List<string>(),
                ["Source"] = new List<string>(),
                ["Quality"] = new List<string>(),
                ["Other"] = new List<string>()
            };

            foreach (var tableName in tableNames)
            {
                var upperName = tableName.ToUpperInvariant();
                bool categorized = false;

                // Well-related
                if (upperName.Contains("WELL") || upperName.Contains("BOREHOLE"))
                {
                    categories["Well"].Add(tableName);
                    categorized = true;
                }
                // Field-related
                else if (upperName.Contains("FIELD") || upperName.Contains("RESERVOIR"))
                {
                    if (upperName.Contains("RESERVOIR"))
                        categories["Reservoir"].Add(tableName);
                    else
                        categories["Field"].Add(tableName);
                    categorized = true;
                }
                // Cost-related
                else if (upperName.Contains("COST") || upperName.Contains("EXPENSE") || upperName.Contains("BUDGET"))
                {
                    categories["Cost"].Add(tableName);
                    categorized = true;
                }
                // Accounting-related
                else if (upperName.Contains("ACCOUNT") || upperName.Contains("ACCOUNTING") || upperName.Contains("REVENUE") || 
                         upperName.Contains("INVOICE") || upperName.Contains("PAYMENT") || upperName.Contains("TAX") ||
                         upperName.Contains("ROYALTY") || upperName.Contains("ALLOCATION"))
                {
                    categories["Accounting"].Add(tableName);
                    categorized = true;
                }
                // Analysis-related
                else if (upperName.Contains("ANL_") || upperName.Contains("ANALYSIS") || upperName.Contains("CALC"))
                {
                    categories["Analysis"].Add(tableName);
                    categorized = true;
                }
                // Facility-related
                else if (upperName.Contains("FACILITY") || upperName.Contains("PLANT") || upperName.Contains("TERMINAL"))
                {
                    categories["Facility"].Add(tableName);
                    categorized = true;
                }
                // Pipeline-related
                else if (upperName.Contains("PIPELINE") || upperName.Contains("LINE"))
                {
                    categories["Pipeline"].Add(tableName);
                    categorized = true;
                }
                // Property-related
                else if (upperName.Contains("PROPERTY") || upperName.Contains("LEASE") || upperName.Contains("TITLE"))
                {
                    categories["Property"].Add(tableName);
                    categorized = true;
                }
                // Production-related
                else if (upperName.Contains("PRODUCTION") || upperName.Contains("PROD"))
                {
                    categories["Production"].Add(tableName);
                    categorized = true;
                }
                // Equipment-related
                else if (upperName.Contains("EQUIPMENT") || upperName.Contains("COMPONENT") || upperName.Contains("ASSET"))
                {
                    categories["Equipment"].Add(tableName);
                    categorized = true;
                }
                // Geology-related
                else if (upperName.Contains("GEOLOGY") || upperName.Contains("STRAT") || upperName.Contains("FORMATION") ||
                         upperName.Contains("LITHOLOGY") || upperName.Contains("ROCK"))
                {
                    categories["Geology"].Add(tableName);
                    categorized = true;
                }
                // Drilling-related
                else if (upperName.Contains("DRILL") || upperName.Contains("RIG") || upperName.Contains("BIT"))
                {
                    categories["Drilling"].Add(tableName);
                    categorized = true;
                }
                // Completion-related
                else if (upperName.Contains("COMPLETION") || upperName.Contains("COMPL"))
                {
                    categories["Completion"].Add(tableName);
                    categorized = true;
                }
                // Measurement-related
                else if (upperName.Contains("MEASURE") || upperName.Contains("UNIT_OF_MEASURE") || upperName.Contains("UOM"))
                {
                    categories["Measurement"].Add(tableName);
                    categorized = true;
                }
                // Unit-related
                else if (upperName.Contains("UNIT") && !upperName.Contains("MEASURE"))
                {
                    categories["Unit"].Add(tableName);
                    categorized = true;
                }
                // Source-related
                else if (upperName.Contains("SOURCE") || upperName.Contains("REFERENCE"))
                {
                    categories["Source"].Add(tableName);
                    categorized = true;
                }
                // Quality-related
                else if (upperName.Contains("QUALITY") || upperName.Contains("ROW_QUALITY"))
                {
                    categories["Quality"].Add(tableName);
                    categorized = true;
                }

                if (!categorized)
                {
                    categories["Other"].Add(tableName);
                }
            }

            // Remove empty categories
            var result = categories.Where(kvp => kvp.Value.Any()).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            return result;
        }

        /// <summary>
        /// Exports categorized tables to JSON
        /// </summary>
        public async Task<string> ExportToJsonAsync(List<string> tableNames, Dictionary<string, List<string>> categories, string outputPath = null)
        {
            var export = new
            {
                TotalTables = tableNames.Count,
                ExtractionDate = DateTime.UtcNow,
                Tables = tableNames,
                Categories = categories.Select(kvp => new
                {
                    Category = kvp.Key,
                    Count = kvp.Value.Count,
                    Tables = kvp.Value
                }).ToList()
            };

            var json = JsonSerializer.Serialize(export, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            if (!string.IsNullOrEmpty(outputPath))
            {
                var directory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                await File.WriteAllTextAsync(outputPath, json);
            }

            return json;
        }

        /// <summary>
        /// Main execution method
        /// </summary>
        public static async Task<int> Main(string[] args)
        {
            try
            {
                var extractor = new ExtractRATables();
                var tableNames = await extractor.ExtractRATableNamesAsync();
                var categories = extractor.CategorizeRATables(tableNames);

                Console.WriteLine($"Extracted {tableNames.Count} RA_* tables");
                Console.WriteLine($"Categorized into {categories.Count} domains:");
                foreach (var category in categories.OrderByDescending(c => c.Value.Count))
                {
                    Console.WriteLine($"  {category.Key}: {category.Value.Count} tables");
                }

                var outputPath = args.Length > 0 ? args[0] : Path.Combine(AppContext.BaseDirectory, "RATablesExport.json");
                await extractor.ExportToJsonAsync(tableNames, categories, outputPath);
                Console.WriteLine($"\nExported to: {outputPath}");

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Console.Error.WriteLine(ex.StackTrace);
                return 1;
            }
        }
    }
}

