using Beep.OilandGas.LifeCycle.Services.AccessControl;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Moq;
using TheTechIdea.Beep.Editor;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class HierarchyPathAccessTests
{
    [Theory]
    [InlineData("empty")]
    [InlineData("null-path")]
    [InlineData("null-node")]
    [InlineData("blank-user")]
    [InlineData("blank-id")]
    [InlineData("blank-type")]
    public async Task MalformedPathsAreDeniedWithoutAccessLookup(string scenario)
    {
        var access = new Mock<IAccessControlService>(MockBehavior.Strict);
        var path = Path();
        var user = "user";
        switch (scenario)
        {
            case "empty": path.Clear(); break;
            case "null-path": path = null!; break;
            case "null-node": path.Add(null!); break;
            case "blank-user": user = " "; break;
            case "blank-id": path[1].AssetId = " "; break;
            case "blank-type": path[1].AssetType = " "; break;
        }
        Assert.False(await Create(access.Object).ValidateAccessAsync(user, path));
        access.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("granted", true)]
    [InlineData("denied", false)]
    [InlineData("null", false)]
    [InlineData("failure", false)]
    public async Task EveryNodeMustHaveAnExplicitGrant(string result, bool expected)
    {
        var access = new Mock<IAccessControlService>(MockBehavior.Strict);
        access.Setup(x => x.CheckAssetAccessAsync("user", "field", "FIELD", null))
            .ReturnsAsync(new AccessCheckResponse { HasAccess = true });
        var well = access.Setup(x => x.CheckAssetAccessAsync("user", "well", "WELL", null));
        if (result == "failure") well.ThrowsAsync(new InvalidOperationException("store unavailable"));
        else well.ReturnsAsync(result == "null" ? null! : new AccessCheckResponse { HasAccess = result == "granted" });
        Assert.Equal(expected, await Create(access.Object).ValidateAccessAsync("user", Path()));
        access.VerifyAll();
    }

    [Fact]
    public async Task CancellationIsNotReportedAsACompletedCheck()
    {
        var access = new Mock<IAccessControlService>(MockBehavior.Strict);
        access.Setup(x => x.CheckAssetAccessAsync("user", "field", "FIELD", null))
            .ThrowsAsync(new OperationCanceledException());
        await Assert.ThrowsAsync<OperationCanceledException>(() => Create(access.Object).ValidateAccessAsync("user", Path()));
    }

    private static List<AssetHierarchyNode> Path() =>
        [new() { AssetId = "field", AssetType = "FIELD" }, new() { AssetId = "well", AssetType = "WELL" }];

    private static AssetHierarchyService Create(IAccessControlService access) => new(
        Mock.Of<IDMEEditor>(), Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(),
        Mock.Of<IPPDMMetadataRepository>(), new PPDMMappingService(), access, "selected-module");
}
