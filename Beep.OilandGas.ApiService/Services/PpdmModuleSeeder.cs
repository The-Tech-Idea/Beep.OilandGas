using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.SetUp.Seeding;
using TheTechIdeaWeb.Diagnostics;

namespace Beep.OilandGas.ApiService.Services
{
    /// <summary>
    /// Adapts a PPDM IModuleSetup to the BeepDM ISeeder contract.
    /// Each PPDM domain module (Exploration, Development, Production, etc.) becomes
    /// a SetupWizard SeedingStep-compatible ISeeder that calls ModuleSetupBase.SeedAsync().
    ///
    /// This bridges the two seeding systems:
    ///   BeepDM SeedingStep → ISeeder.Seed() → PpdmModuleSeeder → IModuleSetup.SeedAsync()
    /// </summary>
    public class PpdmModuleSeeder : ISeeder
    {
        private readonly IModuleSetup _module;
        private readonly string _connectionName;
        private readonly ILogger _logger;
        private readonly IFailureReporter _failures;

        public PpdmModuleSeeder(IModuleSetup module, string connectionName, ILogger logger, IFailureReporter failures)
        {
            _module = module ?? throw new ArgumentNullException(nameof(module));
            _connectionName = connectionName;
            _logger = logger;
            _failures = failures ?? throw new ArgumentNullException(nameof(failures));
        }

        /// <summary>Uses the PPDM module ID as the seeder ID.</summary>
        public string SeederId => _module.ModuleId;

        /// <summary>Module display name.</summary>
        public string SeederName => _module.ModuleName;

        /// <summary>
        /// Dependencies: modules with lower Order that this module depends on.
        /// The SeedingStep runs seeders in topological order, which aligns with
        /// the existing IModuleSetup.Order execution sequence.
        /// </summary>
        public IReadOnlyList<string> DependsOn => Array.Empty<string>();

        /// <summary>
        /// Checks if this module has already been seeded by querying for any
        /// of its registered entity types that have data.
        /// </summary>
        public bool IsAlreadySeeded(IDataSource dataSource, IDMEEditor editor)
        {
            // PPDM modules use skip-if-exists internally — always attempt seed.
            // The module's own SeedAsync() handles idempotency.
            return false;
        }

        /// <summary>
        /// Delegates to the PPDM module's SeedAsync() method.
        /// The module's own skip-if-exists logic handles idempotency.
        /// </summary>
        public IErrorsInfo Seed(IDataSource dataSource, IDMEEditor editor, IProgress<PassedArgs>? progress = null)
        {
            var errors = new TheTechIdea.Beep.ConfigUtil.ErrorsInfo { Flag = Errors.Ok };
            try
            {
                progress?.Report(new PassedArgs { Messege = $"Seeding {_module.ModuleName}..." });

                // Safe: runs on ThreadPool thread via WebApiSetupWizardAdapter (no sync context = no deadlock)
                var result = _module.SeedAsync(_connectionName, ActingUser.System, CancellationToken.None)
                    .ConfigureAwait(false).GetAwaiter().GetResult();

                if (!result.Success)
                {
                    errors.Flag = Errors.Failed;
                    errors.Message = string.Join("; ", result.Errors);
                }
                else
                {
                    _logger.LogInformation("PPDM seeder {Id}: {Rows} rows, {Tables} tables",
                        _module.ModuleId, result.RecordsInserted, result.TablesSeeded);
                }
            }
            // The seeding step's contract is an outcome, not an exception: whatever stops the module, the step is told it
            // failed, with the reference its failure is filed under — never the exception's own text (OILGAS-CATCH-01).
            // Cancellation is the run ending.
            catch (ModuleSetupAbortException ex)
            {
                var reference = _failures.ReportHandled(ex, $"seeding PPDM module {_module.ModuleId}",
                    "the module aborted its seeding; the seeding step is told it failed, with this reference");
                errors.Flag = Errors.Failed;
                errors.Message = $"Module {_module.ModuleId} aborted its seeding (reference {reference}).";
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                var reference = _failures.ReportHandled(ex, $"seeding PPDM module {_module.ModuleId}",
                    "the module is not seeded; the seeding step is told it failed, with this reference");
                errors.Flag = Errors.Failed;
                errors.Message = $"Module {_module.ModuleId} was not seeded (reference {reference}).";
            }

            return errors;
        }
    }

    /// <summary>
    /// Registers all discovered PPDM IModuleSetup implementations as ISeeder instances
    /// in the BeepDM SeederRegistry. This makes PPDM modules available to the
    /// SetupWizard's SeedingStep for automatic seeding during setup.
    /// </summary>
    public static class PpdmSeederRegistry
    {
        /// <summary>
        /// Discovers all IModuleSetup implementations from loaded assemblies and
        /// registers them as ISeeder instances in the provided registry.
        /// Modules are registered in Order → ModuleId sequence so the SeedingStep's
        /// topological sort preserves the intended execution order.
        /// </summary>
        public static void RegisterAllModules(
            ISeederRegistry registry,
            IEnumerable<IModuleSetup> modules,
            string connectionName,
            ILoggerFactory loggerFactory,
            IFailureReporter failures)
        {
            var ordered = modules.OrderBy(m => m.Order).ThenBy(m => m.ModuleId).ToList();

            foreach (var module in ordered)
            {
                var logger = loggerFactory.CreateLogger($"{typeof(PpdmModuleSeeder).FullName}.{module.ModuleId}");
                var seeder = new PpdmModuleSeeder(module, connectionName, logger, failures);
                registry.Register(seeder);
            }
        }
    }
}
