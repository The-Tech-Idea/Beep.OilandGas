using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Beep.OilandGas.PPDM39.DataManagement.SeedData;

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
