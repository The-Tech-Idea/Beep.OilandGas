using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.UserManagement.Models.Scope;
using Beep.OilandGas.UserManagement.Services;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Xunit;

namespace Beep.OilandGas.UserManagement.Tests;

public class FieldAccessTests
{
    private static UserAssetAccess Asset(string id = "F1") => new()
    { USER_ID = "user", ASSET_ID = id, ASSET_TYPE = "FIELD", ACTIVE_IND = "Y" };

    private static UserScopeAssignment Scope(string type = "FIELD", string value = "F2") => new()
    { USER_ID = "user", SCOPE_TYPE = type, SCOPE_VALUE = value, ACTIVE_IND = "Y", EFFECTIVE_FROM_UTC = DateTime.UtcNow.AddDays(-1) };

    [Fact]
    public async Task ResolvesOnlyEffectiveFieldGrants()
    {
        var expired = Asset("expired"); expired.ACCESS_EXPIRES_UTC = DateTime.UtcNow.AddDays(-1);
        var well = Asset("well"); well.ASSET_TYPE = "WELL";
        var inactive = Asset("inactive"); inactive.ACTIVE_IND = "N";
        var other = Asset("other"); other.USER_ID = "other";
        var future = Scope("GLOBAL"); future.EFFECTIVE_FROM_UTC = DateTime.UtcNow.AddDays(1);
        var expiredScope = Scope("GLOBAL"); expiredScope.EFFECTIVE_TO_UTC = DateTime.UtcNow.AddDays(-1);
        var service = Create([Asset(), expired, well, inactive, other], [Scope(), future, expiredScope]);
        Assert.Equal(new[] { "F1", "F2" }, (await service.GetUserFieldsAsync("user")).Order().ToArray());
        Assert.True(await service.HasFieldAccessAsync("user", "f1"));
        Assert.Empty(await service.GetUserFieldsByAssetTypeAsync("user", "WELL"));
    }

    [Fact]
    public async Task DenialOverridesDirectAndGlobalGrants()
    {
        var deny = Asset(); deny.DENY_OVERRIDE_IND = "Y";
        var service = Create([Asset(), deny], [Scope("GLOBAL"), Scope()]);
        Assert.Equal(new[] { "F2" }, await service.GetUserFieldsAsync("user"));
        Assert.False(await service.HasFieldAccessAsync("user", "F1"));
    }

    [Theory]
    [InlineData("*")]
    [InlineData("GLOBAL")]
    public async Task GlobalDenialOverridesEverything(string value)
    {
        var deny = Asset(value); deny.DENY_OVERRIDE_IND = "Y";
        Assert.Empty(await Create([Asset(), deny], [Scope("GLOBAL")]).GetUserFieldsAsync("user"));
    }

    [Fact]
    public async Task EffectiveGlobalGrantProducesWildcard()
    {
        Assert.Equal(new[] { "*" }, await Create([], [Scope("GLOBAL")]).GetUserFieldsAsync("user"));
    }

    [Fact]
    public async Task ScopeLookupFailureDoesNotLeakPreviouslyReadGrants()
    {
        Assert.Empty(await Create([Asset()], [], failScopes: true).GetUserFieldsAsync("user"));
    }

    [Theory]
    [InlineData("SYSTEM")]
    [InlineData("")]
    public async Task UserNameAloneNeverGrantsAccess(string user)
    {
        Assert.Empty(await Create([], []).GetUserFieldsAsync(user));
    }

    [Fact]
    public async Task RejectsClaimSeparatorInFieldIds()
    {
        Assert.Empty(await Create([Asset("F1,F2")], []).GetUserFieldsAsync("user"));
    }

    private static FieldAccessService Create(List<UserAssetAccess> assets, List<UserScopeAssignment> scopes, bool failScopes = false)
    {
        var source = new Mock<IDataSource>();
        source.Setup(s => s.GetEntityAsync("USER_ASSET_ACCESS", It.IsAny<List<AppFilter>>())).ReturnsAsync(assets);
        var scopeCall = source.Setup(s => s.GetEntityAsync("USER_SCOPE_ASSIGNMENT", It.IsAny<List<AppFilter>>()));
        if (failScopes) scopeCall.ThrowsAsync(new InvalidOperationException("Unavailable"));
        else scopeCall.ReturnsAsync(scopes);
        var editor = new Mock<IDMEEditor>();
        editor.Setup(e => e.GetDataSource("PPDM39")).Returns(source.Object);
        return new(editor.Object, Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(), Mock.Of<IPPDMMetadataRepository>());
    }
}
