using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.PPDM39.DataManagement.Services
{
    // ── DTOs ──────────────────────────────────────────────────────────────────

    public enum ModuleInstallStatus
    {
        NotInstalled,
        Partial,
        Complete,
        Error
    }

    public class TableInstallationRecord
    {
        public string TableName { get; set; } = string.Empty;
        public string ScriptType { get; set; } = string.Empty;   // TAB, PK, FK
        public bool Installed { get; set; }
        public DateTime? InstalledAt { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class ModuleInstallationRecord
    {
        public string ModuleName { get; set; } = string.Empty;
        public string ConnectionName { get; set; } = string.Empty;
        public ModuleInstallStatus Status { get; set; } = ModuleInstallStatus.NotInstalled;
        public DateTime? LastUpdated { get; set; }
        public List<TableInstallationRecord> Tables { get; set; } = new();

        [JsonIgnore]
        public int TotalTables => Tables.Select(t => t.TableName).Distinct().Count();

        [JsonIgnore]
        public int InstalledTables => Tables.Where(t => t.ScriptType == "TAB" && t.Installed)
                                             .Select(t => t.TableName).Distinct().Count();
    }

    public class SchemaInstallationState
    {
        public string ConnectionName { get; set; } = string.Empty;
        public DateTime LastSaved { get; set; } = DateTime.UtcNow;
        public List<ModuleInstallationRecord> Modules { get; set; } = new();
    }

    // ── PPDM Module Definitions ───────────────────────────────────────────────

    public class SchemaModuleDefinition
    {
        public string ModuleName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public List<string> TablePrefixes { get; set; } = new();
        public int DisplayOrder { get; set; }
    }

    // ── SERVICE ───────────────────────────────────────────────────────────────

    public class SchemaInstallationTracker
    {
        private readonly string _storageFolder;
        private readonly ILogger<SchemaInstallationTracker> _logger;
        private static readonly JsonSerializerOptions _json = new() { WriteIndented = true };

        // Static module definitions — the canonical list of PPDM module groupings
        private static readonly List<SchemaModuleDefinition> _moduleDefinitions = new()
        {
            new SchemaModuleDefinition
            {
                ModuleName = "WELL",
                DisplayName = "Well Model",
                Description = "Well headers, bores, tubulars, tests, and status tables",
                Icon = "OilBarrel",
                DisplayOrder = 1,
                TablePrefixes = new() { "WELL", "WELLBORE", "WELL_TEST", "WELL_LOG", "WELL_TUBULAR",
                                        "WELL_ACTIVITY", "WELL_EQUIPMENT", "WELL_XREF", "WELL_STATUS",
                                        "WELL_CORE", "WELL_DRLG_MUD", "WELL_DRL_STRING", "WELL_HISTORY" }
            },
            new SchemaModuleDefinition
            {
                ModuleName = "BUSINESS_ASSOCIATE",
                DisplayName = "Business Associate",
                Description = "Business partners, operators, regulators, and contact data",
                Icon = "Business",
                DisplayOrder = 2,
                TablePrefixes = new() { "BUSINESS_ASSOCIATE", "BA_", "BA_ADDR", "BA_ALIAS",
                                        "BA_ORGANIZATION", "BA_REGULATORY" }
            },
            new SchemaModuleDefinition
            {
                ModuleName = "REFERENCE",
                DisplayName = "Reference Values (R_ / RA_)",
                Description = "All R_ and RA_ reference / lookup tables for the PPDM schema",
                Icon = "LibraryBooks",
                DisplayOrder = 3,
                TablePrefixes = new() { "R_", "RA_", "LIST_OF_VALUE" }
            },
            new SchemaModuleDefinition
            {
                ModuleName = "PRODUCTION",
                DisplayName = "Production & Reserves",
                Description = "Production volumes, reserves, pools, and density tables",
                Icon = "LocalGasStation",
                DisplayOrder = 4,
                TablePrefixes = new() { "PDEN", "POOL", "FIELD", "PROD_", "PRDCT" }
            },
            new SchemaModuleDefinition
            {
                ModuleName = "EXPLORATION",
                DisplayName = "Exploration",
                Description = "Prospects, seismic surveys, plays, and geological data",
                Icon = "Explore",
                DisplayOrder = 5,
                TablePrefixes = new() { "PROSPECT", "SEIS_", "PLAY", "STRAT_UNIT", "ZONE" }
            },
            new SchemaModuleDefinition
            {
                ModuleName = "FACILITY",
                DisplayName = "Facilities",
                Description = "Surface facilities, pipelines, compressors, and equipment",
                Icon = "Factory",
                DisplayOrder = 6,
                TablePrefixes = new() { "FACILITY", "PIPE_", "COMPRESSOR", "PUMP_", "TANK_",
                                        "SEPARATOR", "TREATER" }
            },
            new SchemaModuleDefinition
            {
                ModuleName = "AREA",
                DisplayName = "Areas & Locations",
                Description = "Geographic areas, land parcels, and spatial reference data",
                Icon = "Map",
                DisplayOrder = 7,
                TablePrefixes = new() { "AREA", "LAND_", "STRAT_", "COORD_", "SURVEY_",
                                        "GRID_", "SPATIAL_" }
            },
            new SchemaModuleDefinition
            {
                ModuleName = "SUPPORT",
                DisplayName = "Support & Contracts",
                Description = "Work orders, contracts, projects, and equipment maintenance",
                Icon = "Settings",
                DisplayOrder = 8,
                TablePrefixes = new() { "EQUIP_", "CONTRACT", "PROJECT", "WORK_ORDER",
                                        "TASK_", "ACTIVITY", "COST_" }
            }
        };

        public SchemaInstallationTracker(
            string storageFolder,
            ILogger<SchemaInstallationTracker> logger)
        {
            _storageFolder = storageFolder ?? AppContext.BaseDirectory;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Directory.CreateDirectory(_storageFolder);
        }

        // ── Module Definitions ────────────────────────────────────────────────

        public IReadOnlyList<SchemaModuleDefinition> GetModuleDefinitions()
            => _moduleDefinitions.OrderBy(m => m.DisplayOrder).ToList();

        public SchemaModuleDefinition? GetModuleDefinition(string moduleName)
            => _moduleDefinitions.FirstOrDefault(m =>
                m.ModuleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase));

        // ── State Persistence ─────────────────────────────────────────────────

        private string GetStateFilePath(string connectionName)
        {
            var safe = string.Concat(connectionName.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
            return Path.Combine(_storageFolder, $"schema-install-{safe}.json");
        }

        public async Task<SchemaInstallationState> LoadStateAsync(string connectionName)
        {
            var path = GetStateFilePath(connectionName);
            if (!File.Exists(path))
                return new SchemaInstallationState { ConnectionName = connectionName };

            try
            {
                await using var fs = File.OpenRead(path);
                var state = await JsonSerializer.DeserializeAsync<SchemaInstallationState>(fs, _json);
                return state ?? new SchemaInstallationState { ConnectionName = connectionName };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not load schema installation state for {Connection}", connectionName);
                return new SchemaInstallationState { ConnectionName = connectionName };
            }
        }

        public async Task SaveStateAsync(SchemaInstallationState state)
        {
            state.LastSaved = DateTime.UtcNow;
            var path = GetStateFilePath(state.ConnectionName);
            try
            {
                var json = JsonSerializer.Serialize(state, _json);
                await File.WriteAllTextAsync(path, json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not save schema installation state for {Connection}", state.ConnectionName);
            }
        }

        // ── Module Status Queries ─────────────────────────────────────────────

        public async Task<List<ModuleInstallationRecord>> GetModuleStatusAsync(string connectionName)
        {
            var state = await LoadStateAsync(connectionName);
            var result = new List<ModuleInstallationRecord>();

            foreach (var def in GetModuleDefinitions())
            {
                var existing = state.Modules.FirstOrDefault(m =>
                    m.ModuleName.Equals(def.ModuleName, StringComparison.OrdinalIgnoreCase));

                result.Add(existing ?? new ModuleInstallationRecord
                {
                    ModuleName = def.ModuleName,
                    ConnectionName = connectionName,
                    Status = ModuleInstallStatus.NotInstalled
                });
            }

            return result;
        }

        public async Task<ModuleInstallationRecord?> GetModuleStatusAsync(string connectionName, string moduleName)
        {
            var state = await LoadStateAsync(connectionName);
            return state.Modules.FirstOrDefault(m =>
                m.ModuleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase));
        }

        // ── Mark Tables / Modules ─────────────────────────────────────────────

        public async Task MarkTableInstalledAsync(
            string connectionName,
            string moduleName,
            string tableName,
            string scriptType,
            bool success,
            string? errorMessage = null)
        {
            var state = await LoadStateAsync(connectionName);

            var module = state.Modules.FirstOrDefault(m =>
                m.ModuleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase));

            if (module == null)
            {
                module = new ModuleInstallationRecord
                {
                    ModuleName = moduleName,
                    ConnectionName = connectionName
                };
                state.Modules.Add(module);
            }

            // Update or add the record for this table+scriptType
            var rec = module.Tables.FirstOrDefault(t =>
                t.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase) &&
                t.ScriptType.Equals(scriptType, StringComparison.OrdinalIgnoreCase));

            if (rec == null)
            {
                rec = new TableInstallationRecord { TableName = tableName, ScriptType = scriptType };
                module.Tables.Add(rec);
            }

            rec.Installed = success;
            rec.InstalledAt = success ? DateTime.UtcNow : null;
            rec.ErrorMessage = errorMessage;

            // Recalculate module status
            module.LastUpdated = DateTime.UtcNow;
            var tabRecords = module.Tables.Where(t => t.ScriptType == "TAB").ToList();
            if (!tabRecords.Any())
                module.Status = ModuleInstallStatus.NotInstalled;
            else if (tabRecords.All(t => t.Installed))
                module.Status = ModuleInstallStatus.Complete;
            else if (tabRecords.Any(t => t.Installed))
                module.Status = ModuleInstallStatus.Partial;
            else
                module.Status = tabRecords.Any(t => t.ErrorMessage != null)
                    ? ModuleInstallStatus.Error
                    : ModuleInstallStatus.NotInstalled;

            await SaveStateAsync(state);
        }

        public async Task MarkModuleCompleteAsync(string connectionName, string moduleName)
        {
            var state = await LoadStateAsync(connectionName);
            var module = state.Modules.FirstOrDefault(m =>
                m.ModuleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase));

            if (module != null)
            {
                module.Status = ModuleInstallStatus.Complete;
                module.LastUpdated = DateTime.UtcNow;
                await SaveStateAsync(state);
            }
        }

        public async Task ResetModuleAsync(string connectionName, string moduleName)
        {
            var state = await LoadStateAsync(connectionName);
            var module = state.Modules.FirstOrDefault(m =>
                m.ModuleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase));

            if (module != null)
            {
                state.Modules.Remove(module);
                await SaveStateAsync(state);
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>Returns the module name for a given table name, or null if unrecognised.</summary>
        public string? ResolveModule(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName)) return null;
            var upper = tableName.ToUpperInvariant();
            foreach (var def in _moduleDefinitions.OrderBy(d => d.DisplayOrder))
            {
                foreach (var prefix in def.TablePrefixes)
                {
                    if (upper.StartsWith(prefix.ToUpperInvariant(), StringComparison.Ordinal))
                        return def.ModuleName;
                }
            }
            return null;
        }
    }
}
