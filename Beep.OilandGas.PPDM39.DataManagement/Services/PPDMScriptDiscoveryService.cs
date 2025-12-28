using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.DataManagement.Core.Models.DatabaseCreation;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.PPDM39.DataManagement.Services
{
    /// <summary>
    /// Service for discovering and organizing database scripts
    /// </summary>
    public class PPDMScriptDiscoveryService
    {
        private readonly ILogger<PPDMScriptDiscoveryService>? _logger;
        private Dictionary<string, Dictionary<string, string>>? _tableToModuleMap;
        private Dictionary<string, string>? _moduleToSubjectAreaMap;

        public PPDMScriptDiscoveryService(ILogger<PPDMScriptDiscoveryService>? logger = null)
        {
            _logger = logger;
        }

        /// <summary>
        /// Discovers all scripts for a given database type from multiple locations
        /// </summary>
        public async Task<List<ScriptInfo>> DiscoverScriptsAsync(string databaseType, string scriptsBasePath)
        {
            try
            {
                var scripts = new List<ScriptInfo>();
                var scriptsPath = Path.Combine(scriptsBasePath, databaseType);

                // Load module mappings
                await LoadModuleMappingsAsync(scriptsBasePath);

                // Discover scripts from primary location (PPDM39/Scripts)
                if (Directory.Exists(scriptsPath))
                {
                    var sqlFiles = Directory.GetFiles(scriptsPath, "*.sql", SearchOption.TopDirectoryOnly);
                    foreach (var filePath in sqlFiles)
                    {
                        var scriptInfo = await AnalyzeScriptFileAsync(filePath, scriptsPath, databaseType);
                        if (scriptInfo != null)
                        {
                            scripts.Add(scriptInfo);
                        }
                    }
                }

                // Discover scripts from Models/Scripts location
                var modelsScriptsPath = Path.Combine(scriptsBasePath, "..", "..", "Beep.OilandGas.Models", "Scripts", databaseType);
                modelsScriptsPath = Path.GetFullPath(modelsScriptsPath); // Resolve relative path
                
                if (Directory.Exists(modelsScriptsPath))
                {
                    var sqlFiles = Directory.GetFiles(modelsScriptsPath, "*.sql", SearchOption.TopDirectoryOnly);
                    foreach (var filePath in sqlFiles)
                    {
                        var scriptInfo = await AnalyzeScriptFileAsync(filePath, modelsScriptsPath, databaseType);
                        if (scriptInfo != null)
                        {
                            // Check if we already have this script from primary location
                            var existing = scripts.FirstOrDefault(s => 
                                s.FileName.Equals(scriptInfo.FileName, StringComparison.OrdinalIgnoreCase) &&
                                s.TableName == scriptInfo.TableName &&
                                s.ScriptType == scriptInfo.ScriptType);
                            
                            if (existing == null)
                            {
                                scripts.Add(scriptInfo);
                            }
                        }
                    }
                }

                // Sort by execution order
                scripts = scripts.OrderBy(s => s.ExecutionOrder).ThenBy(s => s.FileName).ToList();

                _logger?.LogInformation($"Discovered {scripts.Count} scripts for {databaseType} from multiple locations");
                return scripts;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error discovering scripts for {databaseType}");
                throw;
            }
        }

        /// <summary>
        /// Analyzes a script file to extract metadata
        /// </summary>
        private async Task<ScriptInfo?> AnalyzeScriptFileAsync(string filePath, string scriptsPath, string databaseType)
        {
            try
            {
                var fileName = Path.GetFileName(filePath);
                var fileInfo = new FileInfo(filePath);

                // Skip non-SQL files and certain files
                if (fileName.Equals("install.bat", StringComparison.OrdinalIgnoreCase) ||
                    fileName.Equals("install.sh", StringComparison.OrdinalIgnoreCase) ||
                    fileName.Equals("README.md", StringComparison.OrdinalIgnoreCase) ||
                    fileName.Contains("changes.txt", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                var scriptInfo = new ScriptInfo
                {
                    FileName = fileName,
                    FullPath = filePath,
                    RelativePath = Path.GetRelativePath(scriptsPath, filePath),
                    FileSize = fileInfo.Length,
                    LastModified = fileInfo.LastWriteTime
                };

                // Determine script type
                scriptInfo.ScriptType = DetermineScriptType(fileName);
                scriptInfo.IsConsolidated = IsConsolidatedScript(fileName);
                scriptInfo.IsMandatory = IsMandatoryScript(scriptInfo.ScriptType);
                scriptInfo.IsOptional = IsOptionalScript(scriptInfo.ScriptType);
                scriptInfo.ExecutionOrder = GetExecutionOrder(scriptInfo.ScriptType);

                // Extract table name if individual script
                if (!scriptInfo.IsConsolidated)
                {
                    scriptInfo.TableName = ExtractTableName(fileName);
                    if (!string.IsNullOrEmpty(scriptInfo.TableName))
                    {
                        // Map to module
                        scriptInfo.Module = GetModuleForTable(scriptInfo.TableName);
                        scriptInfo.SubjectArea = GetSubjectAreaForModule(scriptInfo.Module);
                    }
                }

                return await Task.FromResult(scriptInfo);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error analyzing script file: {filePath}");
                return null;
            }
        }

        /// <summary>
        /// Determines the script type from filename
        /// </summary>
        private ScriptType DetermineScriptType(string fileName)
        {
            var upperFileName = fileName.ToUpperInvariant();

            if (upperFileName == "TAB.SQL") return ScriptType.TAB;
            if (upperFileName == "PK.SQL") return ScriptType.PK;
            if (upperFileName == "CK.SQL") return ScriptType.CK;
            if (upperFileName == "FK.SQL") return ScriptType.FK;
            if (upperFileName == "OUOM.SQL") return ScriptType.OUOM;
            if (upperFileName == "UOM.SQL") return ScriptType.UOM;
            if (upperFileName == "RQUAL.SQL") return ScriptType.RQUAL;
            if (upperFileName == "RSRC.SQL") return ScriptType.RSRC;
            if (upperFileName == "TCM.SQL") return ScriptType.TCM;
            if (upperFileName == "CCM.SQL") return ScriptType.CCM;
            if (upperFileName == "SYN.SQL") return ScriptType.SYN;
            if (upperFileName == "GUID.SQL") return ScriptType.GUID;

            // Individual table scripts
            if (upperFileName.EndsWith("_TAB.SQL")) return ScriptType.TAB;
            if (upperFileName.EndsWith("_PK.SQL")) return ScriptType.PK;
            if (upperFileName.EndsWith("_FK.SQL")) return ScriptType.FK;
            if (upperFileName.EndsWith("_CK.SQL")) return ScriptType.CK;
            if (upperFileName.EndsWith("_IX.SQL")) return ScriptType.IX;

            return ScriptType.Other;
        }

        /// <summary>
        /// Checks if script is a consolidated script (TAB.sql, PK.sql, etc.)
        /// </summary>
        private bool IsConsolidatedScript(string fileName)
        {
            var consolidatedScripts = new[] { "TAB.sql", "PK.sql", "CK.sql", "FK.sql", "OUOM.sql", 
                "UOM.sql", "RQUAL.sql", "RSRC.sql", "TCM.sql", "CCM.sql", "SYN.sql", "GUID.sql" };
            
            return consolidatedScripts.Any(s => fileName.Equals(s, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Checks if script is mandatory
        /// </summary>
        private bool IsMandatoryScript(ScriptType scriptType)
        {
            return scriptType switch
            {
                ScriptType.TAB => true,
                ScriptType.PK => true,
                ScriptType.CK => true,
                ScriptType.FK => true,
                ScriptType.OUOM => true,
                ScriptType.UOM => true,
                ScriptType.RQUAL => true,
                ScriptType.RSRC => true,
                _ => false
            };
        }

        /// <summary>
        /// Checks if script is optional
        /// </summary>
        private bool IsOptionalScript(ScriptType scriptType)
        {
            return scriptType switch
            {
                ScriptType.TCM => true,
                ScriptType.CCM => true,
                ScriptType.SYN => true,
                ScriptType.GUID => true,
                _ => false
            };
        }

        /// <summary>
        /// Gets execution order for script type
        /// </summary>
        private int GetExecutionOrder(ScriptType scriptType)
        {
            return scriptType switch
            {
                ScriptType.TAB => 1,
                ScriptType.PK => 2,
                ScriptType.CK => 3,
                ScriptType.FK => 4,
                ScriptType.OUOM => 5,
                ScriptType.UOM => 6,
                ScriptType.RQUAL => 7,
                ScriptType.RSRC => 8,
                ScriptType.TCM => 9,
                ScriptType.CCM => 10,
                ScriptType.SYN => 11,
                ScriptType.GUID => 12,
                ScriptType.IX => 13,
                _ => 99
            };
        }

        /// <summary>
        /// Extracts table name from individual script filename
        /// </summary>
        private string? ExtractTableName(string fileName)
        {
            // Pattern: TABLE_NAME_TYPE.sql (e.g., FIELD_PHASE_TAB.sql)
            var parts = fileName.Replace(".sql", "", StringComparison.OrdinalIgnoreCase).Split('_');
            
            if (parts.Length < 2) return null;

            // Remove the last part (TAB, PK, FK, etc.)
            var typeSuffixes = new[] { "TAB", "PK", "FK", "CK", "IX" };
            if (typeSuffixes.Contains(parts[^1], StringComparer.OrdinalIgnoreCase))
            {
                return string.Join("_", parts.Take(parts.Length - 1));
            }

            return null;
        }

        /// <summary>
        /// Loads module mappings from JSON files
        /// </summary>
        private async Task LoadModuleMappingsAsync(string scriptsBasePath)
        {
            try
            {
                // Try multiple paths to find metadata
                var possiblePaths = new List<string>
                {
                    Path.Combine(scriptsBasePath, "..", "..", "Beep.OilandGas.PPDM39.DataManagement", "Core", "Metadata"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Core", "Metadata"),
                    Path.Combine(Directory.GetCurrentDirectory(), "Core", "Metadata"),
                    Path.GetFullPath(Path.Combine(scriptsBasePath, "..", "..", "..", "Beep.OilandGas.PPDM39.DataManagement", "Core", "Metadata"))
                };

                string? metadataPath = null;
                foreach (var path in possiblePaths)
                {
                    var fullPath = Path.GetFullPath(path);
                    if (Directory.Exists(fullPath))
                    {
                        metadataPath = fullPath;
                        break;
                    }
                }

                if (metadataPath == null)
                {
                    _logger?.LogWarning("Metadata directory not found, continuing without module information");
                    return;
                }
                
                // Load table to module mapping
                var tableMappingPath = Path.Combine(metadataPath, "PPDM39TableToModuleMapping.json");
                if (File.Exists(tableMappingPath))
                {
                    var json = await File.ReadAllTextAsync(tableMappingPath);
                    var mapping = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, List<string>>>>(json);
                    
                    _tableToModuleMap = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
                    
                    if (mapping != null)
                    {
                        foreach (var subjectArea in mapping)
                        {
                            foreach (var module in subjectArea.Value)
                            {
                                foreach (var table in module.Value)
                                {
                                    if (!_tableToModuleMap.ContainsKey(table))
                                    {
                                        _tableToModuleMap[table] = new Dictionary<string, string>
                                        {
                                            ["Module"] = module.Key,
                                            ["SubjectArea"] = subjectArea.Key
                                        };
                                    }
                                }
                            }
                        }
                    }
                }

                // Load subject areas and modules
                var subjectAreaPath = Path.Combine(metadataPath, "PPDM39SubjectAreasAndModules.json");
                if (File.Exists(subjectAreaPath))
                {
                    var json = await File.ReadAllTextAsync(subjectAreaPath);
                    var subjectAreas = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, List<Dictionary<string, string>>>>>(json);
                    
                    _moduleToSubjectAreaMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    
                    if (subjectAreas != null)
                    {
                        foreach (var subjectArea in subjectAreas)
                        {
                            if (subjectArea.Value.ContainsKey("Modules"))
                            {
                                foreach (var module in subjectArea.Value["Modules"])
                                {
                                    if (module.ContainsKey("Name"))
                                    {
                                        _moduleToSubjectAreaMap[module["Name"]] = subjectArea.Key;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error loading module mappings, continuing without module information");
            }
        }

        /// <summary>
        /// Gets module for a table name
        /// </summary>
        private string? GetModuleForTable(string tableName)
        {
            if (_tableToModuleMap?.TryGetValue(tableName, out var moduleInfo) == true)
            {
                return moduleInfo.GetValueOrDefault("Module");
            }
            return null;
        }

        /// <summary>
        /// Gets subject area for a module
        /// </summary>
        private string? GetSubjectAreaForModule(string? module)
        {
            if (string.IsNullOrEmpty(module)) return null;

            if (_tableToModuleMap != null)
            {
                var moduleEntry = _tableToModuleMap.Values.FirstOrDefault(m => 
                    m.GetValueOrDefault("Module")?.Equals(module, StringComparison.OrdinalIgnoreCase) == true);
                if (moduleEntry != null)
                {
                    return moduleEntry.GetValueOrDefault("SubjectArea");
                }
            }

            if (_moduleToSubjectAreaMap?.TryGetValue(module, out var subjectArea) == true)
            {
                return subjectArea;
            }

            return null;
        }
    }
}

