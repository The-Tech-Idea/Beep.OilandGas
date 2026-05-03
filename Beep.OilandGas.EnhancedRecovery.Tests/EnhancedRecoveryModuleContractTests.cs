using Beep.OilandGas.EnhancedRecovery.Data;
using Beep.OilandGas.EnhancedRecovery.Modules;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Xunit;

namespace Beep.OilandGas.EnhancedRecovery.Tests;

public class EnhancedRecoveryModuleContractTests
{
    [Fact]
    public void EnhancedRecoveryModule_HasStableIdentityAndEntityList()
    {
        var ctx = new ModuleSetupContext();
        var module = new EnhancedRecoveryModule(ctx);

        Assert.Equal("ENHANCED_RECOVERY", module.ModuleId);
        Assert.Equal(79, module.Order);
        Assert.Single(module.EntityTypes);
        Assert.Contains(typeof(R_ENHANCED_RECOVERY_REFERENCE_CODE), module.EntityTypes);
    }
}
