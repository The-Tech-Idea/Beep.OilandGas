using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.DataManagement;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.SeedData;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.ApiService.Controllers.PPDM39
{
    /// <summary>
    /// Schema module management — install per-module PPDM39 tables and track progress.
    /// </summary>
    [ApiController]
    [Route("api/ppdm39/schema")]
    public class PPDM39SchemaController : ControllerBase
    {
        private readonly IDMEEditor _editor;
        private readonly SchemaInstallationTracker _tracker;
        private readonly PPDMReferenceDataSeeder? _referenceDataSeeder;
        private readonly EnumReferenceDataSeeder? _enumSeeder;
        private readonly ILogger<PPDM39SchemaController> _logger;

        private static readonly Assembly _ppdmAssembly =
            typeof(Beep.OilandGas.PPDM39.Models.WELL).Assembly;

        private const string PpdmModelsNamespace = "Beep.OilandGas.PPDM39.Models";

        public PPDM39SchemaController(
            IDMEEditor editor,
            SchemaInstallationTracker tracker,
            ILogger<PPDM39SchemaController> logger,
            PPDMReferenceDataSeeder? referenceDataSeeder = null,
            EnumReferenceDataSeeder? enumSeeder = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _tracker = tracker ?? throw new ArgumentNullException(nameof(tracker));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _referenceDataSeeder = referenceDataSeeder;
            _enumSeeder = enumSeeder;
        }

        // ── Module Definitions ────────────────────────────────────────────────

        /// <summary>GET /api/ppdm39/schema/modules — all module definitions with table counts.</summary>
        [HttpGet("modules")]
        public ActionResult<List<SchemaModuleDto>> GetModules()
        {
            try
            {
                var allTypes = GetPpdmEntityTypes();
                var defs = _tracker.GetModuleDefinitions();
                var result = defs.Select(def =>
                {
                    var tables = GetTablesForModule(def, allTypes);
                    return new SchemaModuleDto
                    {
                        ModuleName  = def.ModuleName,
                        DisplayName = def.DisplayName,
                        Description = def.Description,
                        Icon        = def.Icon,
                        DisplayOrder = def.DisplayOrder,
                        TotalTables = tables.Count
                    };
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting schema modules");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        // ── Per-Connection Status ─────────────────────────────────────────────

        /// <summary>GET /api/ppdm39/schema/{connectionName}/status — installation status for each module.</summary>
        [HttpGet("{connectionName}/status")]
        public async Task<ActionResult<List<ModuleStatusDto>>> GetStatusAsync(string connectionName)
        {
            if (string.IsNullOrWhiteSpace(connectionName)) return BadRequest(new { error = "Connection name is required." });
            try
            {
                var allTypes = GetPpdmEntityTypes();
                var defs = _tracker.GetModuleDefinitions();
                var records = await _tracker.GetModuleStatusAsync(connectionName);

                var result = defs.Select(def =>
                {
                    var tables = GetTablesForModule(def, allTypes);
                    var rec = records.FirstOrDefault(r =>
                        r.ModuleName.Equals(def.ModuleName, StringComparison.OrdinalIgnoreCase));

                    return new ModuleStatusDto
                    {
                        ModuleName   = def.ModuleName,
                        DisplayName  = def.DisplayName,
                        Description  = def.Description,
                        Icon         = def.Icon,
                        DisplayOrder = def.DisplayOrder,
                        Status       = rec?.Status.ToString() ?? ModuleInstallStatus.NotInstalled.ToString(),
                        TotalTables  = tables.Count,
                        InstalledTables = rec?.InstalledTables ?? 0,
                        LastUpdated  = rec?.LastUpdated
                    };
                }).OrderBy(m => m.DisplayOrder).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting schema status for {Connection}", connectionName);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        // ── Module Installation ───────────────────────────────────────────────

        /// <summary>
        /// POST /api/ppdm39/schema/{connectionName}/install/{moduleName}
        /// Creates tables for the specified module using MigrationManager.
        /// </summary>
        [HttpPost("{connectionName}/install/{moduleName}")]
        public async Task<ActionResult<ModuleInstallResultDto>> InstallModuleAsync(
            string connectionName, string moduleName)
        {
                if (string.IsNullOrWhiteSpace(connectionName)) return BadRequest(new { error = "Connection name is required." });
                if (string.IsNullOrWhiteSpace(moduleName)) return BadRequest(new { error = "Module name is required." });
            try
            {
                var def = _tracker.GetModuleDefinition(moduleName);
                if (def == null)
                    return BadRequest(new ModuleInstallResultDto
                    {
                        Success = false,
                        Message = $"Unknown module '{moduleName}'"
                    });

                var ds = _editor.GetDataSource(connectionName);
                if (ds == null)
                    return NotFound(new ModuleInstallResultDto
                    {
                        Success = false,
                        Message = $"Connection '{connectionName}' not found. Create the database first."
                    });

                var state = ds.Openconnection();
                if (state != System.Data.ConnectionState.Open)
                    return StatusCode(503, new ModuleInstallResultDto
                    {
                        Success = false,
                        Message = $"Could not open connection to '{connectionName}'"
                    });

                // Get entity types for this module
                var allTypes = GetPpdmEntityTypes();
                var moduleTypes = GetTablesForModule(def, allTypes);

                if (!moduleTypes.Any())
                {
                    return Ok(new ModuleInstallResultDto
                    {
                        Success = true,
                        ModuleName = moduleName,
                        Message = $"No entity types found for module '{def.DisplayName}'",
                        TablesAttempted = 0,
                        TablesCreated = 0
                    });
                }

                _logger.LogInformation(
                    "Installing module '{Module}' on '{Connection}': {Count} entity types",
                    moduleName, connectionName, moduleTypes.Count);

                var migration = new TheTechIdea.Beep.Editor.Migration.MigrationManager(_editor, ds);
                migration.RegisterAssembly(_ppdmAssembly);

                var plan = migration.BuildMigrationPlanForTypes(moduleTypes, detectRelationships: true);

                int tablesCreated = 0;
                bool overallSuccess = true;
                string? errorMessage = null;

                if (plan != null)
                {
                    var execResult = migration.ExecuteMigrationPlan(plan);
                    overallSuccess = execResult.Success;
                    errorMessage = execResult.Success ? null : execResult.Message;
                    tablesCreated = plan.Operations?
                        .Count(o => o.Kind == TheTechIdea.Beep.Editor.Migration.MigrationPlanOperationKind.CreateEntity) ?? 0;
                }
                else
                {
                    overallSuccess = false;
                    errorMessage = "Migration plan could not be built for this module";
                }

                // Track each table
                foreach (var type in moduleTypes)
                {
                    await _tracker.MarkTableInstalledAsync(
                        connectionName, moduleName, type.Name, "TAB",
                        overallSuccess, overallSuccess ? null : errorMessage);
                }

                if (overallSuccess)
                    await _tracker.MarkModuleCompleteAsync(connectionName, moduleName);

                ds.Closeconnection();

                return Ok(new ModuleInstallResultDto
                {
                    Success = overallSuccess,
                    ModuleName = moduleName,
                    Message = overallSuccess
                        ? $"Module '{def.DisplayName}' installed successfully ({tablesCreated} tables created)"
                        : $"Module '{def.DisplayName}' installation failed: {errorMessage}",
                    TablesAttempted = moduleTypes.Count,
                    TablesCreated = tablesCreated,
                    ErrorDetails = errorMessage
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error installing module '{Module}' on '{Connection}'", moduleName, connectionName);
                return StatusCode(500, new ModuleInstallResultDto
                {
                    Success = false,
                    ModuleName = moduleName,
                    Message = $"Installation failed: See server logs for details.",
                    ErrorDetails = "An internal error occurred."
                });
            }
        }

        /// <summary>
        /// POST /api/ppdm39/schema/{connectionName}/install-all — install all modules.
        /// </summary>
        [HttpPost("{connectionName}/install-all")]
        public async Task<ActionResult<AllModulesInstallResultDto>> InstallAllModulesAsync(string connectionName)
        {
              if (string.IsNullOrWhiteSpace(connectionName)) return BadRequest(new { error = "Connection name is required." });
            try
            {
                var ds = _editor.GetDataSource(connectionName);
                if (ds == null)
                    return NotFound(new AllModulesInstallResultDto
                    {
                        Success = false,
                        Message = $"Connection '{connectionName}' not found."
                    });

                var state = ds.Openconnection();
                if (state != System.Data.ConnectionState.Open)
                    return StatusCode(503, new AllModulesInstallResultDto
                    {
                        Success = false,
                        Message = $"Could not open connection to '{connectionName}'"
                    });

                _logger.LogInformation("Installing ALL modules on '{Connection}'", connectionName);

                var migration = new TheTechIdea.Beep.Editor.Migration.MigrationManager(_editor, ds);
                migration.RegisterAssembly(_ppdmAssembly);

                var plan = migration.BuildMigrationPlan(
                    namespaceName: PpdmModelsNamespace,
                    assembly: _ppdmAssembly,
                    detectRelationships: true);

                bool overallSuccess = false;
                int tablesCreated = 0;
                string? errorMessage = null;

                if (plan != null)
                {
                    var execResult = migration.ExecuteMigrationPlan(plan);
                    overallSuccess = execResult.Success;
                    errorMessage = execResult.Success ? null : execResult.Message;
                    tablesCreated = plan.Operations?
                        .Count(o => o.Kind == TheTechIdea.Beep.Editor.Migration.MigrationPlanOperationKind.CreateEntity) ?? 0;
                }
                else
                {
                    errorMessage = "Migration plan could not be built";
                }

                // Mark all modules based on overall result
                var allTypes = GetPpdmEntityTypes();
                foreach (var def in _tracker.GetModuleDefinitions())
                {
                    var moduleTypes = GetTablesForModule(def, allTypes);
                    foreach (var type in moduleTypes)
                    {
                        await _tracker.MarkTableInstalledAsync(
                            connectionName, def.ModuleName, type.Name, "TAB",
                            overallSuccess, overallSuccess ? null : errorMessage);
                    }
                    if (overallSuccess)
                        await _tracker.MarkModuleCompleteAsync(connectionName, def.ModuleName);
                }

                ds.Closeconnection();

                return Ok(new AllModulesInstallResultDto
                {
                    Success = overallSuccess,
                    Message = overallSuccess
                        ? $"All modules installed ({tablesCreated} tables created)"
                        : $"Installation completed with errors: {errorMessage}",
                    TablesCreated = tablesCreated,
                    ErrorDetails = errorMessage
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error installing all modules on '{Connection}'", connectionName);
                return StatusCode(500, new AllModulesInstallResultDto
                {
                    Success = false,
                    Message = $"Installation failed: See server logs for details.",
                    ErrorDetails = "An internal error occurred."
                });
            }
        }

        // ── Reset Tracking ────────────────────────────────────────────────────

        /// <summary>DELETE /api/ppdm39/schema/{connectionName}/module/{moduleName}/reset</summary>
        [HttpDelete("{connectionName}/module/{moduleName}/reset")]
        public async Task<IActionResult> ResetModuleTrackingAsync(string connectionName, string moduleName)
        {
            if (string.IsNullOrWhiteSpace(connectionName)) return BadRequest(new { error = "Connection name is required." });
            if (string.IsNullOrWhiteSpace(moduleName)) return BadRequest(new { error = "Module name is required." });
            try
            {
                await _tracker.ResetModuleAsync(connectionName, moduleName);
                return Ok(new { success = true, message = $"Tracking reset for module '{moduleName}'" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting module tracking");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        // ── Reference Data Seeding ────────────────────────────────────────────

        /// <summary>
        /// POST /api/ppdm39/schema/{connectionName}/seed/reference
        /// Seeds all R_* and RA_* reference tables from templates and enums.
        /// </summary>
        [HttpPost("{connectionName}/seed/reference")]
        public async Task<ActionResult<SeedResultDto>> SeedReferenceDataAsync(
            string connectionName, [FromBody] SeedReferenceRequest? request)
        {
            if (string.IsNullOrWhiteSpace(connectionName)) return BadRequest(new { error = "Connection name is required." });
            try
            {
                int totalSeeded = 0;
                var errors = new List<string>();

                if (_referenceDataSeeder != null)
                {
                    try
                    {
                        var result = await _referenceDataSeeder.SeedPPDMReferenceTablesAsync(
                            connectionName,
                            tableNames: null,         // all tables
                            skipExisting: request?.SkipExisting ?? true,
                            userId: request?.UserId ?? "SYSTEM");

                        if (result.Success)
                            totalSeeded += result.RecordsInserted;
                        else
                            errors.Add(result.Message ?? "Reference data seeder returned failure");
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"PPDMReferenceDataSeeder: An error occurred.");
                        _logger.LogWarning(ex, "Reference data seeder failed for {Connection}", connectionName);
                    }
                }

                if (_enumSeeder != null)
                {
                    try
                    {
                        var enumCount = await _enumSeeder.SeedAllEnumsAsync(request?.UserId ?? "SYSTEM");
                        totalSeeded += enumCount;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"EnumReferenceDataSeeder: An error occurred.");
                        _logger.LogWarning(ex, "Enum seeder failed for {Connection}", connectionName);
                    }
                }

                return Ok(new SeedResultDto
                {
                    Success = !errors.Any(),
                    TotalSeeded = totalSeeded,
                    Message = errors.Any()
                        ? $"Seeding completed with {errors.Count} error(s). {totalSeeded} records seeded."
                        : $"Reference data seeded successfully. {totalSeeded} records.",
                    Errors = errors
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding reference data for {Connection}", connectionName);
                return StatusCode(500, new SeedResultDto
                {
                    Success = false,
                    Message = $"Seeding failed: See server logs for details.",
                    Errors = new List<string> { "An internal error occurred." }
                });
            }
        }

        // ── Available Connections ─────────────────────────────────────────────

        /// <summary>GET /api/ppdm39/schema/connections — all configured connections.</summary>
        [HttpGet("connections")]
        public ActionResult<List<ConnectionSummaryDto>> GetConnections()
        {
            try
            {
                var connections = _editor.ConfigEditor?.DataConnections?
                    .Select(c => new ConnectionSummaryDto
                    {
                        ConnectionName = c.ConnectionName,
                        DatabaseType = c.DatabaseType.ToString(),
                        Host = c.Host,
                        Database = c.Database
                    })
                    .ToList() ?? new List<ConnectionSummaryDto>();

                return Ok(connections);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting connections");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        // ── Private Helpers ───────────────────────────────────────────────────

        private static List<Type> GetPpdmEntityTypes()
        {
            return _ppdmAssembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract &&
                            t.Namespace != null &&
                            t.Namespace.StartsWith(PpdmModelsNamespace, StringComparison.Ordinal))
                .ToList();
        }

        private static List<Type> GetTablesForModule(SchemaModuleDefinition def, List<Type> allTypes)
        {
            return allTypes.Where(t =>
                def.TablePrefixes.Any(prefix =>
                    t.Name.StartsWith(prefix.Replace("_", "").ToUpperInvariant(), StringComparison.OrdinalIgnoreCase) ||
                    t.Name.StartsWith(prefix.TrimEnd('_'), StringComparison.OrdinalIgnoreCase) ||
                    MatchesPrefix(t.Name, prefix)))
            .ToList();
        }

        private static bool MatchesPrefix(string typeName, string prefix)
        {
            // Strip trailing wildcard marker
            var p = prefix.TrimEnd('_', '*');
            return typeName.StartsWith(p, StringComparison.OrdinalIgnoreCase);
        }
    }

    // ── DTOs ──────────────────────────────────────────────────────────────────

    public class SchemaModuleDto
    {
        public string ModuleName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public int TotalTables { get; set; }
    }

    public class ModuleStatusDto : SchemaModuleDto
    {
        public string Status { get; set; } = "NotInstalled";
        public int InstalledTables { get; set; }
        public DateTime? LastUpdated { get; set; }
    }

    public class ModuleInstallResultDto
    {
        public bool Success { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int TablesAttempted { get; set; }
        public int TablesCreated { get; set; }
        public string? ErrorDetails { get; set; }
    }

    public class AllModulesInstallResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TablesCreated { get; set; }
        public string? ErrorDetails { get; set; }
    }

    public class SeedReferenceRequest
    {
        public bool SkipExisting { get; set; } = true;
        public string? UserId { get; set; }
    }

    public class SeedResultDto
    {
        public bool Success { get; set; }
        public int TotalSeeded { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
    }

    public class ConnectionSummaryDto
    {
        public string? ConnectionName { get; set; }
        public string? DatabaseType { get; set; }
        public string? Host { get; set; }
        public string? Database { get; set; }
    }
}
