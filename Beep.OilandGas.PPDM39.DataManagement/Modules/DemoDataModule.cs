using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Beep.OilandGas.PPDM39.DataManagement.SeedData;

namespace Beep.OilandGas.PPDM39.DataManagement.Modules
{
    /// <summary>
    /// Module order 100 — optional demo dataset seed.
    ///
    /// This module intentionally declares no entity types because it seeds data
    /// into tables declared by other modules.
    /// </summary>
    public sealed class DemoDataModule : ModuleSetupBase
    {
        private readonly PPDMDemoDataSeeder _demoSeeder;

        public DemoDataModule(ModuleSetupContext context, PPDMDemoDataSeeder demoSeeder)
            : base(context)
        {
            _demoSeeder = demoSeeder ?? throw new ArgumentNullException(nameof(demoSeeder));
        }

        public override string ModuleId => "DEMO_DATA";
        public override string ModuleName => "Demo Dataset";
        public override int Order => 100;
        public override IReadOnlyList<Type> EntityTypes => Array.Empty<Type>();

        public override async Task<ModuleSetupResult> SeedAsync(
            string connectionName,
            string userId,
            CancellationToken cancellationToken = default)
        {
            var result = NewResult();
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var demo = await _demoSeeder.SeedFullDemoDatasetAsync(userId);
                result.Success = demo.Success;
                result.TablesSeeded = demo.TablesSeeded;
                result.RecordsInserted = demo.RecordsInserted;

                if (!demo.Success && !string.IsNullOrWhiteSpace(demo.Message))
                    result.Errors.Add(demo.Message);
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
