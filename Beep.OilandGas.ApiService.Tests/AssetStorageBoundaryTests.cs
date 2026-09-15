using Beep.OilandGas.LifeCycle.Services.AccessControl;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Moq;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class AssetStorageBoundaryTests
{
    [Fact]
    public async Task EveryAssetOperationRequiresBindingBeforeStorage()
    {
        var store = new Mock<IUserAssetAccessStore>(MockBehavior.Strict);
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var service = Create(editor.Object, store.Object, () => throw new InvalidOperationException("missing binding"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetUserAccessibleAssetsAsync("user"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GrantAssetAccessAsync("user", "well", "WELL"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RevokeAssetAccessAsync("user", "well", "WELL"));
        Assert.False((await service.CheckAssetAccessAsync("user", "well", "WELL")).HasAccess);
        store.VerifyNoOtherCalls();
        editor.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ReadCannotReturnGrantsWhenDatabaseScopeChangesDuringTheOperation()
    {
        var first = new AssetDatabaseScope("module", new string('A', 64));
        var second = first with { Fingerprint = new string('B', 64) };
        var resolutions = 0;
        var store = new Mock<IUserAssetAccessStore>(MockBehavior.Strict);
        store.Setup(x => x.ReadAsync("user", first.Fingerprint, null)).ReturnsAsync(new List<AppUserAssetAccess>
        {
            new() { UserId = "user", AssetType = "WELL", AssetId = "well", IsActive = true, AccessLevel = "READ" }
        });
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var service = Create(editor.Object, store.Object, () => Task.FromResult(++resolutions % 2 == 1 ? first : second));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetUserAccessibleAssetsAsync("user"));
        Assert.False((await service.CheckAssetAccessAsync("user", "well", "WELL")).HasAccess);
        store.Verify(x => x.ReadAsync("user", first.Fingerprint, null), Times.Exactly(2));
        editor.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task MutationsUseCurrentScopeAndNeverOpenDomainSecurityTables()
    {
        var first = new AssetDatabaseScope("first", new string('A', 64));
        var second = new AssetDatabaseScope("second", new string('B', 64));
        var current = first;
        var store = new Mock<IUserAssetAccessStore>(MockBehavior.Strict);
        store.Setup(x => x.GrantAsync("user", first.Fingerprint, "WELL", "well", "WRITE", false, null)).ReturnsAsync(true);
        store.Setup(x => x.RevokeAsync("user", second.Fingerprint, "WELL", "well")).ReturnsAsync(true);
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var service = Create(editor.Object, store.Object, () => Task.FromResult(current));
        Assert.True(await service.GrantAssetAccessAsync("user", "well", "WELL", "WRITE", false));
        current = second;
        Assert.True(await service.RevokeAssetAccessAsync("user", "well", "WELL"));
        store.VerifyAll();
        editor.VerifyNoOtherCalls();
    }

    private static UserAssetAccessService Create(IDMEEditor editor, IUserAssetAccessStore store, Func<Task<AssetDatabaseScope>> resolve) =>
        new(editor, Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(), Mock.Of<IPPDMMetadataRepository>(),
            new PPDMMappingService(), Mock.Of<IApplicationAuthorizationReader>(), Mock.Of<IApplicationRolePermissionStore>(), store, resolve);
}
