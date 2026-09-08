using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.SuckerRodPumping.Services;
using Moq;
using TheTechIdea.Beep.Editor;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class SuckerRodRoutingTests
{
    [Fact]
    public async Task DesignSaveRequiresBindingBeforeDatasourceAccess()
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var calls = 0;
        Task<string> Resolve() { calls++; return Task.FromResult(""); }
        var service = new SuckerRodPumpingService(editor.Object, Mock.Of<ICommonColumnHandler>(),
            Mock.Of<IPPDM39DefaultsRepository>(), Mock.Of<IPPDMMetadataRepository>(), "global-db", resolveConnection: Resolve);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SavePumpDesignAsync(new(), "actor"));
        Assert.Equal(1, calls);
        editor.VerifyNoOtherCalls();
    }
}
