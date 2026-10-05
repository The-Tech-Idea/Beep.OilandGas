using Beep.OilandGas.PPDM39.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.ModuleSetup;
using Beep.OilandGas.PPDM39.DataManagement.SeedData;

namespace Beep.OilandGas.HSE.Modules
{
    /// <summary>
    /// Module order 80 — declares HSE entity types for schema migration
    /// and owns HSE-specific setup orchestration for the HSE project.
    /// Shared reference-data import infrastructure remains in PPDM39.DataManagement.
    /// </summary>
    public sealed class HseModule : ModuleSetupBase
    {
        // EntityTypes intentionally empty: no project-owned persisted table classes exist yet in HSE.
        // All Data classes are projections/contracts, not persisted schema types.
        // Register local table types here when HSE-specific schema classes are added.
        private static readonly IReadOnlyList<Type> _entityTypes = new List<Type>();

        private readonly PPDMReferenceDataSeeder _referenceSeeder;

        public HseModule(ModuleSetupContext context, PPDMReferenceDataSeeder referenceSeeder)
            : base(context)
        {
            _referenceSeeder = referenceSeeder ?? throw new ArgumentNullException(nameof(referenceSeeder));
        }

        public override string ModuleId => "HSE";
        public override string ModuleName => "Health, Safety & Environment";
        public override int Order => 80;
        public override IReadOnlyList<Type> EntityTypes => _entityTypes;

        public override async Task<ModuleSetupResult> SeedAsync(
            string connectionName,
            string userId,
            CancellationToken cancellationToken = default)
        {
            var result = NewResult();
            cancellationToken.ThrowIfCancellationRequested();

            // A failure of the seeder is not caught here: it reaches the module orchestrator, which records this module
            // as failed and goes on with the others, or the API's handler, which reports it and answers with its
            // reference. Copying the exception's text into the result put a fault's words in front of the operator.
            var seed = await _referenceSeeder.SeedIndustryStandardsDataAsync(
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

            return result;
        }
    }
}
