using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.EconomicAnalysis.Constants;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Beep.OilandGas.PPDM39.DataManagement.SeedData;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.EconomicAnalysis.Modules
{
    /// <summary>
        /// Module order 90 — owns economics setup orchestration for the EconomicAnalysis project.
        /// EntityTypes is intentionally empty: all classes in Data/Projections are projection/response
        /// types, not persisted table classes.  If project-owned table classes are added in future,
        /// register them here.
    /// Shared reference-data import infrastructure remains in PPDM39.DataManagement.
    /// </summary>
    public sealed class EconomicsModule : ModuleSetupBase
    {
        private static readonly IReadOnlyList<Type> _entityTypes = new List<Type>
        {
            typeof(R_ECONOMIC_METRIC),
            typeof(R_ECONOMIC_SCENARIO),
            typeof(R_ECONOMIC_SCHEDULE)
        };

        private readonly PPDMReferenceDataSeeder _referenceSeeder;

        public EconomicsModule(ModuleSetupContext context, PPDMReferenceDataSeeder referenceSeeder)
            : base(context)
        {
            _referenceSeeder = referenceSeeder ?? throw new ArgumentNullException(nameof(referenceSeeder));
        }

        public override string ModuleId => "ECONOMICS";
        public override string ModuleName => "Economics & Contracts";
        public override int Order => 90;
        public override IReadOnlyList<Type> EntityTypes => _entityTypes;

        public override async Task<ModuleSetupResult> SeedAsync(
            string connectionName,
            string userId,
            CancellationToken cancellationToken = default)
        {
            var result = NewResult();
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var seed = await _referenceSeeder.SeedAccountingReferenceDataAsync(
                    connectionName,
                    tableNames: null,
                    skipExisting: true,
                    userId: userId);

                result.Success = seed.Success;
                result.TablesSeeded = seed.TablesSeeded;
                result.RecordsInserted = seed.RecordsInserted;

                if (!seed.Success && !string.IsNullOrWhiteSpace(seed.Message))
                    result.Errors.Add(seed.Message);

                if (seed.Errors != null)
                    result.Errors.AddRange(seed.Errors);

                await SeedEconomicReferenceDataAsync(connectionName, userId, result, cancellationToken);
                result.Success = result.Errors.Count == 0;
                if (result.Success && result.RecordsInserted == 0 && result.TablesSeeded == 0)
                    result.SkipReason = "Economic reference rows already seeded.";
                else if (result.Success && result.RecordsInserted == 0 && string.IsNullOrWhiteSpace(result.SkipReason))
                    result.SkipReason = "Seed completed with no additional economic inserts.";
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                result.Success = false;
                result.Errors.Add(ex.Message);
            }

            return result;
        }

        private async Task SeedEconomicReferenceDataAsync(
            string connectionName,
            string userId,
            ModuleSetupResult result,
            CancellationToken cancellationToken)
        {
            var metricRepo = GetRepo<R_ECONOMIC_METRIC>("R_ECONOMIC_METRIC", connectionName);
            var scenarioRepo = GetRepo<R_ECONOMIC_SCENARIO>("R_ECONOMIC_SCENARIO", connectionName);
            var scheduleRepo = GetRepo<R_ECONOMIC_SCHEDULE>("R_ECONOMIC_SCHEDULE", connectionName);

            foreach (var row in EconomicAnalysisReferenceCodeSeed.GetMetricRows())
            {
                cancellationToken.ThrowIfCancellationRequested();
                var existing = await metricRepo.GetAsync(new List<AppFilter>
                {
                    new AppFilter { FieldName = "ECONOMIC_METRIC", Operator = "=", FilterValue = row.Metric }
                });
                var hasExisting = false;
                foreach (var _ in existing) { hasExisting = true; break; }
                if (hasExisting) continue;

                var entity = new R_ECONOMIC_METRIC
                {
                    ECONOMIC_METRIC = row.Metric,
                    ABBREVIATION = row.Abbreviation,
                    LONG_NAME = row.LongName,
                    SHORT_NAME = row.ShortName
                };
                await TryInsertAsync(metricRepo, entity, userId, result, $"R_ECONOMIC_METRIC/{row.Metric}");
            }

            foreach (var row in EconomicAnalysisReferenceCodeSeed.GetScenarioRows())
            {
                cancellationToken.ThrowIfCancellationRequested();
                var existing = await scenarioRepo.GetAsync(new List<AppFilter>
                {
                    new AppFilter { FieldName = "ECONOMIC_SCENARIO", Operator = "=", FilterValue = row.Scenario }
                });
                var hasExisting = false;
                foreach (var _ in existing) { hasExisting = true; break; }
                if (hasExisting) continue;

                var entity = new R_ECONOMIC_SCENARIO
                {
                    ECONOMIC_SCENARIO = row.Scenario,
                    ABBREVIATION = row.Abbreviation,
                    LONG_NAME = row.LongName,
                    SHORT_NAME = row.ShortName,
                    ACTIVE_IND = row.ActiveInd
                };
                await TryInsertAsync(scenarioRepo, entity, userId, result, $"R_ECONOMIC_SCENARIO/{row.Scenario}");
            }

            foreach (var row in EconomicAnalysisReferenceCodeSeed.GetScheduleRows())
            {
                cancellationToken.ThrowIfCancellationRequested();
                var existing = await scheduleRepo.GetAsync(new List<AppFilter>
                {
                    new AppFilter { FieldName = "ECONOMIC_SCHEDULE", Operator = "=", FilterValue = row.Schedule }
                });
                var hasExisting = false;
                foreach (var _ in existing) { hasExisting = true; break; }
                if (hasExisting) continue;

                var entity = new R_ECONOMIC_SCHEDULE
                {
                    ECONOMIC_SCHEDULE = row.Schedule,
                    ABBREVIATION = row.Abbreviation,
                    LONG_NAME = row.LongName,
                    SHORT_NAME = row.ShortName,
                    ACTIVE_IND = row.ActiveInd
                };
                await TryInsertAsync(scheduleRepo, entity, userId, result, $"R_ECONOMIC_SCHEDULE/{row.Schedule}");
            }
        }
    }
}
