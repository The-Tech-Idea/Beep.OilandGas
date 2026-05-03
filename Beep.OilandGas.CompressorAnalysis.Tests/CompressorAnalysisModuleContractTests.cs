using Beep.OilandGas.CompressorAnalysis.Modules;
using Beep.OilandGas.CompressorAnalysis.Data;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Xunit;

namespace Beep.OilandGas.CompressorAnalysis.Tests;

public class CompressorAnalysisModuleContractTests
{
    [Fact]
    public void CompressorAnalysisModule_HasStableIdentityAndEntityList()
    {
        var ctx = new ModuleSetupContext();
        var module = new CompressorAnalysisModule(ctx);

        Assert.Equal("COMPRESSOR_ANALYSIS", module.ModuleId);
        Assert.Equal(78, module.Order);
        Assert.Equal(6, module.EntityTypes.Count);
        Assert.Contains(typeof(COMPRESSOR_OPERATING_CONDITIONS), module.EntityTypes);
        Assert.Contains(typeof(COMPRESSOR_POWER_RESULT), module.EntityTypes);
        Assert.Contains(typeof(COMPRESSOR_PRESSURE_RESULT), module.EntityTypes);
        Assert.Contains(typeof(R_COMPRESSOR_ANALYSIS_REFERENCE_CODE), module.EntityTypes);
    }
}
