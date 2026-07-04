using Beep.OilandGas.PPDM39.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.ApiService.Services
{
    /// <summary>
    /// Runs all PPDM IModuleSetup seeders after the BeepDM setup wizard
    /// completes schema creation. Each domain module (Exploration, Development,
    /// Production, Accounting, etc.) is seeded in dependency order via its
    /// ModuleSetupBase.SeedAsync().
    ///
    /// Designed to be called programmatically after AppBootstrap.OnBootstrapComplete.
    /// </summary>
    public class PpdmModuleSeedingService
    {
        private readonly IEnumerable<IModuleSetup> _modules;
        private readonly IDMEEditor _editor;
        private readonly string _connectionName;
        private readonly ILogger<PpdmModuleSeedingService> _logger;

        public PpdmModuleSeedingService(
            IEnumerable<IModuleSetup> modules,
            IDMEEditor editor,
            string connectionName,
            ILogger<PpdmModuleSeedingService> logger)
        {
            _modules = modules.OrderBy(m => m.Order).ThenBy(m => m.ModuleId).ToList();
            _editor = editor;
            _connectionName = connectionName;
            _logger = logger;
        }

        public IReadOnlyList<IModuleSetup> Modules => _modules.ToList();

        /// <summary>
        /// Seeds all PPDM modules sequentially. Each module's SeedAsync() is called
        /// with idempotent skip-if-exists logic.
        /// </summary>
        /// <param name="userId">Audit user ID (default: "SYSTEM").</param>
        /// <param name="progress">Optional progress reporter.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Per-module results with success/failure/row counts.</returns>
        public async Task<PpdmSeedingReport> SeedAllAsync(
            string userId = "SYSTEM",
            IProgress<(int done, int total, string message)>? progress = null,
            CancellationToken token = default)
        {
            var report = new PpdmSeedingReport();
            var moduleList = _modules.ToList();

            _logger.LogInformation("Starting PPDM module seeding: {Count} modules for {Connection}",
                moduleList.Count, _connectionName);

            for (int i = 0; i < moduleList.Count; i++)
            {
                var module = moduleList[i];
                token.ThrowIfCancellationRequested();

                progress?.Report((i + 1, moduleList.Count, $"Seeding {module.ModuleName}..."));

                try
                {
                    var result = await module.SeedAsync(_connectionName, userId, token);

                    report.Results.Add(new PpdmModuleResult
                    {
                        ModuleId = module.ModuleId,
                        ModuleName = module.ModuleName,
                        Success = result.Success,
                        RecordsInserted = result.RecordsInserted,
                        TablesSeeded = result.TablesSeeded,
                        Errors = result.Errors,
                        Skipped = result.SkipReason != null,
                        SkipReason = result.SkipReason
                    });

                    if (result.Success)
                    {
                        report.Succeeded++;
                        report.TotalRowsInserted += result.RecordsInserted;
                        _logger.LogInformation("Module {Id}: {Rows} rows, {Tables} tables",
                            module.ModuleId, result.RecordsInserted, result.TablesSeeded);
                    }
                    else
                    {
                        report.Failed++;
                        _logger.LogWarning("Module {Id} reported failures: {Errors}",
                            module.ModuleId, string.Join("; ", result.Errors));
                    }
                }
                catch (ModuleSetupAbortException ex)
                {
                    report.Failed++;
                    report.Aborted = true;
                    report.AbortModuleId = module.ModuleId;
                    _logger.LogError(ex, "Module {Id} ABORTED", module.ModuleId);
                    report.Results.Add(new PpdmModuleResult
                    {
                        ModuleId = module.ModuleId,
                        ModuleName = module.ModuleName,
                        Success = false,
                        Errors = new List<string> { ex.Message }
                    });
                    break;
                }
                catch (Exception ex)
                {
                    report.Failed++;
                    _logger.LogError(ex, "Module {Id} failed", module.ModuleId);
                    report.Results.Add(new PpdmModuleResult
                    {
                        ModuleId = module.ModuleId,
                        ModuleName = module.ModuleName,
                        Success = false,
                        Errors = new List<string> { ex.Message }
                    });
                }
            }

            report.TotalModules = moduleList.Count;
            report.Completed = !report.Aborted;

            _logger.LogInformation("PPDM seeding: {Succeeded}/{Total} succeeded, {Rows} rows, aborted={Aborted}",
                report.Succeeded, report.TotalModules, report.TotalRowsInserted, report.Aborted);

            return report;
        }
    }

    public class PpdmSeedingReport
    {
        public int TotalModules { get; set; }
        public int Succeeded { get; set; }
        public int Failed { get; set; }
        public int TotalRowsInserted { get; set; }
        public bool Aborted { get; set; }
        public bool Completed { get; set; }
        public string? AbortModuleId { get; set; }
        public List<PpdmModuleResult> Results { get; set; } = new();
    }

    public class PpdmModuleResult
    {
        public string ModuleId { get; set; } = string.Empty;
        public string ModuleName { get; set; } = string.Empty;
        public bool Success { get; set; }
        public int RecordsInserted { get; set; }
        public int TablesSeeded { get; set; }
        public List<string> Errors { get; set; } = new();
        public bool Skipped { get; set; }
        public string? SkipReason { get; set; }
    }
}
