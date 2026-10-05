using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data;
using Moq;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class BoundAssetHierarchyTests
{
    [Theory]
    [InlineData("hierarchy")]
    [InlineData("user")]
    [InlineData("children")]
    [InlineData("path")]
    [InlineData("validate")]
    [InlineData("config")]
    [InlineData("update")]
    public async Task MissingBindingNeverCreatesGlobalFallback(string operation)
    {
        var created = false;
        var service = new BoundAssetHierarchyService(() => throw new InvalidOperationException("binding missing"), _ =>
        {
            created = true;
            throw new InvalidOperationException("factory must not run");
        });
        await Assert.ThrowsAsync<InvalidOperationException>(() => Invoke(service, operation));
        Assert.False(created);
    }

    [Fact]
    public async Task EachOperationCreatesAServiceForItsCurrentScope()
    {
        var current = new AssetDatabaseScope("first", new string('A', 64));
        var seen = new List<AssetDatabaseScope>();
        var inner = new Mock<IAssetHierarchyService>(MockBehavior.Strict);
        inner.Setup(x => x.GetAssetChildrenAsync("field", "FIELD", null)).ReturnsAsync(new List<AssetHierarchyNode>());
        var service = new BoundAssetHierarchyService(() => Task.FromResult(current), scope =>
        {
            seen.Add(scope);
            return inner.Object;
        });
        Assert.Empty(await service.GetAssetChildrenAsync("field", "FIELD"));
        current = new("second", new string('B', 64));
        Assert.Empty(await service.GetAssetChildrenAsync("field", "FIELD"));
        Assert.Equal(new[] { "first", "second" }, seen.Select(x => x.ConnectionName));
        inner.Verify(x => x.GetAssetChildrenAsync("field", "FIELD", null), Times.Exactly(2));
    }

    [Fact]
    public async Task BindingChangePreventsReturningOldHierarchy()
    {
        var current = new AssetDatabaseScope("first", new string('A', 64));
        var inner = new Mock<IAssetHierarchyService>(MockBehavior.Strict);
        inner.Setup(x => x.GetAssetChildrenAsync("field", "FIELD", null))
            .Callback(() => current = new("second", new string('B', 64)))
            .ReturnsAsync(new List<AssetHierarchyNode> { new() { AssetId = "sensitive", AssetType = "WELL" } });
        var service = new BoundAssetHierarchyService(() => Task.FromResult(current), _ => inner.Object);
        // A binding an administrator changed mid-request is theirs to reload: refused (409), never answered from the old one.
        await Refusals.RefusedAsync(RefusalKind.Conflict, () => service.GetAssetChildrenAsync("field", "FIELD"));
    }

    private static Task Invoke(IAssetHierarchyService service, string operation) => operation switch
    {
        "hierarchy" => service.GetAssetHierarchyAsync("org"),
        "user" => service.GetAssetHierarchyForUserAsync("user"),
        "children" => service.GetAssetChildrenAsync("field", "FIELD"),
        "path" => service.GetAssetPathAsync("well", "WELL"),
        "validate" => service.ValidateAccessAsync("user", []),
        "config" => service.GetHierarchyConfigAsync("org"),
        "update" => service.UpdateHierarchyConfigAsync("org", []),
        _ => throw new ArgumentOutOfRangeException(nameof(operation))
    };
}
