using System.Text.RegularExpressions;
using Beep.OilandGas.DataManager.Core.Interfaces;
using Beep.OilandGas.DataManager.Core.Models;
using Beep.OilandGas.DataManager.Core.Utilities;
using Beep.OilandGas.PPDM39.DataManagement.Core.Models.DatabaseCreation;

namespace Beep.OilandGas.DataManager.Core.Implementations
{
    /// <summary>
    /// Base implementation of IModuleData that reads scripts from the file system
    /// </summary>
    public abstract class FileSystemModuleData : IModuleData
    {
        protected readonly string _scriptsBasePath;

        public abstract string ModuleName { get; }
        public abstract string Description { get; }
        public abstract string ScriptBasePath { get; }
        public abstract int ExecutionOrder { get; }
        public abstract bool IsRequired { get; }

        protected FileSystemModuleData(string? scriptsBasePath = null)
        {
            _scriptsBasePath = scriptsBasePath ?? GetDefaultScriptsBasePath();
        }

        public virtual IEnumerable<string> GetDependencies()
        {
            // Default: PPDM39, Common and Security are usually dependencies
            if (ModuleName != "PPDM39" && ModuleName != "Common" && ModuleName != "Security")
            {
                return new[] { "PPDM39", "Common", "Security" };
            }
            return Enumerable.Empty<string>();
        }

        public virtual async Task<IEnumerable<ModuleScriptInfo>> GetScriptsAsync(string databaseType)
        {
            var scripts = new List<ModuleScriptInfo>();
            var normalizedDatabaseType = DatabaseTypeNormalizer.Normalize(databaseType);
            var scriptsPath = Path.Combine(_scriptsBasePath, normalizedDatabaseType, ScriptBasePath);

            if (!Directory.Exists(scriptsPath))
            {
                return scripts;
            }

            // Get all SQL files in the module directory
            var sqlFiles = Directory.GetFiles(scriptsPath, "*.sql", SearchOption.TopDirectoryOnly);

            foreach (var filePath in sqlFiles)
            {
                var fileName = Path.GetFileName(filePath);
                var fileInfo = new FileInfo(filePath);

                // Determine script type
                var scriptType = DetermineScriptType(fileName);
                if (scriptType == ScriptType.Other)
                {
                    continue; // Skip unknown script types
                }

                // Check if it's a consolidated script (e.g., TAB.sql, PK.sql) or individual (e.g., TABLE_TAB.sql)
                var isConsolidated = IsConsolidatedScript(fileName);
                var tableName = isConsolidated ? null : ExtractTableName(fileName);
                var tableNames = isConsolidated ? await ExtractTableNamesFromConsolidatedScript(filePath) : new List<string>();

                var scriptInfo = new ModuleScriptInfo
                {
                    FileName = fileName,
                    FullPath = filePath,
                    RelativePath = Path.GetRelativePath(Path.Combine(_scriptsBasePath, normalizedDatabaseType), filePath),
                    ScriptType = scriptType,
                    TableName = tableName,
                    TableNames = tableNames,
                    IsConsolidated = isConsolidated,
                    ExecutionOrder = GetExecutionOrder(scriptType),
                    IsRequired = true,
                    FileSize = fileInfo.Length,
                    LastModified = fileInfo.LastWriteTime
                };

                scripts.Add(scriptInfo);
            }

            return scripts.OrderBy(s => s.ExecutionOrder).ThenBy(s => s.FileName);
        }

        protected virtual ScriptType DetermineScriptType(string fileName)
        {
            var upperFileName = fileName.ToUpperInvariant();
            
            if (upperFileName.Contains("_TAB") || upperFileName.Equals("TAB.SQL", StringComparison.OrdinalIgnoreCase))
                return ScriptType.TAB;
            if (upperFileName.Contains("_PK") || upperFileName.Equals("PK.SQL", StringComparison.OrdinalIgnoreCase))
                return ScriptType.PK;
            if (upperFileName.Contains("_FK") || upperFileName.Equals("FK.SQL", StringComparison.OrdinalIgnoreCase))
                return ScriptType.FK;
            if (upperFileName.Contains("_IX") || upperFileName.Equals("IX.SQL", StringComparison.OrdinalIgnoreCase))
                return ScriptType.IX;
            if (upperFileName.Contains("_CK") || upperFileName.Equals("CK.SQL", StringComparison.OrdinalIgnoreCase))
                return ScriptType.CK;
            if (upperFileName.Contains("_OUOM") || upperFileName.Equals("OUOM.SQL", StringComparison.OrdinalIgnoreCase))
                return ScriptType.OUOM;
            if (upperFileName.Contains("_UOM") || upperFileName.Equals("UOM.SQL", StringComparison.OrdinalIgnoreCase))
                return ScriptType.UOM;
            if (upperFileName.Contains("_RQUAL") || upperFileName.Equals("RQUAL.SQL", StringComparison.OrdinalIgnoreCase))
                return ScriptType.RQUAL;
            if (upperFileName.Contains("_RSRC") || upperFileName.Equals("RSRC.SQL", StringComparison.OrdinalIgnoreCase))
                return ScriptType.RSRC;
            if (upperFileName.Contains("_TCM") || upperFileName.Equals("TCM.SQL", StringComparison.OrdinalIgnoreCase))
                return ScriptType.TCM;
            if (upperFileName.Contains("_CCM") || upperFileName.Equals("CCM.SQL", StringComparison.OrdinalIgnoreCase))
                return ScriptType.CCM;
            if (upperFileName.Contains("_SYN") || upperFileName.Equals("SYN.SQL", StringComparison.OrdinalIgnoreCase))
                return ScriptType.SYN;
            if (upperFileName.Contains("_GUID") || upperFileName.Equals("GUID.SQL", StringComparison.OrdinalIgnoreCase))
                return ScriptType.GUID;

            return ScriptType.Other;
        }

        protected virtual bool IsConsolidatedScript(string fileName)
        {
            // Consolidated scripts are named like TAB.sql, PK.sql, FK.sql (without table prefix)
            var patterns = new[] { "^TAB\\.sql$", "^PK\\.sql$", "^FK\\.sql$", "^IX\\.sql$", "^CK\\.sql$",
                "^OUOM\\.sql$", "^UOM\\.sql$", "^RQUAL\\.sql$", "^RSRC\\.sql$", "^TCM\\.sql$",
                "^CCM\\.sql$", "^SYN\\.sql$", "^GUID\\.sql$" };
            
            return patterns.Any(pattern => Regex.IsMatch(fileName, pattern, RegexOptions.IgnoreCase));
        }

        protected virtual string? ExtractTableName(string fileName)
        {
            // Extract from patterns like "TABLE_NAME_TAB.sql" or "TABLE_NAME_PK.sql"
            var match = Regex.Match(fileName, @"^([A-Z_]+)_(TAB|PK|FK|IX|CK|OUOM|UOM|RQUAL|RSRC|TCM|CCM|SYN|GUID)\.sql$", RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value : null;
        }

        protected virtual async Task<List<string>> ExtractTableNamesFromConsolidatedScript(string filePath)
        {
            var tableNames = new List<string>();
            
            try
            {
                var content = await File.ReadAllTextAsync(filePath);
                
                // Extract table names from CREATE TABLE statements
                var createTableMatches = Regex.Matches(content, 
                    @"CREATE\s+TABLE\s+(?:IF\s+NOT\s+EXISTS\s+)?(?:\[?)(\w+)(?:\]?)", 
                    RegexOptions.IgnoreCase | RegexOptions.Multiline);
                
                foreach (Match match in createTableMatches)
                {
                    if (match.Groups.Count > 1)
                    {
                        var tableName = match.Groups[1].Value;
                        if (!string.IsNullOrEmpty(tableName) && !tableNames.Contains(tableName, StringComparer.OrdinalIgnoreCase))
                        {
                            tableNames.Add(tableName);
                        }
                    }
                }

                // Also check for ALTER TABLE statements (for constraints in FK.sql, PK.sql, etc.)
                var alterTableMatches = Regex.Matches(content,
                    @"ALTER\s+TABLE\s+(?:\[?)(\w+)(?:\]?)",
                    RegexOptions.IgnoreCase | RegexOptions.Multiline);

                foreach (Match match in alterTableMatches)
                {
                    if (match.Groups.Count > 1)
                    {
                        var tableName = match.Groups[1].Value;
                        if (!string.IsNullOrEmpty(tableName) && !tableNames.Contains(tableName, StringComparer.OrdinalIgnoreCase))
                        {
                            tableNames.Add(tableName);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // If we can't parse, return empty list
            }

            return tableNames;
        }

        protected virtual int GetExecutionOrder(ScriptType scriptType)
        {
            return scriptType switch
            {
                ScriptType.TAB => 1,
                ScriptType.PK => 2,
                ScriptType.FK => 3,
                ScriptType.IX => 4,
                ScriptType.CK => 5,
                ScriptType.OUOM => 6,
                ScriptType.UOM => 7,
                ScriptType.RQUAL => 8,
                ScriptType.RSRC => 9,
                ScriptType.TCM => 10,
                ScriptType.CCM => 11,
                ScriptType.SYN => 12,
                ScriptType.GUID => 13,
                _ => 99
            };
        }

        protected virtual string GetDefaultScriptsBasePath()
        {
            // Default to Beep.OilandGas.Models/Scripts
            var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
            
            if (assemblyDirectory != null)
            {
                // Navigate from DataManager to Models/Scripts
                var workspaceRoot = Path.GetFullPath(Path.Combine(assemblyDirectory, "..", "..", "..", "..", ".."));
                var scriptsPath = Path.Combine(workspaceRoot, "Beep.OilandGas.Models", "Scripts");
                
                if (Directory.Exists(scriptsPath))
                {
                    return scriptsPath;
                }
            }

            // Fallback: try to find it relative to current directory
            var currentDir = Directory.GetCurrentDirectory();
            var modelsPath = Path.Combine(currentDir, "Beep.OilandGas.Models", "Scripts");
            if (Directory.Exists(modelsPath))
            {
                return modelsPath;
            }

            // Last resort: use environment variable or default
            return Environment.GetEnvironmentVariable("BEEP_SCRIPTS_PATH") 
                ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Beep.OilandGas", "Scripts");
        }
    }
}
