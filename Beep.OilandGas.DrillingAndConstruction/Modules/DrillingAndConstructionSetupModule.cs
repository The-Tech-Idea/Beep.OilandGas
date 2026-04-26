using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.DrillingAndConstruction;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Beep.OilandGas.PPDM39.DataManagement.SeedData;

namespace Beep.OilandGas.DrillingAndConstruction.Modules
{
    /// <summary>
    /// Module order 60 — declares Development (Well lifecycle) entity types
    /// and owns development-specific setup orchestration for the DrillingAndConstruction project.
    /// Shared reference-data import infrastructure remains in PPDM39.DataManagement.
    /// </summary>
    public sealed class DrillingAndConstructionSetupModule : ModuleSetupBase
    {
        private static readonly IReadOnlyList<Type> _entityTypes = new List<Type>
        {
            typeof(DRILLING_PROGRAM),
            typeof(DRILLING_DAILY_REPORT),
            typeof(DRILLING_ACTIVITY),
            typeof(DRILLING_FLUID),
            typeof(DRILLING_BIT),
            typeof(CASING_PROGRAM),
            typeof(CEMENT_JOB),
        };

        private readonly PPDMReferenceDataSeeder _referenceSeeder;

        public DrillingAndConstructionSetupModule(ModuleSetupContext context, PPDMReferenceDataSeeder referenceSeeder)
            : base(context)
        {
            _referenceSeeder = referenceSeeder ?? throw new ArgumentNullException(nameof(referenceSeeder));
        }

        // Legacy name retained for file compatibility; semantic ownership is Drilling & Construction execution.
        // Planning ownership belongs to DevelopmentPlanning/Modules/DevelopmentPlanningModule.
        public override string ModuleId => "DRILLING_EXECUTION";
        public override string ModuleName => "Drilling & Construction Execution";
        public override int Order => 60;
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
                var seed = await _referenceSeeder.SeedLifeCycleReferenceDataAsync(
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
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                result.Success = false;
                result.Errors.Add(ex.Message);
            }

            return result;
        }
    }
}
