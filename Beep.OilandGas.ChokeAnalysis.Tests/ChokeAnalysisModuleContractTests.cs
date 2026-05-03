using Beep.OilandGas.ChokeAnalysis.Modules;
using Beep.OilandGas.Models.Data.ChokeAnalysis;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Models;
using Xunit;

namespace Beep.OilandGas.ChokeAnalysis.Tests;

public class ChokeAnalysisModuleContractTests
{
    [Fact]
    public void ChokeAnalysisModule_HasStableIdentityAndEntityList()
    {
        var ctx = new ModuleSetupContext();
        var module = new ChokeAnalysisModule(ctx);

        Assert.Equal("CHOKE_ANALYSIS", module.ModuleId);
        Assert.Equal(77, module.Order);
        Assert.Equal(4, module.EntityTypes.Count);
        Assert.Contains(typeof(R_CHOKE_ANALYSIS_REFERENCE_CODE), module.EntityTypes);
        Assert.Contains(typeof(CHOKE_FLOW_RESULT), module.EntityTypes);
    }
}
