using Beep.OilandGas.FlashCalculations.Data;
using Beep.OilandGas.FlashCalculations.Modules;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Xunit;

namespace Beep.OilandGas.FlashCalculations.Tests;

public class FlashCalculationsModuleContractTests
{
    [Fact]
    public void FlashCalculationsModule_HasStableIdentityAndEntityList()
    {
        var ctx = new ModuleSetupContext();
        var module = new FlashCalculationsModule(ctx);

        Assert.Equal("FLASH_CALCULATIONS", module.ModuleId);
        Assert.Equal(73, module.Order);
        Assert.Single(module.EntityTypes);
        Assert.Contains(typeof(R_FLASH_CALCULATION_REFERENCE_CODE), module.EntityTypes);
    }
}
