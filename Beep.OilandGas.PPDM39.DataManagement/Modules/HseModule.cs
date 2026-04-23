using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;

namespace Beep.OilandGas.PPDM39.DataManagement.Modules
{
    /// <summary>
    /// Module order 80 — declares HSE entity types for schema migration.
    /// HSE reference enums are seeded by SharedReferenceModule (order 10).
    /// </summary>
    public sealed class HseModule : ModuleSetupBase
    {
        // HSE domain types will be added here as the HSE feature is built out.
        private static readonly IReadOnlyList<Type> _entityTypes = new List<Type>();

        public HseModule(ModuleSetupContext context) : base(context) { }

        public override string ModuleId   => "HSE";
        public override string ModuleName => "Health, Safety & Environment";
        public override int    Order      => 80;
        public override IReadOnlyList<Type> EntityTypes => _entityTypes;

        public override Task<ModuleSetupResult> SeedAsync(
            string connectionName,
            string userId,
            CancellationToken cancellationToken = default)
        {
            var result = NewResult();
            result.Success    = true;
            result.SkipReason = "HSE entity types to be declared as feature is built out.";
            return Task.FromResult(result);
        }
    }
}
