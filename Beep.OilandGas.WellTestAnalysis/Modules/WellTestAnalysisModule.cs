using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.WellTestAnalysis;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.ModuleSetup;

namespace Beep.OilandGas.WellTestAnalysis.Modules;

/// <summary>
/// Registers the well test analysis entity types (WTA-01): the test data, its pressure-time points and the analysis result.
/// </summary>
/// <remarks>
/// The three extension tables existed in the models but only LifeCycle's calculation service referenced them, so no module
/// setup created them at provisioning time. The PPDM core <c>WELL_TEST</c> table itself is not registered here - it belongs
/// to the PPDM model. There is no reference set to seed: the analysis and method tokens are
/// <c>WellTestAnalysisWellKnown</c> constants, and <c>R_WELL_TEST_TYPE</c> is seeded by the PPDM data-management seeders.
/// </remarks>
public sealed class WellTestAnalysisModule : ModuleSetupBase
{
    private static readonly IReadOnlyList<Type> _entityTypes = new List<Type>
    {
        typeof(WELL_TEST_DATA),
        typeof(PRESSURE_TIME_POINT),
        typeof(WELL_TEST_ANALYSIS_RESULT),
    };

    public WellTestAnalysisModule(ModuleSetupContext context)
        : base(context)
    {
    }

    public override string ModuleId => "WELL_TEST_ANALYSIS";

    public override string ModuleName => "Well test analysis (pressure transient)";

    /// <summary>After compressor analysis (78), so the feature modules run in stage order.</summary>
    public override int Order => 80;

    public override IReadOnlyList<Type> EntityTypes => _entityTypes;

    public override Task<ModuleSetupResult> SeedAsync(
        string connectionName,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var result = NewResult();

        // No reference rows: R_WELL_TEST_TYPE is PPDM data-management seed data, not this module's.
        return Task.FromResult(result);
    }
}
