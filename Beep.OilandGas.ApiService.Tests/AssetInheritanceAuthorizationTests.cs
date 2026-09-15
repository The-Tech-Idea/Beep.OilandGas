using Beep.OilandGas.LifeCycle.Services.AccessControl;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Xunit;
using TheTechIdea.Data.OilGas;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;

namespace Beep.OilandGas.ApiService.Tests;

public class AssetInheritanceAuthorizationTests
{
    [Theory]
    [InlineData("FIELD", "other-field", null, false, false)]
    [InlineData("WELL", "child-well", null, false, true)]
    [InlineData("WELL", "other-well", null, false, false)]
    [InlineData("WELL", "child-well", "approve", false, false)]
    [InlineData("WELL", "child-well", "approve", true, true)]
    public async Task InheritanceRequiresAnActualChildAndAnyRequestedPermission(string type, string id,
        string? permission, bool hasPermission, bool expected)
    {
        var source = new Mock<IDataSource>(MockBehavior.Strict);
        source.Setup(x => x.GetEntityAsync("WELL", It.Is<List<AppFilter>>(filters =>
            filters.Count == 1 && filters[0].FieldName == "ASSIGNED_FIELD" && filters[0].FilterValue == "field")))
            .ReturnsAsync(new object[] { new { UWI = "child-well", ASSIGNED_FIELD = "field" },
                new { UWI = "other-well", ASSIGNED_FIELD = "FIELD" } });
        foreach (var table in new[] { "POOL", "FACILITY" })
            source.Setup(x => x.GetEntityAsync(table, It.IsAny<List<AppFilter>>())).ReturnsAsync(Array.Empty<object>());
        var authorization = new Mock<IApplicationAuthorizationReader>(MockBehavior.Strict);
        if (permission is not null && id == "child-well")
            authorization.Setup(x => x.HasPermissionAsync("user", permission)).ReturnsAsync(hasPermission);
        var service = Create(source.Object, authorization.Object);
        var result = await service.CheckAssetAccessAsync("user", id, type, permission);
        Assert.Equal(expected, result.HasAccess);
        if (expected) Assert.Equal("WRITE", result.AccessLevel);
        source.VerifyAll();
        authorization.VerifyAll();
    }

    [Theory]
    [InlineData("WRITE")]
    [InlineData("DELETE")]
    public async Task DirectElevatedAccessDoesNotReplaceRequiredApplicationPermission(string level)
    {
        var source = new Mock<IDataSource>(MockBehavior.Strict);
        var authorization = new Mock<IApplicationAuthorizationReader>(MockBehavior.Strict);
        authorization.Setup(x => x.HasPermissionAsync("user", "approve")).ReturnsAsync(false);
        Assert.False((await Create(source.Object, authorization.Object, false, level).CheckAssetAccessAsync("user", "well", "WELL", "approve")).HasAccess);
        source.VerifyAll();
        authorization.VerifyAll();
    }

    [Fact]
    public async Task StaleCachedDatasourceCannotSupplyInheritedAssets()
    {
        var source = new Mock<IDataSource>(MockBehavior.Strict);
        var service = Create(source.Object, Mock.Of<IApplicationAuthorizationReader>(), stale: true);
        Assert.False((await service.CheckAssetAccessAsync("user", "child-well", "WELL")).HasAccess);
        source.Verify(x => x.GetEntityAsync(It.IsAny<string>(), It.IsAny<List<AppFilter>>()), Times.Never);
    }

    private static UserAssetAccessService Create(IDataSource source, IApplicationAuthorizationReader authorization, bool inherit = true, string level = "WRITE", bool stale = false)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        editor.Setup(x => x.GetDataSource("module")).Returns(source);
        var properties = new ConnectionProperties { ConnectionName = "module" };
        var config = new Mock<IConfigEditor>();
        config.SetupGet(x => x.DataConnections).Returns([properties]);
        editor.SetupGet(x => x.ConfigEditor).Returns(config.Object);
        var connection = new Mock<IDataConnection>();
        connection.SetupGet(x => x.ConnectionProp).Returns(stale ? new ConnectionProperties { ConnectionName = "module", Database = "wrong-database" } : properties);
        if (inherit)
        {
            Mock.Get(source).SetupGet(x => x.Dataconnection).Returns(connection.Object);
            Mock.Get(source).SetupGet(x => x.DatasourceName).Returns("module");
        }
        var store = new Mock<IUserAssetAccessStore>(MockBehavior.Strict);
        store.Setup(x => x.ReadAsync("user", new string('A', 64), null)).ReturnsAsync(new List<AppUserAssetAccess>
        {
            new() { UserId = "user", AssetId = inherit ? "field" : "well", AssetType = inherit ? "FIELD" : "WELL",
                AccessLevel = level, Inherit = inherit, IsActive = true }
        });
        return new(editor.Object, Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(),
            Mock.Of<IPPDMMetadataRepository>(), new PPDMMappingService(), authorization,
            Mock.Of<IApplicationRolePermissionStore>(), store.Object, () => Task.FromResult(new AssetDatabaseScope("module", new string('A', 64))));
    }
}
