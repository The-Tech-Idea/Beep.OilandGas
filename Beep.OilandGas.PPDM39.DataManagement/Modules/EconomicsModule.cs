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
    /// Module order 90 — declares Economics entity types for schema migration.
    /// </summary>
    public sealed class EconomicsModule : ModuleSetupBase
    {
        private static readonly IReadOnlyList<Type> _entityTypes = new List<Type>
        {
            typeof(CONTRACT)
        };

        public EconomicsModule(ModuleSetupContext context) : base(context) { }

        public override string ModuleId   => "ECONOMICS";
        public override string ModuleName => "Economics & Contracts";
        public override int    Order      => 90;
        public override IReadOnlyList<Type> EntityTypes => _entityTypes;

        public override Task<ModuleSetupResult> SeedAsync(
            string connectionName,
            string userId,
            CancellationToken cancellationToken = default)
        {
            var result = NewResult();
            result.Success    = true;
            result.SkipReason = "No reference seed data defined for Economics module.";
            return Task.FromResult(result);
        }
    }
}
