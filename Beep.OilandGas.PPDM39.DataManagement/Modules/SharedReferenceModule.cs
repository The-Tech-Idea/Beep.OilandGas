using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.DataManagement.SeedData;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.PPDM39.DataManagement.Modules
{
    /// <summary>
    /// Module order 10 — seeds cross-domain shared reference tables.
    /// This module stays in PPDM39.DataManagement because the enum-to-reference import
    /// pipeline is shared infrastructure consumed across multiple domain projects.
    /// </summary>
    public sealed class SharedReferenceModule : ModuleSetupBase
    {
        private static readonly IReadOnlyList<Type> _entityTypes = new List<Type>
        {
            typeof(R_FLUID_TYPE),
            typeof(R_SEVERITY),
            typeof(PPDM_UNIT_OF_MEASURE),
            typeof(R_WELL_CLASS),
            typeof(R_WELL_PROFILE_TYPE),
            typeof(R_COMPLETION_STATUS),
            typeof(R_COMPLETION_TYPE),
            typeof(R_PRODUCTION_METHOD),
            typeof(R_ALLOCATION_TYPE),
            typeof(R_WELL_QUALIFIC_TYPE),
            typeof(R_WELL_COMPONENT_TYPE),
            typeof(R_PLATFORM_TYPE),
            typeof(R_LOCATION_TYPE),
            typeof(R_AREA_TYPE),
            typeof(R_WELL_ACTIVITY_CAUSE),
            typeof(R_FIN_STATUS),
            typeof(R_BA_PERMIT_TYPE),
            typeof(R_LICENSE_STATUS),
            typeof(R_CONDITION_TYPE),
            typeof(R_DIRECTION),
            typeof(PROJECT_STEP),
            typeof(R_WELL_TEST_TYPE),
            typeof(R_TEST_EQUIPMENT),
            typeof(R_TEST_RESULT),
            typeof(R_CAT_EQUIP_TYPE),
            typeof(R_CAT_EQUIP_SPEC),
            typeof(R_PROJECT_STATUS),
            typeof(R_PROJECT_TYPE)
        };

        private readonly LOVManagementService _lovService;

        public SharedReferenceModule(ModuleSetupContext context, LOVManagementService lovService)
            : base(context)
        {
            _lovService = lovService ?? throw new ArgumentNullException(nameof(lovService));
        }

        public override string ModuleId => "R_SHARED_REFERENCES";
        public override string ModuleName => "Shared Reference Tables (Enums)";
        public override int Order => 10;
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
                var enumSeeder = new EnumReferenceDataSeeder(
                    _ctx.Editor,
                    _ctx.CommonColumnHandler,
                    _ctx.Defaults,
                    _ctx.Metadata,
                    _lovService,
                    connectionName);

                int seeded = await enumSeeder.SeedAllEnumsAsync(userId);
                result.Success = true;
                result.RecordsInserted = seeded;
                result.TablesSeeded = _entityTypes.Count;
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