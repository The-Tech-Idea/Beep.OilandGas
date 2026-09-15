using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Repository;
using Microsoft.EntityFrameworkCore;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Data.OilGas;
using Xunit;
using Xunit.Abstractions;

namespace Beep.OilandGas.ApiService.Tests;

public class AssetDatabaseScopeTests(ITestOutputHelper output)
{
    [LocalDbModuleFact]
    public async Task ScopeTracksReviewedBindingAndConfiguredPhysicalTarget()
    {
        var database = $"BeepOilGas_AssetScope_{Guid.NewGuid():N}";
        output.WriteLine($"Retained asset scope repository: {database}");
        await using var db = new SqlServerRepositoryDbContext(new DbContextOptionsBuilder<SqlServerRepositoryDbContext>()
            .UseSqlServer($"Server=(localdb)\\MSSQLLocalDB;Database={database};Integrated Security=true;TrustServerCertificate=true").Options);
        await db.Database.MigrateAsync();
        var configured = new ConnectionProperties { ConnectionName = "selected", Host = "(localdb)\\MSSQLLocalDB", Database = "assets-a" };
        var config = new Mock<IConfigEditor>();
        config.SetupGet(x => x.DataConnections).Returns([configured]);
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        editor.SetupGet(x => x.ConfigEditor).Returns(config.Object);
        var resolver = new ModuleConnectionResolver(db, editor.Object);
        await Assert.ThrowsAsync<InvalidOperationException>(() => resolver.ResolveAssetScopeAsync());
        var binding = new ModuleDatabaseBinding { ModuleId = "PPDM_CORE", ConnectionName = "selected", ConcurrencyStamp = "review-one" };
        db.ModuleDatabases.Add(binding);
        await db.SaveChangesAsync();
        var original = await resolver.ResolveAssetScopeAsync();
        Assert.Equal("selected", original.ConnectionName);
        Assert.Equal(64, original.Fingerprint.Length);
        Assert.Equal(original, await resolver.ResolveAssetScopeAsync());
        binding.ConcurrencyStamp = "review-two";
        await db.SaveChangesAsync();
        var rebound = await resolver.ResolveAssetScopeAsync();
        Assert.NotEqual(original.Fingerprint, rebound.Fingerprint);
        configured.Database = "assets-b";
        Assert.NotEqual(rebound.Fingerprint, (await resolver.ResolveAssetScopeAsync()).Fingerprint);
        binding.ConcurrencyStamp = "";
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<InvalidOperationException>(() => resolver.ResolveAssetScopeAsync());
        editor.Verify(x => x.GetDataSource(It.IsAny<string>()), Times.Never);
    }
}
