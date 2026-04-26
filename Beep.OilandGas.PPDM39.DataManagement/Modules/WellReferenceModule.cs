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
    /// This module stays in PPDM39.DataManagement because these are shared reference entities
    /// and their seeding boundary belongs with the central reference-data infrastructure.
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