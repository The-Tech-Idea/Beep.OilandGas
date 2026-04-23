using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.PPDM39.DataManagement.Modules
{
    /// <summary>
    /// Module order 30 — declares well-specific reference entity types.
    ///
    /// Seeding for these references is currently handled by SharedReferenceModule
    /// to preserve existing behavior; this module exists so well references have
    /// a dedicated ownership point for future extraction.
    /// </summary>
    public sealed class WellReferenceModule : ModuleSetupBase
    {
        private static readonly IReadOnlyList<Type> _entityTypes = new List<Type>
        {
            typeof(R_WELL_CLASS),
            typeof(R_WELL_PROFILE_TYPE),
            typeof(R_WELL_XREF_TYPE)
        };

        public WellReferenceModule(ModuleSetupContext context) : base(context) { }

        public override string ModuleId => "WELL_REFERENCES";
        public override string ModuleName => "Well Reference Tables";
        public override int Order => 30;
        public override IReadOnlyList<Type> EntityTypes => _entityTypes;

        public override Task<ModuleSetupResult> SeedAsync(
            string connectionName,
            string userId,
            CancellationToken cancellationToken = default)
        {
            var result = NewResult();
            result.Success = true;
            result.SkipReason = "Well reference enum seeding is currently delegated to SharedReferenceModule.";
            return Task.FromResult(result);
        }
    }
}
