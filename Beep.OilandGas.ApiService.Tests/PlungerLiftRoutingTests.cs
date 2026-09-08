using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PlungerLift.Services;
using Moq;
using TheTechIdea.Beep.Editor;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class PlungerLiftRoutingTests
{
    [Theory]
    [InlineData("save-design")]
    [InlineData("read-design")]
    [InlineData("update-design")]
    [InlineData("save-performance")]
    [InlineData("read-performance")]
    public async Task EveryStoragePathRequiresBindingBeforeDatasourceAccess(string operation)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var calls = 0;
        Task<string> Resolve() { calls++; throw new InvalidOperationException("Unbound"); }
        var service = new PlungerLiftService(editor.Object, Mock.Of<ICommonColumnHandler>(),
            Mock.Of<IPPDM39DefaultsRepository>(), Mock.Of<IPPDMMetadataRepository>(), "global-db", resolveConnection: Resolve);
        await Assert.ThrowsAsync<InvalidOperationException>(() => operation switch
        {
            "save-design" => service.SavePlungerLiftDesignAsync(new(), "actor"),
            "read-design" => service.GetPlungerLiftDesignAsync("well"),
            "update-design" => service.UpdatePlungerLiftDesignAsync(new(), "actor"),
            "save-performance" => service.SavePerformanceDataAsync(new(), "actor"),
            _ => service.GetPerformanceDataAsync("well", DateTime.UtcNow.AddDays(-1), DateTime.UtcNow)
        });
        Assert.Equal(1, calls);
        editor.VerifyNoOtherCalls();
    }
}
