using Beep.OilandGas.GasLift.Data;
using Beep.OilandGas.GasLift.Modules;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Xunit;

namespace Beep.OilandGas.GasLift.Tests;

public class GasLiftModuleContractTests
{
    [Fact]
    public void GasLiftModule_HasStableIdentityAndEntityList()
    {
        var ctx = new ModuleSetupContext();
        var module = new GasLiftModule(ctx);

        Assert.Equal("GAS_LIFT", module.ModuleId);
        Assert.Equal(74, module.Order);
        Assert.Single(module.EntityTypes);
        Assert.Contains(typeof(R_GAS_LIFT_REFERENCE_CODE), module.EntityTypes);
    }
}
