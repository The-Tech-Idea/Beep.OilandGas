using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup
{
    /// <summary>
    /// Resolves all <see cref="IModuleSetup"/> registrations from DI, sorts them
    /// by <see cref="IModuleSetup.Order"/>, and runs schema collection or seeding
    /// across every module with per-module error isolation.
    ///
    /// Only a <see cref="ModuleSetupAbortException"/> stops the entire run.
    /// All other exceptions are captured in <see cref="ModuleSetupResult.Errors"/>
    /// and the orchestrator continues to the next module.
    /// </summary>
    public sealed class ModuleSetupOrchestrator
    {
        private readonly IReadOnlyList<IModuleSetup> _modules;
        private readonly ILogger<ModuleSetupOrchestrator> _logger;

        public ModuleSetupOrchestrator(
            IEnumerable<IModuleSetup> modules,
            ILogger<ModuleSetupOrchestrator> logger)
        {
            _modules = (modules ?? throw new ArgumentNullException(nameof(modules)))
                .OrderBy(m => m.Order)
                .ThenBy(m => m.ModuleId)
                .ToList();

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ── Public surface ────────────────────────────────────────────────────

        /// <summary>
        /// Returns the full, ordered entity type manifest across all modules.
        /// Pass the result to the migration manager to build the schema DDL plan.
        /// </summary>
        public IReadOnlyList<Type> GetAllEntityTypes()
        {
            var seen = new HashSet<Type>();
            var result = new List<Type>();

            foreach (var module in _modules)
            {
                foreach (var t in module.EntityTypes)
                {
                    if (seen.Add(t))
                        result.Add(t);
                }
            }

            return result;
        }

        /// <summary>
        /// Runs <see cref="IModuleSetup.SeedAsync"/> for every registered module in order.
        /// Each module is isolated; an exception from one module does not prevent the others
        /// from running unless it is a <see cref="ModuleSetupAbortException"/>.
        /// </summary>
        public async Task<OrchestratorSetupResult> RunSeedAsync(
            string connectionName,
            string userId,
            CancellationToken cancellationToken = default)
        {
            var aggregate = new OrchestratorSetupResult();

            _logger.LogInformation(
                "ModuleSetupOrchestrator: starting seed run for {Count} modules on connection {Conn}",
                _modules.Count, connectionName);

            foreach (var module in _modules)
            {
                cancellationToken.ThrowIfCancellationRequested();

                _logger.LogDebug(
                    "Running module [{Order}] {ModuleId} ({ModuleName})",
                    module.Order, module.ModuleId, module.ModuleName);

                ModuleSetupResult moduleResult;
                try
                {
                    moduleResult = await module.SeedAsync(connectionName, userId, cancellationToken);
                }
                catch (ModuleSetupAbortException abort)
                {
                    // Unrecoverable — stop everything and re-throw.
                    _logger.LogError(abort,
                        "ModuleSetupOrchestrator: abort signal from module {ModuleId}",
                        abort.ModuleId);
                    throw;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "ModuleSetupOrchestrator: unhandled exception in module {ModuleId} — continuing",
                        module.ModuleId);

                    moduleResult = new ModuleSetupResult
                    {
                        ModuleId   = module.ModuleId,
                        ModuleName = module.ModuleName,
                        Success    = false,
                        Errors     = new List<string> { ex.Message }
                    };
                }

                aggregate.ModuleResults.Add(moduleResult);
                aggregate.ModulesRun++;

                if (moduleResult.Success)
                    aggregate.ModulesSucceeded++;

                aggregate.TotalRecordsInserted += moduleResult.RecordsInserted;

                if (moduleResult.Errors.Count > 0)
                {
                    _logger.LogWarning(
                        "Module {ModuleId} completed with {ErrorCount} error(s): {FirstError}",
                        moduleResult.ModuleId,
                        moduleResult.Errors.Count,
                        moduleResult.Errors[0]);
                }
                else
                {
                    _logger.LogInformation(
                        "Module {ModuleId}: {Records} records inserted across {Tables} tables",
                        moduleResult.ModuleId,
                        moduleResult.RecordsInserted,
                        moduleResult.TablesSeeded);
                }
            }

            aggregate.AllSucceeded = aggregate.ModulesSucceeded == aggregate.ModulesRun;

            _logger.LogInformation(
                "ModuleSetupOrchestrator: seed run complete — {Succeeded}/{Run} modules succeeded, " +
                "{Records} total records inserted",
                aggregate.ModulesSucceeded, aggregate.ModulesRun, aggregate.TotalRecordsInserted);

            return aggregate;
        }

        /// <summary>
        /// Runs seed for only the specified module IDs, in their declared Order.
        /// Modules not in the list are skipped.
        /// </summary>
        public async Task<OrchestratorSetupResult> RunSeedForModulesAsync(
            IReadOnlyList<string> selectedModuleIds,
            string connectionName,
            string userId,
            CancellationToken cancellationToken = default)
        {
            if (selectedModuleIds == null || selectedModuleIds.Count == 0)
            {
                _logger.LogWarning("RunSeedForModulesAsync: empty module list — running all modules instead");
                return await RunSeedAsync(connectionName, userId, cancellationToken);
            }

            var selectedSet = new HashSet<string>(selectedModuleIds, StringComparer.OrdinalIgnoreCase);
            var selectedModules = _modules
                .Where(m => selectedSet.Contains(m.ModuleId))
                .ToList();

            if (selectedModules.Count == 0)
            {
                _logger.LogWarning(
                    "RunSeedForModulesAsync: no matching modules found for {Count} requested IDs",
                    selectedModuleIds.Count);

                return new OrchestratorSetupResult();
            }

            _logger.LogInformation(
                "ModuleSetupOrchestrator: starting selective seed for {Count} of {Total} modules on connection {Conn}",
                selectedModules.Count, _modules.Count, connectionName);

            var aggregate = new OrchestratorSetupResult();

            foreach (var module in selectedModules)
            {
                cancellationToken.ThrowIfCancellationRequested();

                _logger.LogDebug(
                    "Running selected module [{Order}] {ModuleId} ({ModuleName})",
                    module.Order, module.ModuleId, module.ModuleName);

                ModuleSetupResult moduleResult;
                try
                {
                    moduleResult = await module.SeedAsync(connectionName, userId, cancellationToken);
                }
                catch (ModuleSetupAbortException abort)
                {
                    _logger.LogError(abort,
                        "ModuleSetupOrchestrator: abort signal from module {ModuleId}",
                        abort.ModuleId);
                    throw;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "ModuleSetupOrchestrator: unhandled exception in module {ModuleId} — continuing",
                        module.ModuleId);

                    moduleResult = new ModuleSetupResult
                    {
                        ModuleId   = module.ModuleId,
                        ModuleName = module.ModuleName,
                        Success    = false,
                        Errors     = new List<string> { ex.Message }
                    };
                }

                aggregate.ModuleResults.Add(moduleResult);
                aggregate.ModulesRun++;

                if (moduleResult.Success)
                    aggregate.ModulesSucceeded++;

                aggregate.TotalRecordsInserted += moduleResult.RecordsInserted;

                if (moduleResult.Errors.Count > 0)
                {
                    _logger.LogWarning(
                        "Module {ModuleId} completed with {ErrorCount} error(s): {FirstError}",
                        moduleResult.ModuleId,
                        moduleResult.Errors.Count,
                        moduleResult.Errors[0]);
                }
                else
                {
                    _logger.LogInformation(
                        "Module {ModuleId}: {Records} records inserted across {Tables} tables",
                        moduleResult.ModuleId,
                        moduleResult.RecordsInserted,
                        moduleResult.TablesSeeded);
                }
            }

            aggregate.AllSucceeded = aggregate.ModulesSucceeded == aggregate.ModulesRun;

            _logger.LogInformation(
                "ModuleSetupOrchestrator: selective seed run complete — {Succeeded}/{Run} modules succeeded, " +
                "{Records} total records inserted",
                aggregate.ModulesSucceeded, aggregate.ModulesRun, aggregate.TotalRecordsInserted);

            return aggregate;
        }

        /// <summary>
        /// Returns metadata about all registered modules.
        /// </summary>
        public IReadOnlyList<(string ModuleId, string ModuleName, int Order, IReadOnlyList<Type> EntityTypes)> GetModuleMetadata()
        {
            return _modules
                .Select(m => (m.ModuleId, m.ModuleName, m.Order, m.EntityTypes))
                .ToList();
        }
    }
}
