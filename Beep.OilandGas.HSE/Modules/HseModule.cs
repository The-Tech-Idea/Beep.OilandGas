using Beep.OilandGas.PPDM39.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.ModuleSetup;
using Beep.OilandGas.PPDM39.DataManagement.SeedData;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.HSE.Modules
{
    /// <summary>
    /// Module order 80 — declares HSE entity types for schema migration
    /// and owns HSE-specific setup orchestration for the HSE project.
    /// Shared reference-data import infrastructure remains in PPDM39.DataManagement.
    /// </summary>
    public sealed class HseModule : ModuleSetupBase
    {
        // The incident command binds to PPDM_CORE; its private intent is migrated by CorePpdmModule.
        //
        // HSE-01 and HSE-02 read these records: an incident with its detail (type and severity), its field/facility/
        // jurisdiction component and its status history; an obligation; and the PROJECT family that carries a HAZOP study
        // or a corrective action with its steps and statuses. They are declared here, in the module that owns HSE, so a
        // desktop document provisions the tables its own HSE screens read - the PPDM foundation declares none of its own.
        private static readonly IReadOnlyList<Type> _entityTypes =
        [
            typeof(HSE_INCIDENT),
            typeof(HSE_INCIDENT_DETAIL),
            typeof(HSE_INCIDENT_COMPONENT),
            // HSE-03: the cause chain, and the business associates (owner and verifier) on a corrective-action step.
            typeof(HSE_INCIDENT_CAUSE),
            typeof(PROJECT_STEP_BA),
            // HSE-04: the study's field context and the node-to-action link.
            typeof(PROJECT_COMPONENT),
            typeof(PROJECT_STEP_XREF),
            // HSE-05: the compliance register.
            typeof(OBLIGATION),
            typeof(OBLIGATION_COMPONENT),
            // HSE-06: the production entities and their recorded volume dispositions the emissions report is built from. The
            // shared factor catalog, the company's selection and the company environment are owned by SystemSetupModule.
            typeof(PDEN),
            typeof(PDEN_VOL_DISPOSITION),
            typeof(HSE_INCIDENT_RESPONSE),
            typeof(OBLIGATION),
            typeof(PROJECT),
            typeof(PROJECT_STEP),
            typeof(PROJECT_STATUS),
        ];

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
