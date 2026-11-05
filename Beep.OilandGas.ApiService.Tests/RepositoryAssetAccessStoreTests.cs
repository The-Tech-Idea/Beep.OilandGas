using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Repository.Models;
using Moq;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.ApiService.Tests.Infrastructure;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace Beep.OilandGas.ApiService.Tests;

public class RepositoryAssetAccessStoreTests(ITestOutputHelper output)
{
    [LocalDbModuleFact]
    public async Task Company_grants_persist_and_revoke_without_deleting_another_companys_grant()
    {
        var database = $"BeepOilGas_AssetStore_{Guid.NewGuid():N}";
        output.WriteLine($"Retained asset store database: {database}");
        var options = new DbContextOptionsBuilder<SqlServerRepositoryDbContext>().UseSqlServer(
            $"Server=(localdb)\\MSSQLLocalDB;Database={database};Integrated Security=true;TrustServerCertificate=true").Options;
        await using var db = new SqlServerRepositoryDbContext(options); await db.Database.MigrateAsync();
        db.Users.AddRange(new OilGasUser { Id = "actor", UserName = "actor", IsActive = true }, new OilGasUser { Id = "member", UserName = "member", IsActive = true });
        db.Roles.Add(new IdentityRole { Id = "admin", Name = "Administrator", NormalizedName = "ADMINISTRATOR" });
        db.UserRoles.Add(new IdentityUserRole<string> { UserId = "actor", RoleId = "admin" }); await db.SaveChangesAsync();
        var firstGate = await TestCompanyModules.CreateAsync(db, "actor", "first"); var scope = await firstGate.ResolveAsync();
        var first = new RepositoryAssetAccessStore(db, firstGate, Mock.Of<IAssetGrantTargetVerifier>());
        Assert.True(await first.GrantAsync("member", scope.Fingerprint, "FIELD", "same-id", "WRITE", false));
        var original = Assert.Single(await first.ReadAsync("member", scope.Fingerprint));
        var otherGate = await TestCompanyModules.CreateAsync(db, "actor", "other"); var other = new RepositoryAssetAccessStore(db, otherGate, Mock.Of<IAssetGrantTargetVerifier>());
        Assert.True(await other.GrantAsync("member", scope.Fingerprint, "FIELD", "same-id", "READ", false));
        var separate = Assert.Single(await other.ReadAsync("member", scope.Fingerprint)); Assert.NotEqual(original.Id, separate.Id);
        Assert.True(await first.RevokeAsync("member", scope.Fingerprint, "FIELD", "same-id"));
        Assert.Empty(await first.ReadAsync("member", scope.Fingerprint)); Assert.Single(await other.ReadAsync("member", scope.Fingerprint));
        await using var reopened = new SqlServerRepositoryDbContext(options);
        var reopenedGate = await TestCompanyModules.CreateAsync(reopened, "actor", "other");
        var persisted = Assert.Single(await new RepositoryAssetAccessStore(reopened, reopenedGate, Mock.Of<IAssetGrantTargetVerifier>()).ReadAsync("member", scope.Fingerprint));
        Assert.Equal(separate.Id, persisted.Id); Assert.Equal(separate.CreatedUtc, persisted.CreatedUtc); Assert.Equal("actor", persisted.CreatedBy);
        Assert.Equal(2, await reopened.Set<AppUserAssetAccess>().CountAsync());
        db.UserRoles.Remove(await db.UserRoles.SingleAsync()); await db.SaveChangesAsync();
        await Refusals.ForbiddenAsync(() => first.GrantAsync("member", scope.Fingerprint, "FIELD", "same-id", "READ", false));
        await Refusals.ForbiddenAsync(() => first.RevokeAsync("member", scope.Fingerprint, "FIELD", "same-id"));
    }
}
