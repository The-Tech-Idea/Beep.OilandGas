using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Beep.OilandGas.PPDM39.DataManagement.SeedData;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.PPDM39.DataManagement.Modules
{
    /// <summary>
    /// Module order 20 — seeds the six PPDM 3.9 well-status reference tables.
    /// Delegates to <see cref="WellStatusFacetSeeder"/> which already implements
    /// idempotent, ordered seeding for this table set.
    /// </summary>
    public sealed class WellStatusFacetModule : ModuleSetupBase
    {
        private static readonly IReadOnlyList<Type> _entityTypes = new List<Type>
        {
            typeof(R_WELL_STATUS_TYPE),
            typeof(R_WELL_STATUS),
            typeof(R_WELL_STATUS_QUAL),
            typeof(R_WELL_STATUS_QUAL_VALUE),
            typeof(RA_WELL_STATUS_TYPE),
            typeof(RA_WELL_STATUS)
        };

        private readonly WellStatusFacetSeeder _seeder;

        public WellStatusFacetModule(ModuleSetupContext context) : base(context)
        {
            _seeder = new WellStatusFacetSeeder(
                context.Editor,
                context.CommonColumnHandler,
                context.Defaults,
                context.Metadata,
                context.ConnectionName);
        }

        public override string ModuleId   => "WELL_STATUS_FACETS";
        public override string ModuleName => "Well Status Facets (PPDM 3.9)";
        public override int    Order      => 20;
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
                var facetResult = await _seeder.SeedAllAsync(userId);
                result.Success         = facetResult.Success;
                result.RecordsInserted = facetResult.FacetTypeRows
                                       + facetResult.FacetValueRows
                                       + facetResult.FacetQualifierRows
                                       + facetResult.FacetQualValueRows
                                       + facetResult.RaFacetTypeRows
                                       + facetResult.RaFacetValueRows;
                result.TablesSeeded    = 6;

                if (!facetResult.Success)
                    result.Errors.Add(facetResult.Message ?? "WellStatusFacetSeeder reported failure");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                result.Success = false;
                result.Errors.Add(ex.Message);
            }

            if (result.Errors.Count == 0)
                result.Success = true;

            return result;
        }
    }
}
