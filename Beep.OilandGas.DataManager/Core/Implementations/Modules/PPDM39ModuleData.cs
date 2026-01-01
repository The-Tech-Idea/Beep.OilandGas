using Beep.OilandGas.DataManager.Core.Models;
using Beep.OilandGas.DataManager.Core.Utilities;
using Beep.OilandGas.PPDM39.DataManagement.Core.Models.DatabaseCreation;

namespace Beep.OilandGas.DataManager.Core.Implementations
{
    /// <summary>
    /// Module data for PPDM39 standard database scripts
    /// PPDM39 scripts are organized as consolidated scripts (TAB.sql, PK.sql, FK.sql, etc.)
    /// directly in Scripts/{DatabaseType}/ folders
    /// </summary>
    public class PPDM39ModuleData : FileSystemModuleData
    {
        public override string ModuleName => "PPDM39";
        public override string Description => "PPDM 3.9 standard database schema scripts";
        public override string ScriptBasePath => ""; // PPDM39 scripts are directly in database type folders
        public override int ExecutionOrder => 0; // Execute before all other modules
        public override bool IsRequired => true;

        public PPDM39ModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }

        public override IEnumerable<string> GetDependencies() => Enumerable.Empty<string>();

        protected override string GetDefaultScriptsBasePath()
        {
            // Default to Beep.OilandGas.PPDM39/Scripts
            var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
            
            if (assemblyDirectory != null)
            {
                // Navigate from DataManager to PPDM39/Scripts
                var workspaceRoot = Path.GetFullPath(Path.Combine(assemblyDirectory, "..", "..", "..", "..", ".."));
                var scriptsPath = Path.Combine(workspaceRoot, "Beep.OilandGas.PPDM39", "Scripts");
                
                if (Directory.Exists(scriptsPath))
                {
                    return scriptsPath;
                }
            }

            // Fallback: try to find it relative to current directory
            var currentDir = Directory.GetCurrentDirectory();
            var ppdm39Path = Path.Combine(currentDir, "Beep.OilandGas.PPDM39", "Scripts");
            if (Directory.Exists(ppdm39Path))
            {
                return ppdm39Path;
            }

            // Last resort: use environment variable or default
            return Environment.GetEnvironmentVariable("BEEP_PPDM39_SCRIPTS_PATH") 
                ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Beep.OilandGas", "PPDM39", "Scripts");
        }

        public override async Task<IEnumerable<ModuleScriptInfo>> GetScriptsAsync(string databaseType)
        {
            // Normalize database type to match folder names
            var normalizedDatabaseType = DatabaseTypeNormalizer.Normalize(databaseType);
            
            var scripts = new List<ModuleScriptInfo>();
            // PPDM39 scripts are directly in Scripts/{DatabaseType}/, not in subfolders
            var scriptsPath = Path.Combine(_scriptsBasePath, normalizedDatabaseType);

            if (!Directory.Exists(scriptsPath))
            {
                return scripts;
            }

            // Get all consolidated SQL files (TAB.sql, PK.sql, FK.sql, etc.)
            var sqlFiles = Directory.GetFiles(scriptsPath, "*.sql", SearchOption.TopDirectoryOnly)
                .Concat(Directory.GetFiles(scriptsPath, "*.SQL", SearchOption.TopDirectoryOnly))
                .Distinct();

            foreach (var filePath in sqlFiles)
            {
                var fileName = Path.GetFileName(filePath);
                var fileInfo = new FileInfo(filePath);

                // Determine script type
                var scriptType = DetermineScriptType(fileName);
                if (scriptType == ScriptType.Other)
                {
                    // Skip non-standard files like install scripts, README, etc.
                    var lowerFileName = fileName.ToLowerInvariant();
                    if (lowerFileName.Contains("install") || 
                        lowerFileName.Contains("readme") || 
                        lowerFileName.Contains("changes") ||
                        lowerFileName.Contains("process") ||
                        lowerFileName.Contains("terms"))
                    {
                        continue;
                    }
                }

                // PPDM39 scripts are always consolidated (contain multiple tables)
                var tableNames = await ExtractTableNamesFromConsolidatedScript(filePath);

                var scriptInfo = new ModuleScriptInfo
                {
                    FileName = fileName,
                    FullPath = filePath,
                    RelativePath = Path.GetRelativePath(Path.Combine(_scriptsBasePath, normalizedDatabaseType), filePath),
                    ScriptType = scriptType,
                    TableNames = tableNames,
                    IsConsolidated = true,
                    ExecutionOrder = GetExecutionOrder(scriptType),
                    IsRequired = scriptType != ScriptType.TCM && scriptType != ScriptType.CCM && 
                                 scriptType != ScriptType.SYN && scriptType != ScriptType.GUID,
                    IsOptional = scriptType == ScriptType.TCM || scriptType == ScriptType.CCM || 
                                scriptType == ScriptType.SYN || scriptType == ScriptType.GUID,
                    FileSize = fileInfo.Length,
                    LastModified = fileInfo.LastWriteTime
                };

                scripts.Add(scriptInfo);
            }

            return scripts.OrderBy(s => s.ExecutionOrder).ThenBy(s => s.FileName);
        }
    }
}
